using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class TestLoad : MonoBehaviour
{
    public AssetReference a;

    //AssetHandle<GameObject> ah1;
    //AssetHandle<GameObject> ah2;
    // Start is called before the first frame update
    void Start()
    {
        //不要用FileName打包，坑吐了兄弟
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CheckUpdate()
    {
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        var handler = Addressables.CheckForCatalogUpdates(false);
        yield return handler;
        var catalogs = handler.Result;

        if (catalogs.Count > 0)
        {
            Debug.Log("have new catalogs");

            var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            yield return updateHandle;

            var locators = updateHandle.Result;

            //暴力遍历所有的key
            long downloadSize = 0;

            foreach (var locator in locators)
            {
                var handle = Addressables.GetDownloadSizeAsync(locator.Keys);
                yield return handle;
                downloadSize += handle.Result;

                if (handle.Result > 0)
                {
                    yield return Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.Union, true);
                }

                Addressables.Release(handle);
            }

            Addressables.Release(updateHandle);

            Debug.Log("down load end:" + downloadSize);

            var reInitHandle = Addressables.InitializeAsync();
            yield return reInitHandle;

            Debug.Log("reinit done");

            DeleteNoUse(reInitHandle.Result as ResourceLocationMap);

            Addressables.Release(reInitHandle);
        }
        Addressables.Release(handler);
    }

    private void DeleteNoUse(ResourceLocationMap rlm)
    {
        string text = File.ReadAllText(Addressables.RuntimePath + "/settings.json");

        ResourceManagerRuntimeData runtimeData = JsonUtility.FromJson<ResourceManagerRuntimeData>(text);
        CacheInitializationData data = JsonUtility.FromJson<CacheInitializationData>(runtimeData.InitializationObjects[0].Data);

        string path = Addressables.ResolveInternalId(data.CacheDirectoryOverride);

        List<string> firstname = new List<string>();
        List<string> secondname = new List<string>();

        foreach (var location in rlm.Locations)
        {
            if (location.Key.ToString().EndsWith(".bundle"))
            {
                firstname.Add(((location.Value[0].Data) as AssetBundleRequestOptions).BundleName);
                secondname.Add(((location.Value[0].Data) as AssetBundleRequestOptions).Hash);
            }
        }

        string[] firstdirs = Directory.GetDirectories(path);

        foreach (string firstdir in firstdirs)
        {
            string[] temps = firstdir.Split('\\');

            if (firstname.Contains(temps[temps.Length - 1]) == false)
            {
                //Debug.Log(firstdir + "no use");

                DeleteDirectory(firstdir);
            }
            else
            {
                string[] seconddirs = Directory.GetDirectories(firstdir);

                foreach (string seconddir in seconddirs)
                {
                    string[] temp1s = seconddir.Split('\\');


                    if (secondname.Contains(temp1s[temp1s.Length - 1]) == false)
                    {
                        //Debug.Log(seconddir + "no use");

                        DeleteDirectory(seconddir);
                    }
                }
            }
        }

        Debug.Log("Delete No Use AB Done");
    }

    private bool DeleteDirectory(string strPath)
    {
        string[] strTemp;
        try
        {
            //先删除该目录下的文件
            strTemp = System.IO.Directory.GetFiles(strPath);
            foreach (string str in strTemp)
            {
                System.IO.File.Delete(str);
            }

            strTemp = System.IO.Directory.GetDirectories(strPath);
            foreach (string str in strTemp)
            {
                DeleteDirectory(str);
            }

            Directory.Delete(strPath);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private void OnGUI()
    {
        if(GUILayout.Button("log locations"))
        {
            ContentCatalogProvider ccp = Addressables.ResourceManager.ResourceProviders
                .FirstOrDefault(rp => rp.GetType() == typeof(ContentCatalogProvider)) as ContentCatalogProvider;
        }

        if (GUILayout.Button("check Update"))
        {
            StartCoroutine(CheckUpdate());
        }

        //if(GUILayout.Button("Load1"))
        //{
        //    if (ah1 == null)
        //        ah1 = new AssetHandle<GameObject>();

        //    ah1.AsyncLoad("a.prefab", prefab =>
        //    {
        //        GameObject go = GameObject.Instantiate(prefab);

        //        go.transform.parent = transform;
        //        go.transform.localPosition = Vector3.zero;
        //    });
        //}

        //if (GUILayout.Button("Unload1"))
        //{
        //    if (ah1 != null)
        //        ah1.Unload();
        //}

        //if (GUILayout.Button("Load2"))
        //{
        //    if (ah2 == null)
        //        ah2 = new AssetHandle<GameObject>();

        //    ah2.AsyncLoad("Assets_test_2.prefab", prefab =>
        //    {
        //        GameObject go = GameObject.Instantiate(prefab);

        //        go.transform.parent = transform;
        //        go.transform.localPosition = Vector3.zero;
        //    });
        //}

        //if (GUILayout.Button("Unload2"))
        //{
        //    if (ah2 != null)
        //        ah2.Unload();
        //}

        if (GUILayout.Button("Load ref"))
        {
            a.LoadAssetAsync<GameObject>().Completed += res => {
                GameObject go = GameObject.Instantiate(res.Result);

                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
            };
        }

        if (GUILayout.Button("Unload ref"))
        {
            a.ReleaseAsset();
        }

        if (GUILayout.Button("GC"))
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }
}
