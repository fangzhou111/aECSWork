// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月19日 9:43
//     文件名称： ResMngEditor.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SuperMobs.Game.AssetLoader
{
    [CustomEditor(typeof(ResManager))]
    public class ResMngEditor : Editor
    {
        bool showBuffer = false;
        bool showSimple = false;
        bool showAsset = false;
        bool showPreload = false;

        string search = "";

        public override void OnInspectorGUI()
        {
            GUI.color = Color.white;
            search = EditorGUILayout.TextField("模糊搜索（资源路径）:", search);

            showAsset = EditorGUILayout.BeginFoldoutHeaderGroup(showAsset, "原始资源表");

            if (showAsset)
            {
                Dictionary<string, AssetResLoader> dic = GetField<Dictionary<string, AssetResLoader>>(ResManager.Instance, "_dicAssetResLoadersByPath");

                GUI.color = Color.red;
                EditorGUILayout.LabelField("目前的数量:" + dic.Count);
                GUI.color = Color.white;

                AssetResLoader[] loaders = dic.Values.Where(v =>
                {
                    string str = GetField<string>(v, "_path");

                    return str.Contains(search);
                }).ToArray();

                foreach (var loader in loaders)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(" >路径:" + GetField<string>(loader, "_path"));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField("    >数量:" + GetField<int>(loader, "_referenceCount").ToString());

                    VEngine.Asset ah = GetField<VEngine.Asset>(loader, "_assetHandler");

                    if (ah == null)
                    {
                        GUI.color = Color.gray;
                        EditorGUILayout.LabelField("    >资源句柄状态:已释放");
                        GUI.color = Color.white;
                    }
                    else if (!ah.isDone)
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("    >资源句柄状态:加载中");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("    >资源句柄状态:准备好");
                        GUI.color = Color.white;
                    }

                    List<GameObject> owners = GetField<List<GameObject>>(loader, "_owners");

                    string str = "";

                    foreach(GameObject o in owners)
                    {
                        if (o != null)
                            str += o.name + " "; 
                    }

                    GUI.color = Color.magenta;
                    EditorGUILayout.LabelField("    >持有者:" + str);
                    GUI.color = Color.white;
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showSimple = EditorGUILayout.BeginFoldoutHeaderGroup(showSimple, "非缓冲资源表");

            if (showSimple)
            {
                Dictionary<string, SimpleResLoader> dic = GetField<Dictionary<string, SimpleResLoader>>(ResManager.Instance, "_dicSimpleLoaders");

                GUI.color = Color.red;
                EditorGUILayout.LabelField("目前的数量:" + dic.Count);
                GUI.color = Color.white;

                SimpleResLoader[] loaders = dic.Values.Where(v =>
                {
                    string str = GetField<string>(v, "_path");

                    return str.Contains(search);
                }).ToArray();

                foreach (var loader in loaders)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(" >路径:" + GetField<string>(loader, "_path"));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField("    >上次更新时的时间戳:" + GetField<float>(loader, "_curUtc").ToString());

                    VEngine.Asset ah = GetField<VEngine.Asset>(loader, "_assetHandler");

                    if (ah == null)
                    {
                        GUI.color = Color.gray;
                        EditorGUILayout.LabelField("    >资源句柄状态:已释放");
                        GUI.color = Color.white;
                    }
                    else if (!ah.isDone)
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("    >资源句柄状态:加载中");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("    >资源句柄状态:准备好");
                        GUI.color = Color.white;
                    }

                    List<Res> usings = GetField<List<Res>>(loader, "_usingRes");

                    if (usings.Count > 0)
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("    >实例化个数:" + usings.Count);
                        GUI.color = Color.white;
                        for (int i = 0;i < usings.Count;i++)
                        {
                            EditorGUILayout.LabelField("        >第" + (i + 1) + "个");
                            EditorGUILayout.ObjectField("           >实例:", GetField<Object>(usings[i], "o"), typeof(Object), true);
                            EditorGUILayout.LabelField("           >剩余时间:" + GetField<float>(usings[i], "ratainTime"));
                        }
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showBuffer = EditorGUILayout.BeginFoldoutHeaderGroup(showBuffer, "缓冲资源表");

            if (showBuffer)
            {
                Dictionary<string, BufferResLoader> dic = GetField<Dictionary<string, BufferResLoader>>(ResManager.Instance, "_dicBufferResLoaders");

                GUI.color = Color.red;
                EditorGUILayout.LabelField("目前的数量:" + dic.Count);
                GUI.color = Color.white;

                BufferResLoader[] loaders = dic.Values.Where(v =>
                {
                    string str = GetField<string>(v, "_path");

                    return str.Contains(search);
                }).ToArray();

                foreach (var loader in loaders)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(" >路径:" + GetField<string>(loader, "_path"));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField("    >上次更新时的时间戳:" + GetField<float>(loader, "_curUtc").ToString());

                    VEngine.Asset ah = GetField<VEngine.Asset>(loader, "_assetHandler");

                    if (ah == null)
                    {
                        GUI.color = Color.gray;
                        EditorGUILayout.LabelField("    >资源句柄状态:已释放");
                        GUI.color = Color.white;
                    }
                    else if (!ah.isDone)
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("    >资源句柄状态:加载中");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("    >资源句柄状态:准备好");
                        GUI.color = Color.white;
                    }

                    EditorGUILayout.LabelField("    >资源类型:" + GetField<ResConfig>(loader, "_resConfig").OwnerLevel.ToString());

                    List<GameObject> owners = GetField<List<GameObject>>(loader, "_owners");

                    string str = "";

                    foreach (GameObject o in owners)
                    {
                        if (o != null)
                            str += o.name + " ";
                    }

                    GUI.color = Color.magenta;
                    EditorGUILayout.LabelField("    >持有者:" + str);
                    GUI.color = Color.white;

                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField("    >开始自动删除（Custom）:" + GetField<bool>(loader, "_startAutoDestroy"));
                    GUI.color = Color.white;

                    Res[] usings = GetField<Dictionary<int, Res>>(loader, "_usings").Values.ToArray();

                    if (usings.Length > 0)
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("    >实例化个数:" + usings.Length);
                        GUI.color = Color.white;
                        for (int i = 0;i < usings.Length;i++)
                        {
                            EditorGUILayout.LabelField("        >第" + (i + 1) + "个");
                            EditorGUILayout.ObjectField("           >实例:", GetField<Object>(usings[i], "o"), typeof(Object), true);
                            EditorGUILayout.LabelField("           >当前使用index:" + GetField<int>(usings[i], "useIndex"));
                            EditorGUILayout.LabelField("           >剩余时间:" + GetField<float>(usings[i], "ratainTime"));
                        }
                    }

                    Res[] frees = GetField<Stack<Res>>(loader, "_frees").ToArray();

                    if (frees.Length > 0)
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("    >空闲个数:" + frees.Length);
                        GUI.color = Color.white;

                        for (int i = 0; i < frees.Length; i++)
                        {
                            EditorGUILayout.LabelField("        >第" + (i + 1) + "个");
                            EditorGUILayout.ObjectField("           >实例:", GetField<Object>(frees[i], "o"), typeof(Object), true);
                        }
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showPreload = EditorGUILayout.BeginFoldoutHeaderGroup(showPreload, "预加载列表");

            if (showPreload)
            {
                BufferResLoader[] loaders = GetField<Queue<BufferResLoader>>(ResManager.Instance, "_preloaders").ToArray();
                EditorGUILayout.LabelField("预加载中的个数:" + loaders.Length);

                foreach(var loader in loaders)
                {
                    EditorGUILayout.LabelField(GetField<string>(loader, "_path"));
                }
            }


        }

        public static T GetField<T>(System.Object obj, string name)
        {
           var value = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(obj);
           return (T)value;
        }
    }

}