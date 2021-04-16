// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月12日 15:12
//     文件名称： BuildToLua.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Build.Editor
{
    public static class BuildConstants
    {
        public const string OutputPath = "Assets/PublicAssets/LuaOutput";
        public const string OutputEx = ".bytes";
        public const string BuildConfigPath = "Assets/Editor/Build/BuildSettings.asset";
    }
    public static class PackagerLua
    {
        [MenuItem("Build/PackagerLua")]
        public static void GenerateLua()
        {
            foreach (var dir in new string[]{LuaConst.luaDir, LuaConst.toluaDir})
            {
                var files = Directory.GetFiles(dir, "*.lua", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var path = file.Replace(UnityEngine.Application.dataPath, "");
                    path = BuildConstants.OutputPath + path;
                    path = Regex.Replace(path, "\\.lua$", BuildConstants.OutputEx);
                    string curDir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(curDir))
                    {
                        Directory.CreateDirectory(curDir);
                    }
                    File.Copy(file, path, true);
                }
            }
            AssetDatabase.Refresh();
        }
 
    }

    public static class EditorUtility
    {
        internal static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
    public class BuildSettings:ScriptableObject
    {
        public static BuildSettings GetBuildConfig()
        {
            return EditorUtility.GetOrCreateAsset<BuildSettings>(BuildConstants.BuildConfigPath);
        }

        public bool LuaBundleMode;
        
    }
}