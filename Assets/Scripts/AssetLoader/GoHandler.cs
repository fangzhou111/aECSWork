// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:54
//     文件名称： GoHandler.cs
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
using Object = UnityEngine.Object;

namespace SuperMobs.Game.AssetLoader
{
    public class GoHandler:IHandler
    {
        private readonly string _path;
        private readonly ResConfig _resConfig;
        private Asset _assetHandler;
        private readonly List<Object> _useObjects = new List<Object>();
        private readonly Stack<Object> _unUseObjects = new Stack<Object>();
        private readonly Queue<Action<Object>> _completedQueue = new Queue<Action<Object>>();
        private readonly List<GameObject> _owners = new List<GameObject>();
        private bool _loadMin = false;
        private int _useIndex = -1;
        public GoHandler(string path, string ownerLabel)
        {
            _path = path;
            _resConfig = ResManager.Instance.GetResConfigs(path);

            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);
            if (!_owners.Contains(owner))
            {
                _owners.Add(owner);
            }
        }

        public Object LoadAsset(Type t)
        {
            if (_assetHandler == null || !_assetHandler.isDone)
            {
                _assetHandler = Asset.Load(_path, t);
            }

            var obj = Instantiate();
            return obj;
        }

        public void LoadAsync(Type t, Action<Object> completed)
        {
            if (_assetHandler == null)
            {
                _assetHandler = Asset.LoadAsync(_path, t,
                    completed: (asset) => { InstantiateAsync(completed); });
            }
            else
            {
                if (_assetHandler.isDone)
                {
                    InstantiateAsync(completed);
                }
                else
                {
                    _assetHandler.completed += (asset) => { InstantiateAsync(completed); };
                }
            }
            LoadMin();
        }

        private void LoadMin()
        {
            if (_loadMin)
            {
                return;
            }
            _loadMin = true;
            if (_resConfig.Min > 0)
            {
                var start = _useObjects.Count + _unUseObjects.Count + _completedQueue.Count;
                if (start < _resConfig.Min)
                {
                    var isDone = _assetHandler.isDone;
                    if (isDone)
                    {
                        
                        for (int i = start; i < _resConfig.Min; i++)
                        {
                            InstantiateAsync(Retain);
                        }
                    }
                    else
                    {
                        _assetHandler.completed += (asset) =>
                        {
                            for (int i = _useObjects.Count + _unUseObjects.Count + _completedQueue.Count; i < _resConfig.Min; i++)
                            {
                                InstantiateAsync(Retain);
                            }
                        };
                    }
                }
            }
        }

        public void Retain(Object obj)
        {
            if (obj == null)
            {
                return;
            }
            _useObjects.Remove(obj);
            _unUseObjects.Push(obj);
            (obj as GameObject)?.SetActive(false);
        }
        public void Release()
        {
            OnRelease();
        }
        
        /// <summary>
        /// 返回是否busy
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            // 判断是否可以释放
            if (_owners.Count > 0)
            {
                switch (_resConfig.OwnerLevel)
                {
                    case OwnerLevel.Scene:
                    case OwnerLevel.System:
                        for (int i = _owners.Count - 1; i > 0; i--)
                        {
                            if (!_owners[i])
                            {
                                _owners.RemoveAt(i);
                            }
                        }

                        if (_owners.Count == 0)
                        {
                            OnRelease();
                            return false;
                        }

                        break;
                    default:
                        break;
                    ////
                }
            }
            
            //分帧实例化
            while (_completedQueue.Count > 0)
            {
                if (Updater.Instance.busy)
                {
                    return true;
                }

                var action = _completedQueue.Dequeue();
                action(Instantiate());
            }
            
            //逐步开始释放
            if (_unUseObjects.Count > 0)
            {
                
            }
            return false;
        }

        public bool IsInvalid()
        {
            return _assetHandler == null;
        }
        private Object Instantiate()
        {
            var obj = GetUnUseObj();
            if (null == obj)
            {
                if (_resConfig.Max == 0 || _useObjects.Count == 0 || _resConfig.Max >= _useObjects.Count + _unUseObjects.Count)
                {
                    obj = InstantiateObj();
                }
                else
                {
                    _useIndex++;
                    if (_useIndex >= _useObjects.Count)
                    {
                        _useIndex = 0;
                    }
                    obj = _useObjects[_useIndex];
                    if (obj == null)
                    {
                        _useObjects.RemoveAt(_useIndex);
                        obj = InstantiateObj();
                    }
                }
            }

            return obj;
        }

        private Object InstantiateObj()
        {
            var obj = Object.Instantiate(_assetHandler.asset);
                    
            _useObjects.Add(obj);
            _useIndex = -1;
            return obj;
        }
        private void InstantiateAsync(Action<Object> completed)
        {
            var obj = GetUnUseObj();
            if (null != obj)
            {
                completed(obj);
            }

            if (_completedQueue.Count > 0 || Updater.Instance.busy)
            {
                _completedQueue.Enqueue(completed);
            }
            else
            {
                obj = Instantiate();
                completed(obj);
            }
        }

        private Object GetUnUseObj()
        {
            while (_unUseObjects.Count > 0)
            {
                var obj = _unUseObjects.Pop();
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        private void OnRelease()
        {
            _assetHandler?.Release();
            _assetHandler = null;
            foreach (var obj in _unUseObjects)
            {
                if (obj)
                {
                    Object.Destroy(obj);
                }
            }

            foreach (var obj in _useObjects)
            {
                if (obj)
                {
                    Object.Destroy(obj);
                }
            }
            _useObjects.Clear();
            _unUseObjects.Clear();
            _completedQueue.Clear();
            _loadMin = false;
        }
    }
}