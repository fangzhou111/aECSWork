using System;
using System.Collections.Generic;
using UnityEngine;
using VEngine;
using UniRx;
using Object = UnityEngine.Object;

namespace SuperMobs.Game.AssetLoader
{
    public class SimpleResLoader
    {
        private readonly string _path;//资源路径
        private Asset _assetHandler;//资源句柄
        private float _curUtc;//时间戳

        private List<Res> _usingRes;//在使用的资源（不需要返回缓冲池）

        private IDisposable _disposable;

        public SimpleResLoader(string path)
        {
            _path = path;
            _curUtc = Time.realtimeSinceStartup;

            _usingRes = new List<Res>();

            _disposable = Observable.Interval(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
            {
                Update();
            }).AddTo(ResManager.Instance);
        }

        private Object Get(float destroytime)
        {
            Object o = Object.Instantiate(_assetHandler.asset, Vector3.zero, Quaternion.identity, null);

            Res resGO = new Res(o, 0, destroytime);
            _usingRes.Add(resGO);

            return o;
        }

        public Object LoadAsset(Type t, float destroytime)
        {
            if (_assetHandler == null || !_assetHandler.isDone)
            {
                _assetHandler = Asset.Load(_path, t);
            }

            return Get(destroytime);
        }

        public void LoadAsync(Type t, Action<Object> completed, float destroytime)
        {
            if (_assetHandler == null)
            {
                _assetHandler = Asset.LoadAsync(_path, t, (asset) =>
                {
                    var obj = Get(destroytime);
                    completed?.Invoke(obj);
                    completed = null;
                });
            }
            else
            {
                if (_assetHandler.isDone)
                {
                    var obj = Get(destroytime);
                    completed?.Invoke(obj);
                    completed = null;
                }
                else
                {
                    _assetHandler.completed += (asset) =>
                    {
                        var obj = Get(destroytime);
                        completed?.Invoke(obj);
                        completed = null;
                    };
                }
            }
        }

        public void Update()
        {
            float dt = Time.realtimeSinceStartup - _curUtc;
            _curUtc = Time.realtimeSinceStartup;

            List<Res> deletes = new List<Res>();
            for (int i = 0; i < _usingRes.Count; i++)
            {
                _usingRes[i].Update(dt);

                if (_usingRes[i].o == null || _usingRes[i].ratainTime <= 0f)
                    deletes.Add(_usingRes[i]);
            }

            for (int i = 0; i < deletes.Count; i++)
            {
                deletes[i].Dispose();
                _usingRes.Remove(deletes[i]);
            }

            deletes.Clear();

            if (_usingRes.Count == 0 && _assetHandler != null)
            {
                _assetHandler.Release();
                _assetHandler = null;
            }
        }

        //强制释放接口
        public void Dispose()
        {
            int count = _usingRes.Count;
            for (int i = 0; i < count; i++)
            {
                _usingRes[0].Dispose();
                _usingRes.RemoveAt(0);
            }

            if (_assetHandler != null)
            {
                _assetHandler.Release();
                _assetHandler = null;
            }

            _usingRes.Clear();

            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }
    }
}