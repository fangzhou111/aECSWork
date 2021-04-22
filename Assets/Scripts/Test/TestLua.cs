using SuperMobs.Game;
using SuperMobs.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLua : MonoBehaviour
{
    [Header("¿ªÆôECS_DEBUG")]
    public bool ShowECSInfo = false;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        LuaLoader.RegLuaFileLoader();

        GameController.GameInit(ShowECSInfo);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.LuaUpdate != null)
        {
            GameController.LuaUpdate.Call();

        }
    }


    void OnGUI()
    {
        if (GUILayout.Button("GClua"))
        {
            GameController.GCLua();
        }

        if (GUILayout.Button("GC"))
        {
            System.GC.Collect();
        }

        if (GUILayout.Button("BBB"))
        {
            //GameController.lua.GetFunction("BBB").Call();
            
        }

        if (GUILayout.Button("AAA"))
        {
            //GameController.lua.GetFunction("AAA").Call();
        }

        if (GUILayout.Button("LUAGC"))
        {
            GameController.lua.GetFunction("LUAGC").Call();
        }
    }
}
