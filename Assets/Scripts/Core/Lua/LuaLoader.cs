using LuaInterface;
using System.IO;
using UnityEngine;

namespace SuperMobs.Lua
{
    public class LuaLoader : LuaFileUtils
    {
        public static void RegLuaFileLoader() { new LuaLoader(); }

//#if UNITY_EDITOR
//        [UnityEditor.MenuItem("Lua/temp pack")]
//        static void TempPackLuaScript()
//        {
//            if (Directory.Exists("Assets/Scripts/Lua/Resources/"))
//                Directory.Delete("Assets/Scripts/Lua/Resources/", true);
//            Directory.CreateDirectory("Assets/Scripts/Lua/Resources/");

//            CopyScript(new DirectoryInfo("Assets/Lua/"), "");
//            CopyScript(new DirectoryInfo("Assets/Lib/ToLua/ToLua/Lua/"), "");

//            UnityEditor.AssetDatabase.Refresh();
//            Debug.Log("temp pack lua Done !");
//        }

//        static void CopyScript(DirectoryInfo di, string prefix)
//        {
//            foreach (FileInfo fi in di.GetFiles())
//                if (fi.Name.EndsWith(".lua"))
//                    File.WriteAllBytes("Assets/Scripts/Lua/Resources/" + prefix + fi.Name + ".bytes", File.ReadAllBytes(fi.FullName));
//            foreach (DirectoryInfo cdi in di.GetDirectories())
//                CopyScript(cdi, prefix + cdi.Name + ".");
//        }
//#endif


        public override byte[] ReadFile(string fileName)
        {
            byte[] data;
            Debug.Log("load lua file " + fileName);

            //临时加载代码
            string temp = fileName.Replace(".lua", "");
            string assetPath = "Assets/Lua/" + temp.Replace(".", "/") + ".lua";

            if (!File.Exists(assetPath))
            {
                assetPath = "Assets/Lib/ToLua/Lua/" + temp.Replace(".", "/") + ".lua";
            }

            var asset = File.ReadAllBytes(assetPath);
            if (asset == null)
            {
                Debug.LogError("没有这个文件");
            }

            data = asset;
            return data;
        }
    }
}