// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:51
//     文件名称： ResManager.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperMobs.Game.AssetLoader
{
    public enum OwnerLevel
    {
        System,
        Scene,
        Custom,
    }
    public class ResManager : MonoBehaviour
    {
        private static object _lock = new object();
        private static ResManager _instance;

        public static ResManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("ResManager").AddComponent<ResManager>();
                        DontDestroyOnLoad(_instance);
                    }
                    return _instance;
                }
            }
        }

        private readonly Dictionary<string, ResConfig> _resConfigs = new Dictionary<string, ResConfig>();
        /// <summary>
        /// 可以实例化
        /// </summary>
        private readonly Dictionary<string, GoHandler> _goHandlers = new Dictionary<string, GoHandler>();
        /// <summary>
        /// 不可实例化的
        /// </summary>
        private readonly Dictionary<string, ResHandler> _resHandlers = new Dictionary<string, ResHandler>(); 
        private readonly Dictionary<int,ResHandler> _resIdHandlers = new Dictionary<int, ResHandler>(); //实例化的非GO列表
        
        private ResConfig _defaultResConfig = new ResConfig();

        private readonly Dictionary<string, GameObject> _systemOwners = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _sceneOwners = new Dictionary<string, GameObject>();

        private GameObject _systemDefaultOwner;
        private GameObject _sceneDefaultOwner;

        private readonly List<string> _invalidSysOwners = new List<string>();
        private readonly List<string> _invalidSceneOwners = new List<string>();
        private readonly List<string> _invalidResHandlers = new List<string>();

        private GameObject systemDefaultOwner
        {
            get
            {
                if (_systemDefaultOwner == null)
                {
                    _systemDefaultOwner = new GameObject("Owner_Default_System");
                    DontDestroyOnLoad(_systemDefaultOwner);
                }

                return _systemDefaultOwner;
            }
        }

        private GameObject sceneDefaultOwner
        {
            get
            {
                if (_sceneDefaultOwner == null)
                {
                    _sceneDefaultOwner = new GameObject("Owner_Default_Scene");
                }

                return _sceneDefaultOwner;
            }
        }

        internal ResConfig GetResConfigs(string path)
        {
            if (!_resConfigs.TryGetValue(path, out var config))
            {
                config = _defaultResConfig;
            }

            return config;
        }

        internal GameObject GetOwnerObj(ResConfig config, string ownerLabel)
        {
            if (config.OwnerLevel == OwnerLevel.Scene)
            {
                if (string.IsNullOrEmpty(ownerLabel))
                {
                    return sceneDefaultOwner;
                }

                if (!_systemOwners.TryGetValue(ownerLabel, out var obj))
                {
                    obj = new GameObject($"Owner_Scene_{ownerLabel}");
                    DontDestroyOnLoad(obj);
                    _systemOwners.Add(ownerLabel, obj);
                }

                return obj;
            }
            else if (config.OwnerLevel == OwnerLevel.System)
            {
                if (string.IsNullOrEmpty(ownerLabel))
                {
                    return systemDefaultOwner;
                }

                if (!_sceneOwners.TryGetValue(ownerLabel, out var obj))
                {
                    obj = new GameObject($"Owner_System_{ownerLabel}");
                    _sceneOwners.Add(ownerLabel, obj);
                }

                return obj;
            }

            return null;
        }

        #region 非GameObject
        /// <summary>
        /// 同步接口 使用需要申请
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ownerLabel"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Object LoadAsset(Type t, string path, string ownerLabel = "") 
        {
            if (!_resHandlers.TryGetValue(path, out var resHandler))
            {
                resHandler = new ResHandler(path, ownerLabel);
                _resHandlers.Add(path, resHandler);
                var ret = resHandler.LoadAsset(t);
                _resIdHandlers.Add(ret.GetInstanceID(), resHandler);

            }

            return resHandler.LoadAsset(t);
        }
        
        public void LoadAssetAsync(Type t, string path, Action<Object> completed)
        {
            LoadAssetAsync(t, path, completed, string.Empty);
        }

        public void LoadAssetAsync(Type t,string path, Action<Object> completed, string ownerLabel)
        {
            if (!_resHandlers.TryGetValue(path, out var resHandler))
            {
                resHandler = new ResHandler(path, ownerLabel);
                _resHandlers.Add(path, resHandler);
            }

            resHandler.LoadAsync(t, completed);
        }
        #endregion
        
        #region GameObject
        /// <summary>
        /// 同步接口 使用需要申请
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ownerLabel"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Object Instantiate(Type t, string path, string ownerLabel = "")
        {
            if (!_goHandlers.TryGetValue(path, out var resHandler))
            {
                resHandler = new GoHandler(path, ownerLabel);
                _goHandlers.Add(path, resHandler);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
            }

            return resHandler.LoadAsset(t);
        }

        public void InstantiateAsync(Type t,string path, Action<Object> completed)
        {
            InstantiateAsync(t, path, completed, string.Empty);
        }

        public void InstantiateAsync(Type t, string path,Action<Object> completed, string ownerLabel)
        {
            if (!_goHandlers.TryGetValue(path, out var resHandler))
            {
                resHandler = new GoHandler(path, ownerLabel);
                _goHandlers.Add(path, resHandler);
            }

            resHandler.LoadAsync(t ,completed);
        }
        #endregion
        
        public void Retain(Object obj)
        {
            if (obj is GameObject go)
            {
                var pathObj = go.transform.GetChild(go.transform.childCount - 1);
                var path = pathObj.name;
                if(_goHandlers.TryGetValue(path, out var goHandler))
                {
                    goHandler.Retain(obj);
                    return;
                }
                
            }
            else
            {
             
                var id = obj.GetInstanceID();
                if (_resIdHandlers.TryGetValue(id, out var resHandler))
                {
                    
                    resHandler.Retain(obj);
                    return;
                }
                
            }
            
            Debug.LogError("retain obj error");
        }

        public void ReleaseOwner(string ownerLabel)
        {
            
            if (string.IsNullOrEmpty(ownerLabel))
            {
                return;
            }
            GameObject obj = null;
            if (_systemDefaultOwner && _systemDefaultOwner.name == ownerLabel)
            {
                _systemDefaultOwner = null;
            } 
            else if (_sceneDefaultOwner && _sceneDefaultOwner.name == ownerLabel)
            {
                _sceneDefaultOwner = null;
                return;
            }
            else if (_systemOwners.TryGetValue(ownerLabel, out  obj))
            {
                _systemOwners.Remove(ownerLabel);
            } else if (_sceneOwners.TryGetValue(ownerLabel, out  obj))
            {
                _sceneOwners.Remove(ownerLabel);
            }

            if (obj)
            {
                Destroy(obj);
            }
            
        }

        private void OnDestroy()
        {
            foreach (var handler in _resHandlers.Values)
            {
                handler.Release();
            }

            foreach (var handlers in _goHandlers.Values)
            {
                handlers.Release();
            }

            if (_systemDefaultOwner != null)
            {
                Destroy(_systemDefaultOwner);
            }

            if (_sceneDefaultOwner != null)
            {
                Destroy(_sceneDefaultOwner);
            }
        }

        private void Update()
        {
            foreach (var item in _systemOwners)
            {
                if (item.Value == null)
                {
                    _invalidSysOwners.Add(item.Key);
                }
            }

            if (_invalidSysOwners.Count > 0)
            {
                foreach (var name in _invalidSysOwners)
                {
                    _systemOwners.Remove(name);
                }

                _invalidSysOwners.Clear();
            }

            foreach (var item in _sceneOwners)
            {
                if (item.Value == null)
                {
                    _invalidSceneOwners.Add(item.Key);
                }
            }

            if (_invalidSceneOwners.Count > 0)
            {
                foreach (var name in _invalidSceneOwners)
                {
                    _sceneOwners.Remove(name);
                }

                _invalidSceneOwners.Clear();
            }

            foreach (var item in _goHandlers)
            {
                bool ret = item.Value.Update();
                if (item.Value.IsInvalid())
                {
                    _invalidResHandlers.Add(item.Key);
                }

                if (ret)
                {
                    break;
                }
            }

            if (_invalidResHandlers.Count > 0)
            {
                foreach (var name in _invalidResHandlers)
                {
                    _goHandlers.Remove(name);
                }

                _invalidResHandlers.Clear();
                
            }
        }
    }
}