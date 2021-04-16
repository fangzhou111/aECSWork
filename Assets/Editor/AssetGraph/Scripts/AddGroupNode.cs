// /********************************************************************
//                  
//                          All rights reserved
// 
//     创建日期： 2021年03月22日 16:00
//     文件名称： AddGroupNode.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：CQ
//     概    述：Summary
// 
// *********************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;

namespace VEngine.Editor.AssetGraph
{
    [CustomNode("Xasset/AddGroup", 1000)]
    public class AddGroupNode : Node
    {
        private readonly GUIContent manifestContent = new GUIContent("Manifest", "分组所在的清单");
        private readonly GUIContent bundleModeContent = new GUIContent("BundleMode", "分组的打包模式，决定了分组资源的打包粒度");
        private readonly GUIContent includeInBuildContent = new GUIContent("IncludeInBuild", "分组是否要包含在包体");
        private readonly GUIContent labelContent = new GUIContent("Label", "资源的 label，用来生成 bundle 的名字");

        [SerializeField] private int selectIndex;

        [SerializeField] private BundleMode bundleMode;
        [SerializeField] private bool includeInBuild;
        [SerializeField] private string groupName;
        [SerializeField] private string newManifestName;
        [SerializeField] private string label;
        [SerializeField] private Manifest manifest;
        [SerializeField] private Settings settings;
        
        private Settings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = Settings.GetDefaultSettings();
                }

