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


        //配置信息
        private ResConfig _defaultResConfig = new ResConfig();
        private readonly Dictionary<string, ResConfig> _resConfigs = new Dictionary<string, ResConfig>();

        //GO缓冲实例列表
        private readonly Dictionary<string, BufferResLoader> _dicBufferResLoaders = new Dictionary<string, BufferResLoader>();

        //原始资源列表
        private readonly Dictionary<string, AssetResLoader> _dicAssetResLoadersByPath = new Dictionary<string, AssetResLoader>();
        //原始资源归还索引，不要用来处理业务
        private readonly Dictionary<int,AssetResLoader> _dicAssetResLoadersById = new Dictionary<int, AssetResLoader>();


        //不缓冲的资源列表
        private readonly Dictionary<string, SimpleResLoader> _dicSimpleLoaders = new Dictionary<string, SimpleResLoader>();
        
        //持有者列表
        private readonly Dictionary<string, GameObject> _systemOwners = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _sceneOwners = new Dictionary<string, GameObject>();

        //预加载列表
        private readonly Queue<BufferResLoader> _preloaders = new Queue<BufferResLoader>();

        private GameObject systemDefaultOwner
        {
            get
            {
                if (!_systemOwners.TryGetValue("Default", out GameObject owner))
                {
                    owner = new GameObject("Owner_System_Default");
                    DontDestroyOnLoad(owner);

                    _systemOwners.Add("Default", owner);
                }
                else
                {
                    if (owner == null)
                    {
                        owner = new GameObject("Owner_System_Default");
                        DontDestroyOnLoad(owner);

                        _systemOwners["Default"] = owner;
                    }
                }

                return owner;
            }
        }

        private GameObject sceneDefaultOwner
        {
            get
            {
                if (!_sceneOwners.TryGetValue("Default", out GameObject owner))
                {
                    owner = new GameObject("Owner_Scene_Default");
                    _sceneOwners.Add("Default", owner);
                }
                else
                {
                    if (owner == null)
                    {
                        owner = new GameObject("Owner_Scene_Default");
                        _sceneOwners["Default"] = owner;
                    }
                }

                return owner;
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
                    _sceneOwners.Add(ownerLabel, obj);
                }
                else
                {
                    if (obj == null)
                    {
                        obj = new GameObject($"Owner_Scene_{ownerLabel}");
                        _sceneOwners[ownerLabel] = obj;
                    }
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
                    DontDestroyOnLoad(obj);
                    _systemOwners.Add(ownerLabel, obj);
                }
                else
                {
                    if (obj == null)
                    {
                        obj = new GameObject($"Owner_System_{ownerLabel}");
                        DontDestroyOnLoad(obj);

                        _systemOwners[ownerLabel] = obj;
                    }
                }

                return obj;
            }

            return null;
        }

        #region 申请原始资源（非GO）
        //同步接口 使用需要申请
        public Object LoadAsset(Type t, string path, string ownerLabel = "") 
        {
            if (t == typeof(GameObject))
            {
                Debug.LogError("禁止申请Prefab");
                return null;
            }

            if (!_dicAssetResLoadersByPath.TryGetValue(path, out var assetLoader))
            {
                assetLoader = new AssetResLoader(path);
                _dicAssetResLoadersByPath.Add(path, assetLoader);
                var ret = assetLoader.LoadAsset(t, ownerLabel);
                _dicAssetResLoadersById.Add(ret.GetInstanceID(), assetLoader);

                return ret;
            }
            else
                return assetLoader.LoadAsset(t, ownerLabel);
        }

        //异步加载原始资源，常规接口
        //如果使用的资源不会发生污染及不可逆的变化使用该接口获取
        public void LoadAssetAsync(Type t,string path, Action<Object> completed, string ownerLabel = "")
        {
            if (t == typeof(GameObject))
            {
                Debug.LogError("禁止申请Prefab");
                return;
            }

            if (!_dicAssetResLoadersByPath.TryGetValue(path, out var assetLoader))
            {
                assetLoader = new AssetResLoader(path);
                _dicAssetResLoadersByPath.Add(path, assetLoader);
            }

            assetLoader.LoadAsync(t, ownerLabel,(asset) =>
            {
                if (!_dicAssetResLoadersById.ContainsKey(asset.GetInstanceID()))
                    _dicAssetResLoadersById.Add(asset.GetInstanceID(), assetLoader);

                completed?.Invoke(asset);
                completed = null;
            });
        }
        #endregion

        #region 实例化参加缓冲的GO
        //同步获取 使用需要申请
        public Object Instantiate(string path, float retaintime = float.NaN, string ownerLabel = "")
        {
            if (!_dicBufferResLoaders.TryGetValue(path, out var bufferLoader))
            {
                bufferLoader = new BufferResLoader(path);
                _dicBufferResLoaders.Add(path, bufferLoader);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
            }

            return bufferLoader.LoadAsset(typeof(GameObject), retaintime, ownerLabel);
        }
        //同步获取 使用需要申请
        public Object Instantiate(string path, string ownerLabel)
        {
            return Instantiate(path, float.NaN, ownerLabel);
        }

        //异步获取缓冲GO 常规接口
        //如果使用的GO不会发生污染及不可逆的变化使用该接口获取GO（典型会被污染的情景为提供给UI使用GO，layer会被污染，且会被FGUI接管）
        public void InstantiateAsync(string path,Action<Object> completed, float retaintime = float.NaN, string ownerLabel = "")
        {
            if (!_dicBufferResLoaders.TryGetValue(path, out var bufferLoader))
            {
                bufferLoader = new BufferResLoader(path);
                _dicBufferResLoaders.Add(path, bufferLoader);
            }

            bufferLoader.LoadAsync(typeof(GameObject), completed, retaintime, ownerLabel);
        }

        public void InstantiateAsync(string path, Action<Object> completed, string ownerLabel)
        {
            InstantiateAsync(path, completed, float.NaN, ownerLabel);
        }

        //只有需要缓冲的GO类资源提供preload服务
        //建议在loading时批量使用，常态使用意义不明
        public void Prelaod(string path, bool needPreInstantiate, string ownerLabel = "")
        {
            if (!_dicBufferResLoaders.TryGetValue(path, out var bufferLoader))
            {
                bufferLoader = new BufferResLoader(path);
                _dicBufferResLoaders.Add(path, bufferLoader);
            }

            bufferLoader.Preload(typeof(GameObject), needPreInstantiate, ownerLabel);

            _preloaders.Enqueue(bufferLoader);
        }

        //检查manager所有需要preload的资源是否加载完毕
        public bool PreLoadDone()
        {
            return _preloaders.Count == 0;
        }
        #endregion

        #region 实例化非缓冲的资源
        //同步获取 使用需要申请
        public Object InstantiateSimple(Type t, string path,float destroytime = float.NaN)
        {
            if (_dicSimpleLoaders.TryGetValue(path, out SimpleResLoader loader))
            {
                return loader.LoadAsset(t, destroytime);
            }
            else
            {
                loader = new SimpleResLoader(path);
                _dicSimpleLoaders.Add(path, loader);

                return loader.LoadAsset(t, destroytime);
            }
        }

        //异步获取不被缓冲的资源 常规接口
        //如果使用的资源会发生污染及不可逆的变化使用该接口获取，主要提供给UI使用（典型会被污染的情景为提供给UI使用GO，layer会被污染，且会被FGUI接管）
        public void InstantiateSimpleAsync(Type t, string path, Action<Object> completed, float destroytime = float.NaN)
        {
            if (_dicSimpleLoaders.TryGetValue(path, out SimpleResLoader loader))
            {
                loader.LoadAsync(t, completed, destroytime);
            }
            else
            {
                loader = new SimpleResLoader(path);
                _dicSimpleLoaders.Add(path, loader);

                loader.LoadAsync(t, completed, destroytime);
            }
        }
        #endregion



        //归还接口
        //被缓冲的GO反复归还会引起异常，请注意避免
        public void Retain(Object obj)
        {
            if (obj == null)
                return;

            if (obj is GameObject go)
            {
                var pathObj = go.transform.GetChild(go.transform.childCount - 1);
                var path = pathObj.name;
                if(_dicBufferResLoaders.TryGetValue(path, out var bufferLoader))
                {
                    bufferLoader.Retain(obj);
                    return;
                }       
            }
            else
            {
                var id = obj.GetInstanceID();
                if (_dicAssetResLoadersById.TryGetValue(id, out var assetloader))
                {
                    assetloader.Retain(obj);
                    return;
                }
            }

            //非缓冲资源直接删除
            Destroy(obj);           
        }

        //按持有者类型卸载
        public void ReleaseOwner(string ownerLabel)
        {
            if (string.IsNullOrEmpty(ownerLabel))
            {
                return;
            }

            GameObject obj = null;

            if (_systemOwners.TryGetValue(ownerLabel, out obj))
            {
                _systemOwners.Remove(ownerLabel);
                GameObject.Destroy(obj);
            }

            if (_sceneOwners.TryGetValue(ownerLabel, out obj))
            {
                _sceneOwners.Remove(ownerLabel);
                GameObject.Destroy(obj);
            }        
        }

        //立即干翻全部已经不活跃的资源及实例
        public void ClearUnuseImmediate()
        {
            foreach(var kv in _dicSimpleLoaders)
            {
                kv.Value.Update();
            }

            foreach (var kv in _dicAssetResLoadersByPath)
            {
                kv.Value.Update();
            }

            foreach (var kv in _dicBufferResLoaders)
            {
                kv.Value.Update();
            }
        }

        //立即干翻全家(警告：此接口将卸载所有通过resmanager加载的一切)
        public void Clear()
        {
            GameObject.DestroyImmediate(gameObject);
        }

        private void OnDestroy()
        {
            foreach(var kv in _dicSimpleLoaders)
            {
                kv.Value.Dispose();
            }
            _dicSimpleLoaders.Clear();


            foreach(var kv in _dicAssetResLoadersByPath)
            {
                kv.Value.Dispose();
            }
            _dicAssetResLoadersByPath.Clear();
            _dicAssetResLoadersById.Clear();

            foreach(var kv in _dicBufferResLoaders)
            {
                kv.Value.Dispose();
            }
            _dicBufferResLoaders.Clear();
            _preloaders.Clear();

            foreach (var kv in _systemOwners)
            {
                GameObject.Destroy(kv.Value);
            }
            _systemOwners.Clear();

            foreach (var kv in _sceneOwners)
            {
                GameObject.Destroy(kv.Value);
            }
            _sceneOwners.Clear();
        }

        private void Update()
        {
            //维护preload队列
            while (_preloaders.Count > 0 && _preloaders.Peek().LoadDone())
            {
                _preloaders.Dequeue();
            }
        }
    }
}