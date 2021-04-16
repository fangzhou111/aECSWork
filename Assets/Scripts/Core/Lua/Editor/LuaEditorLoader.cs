// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月13日 14:36
//     文件名称： LuaEditorLoader.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System;
using System.IO;
using Build.Editor;
using LuaInterface;
using UnityEngine;
using VEngine;
using VEngine.Editor;

namespace SuperMobs.Lua.Editor
{
    public class LuaEditorLoader:LuaFileUtils
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (Settings.GetDefaultSettings().scriptPlayMode == ScriptPlayMode.Simulation || !BuildSettings.GetBuildConfig().LuaBundleMode)
            {
                LuaLoader.LoadLuaFileUtils = () => new LuaEditorLoader();
            }
        }

        public override byte[] ReadFile(string fileName)
        {

            if (fileName.EndsWith(".lua"))
            {
                fileName = fileName.Replace(".lua", "").Replace(".", "/") + ".lua";
            }
            else
            {
                fileName = fileName.Replace(".", "/");
            }
            return base.ReadFile(fileName);
        }
    }
}