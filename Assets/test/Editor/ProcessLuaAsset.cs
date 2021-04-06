using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline;
using System;

public class ProcessLuaAsset
{
    [MenuItem("SuperMobs/AssetManager/Processor/GenerateLua")]
    public static void GenerateLua()
    {
        CleanDirectory(Application.dataPath + "/GenerateLua");

        CopyDirectory(Application.dataPath + "/ToLua/Lua", Application.dataPath + "/GenerateLua/ToLua/Lua");

        GenerateLuaPath(Application.dataPath + "/GenerateLua/ToLua/Lua");
    }

    //删除目标文件夹下面所有文件
    private static void CleanDirectory(string dir)
    {
        foreach (string subdir in Directory.GetDirectories(dir))
        {
            Directory.Delete(subdir, true);
        }

        foreach (string subFile in Directory.GetFiles(dir))
        {
            File.Delete(subFile);
        }
    }

    private static void CopyDirectory(string srcDir, string tgtDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo target = new DirectoryInfo(tgtDir);

        if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new Exception("父目录不能拷贝到子目录！");
        }

        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            target.Create();
        }

        FileInfo[] files = source.GetFiles();
        DirectoryInfo[] dirs = source.GetDirectories();

        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
        }
        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
        }
    }

    private static void GenerateLuaPath(string path)
    {
        string[] dirpaths = Directory.GetDirectories(path);

        foreach (var dirpath in dirpaths)
        {
            GenerateLuaPath(dirpath);
        }

        string[] filepaths = Directory.GetFiles(path);

        foreach (var filepath in filepaths)
        {
            if (filepath.EndsWith(".lua"))
            {
                string targetpath = filepath.Replace(Application.dataPath, "Assets");

                GenerateLua(targetpath);
            }
        }
    }

    private static void GenerateLua(string path)
    {
        string str = File.ReadAllText(path);

        string newpath = path.Replace(".lua", ".byte");

        if (!File.Exists(newpath))
            File.Create(newpath);

        File.WriteAllText(newpath, str);

        File.Delete(path);

    }
}
