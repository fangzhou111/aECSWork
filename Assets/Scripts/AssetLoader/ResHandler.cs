// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:49
//     文件名称： ResHandler.cs
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
    interface IHandler
    {
        Object LoadAsset(Type t);
        void LoadAsync(Type t, Action<Object> completed);
        void Release();
        bool Update();
        bool IsInvalid();
        void Retain(Object o);
    }
    public class ResHandler:IHandler
    {
        private readonly string _path;
        private readonly ResConfig _resConfig;
        private Asset _assetHandler;
        private readonly List<GameObject> _owners = new List<GameObject>();
        private int _referenceCount = 0;
        public ResHandler(string path, string ownerLabel)
        {
            _path = path;
            _resConfig = ResManager.instance.GetResConfigs(path);

            var owner = ResManager.instance.GetOwnerObj(_resConfig, ownerLabel);
            if (!_owners.Contains(owner))
            {
                _owners.Add(owner);
            }
        }

        public Object LoadAsset(Type t)
        {
            if (_assetHandler == null || !_assetHandler.isDone)
            {
                _assetHandler = Asset.Load(_path,t);
            }

            _referenceCount++;
            return _assetHandler.asset;
        }

        public void LoadAsync(Type t, Action<Object> completed) 
        {
            _referenceCount++;
            if (_assetHandler == null)
            {
                _assetHandler = Asset.LoadAsync(_path, t, completed: (asset) => { completed(asset.asset); });
            }
            else
            {
                if (_assetHandler.isDone)
                {
                    completed(_assetHandler.asset);
                }
                else
                {
                    _assetHandler.completed += (asset) => { completed(_assetHandler.asset); };
                }
            }
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
            if (_owners.Count > 0)
            {
                switch (_resConfig.OwnerLevel)
                {
                    case OwnerLevel.Scene:
                    case OwnerLevel.System:
                        for (int i = _owners.Count - 1; i > 0; i--)
                        {
                            if (_owners[i] == null)
                            {
                                _owners.RemoveAt(i);
                            }
                        }
                        if (_owners.Count == 0 || _referenceCount <= 0)
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
          
            return false;
        }

        public bool IsInvalid()
        {
            return _assetHandler == null;
        }

        private void OnRelease()
        {
            _assetHandler?.Release();
            _assetHandler = null;
            _referenceCount = 0;
        }

        public void Retain(Object o)
        {
            _referenceCount--;
        }
    }
}