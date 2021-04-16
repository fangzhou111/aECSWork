// /********************************************************************
//                 
//                          All rights reserved
// 
//     创建日期： 2021年03月23日 13:06
//     文件名称： BuildManifestNode.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using Object = UnityEngine.Object;

namespace VEngine.Editor.AssetGraph
{
    [CustomNode("Xasset/BuildManifest", 1001)]
    public class BuildManifestNode : Node
    {
        enum BuildOptions
        {
            BuildGroups,
            BuildBundles,
            BuildManifest,
        }

        private readonly GUIContent buildOptionsContent = new GUIContent("BuildOptions", "编译选项");
        [SerializeField] private BuildOptions buildOptions;

        // private List<AssetReference> lastImportedAssets;

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
            var newNode = new BuildManifestNode();
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
            base.OnAssetsReimported(nodeData, streamManager, target, ctx, isBuilding);
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
            buildOptions = (BuildOptions) EditorGUILayout.EnumPopup(buildOptionsContent, buildOptions);
            if (EditorGUI.EndChangeCheck())
            {
                using (new RecordUndoScope("Change BuildManifest Setting", node))
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

            foreach (var incom in incoming)
            {
                foreach (var assetGroup in incom.assetGroups)
                {
                    foreach (var asset in assetGroup.Value)
                    {
                        if (asset.path.EndsWith(".asset"))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(asset.path);
                            if (obj is Manifest manifest)
                            {
                                switch (buildOptions)
                                {
                                    case BuildOptions.BuildGroups:
                                        manifest.BuildGroups(out _, out _, out _);
                                        break;
                                    case BuildOptions.BuildBundles:
                                        BuildScript.BuildBundles(manifest);
                                        break;
                                    case BuildOptions.BuildManifest:
                                        manifest.CreateBuild();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
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


            // OnOutput(node, connectionsToOutput, Output, manifest);
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