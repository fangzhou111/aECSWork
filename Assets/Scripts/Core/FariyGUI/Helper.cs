using System.Collections.Generic;
using SuperMobs.Game.AssetLoader;
using UnityEngine;

namespace FairyGUI
{
    public static class Helper
    {
        
        private const string PathPrefix = "Assets/Arts/UI/";
        private static Dictionary<System.Type, System.Func<string, string, Object>> _uiTypeResLoader = new Dictionary<System.Type, System.Func<string, string, Object>>();

        static Helper()
        {
            GameObject stageCamera = new GameObject("Stage Camera");
            Object.DontDestroyOnLoad(stageCamera);

            Camera camera = stageCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.nearClipPlane = -30;
            camera.farClipPlane = 30;
            camera.depth = 50f;

            stageCamera.AddComponent<StageCamera>();

            UIConfig.defaultFont = "BigYoungBoldGB2.0";
            GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

            // ui各种资源的加载 
            // _uiTypeResLoader[typeof(TextAsset)] = TestLoad;
            // _uiTypeResLoader[typeof(Texture2D)] = TestLoad;
            // _uiTypeResLoader[typeof(Texture)] = TestLoad;
            // _uiTypeResLoader[typeof(AudioClip)] = TestLoad;
            
        }
        private static void LoadObject(System.Type type,  string packageName, string extension , string ownerLabel, out Object obj)
        {
            var path = PathPrefix + packageName + extension;
            obj = ResManager.instance.LoadAsset(type, path, ownerLabel);
            if (obj == null)
            {
                Debug.LogError($" load ui error: {packageName} {type.Name} !!!!");
            }
        }
        
        
        //临时资源加载接口
        // public static Object TestLoad(string name, string pkgname)
        // {
        //     string assetPath = "Assets/test/UI/" + name;
        //     return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        // }

        // lua通过这里加载、释放uipackage资源
        public static void RmovePackage(string pkgName)
        {
            if (UIPackage.GetByName(pkgName) == null)
            {
                Debug.Log("UIHelper RmovePackage " + pkgName + ", but pkg do not exist in memory!");
            }
            else
            {
                UIPackage.RemovePackage(pkgName);
                ResManager.instance.ReleaseOwner(pkgName);
            }
        }
        public static void AddPackage(string pkgName)
        {
            if (UIPackage.GetByName(pkgName) != null)
            {
                return;
                //Debug.LogError("UIHelper AddPackage " + pkgName + ", but pkg has exist in memory!");
            }

            UIPackage.AddPackage(pkgName, (string name, string extension, System.Type type, out DestroyMethod destroyMethod) =>
            {
                destroyMethod = DestroyMethod.None;
                LoadObject(type, name, extension, pkgName, out var obj);
                return obj;
            });
        }

        class ListenDestroyBehaviour : MonoBehaviour
        {
            public System.Action callBack = null;
            void OnApplicationQuit() { callBack = null; }
            void OnDestroy()
            {
                if (callBack != null && SuperMobs.Game.GameController.lua != null)
                {
                    callBack();

                    callBack = null;
                }
            }
        }

        // 提供的lua的接口，侦听界面销毁
        public static void ListenDestroy(GComponent com, System.Action callBack)
        {
            com.displayObject.gameObject.AddComponent<ListenDestroyBehaviour>().callBack = callBack;
        }
    }
}
