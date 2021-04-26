using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<Object> lst = new List<Object>();

    private void Add(Object o)
    {
        if (!lst.Contains(o))
            lst.Add(o);
    }
    private void OnGUI()
    {
        if (GUILayout.Button("load"))
        {
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(GameObject), "Assets/Arts/Prefabs/1.prefab", 2f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(GameObject), "Assets/Arts/Prefabs/2.prefab", 3f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(GameObject), "Assets/Arts/Prefabs/3.prefab", 4f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(GameObject), "Assets/Arts/Prefabs/4.prefab", 5f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(GameObject), "Assets/Arts/Prefabs/5.prefab"));

            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(Material), "Assets/Arts/Mat/1.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(Material), "Assets/Arts/Mat/2.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(Material), "Assets/Arts/Mat/3.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(Material), "Assets/Arts/Mat/4.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimple(typeof(Material), "Assets/Arts/Mat/5.mat"));

            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.Instantiate("Assets/Arts/Prefabs/1.prefab", 1f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.Instantiate("Assets/Arts/Prefabs/2.prefab", 2f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.Instantiate("Assets/Arts/Prefabs/3.prefab", 5f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.Instantiate("Assets/Arts/Prefabs/4.prefab", 10f));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.Instantiate("Assets/Arts/Prefabs/5.prefab"));

            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAsset(typeof(Material), "Assets/Arts/Mat/1.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAsset(typeof(Material), "Assets/Arts/Mat/2.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAsset(typeof(Material), "Assets/Arts/Mat/3.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAsset(typeof(Material), "Assets/Arts/Mat/4.mat"));
            //Add(SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAsset(typeof(Material), "Assets/Arts/Mat/5.mat"));

            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/1.prefab", (o) =>
            //{
            //    Add(o);
            //},1f);
            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/2.prefab", (o) =>
            //{
            //    Add(o);
            //},2f);
            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/3.prefab", (o) =>
            //{
            //    Add(o);
            //},3.5f);
            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/4.prefab", (o) =>
            //{
            //    Add(o);
            //},4f);
            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/5.prefab", (o) =>
            //{
            //    Add(o);
            //}, 7f);

            //SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateSimpleAsync(typeof(GameObject), "Assets/Arts/Prefabs/1.prefab", (o) =>
            // {
            //     Add(o);
            // },10f);

            //SuperMobs.Game.AssetLoader.ResManager.Instance.LoadAssetAsync(typeof(Material), "Assets/Arts/Mat/1.mat", (o) =>
            // {
            //     Add(o);
            // });

            //for (int i = 0; i < 100000; i++)
            //{
            //    SuperMobs.Game.AssetLoader.ResManager.Instance.InstantiateAsync("Assets/Arts/Prefabs/1.prefab", (o) =>
            //    {
            //        Add(o);
            //    }, 10f);
            //}


            SuperMobs.Game.AssetLoader.ResManager.Instance.Prelaod("Assets/Arts/Prefabs/1.prefab", true);
            SuperMobs.Game.AssetLoader.ResManager.Instance.Prelaod("Assets/Arts/Prefabs/2.prefab", false);
        }

        if (GUILayout.Button("retain"))
        {
            foreach (var v in lst)
            {
                SuperMobs.Game.AssetLoader.ResManager.Instance.Retain(v);
            }

            lst.Clear();
        }
    }
}
