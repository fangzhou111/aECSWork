﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class FairyGUI_EventDispatcherWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(FairyGUI.EventDispatcher), typeof(System.Object));
		L.RegFunction("AddEventListener", AddEventListener);
		L.RegFunction("RemoveEventListener", RemoveEventListener);
		L.RegFunction("AddCapture", AddCapture);
		L.RegFunction("RemoveCapture", RemoveCapture);
		L.RegFunction("RemoveEventListeners", RemoveEventListeners);
		L.RegFunction("hasEventListeners", hasEventListeners);
		L.RegFunction("isDispatching", isDispatching);
		L.RegFunction("DispatchEvent", DispatchEvent);
		L.RegFunction("BubbleEvent", BubbleEvent);
		L.RegFunction("BroadcastEvent", BroadcastEvent);
		L.RegFunction("New", _CreateFairyGUI_EventDispatcher);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateFairyGUI_EventDispatcher(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				FairyGUI.EventDispatcher obj = new FairyGUI.EventDispatcher();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: FairyGUI.EventDispatcher.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddEventListener(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<FairyGUI.EventCallback1>(L, 3))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				FairyGUI.EventCallback1 arg1 = (FairyGUI.EventCallback1)ToLua.ToObject(L, 3);
				obj.AddEventListener(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<FairyGUI.EventCallback0>(L, 3))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				FairyGUI.EventCallback0 arg1 = (FairyGUI.EventCallback0)ToLua.ToObject(L, 3);
				obj.AddEventListener(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.EventDispatcher.AddEventListener");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveEventListener(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<FairyGUI.EventCallback1>(L, 3))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				FairyGUI.EventCallback1 arg1 = (FairyGUI.EventCallback1)ToLua.ToObject(L, 3);
				obj.RemoveEventListener(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<FairyGUI.EventCallback0>(L, 3))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				FairyGUI.EventCallback0 arg1 = (FairyGUI.EventCallback0)ToLua.ToObject(L, 3);
				obj.RemoveEventListener(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.EventDispatcher.RemoveEventListener");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddCapture(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.EventCallback1 arg1 = (FairyGUI.EventCallback1)ToLua.CheckDelegate<FairyGUI.EventCallback1>(L, 3);
			obj.AddCapture(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveCapture(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			FairyGUI.EventCallback1 arg1 = (FairyGUI.EventCallback1)ToLua.CheckDelegate<FairyGUI.EventCallback1>(L, 3);
			obj.RemoveCapture(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveEventListeners(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				obj.RemoveEventListeners();
				return 0;
			}
			else if (count == 2)
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				obj.RemoveEventListeners(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.EventDispatcher.RemoveEventListeners");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int hasEventListeners(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			bool o = obj.hasEventListeners(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int isDispatching(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			bool o = obj.isDispatching(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DispatchEvent(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<string>(L, 2))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				bool o = obj.DispatchEvent(arg0);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<FairyGUI.EventContext>(L, 2))
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				FairyGUI.EventContext arg0 = (FairyGUI.EventContext)ToLua.ToObject(L, 2);
				bool o = obj.DispatchEvent(arg0);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else if (count == 3)
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				bool o = obj.DispatchEvent(arg0, arg1);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else if (count == 4)
			{
				FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				bool o = obj.DispatchEvent(arg0, arg1, arg2);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: FairyGUI.EventDispatcher.DispatchEvent");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int BubbleEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			object arg1 = ToLua.ToVarObject(L, 3);
			bool o = obj.BubbleEvent(arg0, arg1);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int BroadcastEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			FairyGUI.EventDispatcher obj = (FairyGUI.EventDispatcher)ToLua.CheckObject<FairyGUI.EventDispatcher>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			object arg1 = ToLua.ToVarObject(L, 3);
			bool o = obj.BroadcastEvent(arg0, arg1);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

