// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:54
//     文件名称： BufferResLoader.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using VEngine;
using UniRx;
using Object = UnityEngine.Object;

namespace SuperMobs.Game.AssetLoader
{
    internal class CompleteInfo
    {
        public Action<Object> action;
        public float time;
        public bool forceNew = false;

        public CompleteInfo(Action<Object> a, float t,bool force)
        {
            action = a;
            time = t;
            forceNew = force;
        }

        public void Dispose()
        {
            action = null;
        }
    }

    public class BufferResLoader
    {
        private int _curUseCount = 1;

        private readonly string _path;//资源路径
        private readonly ResConfig _resConfig;//资源配置
        private Asset _assetHandler;//资源句柄
        private float _curUtc;//时间戳

        private readonly List<GameObject> _owners;//资源持有者
        private readonly Dictionary<int, Res> _usings;//使用中列表
        private readonly Stack<Res> _frees;//待使用列表

        private readonly Queue<CompleteInfo> _getAsyncCompletes = new Queue<CompleteInfo>();//异步实例化列表

        //Custom时间管理参数
        private float _curWaitTime = 0f;
        private bool _startAutoDestroy = false;
        private float _curIntervalTime = 0f;

        private IDisposable _updateOB;
        private IDisposable _getAsyncOB;
        public BufferResLoader(string path)
        {
            _path = path;
            _resConfig = ResManager.Instance.GetResConfigs(path);
            _curUtc = Time.realtimeSinceStartup;

            ResetAutoDestroyTime();

            _owners = new List<GameObject>();
            _usings = new Dictionary<int, Res>();
            _frees = new Stack<Res>();

            _updateOB = Observable.Interval(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
            {
                Update();
            }).AddTo(ResManager.Instance);

            _getAsyncOB = Observable.EveryUpdate().Subscribe(_ =>
            {
                if (_assetHandler != null && _assetHandler.isDone)
                {
                    while(_getAsyncCompletes.Count > 0 && !Updater.Instance.busy)
                    {
                        CompleteInfo info = _getAsyncCompletes.Dequeue();
                        Object obj = Get(info.time, info.forceNew);

                        info.action.Invoke(obj);
                        info.Dispose();
                    }
                }

            }).AddTo(ResManager.Instance);
        }

        private void ResetAutoDestroyTime()
        {
            _curWaitTime = 0f;
            _startAutoDestroy = false;
            _curIntervalTime = 0f;
        }

        private Object Get(float retaintime, bool forceNew = false)
        {
            Object res = null;

            //已有
            if (_frees.Count > 0 && !forceNew)
            {
                Res r = _frees.Pop();
                r.ratainTime = retaintime;
                r.useIndex = _curUseCount++;

                _usings.Add(r.o.GetInstanceID(),r);

                res = r.o;
            }           
            //超过实例上限
            else if (_usings.Count >= _resConfig.Max && !forceNew)
            {
                Debug.Log(_path + " 资源使用过热");

                Res r = null;

                foreach(var kv in _usings)
                {
                    if (r == null)
                        r = kv.Value;
                    else
                    {
                        if (r.useIndex > kv.Value.useIndex)
                            r = kv.Value;
                    }
                }

                r.ratainTime = retaintime;
                r.useIndex = _curUseCount++;

                res = r.o;
            }
            //做个新的
            else
            {
                Object o = Object.Instantiate(_assetHandler.asset, Vector3.zero, Quaternion.identity, null);
                Res r = new Res(o, _curUseCount++, retaintime);

                _usings.Add(r.o.GetInstanceID(), r);

                res = r.o;
            }

            GameObject go = res as GameObject;
            if (go != null)
            {
                if (go.transform.GetChild(go.transform.childCount - 1).name != _path)
                {
                    GameObject name = new GameObject(_path);
                    name.transform.parent = go.transform;
                }
                go.transform.parent = null;
                go.SetActive(true);
            }

            ResetAutoDestroyTime();

            return res;
        }

        public Object LoadAsset(Type t, float retaintime, string ownerLabel)
        {
            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);
            if (!_owners.Contains(owner))
                _owners.Add(owner);

            if (_assetHandler == null || !_assetHandler.isDone)
                _assetHandler = Asset.Load(_path, t);

            var obj = Get(retaintime);
            return obj;
        }

        public void LoadAsync(Type t, Action<Object> completed, float retaintime, string ownerLabel)
        {
            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);
            if (!_owners.Contains(owner))
                _owners.Add(owner);

