#if UNITY_EDITOR
using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using System;
using Entitas.Unity.VisualDebugging;

namespace SuperMobs.Lua
{
    public class LuaSystems : MonoBehaviour
    {
        public static void RegToLua(LuaState state)
        {
            state.LuaPushFunction(L =>
            {
                LuaState l = LuaState.Get(L);
                profileTable = l.GetTable("ecs.debugsystems.profile");
                step = l.GetFunction("ecs.bridge.step");
                reset = l.GetFunction("ecs.bridge.reset");
                getSystemsInitializeChild = l.GetFunction("ecs.bridge.getsystemsinitializechildren");
                getSystemsExecuteChild = l.GetFunction("ecs.bridge.getsystemsexecutechildren");
                isreactive = l.GetFunction("ecs.bridge.isreactive");
                return 0;
            });
            state.LuaSetGlobal("ECS_INIT_EDITOR_SYSTEMS");

            state.LuaPushFunction(L =>
            {
                LuaSystems luaSystems = new GameObject().AddComponent<LuaSystems>();
                DontDestroyOnLoad(luaSystems);
                luaSystems.systemsName = ToLua.CheckString(L, 1);
                luaSystemsObjs.Add(luaSystems.systemsName, luaSystems);

                return 0;
            });
            state.LuaSetGlobal("ECS_DEBUGSYSTEMS_ONCREATE");

            state.LuaPushFunction(L =>
            {
                string systemsName = ToLua.CheckString(L, 1);
                string parentName = ToLua.CheckString(L, 2);
                if (!luaSystemsObjs.ContainsKey(parentName) || !luaSystemsObjs.ContainsKey(systemsName))
                {
                    Debug.LogError("systems do not exist when DEBUGSYSTEMS_ONPARENT, parent = " + parentName + ", child = " + systemsName);
                }
                else
                {
                    luaSystemsObjs[systemsName].transform.parent = luaSystemsObjs[parentName].transform;
                }

                return 0;
            });
            state.LuaSetGlobal("ECS_DEBUGSYSTEMS_ONPARENT");
        }

        private static LuaTable profileTable = null;
        private static LuaFunction step = null;
        private static LuaFunction reset = null;
        private static LuaFunction getSystemsInitializeChild = null;
        private static LuaFunction getSystemsExecuteChild = null;
        private static LuaFunction isreactive = null;
        private static Dictionary<string, LuaSystems> luaSystemsObjs = new Dictionary<string, LuaSystems>();

        public static bool IsReactive(string name)
        {
            isreactive.BeginPCall();
            isreactive.Push(name);
            isreactive.PCall();
            bool ret = isreactive.CheckBoolean();
            isreactive.EndPCall();
            return ret;
        }

        public static void Step(string systemsName)
        {
            if (luaSystemsObjs[systemsName].stepState == StepState.Disable)
                luaSystemsObjs[systemsName].stepState = StepState.Enable;
        }

        public static bool IsSystems(string name)
        {
            return GetProfile(name)["initializesystemcount"] != null;
        }

        public static float GetAverageCost(string name)
        {
            LuaTable tab = GetProfile(name);
            int count = Convert.ToInt32(tab["executecount"]);
            return count > 0 ? Convert.ToSingle(tab["executecosttotal"]) / count : 0;
        }

        public static void Reset(string systemsName)
        {
            reset.BeginPCall();
            reset.Push(systemsName);
            reset.PCall();
            reset.EndPCall();
        }

        public static string[] GetInitializeChildNameList(string systemsName)
        {
            getSystemsInitializeChild.BeginPCall();
            getSystemsInitializeChild.Push(systemsName);
            getSystemsInitializeChild.PCall();
            object[] ret = getSystemsInitializeChild.CheckLuaTable().ToArray();
            getSystemsInitializeChild.EndPCall();

            string[] list = new string[ret.Length];
            for (int i = 0; i < ret.Length; i++)
                list[i] = (ret[i] as LuaTable)["name"].ToString();
            return list;

        }

        public static string[] GetExecuteChildNameList(string systemsName)
        {
            getSystemsExecuteChild.BeginPCall();
            getSystemsExecuteChild.Push(systemsName);
            getSystemsExecuteChild.PCall();
            object[] ret = getSystemsExecuteChild.CheckLuaTable().ToArray();
            getSystemsExecuteChild.EndPCall();

            string[] list = new string[ret.Length];
            for (int i = 0; i < ret.Length; i++)
                list[i] = (ret[i] as LuaTable)["name"].ToString();
            return list;
        }

        public static LuaTable GetProfile(string name) { return profileTable[name] as LuaTable; }


        public enum StepState { Disable, Enable, Over }

        public string systemsName;
        public StepState stepState;
        public AvgResetInterval avgResetInterval = AvgResetInterval.Never;
        private LuaTable profile;

        void Start()
        {
            profile = GetProfile(systemsName);
        }

        void Update()
        {
            if (stepState == StepState.Enable)
            {
                stepState = StepState.Over;
                step.BeginPCall();
                step.Push(systemsName);
                step.PCall();
                step.EndPCall();
            }

            name = string.Format("{0} ({1} init, {2} exe, {3:0.###} ms)",
                systemsName, profile["initializesystemcount"], profile["executesystemcount"], Convert.ToSingle(profile["executecostnow"]) * 1000);

            if (Time.frameCount % (int)avgResetInterval == 0 && (bool)profile["enable"])
            {
                Reset(systemsName);
            }
        }

    }
}
#endif