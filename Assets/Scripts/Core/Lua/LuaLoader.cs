using System;
using System.Collections.Generic;
using LuaInterface;
using System.IO;
using UnityEngine;
using VEngine;

namespace SuperMobs.Lua
{
    public class LuaLoader : LuaFileUtils
    {
        private string[] luaAssetStart =
            {"Assets/PublicAssets/LuaOutput/Lua/", "Assets/PublicAssets/LuaOutput/Lib/ToLua/Lua/"};

        const string OutputEx = ".bytes";
        public static Func<LuaFileUtils> LoadLuaFileUtils { get; set; }

        public static void RegLuaFileLoader()
        {
            LuaFileUtils luaFileUtils = LoadLuaFileUtils?.Invoke();
            if (luaFileUtils == null)
            {
                var loader = new LuaLoader();
                // luaFileUtils = loader;
            }
        }


        public override byte[] ReadFile(string fileName)
        {
            fileName = fileName.Replace(".lua", "").Replace(".", "/") + OutputEx;
            foreach (var start in luaAssetStart)
            {
                var path = start + fileName;
                var asset = Asset.Load(path, typeof(TextAsset), true);
                if (asset != null)
                {
                    return (asset.asset as TextAsset)?.bytes;   
                }
            }

            return null;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}