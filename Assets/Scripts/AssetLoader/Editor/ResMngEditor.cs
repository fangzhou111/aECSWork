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

        private SearchField searchField ;
        private string _search;
        public bool showGoHandlers;
        public bool showResHandler;
        public override void OnInspectorGUI()
        {
            if (searchField == null)
            {
                searchField = new SearchField();
            }
            EditorGUILayout.BeginHorizontal();
            _search = searchField.OnGUI(_search); 
            EditorGUILayout.EndHorizontal();  
            showGoHandlers = EditorGUILayout.BeginFoldoutHeaderGroup(showGoHandlers, "GoHandlers");
            if (showGoHandlers)
            {
                var goHandlers = GetField<Dictionary<string, GoHandler>>(ResManager.Instance, "_goHandlers");
                foreach (var item in goHandlers)
                {
                    if (!string.IsNullOrEmpty(_search))
                    {
                        if (!item.Key.Contains(_search))
                        {
                            continue;
                        }
                    }
                    DrawGoHandler(item.Key, item.Value);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            showResHandler = EditorGUILayout.BeginFoldoutHeaderGroup(showResHandler, "ResHandler");
            if (showResHandler)
            {
                var resHandlers = GetField<Dictionary<string, ResHandler>>(ResManager.Instance, "_resHandlers");
                foreach (var item in resHandlers)
                {
                    if (!string.IsNullOrEmpty(_search))
                    {
                        if (!item.Key.Contains(_search))
                        {
                            continue;
                        }
                    }
                    DrawResHandler(item.Key, item.Value);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawResHandler(string path, ResHandler handler)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(path);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var asset = GetField<VEngine.Asset>(handler, "_assetHandler");
            GUILayout.Label(asset.status.ToString());
            if (asset.asset != null)
            {
                EditorGUILayout.ObjectField(asset.asset ,typeof(Object),true);
            }
            EditorGUILayout.EndHorizontal();
                    
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Owners");
            EditorGUILayout.EndHorizontal();                    
            var owners = GetField< List<GameObject> >(handler, "_owners");
            foreach (var obj in owners)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj ,typeof(GameObject),true);
                EditorGUILayout.EndHorizontal();
            }
                    
            EditorGUILayout.BeginHorizontal();
            var count = GetField< int >(handler, "_referenceCount");
            GUILayout.Label("ReferenceCount : " + count);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGoHandler(string path, GoHandler handler)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(path);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var asset = GetField<VEngine.Asset>(handler, "_assetHandler");
            GUILayout.Label(asset.status.ToString());
            if (asset.asset != null)
            {
                EditorGUILayout.ObjectField(asset.asset ,typeof(Object),true);
            }
            EditorGUILayout.EndHorizontal();
                    
                    
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("UseObjects");
            EditorGUILayout.EndHorizontal();
            var useObjects = GetField<List<Object> >(handler, "_useObjects");
            foreach (var obj in useObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj ,typeof(Object),true);
                EditorGUILayout.EndHorizontal();
            }
            
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("UnUseObjects");
            EditorGUILayout.EndHorizontal();
            var unUseObjects = GetField<List<Object> >(handler, "_unUseObjects");
            foreach (var obj in unUseObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj ,typeof(Object),true);
                EditorGUILayout.EndHorizontal();
            }
            
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Owners");
            EditorGUILayout.EndHorizontal();
            var owners = GetField< List<GameObject> >(handler, "_owners");
            foreach (var obj in owners)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj ,typeof(GameObject),true);
                EditorGUILayout.EndHorizontal();
            }
                    
            EditorGUILayout.BeginHorizontal();
            var count = GetField< int >(handler, "_useIndex");
            GUILayout.Label("useIndex : " + count);
            EditorGUILayout.EndHorizontal();
        }
        public static T GetField<T>(System.Object obj, string name)
        {
           var value = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(obj);
           return (T)value;
        }
    }

}