using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using LuaInterface;
using SuperMobs.Lua;
using System;


namespace SuperMobs.Game
{
    public static class GameController
    {
        public static bool bInit = false;
        public static LuaState lua = null;
        public static LuaFunction LuaUpdate = null;
        public static LuaTable LuaProfile = null;

        public static bool StartDestroyGame = false;

        private static bool _isDebugECS = true;

        public static void GameInit(bool b)
        {
            _isDebugECS = b;

            Application.targetFrameRate = 60;

            Application.runInBackground = true;

            InitLuaState("teststart");
        }

        public static void InitLuaState(string name)
        {
            CreateLuaState();

            if (_isDebugECS)
            {
#if UNITY_EDITOR
                LuaSystems.RegToLua(lua);
                LuaEntity.RegToLua(lua);
                LuaPool.RegToLua(lua);
#endif
            }

            lua.DoFile(name);
            LuaUpdate = lua.GetFunction("__UPDATE");

            lua.GetFunction("START").Call();

            bInit = true;
        }

        public static void DestroyGame()
        {
            if (lua != null)
                lua.GetFunction("ui.closeall").Call();

            DestroyLuaState();
        }

        public static void CreateLuaState()
        {
            lua = new LuaState();
            lua.Start();
   
            LuaBinder.Bind(lua);
            LuaHelper.Bind(lua);
            
            DelegateFactory.Init();
        }

        private static void DestroyLuaState()
        {
            LuaUpdate = null;
            lua.Dispose();
            lua = null;

            Resources.UnloadUnusedAssets();
            GC.Collect();

            bInit = false;
        }

        public static void GCLua()
        {
            if (lua != null)
            {
                lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
                lua.Collect();
            }
        }

        public static bool ReStartGame()
        {
#if UNITY_EDITOR
            if (_isDebugECS == false)
            {
#endif
                Debug.Log("ReStartGame");
                

                return true;
#if UNITY_EDITOR
            }
            else
            {
                return false;
            }
#endif
        }
    }
}
