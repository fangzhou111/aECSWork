using LuaInterface;
using System;
using System.Runtime.InteropServices;

namespace SuperMobs.Lua
{
    public static class LuaHelper
    {
        public static void Bind(LuaState state)
        {
            // lpeg
            state.OpenLibs(LuaDLL.luaopen_lpeg);
            // struct
            state.OpenLibs(LuaDLL.luaopen_struct);
            // cjson
            OpenCJson(state);
            // md5
            //state.OpenLibs(luaopen_md5_core);
            // sproto
            //state.OpenLibs(luaopen_sproto_core);
            // socket
            OpenLuaSocket(state);
        }

#if !UNITY_EDITOR && UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
        const string LUADLL = "tolua";
#endif
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        static extern int luaopen_sproto_core(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_md5_core(IntPtr luaState);


        static void OpenCJson(LuaState luaState)
        {
            luaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            luaState.OpenLibs(LuaDLL.luaopen_cjson);
            luaState.LuaSetField(-2, "cjson");

            luaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            luaState.LuaSetField(-2, "cjson.safe");
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int LuaOpen_Socket_Core(IntPtr L) { return LuaDLL.luaopen_socket_core(L); }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int LuaOpen_Mime_Core(IntPtr L) { return LuaDLL.luaopen_mime_core(L); }

        static void OpenLuaSocket(LuaState luaState)
        {
            luaState.BeginPreLoad();
            luaState.RegFunction("socket.core", LuaOpen_Socket_Core);
            luaState.RegFunction("mime.core", LuaOpen_Mime_Core);
            luaState.EndPreLoad();
        }
    }
}