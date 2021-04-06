using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline;

public class ProcessTestAsset
{
    [MenuItem("SuperMobs/AssetManager/Processor/ProcessTestPrefab")]
    public static void ProcessTest()
    {
        ProcessAllTestPrefab(Application.dataPath + "/test/");
    }

    private static void ProcessAllTestPrefab(string path)
    {
        string[] dirpaths = Directory.GetDirectories(path);

        foreach (var dirpath in dirpaths)
        {
            ProcessAllTestPrefab(dirpath);
        }

        string[] filepaths = Directory.GetFiles(path);

        foreach (var filepath in filepaths)
        {
            if (filepath.EndsWith(".prefab"))
            {
                string targetpath = filepath.Replace(Application.dataPath, "Assets");

                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(targetpath);

                if (go != null)
                {
                    RealProcessTestAsset(targetpath);

                    string[] deppaths = AssetDatabase.GetDependencies(targetpath);

                    foreach (var deppath in deppaths)
                    {
                        if (deppath.EndsWith(".cs") == false)
                        {
                            targetpath = deppath.Replace(Application.dataPath, "Assets");

                            RealProcessTestAsset(targetpath);
                        }
                    }

                }
            }
        }
    }

    private static void RealProcessTestAsset(string path)
    {
        AddressableAssetSettings setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
        AddressableAssetGroup group = setting.FindGroup("Test");

        if (group == null)
            Debug.LogError("No Test Group");

        string guid = AssetDatabase.AssetPathToGUID(path);

        if (group.GetAssetEntry(guid) == null && setting.FindAssetEntry(guid) == null)
        {
            AddressableAssetEntry entry = setting.CreateOrMoveEntry(guid, group);
            //entry.SetAddress(entry.address.Replace('/', '_'));
        }
        else if (group.GetAssetEntry(guid) == null && setting.FindAssetEntry(guid) != null)
        {
            foreach (var g in setting.groups)
            {
                if (g.GetAssetEntry(guid) != null)
                    Debug.LogWarning(path + " in other group,groupname: " + g.Name);
            }
        }
    }
}
