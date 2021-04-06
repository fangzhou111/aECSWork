using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.Initialization;
using System.IO;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    [CreateAssetMenu(fileName = "BuildScriptPackedCustom.asset", menuName = "Addressables/Content Builders/Custom Build Script")]
    public class BuildScriptCustomPackedMode : BuildScriptPackedMode
    {
        public override string Name
        {
            get
            {
                return "Custom Build Script";
            }
        }

        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            Debug.Log("Process Pre");

            TResult res = base.DoBuild<TResult>(builderInput, aaContext);

            Debug.Log("Process After");

            ContentCatalogData newCatalog = new ContentCatalogData(aaContext.locations, ResourceManagerRuntimeData.kCatalogAddress);

            string[] fs = Directory.GetFiles(aaContext.Settings.RemoteCatalogBuildPath.GetValue(aaContext.Settings));

            foreach (string f in fs)
            {
                if (f.EndsWith(".bundle"))
                {
                    string[] temp = f.Split('\\');


                    bool isDelete = true;
                    foreach (string internalid in newCatalog.InternalIds)
                    {
                        if (internalid.Contains(temp[temp.Length - 1]))
                            isDelete = false;
                    }

                    if (isDelete)
                    {
                        Debug.LogWarning(f + " NO use, DELETE!");
                        File.Delete(f);
                    }
                }
            }

            return res;
        }
    }
}