            if (_assetHandler == null)
            {
                _assetHandler = Asset.LoadAsync(_path, t, (asset) => 
                {
                    _getAsyncCompletes.Enqueue(new CompleteInfo(completed, retaintime, false));
                });
            }
            else
            {
                if (_assetHandler.isDone)
                {
                    _getAsyncCompletes.Enqueue(new CompleteInfo(completed, retaintime, false));
                }
                else
                {
                    _assetHandler.completed += (asset) =>
                    {
                        _getAsyncCompletes.Enqueue(new CompleteInfo(completed, retaintime, false));
                    };
                }
            }
        }

        public void Preload(Type t,bool needPreInstantiate, string ownerLabel)
        {
            if (_resConfig.OwnerLevel == OwnerLevel.Custom)
            {
                Debug.LogError("无法预加载Custom类型:" + _path);
                return;
            }

            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);
            if (!_owners.Contains(owner))
                _owners.Add(owner);

            if (needPreInstantiate)
            {
                if (_assetHandler == null)
                {
                    _assetHandler = Asset.LoadAsync(_path, t, (asset) =>
                    {
                        uint count = _resConfig.Min;

                        for (int i = 0; i < count; i++)
                        {
                            _getAsyncCompletes.Enqueue(new CompleteInfo((o => { Retain(o); }), float.NaN, true));
                        }
                    });
                }
                else
                {
                    if (_assetHandler.isDone)
                    {
                        uint count = _resConfig.Min - (uint)_frees.Count - (uint)_usings.Count;

                        for (int i = 0; i < count; i++)
                        {
                            _getAsyncCompletes.Enqueue(new CompleteInfo((o => { Retain(o); }), float.NaN, true));
                        }
                    }
                    else
                    {
                        _assetHandler.completed += (asset) =>
                        {
                            uint count = _resConfig.Min - (uint)_frees.Count - (uint)_usings.Count;

                            for (int i = 0; i < count; i++)
                            {
                                _getAsyncCompletes.Enqueue(new CompleteInfo((o => { Retain(o); }), float.NaN, true));
                            }
                        };
                    }
                }
            }
            else
            {
                if (_assetHandler == null)
                    _assetHandler = Asset.LoadAsync(_path, t, null);
            }
        }

        public bool LoadDone()
        {
            if (_assetHandler == null || _assetHandler.isDone == false || _getAsyncCompletes.Count > 0)
                return false;

            return true;
        }

        //归还
        public void Retain(Object obj)
        {
            if (!_usings.ContainsKey(obj.GetInstanceID()))
            {
                Debug.LogError("归还了奇怪的东西");
                return;
            }

            Res r = _usings[obj.GetInstanceID()];

            r.useIndex = -1;
            r.ratainTime = float.NaN;

            GameObject go = r.o as GameObject;

            if (go != null)
            {
                //GameObject.Destroy(go.transform.GetChild(go.transform.childCount - 1).gameObject);
                go.SetActive(false);
                go.transform.parent = ResManager.Instance.transform;
            }

            _usings.Remove(obj.GetInstanceID());
            _frees.Push(r);
        }

        public void Update()
        {
            float dt = Time.realtimeSinceStartup - _curUtc;
            _curUtc = Time.realtimeSinceStartup;

            //维护孩子们
            List<Object> retains = new List<Object>();
            foreach(var kv in _usings)
            {
                kv.Value.Update(dt);

                if (kv.Value.ratainTime <= 0f)
                {
                    retains.Add(kv.Value.o);

                }
            }

            for(int i = 0;i < retains.Count;i++)
            {
                Retain(retains[i]);
            }
            retains.Clear();


            //自动release
            //Custom类型
            if (_resConfig.OwnerLevel == OwnerLevel.Custom)
            {
                _curWaitTime += dt;

                if (_curWaitTime >= _resConfig.StartAutoDestroyTime)
                {
                    _startAutoDestroy = true;
                }

                if (_startAutoDestroy)
                {
                    _curIntervalTime += dt;

                    if (_curIntervalTime >= _resConfig.DestroyIntervalTime)
                    {
                        _curIntervalTime = 0f;

                        if (_frees.Count > 0)
                        {
                            Res r = _frees.Pop();
                            r.Dispose();
                        }
                    }
                }

                if (_usings.Count + _frees.Count == 0 && _startAutoDestroy)
                {
                    Clear();
                }
            }
            else
            //非Custom类型
            {
                List<int> deletes = new List<int>();

                for(int i = 0;i < _owners.Count;i++)
                {
                    if (_owners[i] == null)
                        deletes.Add(i);
                }

                for(int i = 0;i < deletes.Count;i++)
                {
                    _owners.RemoveAt(deletes[i]);
                }

                deletes.Clear();

                if (_owners.Count == 0)
                {
                    Clear();
                }
            }
        }
        
        public void Clear()
        {
            foreach (var kv in _usings)
            {
                kv.Value.Dispose();
            }

            while (_frees.Count > 0)
            {
                Res r = _frees.Pop();

                r.Dispose();
            }

            while(_getAsyncCompletes.Count > 0)
            {
                CompleteInfo info = _getAsyncCompletes.Dequeue();
                info.Dispose();
            }

            _usings.Clear();
            _frees.Clear();
            _getAsyncCompletes.Clear();

            if (_assetHandler != null)
            {
                _assetHandler.Release();
                _assetHandler = null;
            }
        }

        //强制释放接口
        public void Dispose()
        {
            Clear();

            if (_updateOB != null)
            {
                _updateOB.Dispose();
                _updateOB = null;
            }

            if (_getAsyncOB != null)
            {
                _getAsyncOB.Dispose();
                _getAsyncOB = null;
            }
        }
    }
}