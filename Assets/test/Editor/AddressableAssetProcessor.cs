using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public class AddressableAssetProcessor : AssetPostprocessor
{ 
    //�˴����ڴ���AssetImport
    //���е���Դ�ĵ��룬ɾ�����ƶ���������ô˷�����ע�⣬���������static��
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        
    }
}
