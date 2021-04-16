// /********************************************************************
//                
//                          All rights reserved
// 
//     创建日期： 2021年04月08日 13:57
//     文件名称： BuildScript.cs
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
using System.Reflection;

namespace VEngine.Editor.AssetGraph
{
    [CustomNode("Xasset/BuildScript", 1002)]
    public class BuildScriptNode : Node
    {
        private readonly GUIContent buildOptionsContent = new GUIContent("BuildOptions", "编译选项");
        private int buildFuncIndex = -1;
        [SerializeField] private string buildFuncName;
        [SerializeField] private string assemblyQualifiedName = "VEngine.Editor.BuildScript, VEngine.Editor";

        private static string[] methodInfos;

        private string[] MethodInfos
        {
            get
            {
                if (methodInfos == null)
                {
                    buildFuncIndex = -1;
                    var methodInfoList = new List<string>();
                    var type = Type.GetType(assemblyQualifiedName);
                    if (type != null)
                    {
                        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                        foreach (var method in methods)
                        {
                            if (method.ReturnType == typeof(void))
                            {
                                var parameters = method.GetParameters();
                                if (parameters == null || parameters.Length == 0)
                                {
                                    if (buildFuncName == method.Name)
                                    {
                                        buildFuncIndex = methodInfoList.Count;
                                    }

                                    methodInfoList.Add(method.Name);
                                }
                            }
                        }

                        if (buildFuncIndex < 0)
                        {
                            buildFuncName = string.Empty;
                        }

                        methodInfos = methodInfoList.ToArray();
                    }
                }

                return methodInfos;
            }
        }

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
            get { return Model.NodeOutputSemantics.None; }
        }

        public override void Initialize(Model.NodeData data)
        {
            data.AddDefaultInputPoint();
        }

        public override Node Clone(Model.NodeData newData)
        {
            var newNode = new BuildManifestNode();
            newData.AddDefaultInputPoint();
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
            var methodInfos = MethodInfos;
            if (methodInfos != null)
            {
                buildFuncIndex = EditorGUILayout.Popup(buildOptionsContent, buildFuncIndex, methodInfos);
                if (buildFuncIndex >= 0)
                {
                    buildFuncName = methodInfos[buildFuncIndex];
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                using (new RecordUndoScope("Change BuildScript Setting", node))
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
            var type = Type.GetType(assemblyQualifiedName);
            if (null != type)
            {
                var method = type.GetMethod(buildFuncName);
                var obj = Activator.CreateInstance(type);
                method?.Invoke(obj, null);
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