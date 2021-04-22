// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:49
//     文件名称： AssetResLoader.cs
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
    public class AssetResLoader 
    {
        private readonly string _path;//资源路径
        private readonly ResConfig _resConfig;//资源配置
        private Asset _assetHandler;//资源句柄

        private readonly List<GameObject> _owners;//资源持有者
        private int _referenceCount = 0;//引用计数

        private IDisposable _disposable;

        public AssetResLoader(string path)
        {
            _path = path;
            _resConfig = ResManager.Instance.GetResConfigs(path);

            _owners = new List<GameObject>();

            _disposable = Observable.Interval(TimeSpan.FromSeconds(10f)).Subscribe(_ =>
            {
                Update();
            }).AddTo(ResManager.Instance);
        }

        public Object LoadAsset(Type t, string ownerLabel)
        {
            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);

            if (owner == null)
                Debug.LogError("原始资源禁止使用Custom加载模式!请修改，资源路径:" + _path);

            if (!_owners.Contains(owner))
                _owners.Add(owner);

            if (_assetHandler == null || !_assetHandler.isDone)
                _assetHandler = Asset.Load(_path,t);

            _referenceCount++;
            return _assetHandler.asset;
        }

        public void LoadAsync(Type t, string ownerLabel, Action<Object> completed) 
        {
            var owner = ResManager.Instance.GetOwnerObj(_resConfig, ownerLabel);

            if (owner == null)
                Debug.LogError("原始资源禁止使用Custom加载模式!请修改，资源路径:" + _path);

            if (!_owners.Contains(owner))
                _owners.Add(owner);

            _referenceCount++;
            if (_assetHandler == null)
            {
                _assetHandler = Asset.LoadAsync(_path, t, (asset) => 
                {
                    completed?.Invoke(asset.asset);
                    completed = null;
                });
            }
            else
            {
                if (_assetHandler.isDone)
                {
                    completed?.Invoke(_assetHandler.asset);
                    completed = null;
                }
                else
                {
                    _assetHandler.completed += (asset) => 
                    {
                        completed?.Invoke(asset.asset);
                        completed = null;
                    };
                }
            }
        }

        //归还
        public void Retain(Object o)
        {
            _referenceCount--;
        }

        public void Update()
        {
            if (_owners.Count > 0)
            {
                List<int> deletes = new List<int>();

                for (int i = 0; i < _owners.Count; i++)
                {
                    if (_owners[i] == null)
                        deletes.Add(i);
                }

                for (int i = 0; i < deletes.Count; i++)
                {
                    _owners.RemoveAt(deletes[i]);
                }

                deletes.Clear();
            }

            if (_owners.Count == 0 || _referenceCount <= 0)
                Clear();
        }

        public void Clear()
        {
            if (_assetHandler != null)
            {
                _assetHandler.Release();
                _assetHandler = null;
            }

            _referenceCount = 0;
        }

        //强制释放接口
        public void Dispose()
        {
            Clear();

            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }
    }
}