                return settings;
            }
        }

        // private List<AssetReference> lastImportedAssets;

        private string[] _manifestNames;

        private string[] manifestNames
        {
            get
            {
                 
                if (_manifestNames == null)
                {
                    var settings = Settings;
                    selectIndex = 0;
                    var files = new List<string>() {"Create New..."};
                    for (int i = 0; i < settings.manifests.Count; i++)
                    {
                        var main = settings.manifests[i];
                        var path = AssetDatabase.GetAssetPath(main);
                        path = System.IO.Path.GetFileNameWithoutExtension(path);
                        files.Add(path);
                        if (manifest == main)
                        {
                            selectIndex = i + 1;
                        }
                    }

                    _manifestNames = files.ToArray();
                }

                return _manifestNames;
            }
            set
            {
                _manifestNames = value;
            }
        }

        public override string ActiveStyle
        {
            get { return "node 2 on"; }
        }

        public override string InactiveStyle
        {
            get { return "node 2"; }
        }

        public override string Category
        {
            get { return "Xasset"; }
        }

        public override Model.NodeOutputSemantics NodeInputType
        {
            get { return Model.NodeOutputSemantics.Assets; }
        }

        public override void Initialize(Model.NodeData data)
        {
            data.AddDefaultInputPoint();
            data.AddDefaultOutputPoint();
        }

        public override Node Clone(Model.NodeData newData)
        {
            var newNode = new AddGroupNode();

            newNode.bundleMode = bundleMode;
            newNode.includeInBuild = includeInBuild;
            newNode.groupName = groupName;
            newNode.label = label;
            newNode.manifest = manifest;
            newNode.selectIndex = selectIndex;


            newData.AddDefaultInputPoint();
            newData.AddDefaultOutputPoint();
            return newNode;
        }

        public override bool OnAssetsReimported(
            Model.NodeData nodeData,
            AssetReferenceStreamManager streamManager,
            BuildTarget target,
            AssetPostprocessorContext ctx,
            bool isBuilding)
        {
            // if (lastImportedAssets == null)
            // {
            //     lastImportedAssets = new List<AssetReference>();
            // }
            //
            // lastImportedAssets.Clear();
            // lastImportedAssets.AddRange(ctx.ImportedAssets);
            // lastImportedAssets.AddRange(ctx.MovedAssets);

            return true;
        }

        public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager,
            NodeGUIEditor editor, Action onValueChanged)
        {
           

            EditorGUILayout.HelpBox("Last Imported Items: Load assets just imported.", MessageType.Info);
            editor.UpdateNodeName(node);
            EditorGUI.BeginChangeCheck();
            settings = EditorGUILayout.ObjectField("Settings",settings, typeof(Settings), true) as Settings;
            var manifestNames = this.manifestNames;
            selectIndex = EditorGUILayout.Popup(manifestContent, selectIndex, manifestNames);
            if (selectIndex == 0)
            {
                newManifestName = EditorGUILayout.TextField("New Manifest", newManifestName);
            }

            bundleMode = (BundleMode) EditorGUILayout.EnumPopup(bundleModeContent, bundleMode);

            includeInBuild = EditorGUILayout.Toggle(includeInBuildContent, includeInBuild);
            groupName = EditorGUILayout.TextField("GroupName", groupName);
            label = EditorGUILayout.TextField(labelContent, label);
            if (EditorGUI.EndChangeCheck())
            {
                // m_component.Save ();
                if (selectIndex == 0 && !string.IsNullOrEmpty(newManifestName))
                {
                    for (int i = 1; i < manifestNames.Length; i++)
                    {
                        var name = manifestNames[i];
                        if (name == newManifestName)
                        {
                            newManifestName = string.Empty;
                            selectIndex = i;
                            break;
                        }
                    }
                }

                // foreach (var outputPoint in node.Data.OutputPoints)
                // {
                // 	NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_NODE_UPDATED, node, Vector2.zero, outputPoint));
                // }
                using (new RecordUndoScope("Change Filter Setting", node))
                {
                    onValueChanged();
                }
            }
        }

        public override void Prepare(BuildTarget target,
            Model.NodeData node,
            IEnumerable<PerformGraph.AssetGroups> incoming,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output)
        {
            Load(target, node, connectionsToOutput, Output);
        }

        public override void Build(BuildTarget target, Model.NodeData nodeData,
            IEnumerable<PerformGraph.AssetGroups> incoming, IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output outputFunc, Action<Model.NodeData, string, float> progressFunc)
        {
            base.Build(target, nodeData, incoming, connectionsToOutput, outputFunc, progressFunc);

            if (selectIndex == 0 && string.IsNullOrEmpty(newManifestName))
            {
                return;
            }

            if (string.IsNullOrEmpty(groupName))
            {
                return;
            }

            bool add = true;
            var settings = Settings;
            if (selectIndex != 0)
            {
                manifest = settings.manifests[selectIndex - 1];
            }
            else
            {
                var index = settings.AddManifest($"Assets/VEngine/Data/{newManifestName}.asset");
                manifest = settings.manifests[index];
                selectIndex = index + 1;
            }

            if (manifest == null)
            {
                return;
            }
            Group curGroup = null;
            foreach (var group in manifest.groups)
            {
                var path = AssetDatabase.GetAssetPath(group);
                path = System.IO.Path.GetFileNameWithoutExtension(path);
                if (path == groupName)
                {
                    add = false;
                    curGroup = group;
                    break;
                }
            }

            if (add)
            {
                curGroup = manifest.AddGroup(groupName, bundleMode == BundleMode.PackByRaw);
                manifestNames = null;
                // manifestNames = null;
            }

            curGroup.bundleMode = bundleMode;

            foreach (var incom in incoming)
            {
                foreach (var assetGroup in incom.assetGroups)
                {
                    foreach (var asset in assetGroup.Value)
                    {
                        manifest.AddAsset(asset.path, curGroup, label);
                    }
                }
            }
            OnOutput(nodeData, connectionsToOutput, outputFunc, manifest);
        }

        void Load(BuildTarget target,
            Model.NodeData node,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output)
        {
            if (connectionsToOutput == null || Output == null)
            {
                return;
            }

            Manifest manifest = null;
            if (selectIndex == 0 && string.IsNullOrEmpty(newManifestName))
            {
                manifest = this.manifest;
            }
            else
            {
                var settings = Settings;
                if (selectIndex != 1)
                {
                    manifest = settings.manifests[selectIndex - 1];
                }
            }

            if (manifest == null)
            {
                return;
            }
            OnOutput(node, connectionsToOutput, Output, manifest);
            // var dst = (connectionsToOutput == null || !connectionsToOutput.Any()) ? null : connectionsToOutput.First();
            // Output(dst, output);
        }

        void OnOutput(Model.NodeData node,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output, Manifest manifest)
        {
            var allOutput = new Dictionary<string, Dictionary<string, List<AssetReference>>>();
            List<AssetReference> assetReferences = new List<AssetReference>();
            if (manifest != null)
            {
                var path = AssetDatabase.GetAssetPath(manifest);
                assetReferences.Add(AssetReference.CreateReference(path));
            }

            var output = new Dictionary<string, List<AssetReference>>
            {
                {"0", assetReferences}
            };
            foreach (var outPoints in node.OutputPoints)
            {
                allOutput[outPoints.Id] = output;
            }

            foreach (var dst in connectionsToOutput)
            {
                if (allOutput.ContainsKey(dst.FromNodeConnectionPointId))
                {
                    Output(dst, allOutput[dst.FromNodeConnectionPointId]);
                }
            }
        }
    }
}