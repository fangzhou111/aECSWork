using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ResConfigWindow : EditorWindow
{
    private string _configPath = "/temp/resconfig.json";
    private string[] _publishPaths =
    {
        "/test/"
    };

    private JSONClass _json = null;
    private List<Object> _targets = new List<Object>();
    private Vector2 _size = Vector2.zero;
    private string _searchStr = "";

    [MenuItem("Window/ResConfigWindow")]
    static void Init()
    {
        ResConfigWindow window = (ResConfigWindow)EditorWindow.GetWindow(typeof(ResConfigWindow), false, "ResConfigWindow", true);

        window.Show();//展示
    }

    public ResConfigWindow()
    {

    }

    void OnGUI()
    {
        if (_json == null)
        {
            _configPath = EditorGUILayout.TextField("配置的路径", _configPath);

            if (GUILayout.Button("start"))
            {
                if (File.Exists(Application.dataPath + _configPath))
                {
                    _json = JSONNode.LoadFromFile(Application.dataPath + _configPath) as JSONClass;
                }
                else
                    _json = new JSONClass();

                RefreshResList();
            }
        }
        else
        {
            _size = GUILayout.BeginScrollView(_size);

            _searchStr = EditorGUILayout.TextField("快速查找（模糊）:", _searchStr);

            Object[] os = _targets.Where(v =>
            {
                string path = AssetDatabase.GetAssetPath(v);

                return path.Contains(_searchStr);
            }).ToArray();

            for (int i = 0;i < os.Length;i++)
            {
                Object obj = os[i];

                string name = AssetDatabase.GetAssetPath(obj);

                GUI.color = Color.white;

                if (_json[name]["Level"].AsInt == 0)
                    GUI.color = Color.red;
                else if (_json[name]["Level"].AsInt == 2)
                    GUI.color = Color.yellow;

                EditorGUILayout.LabelField("资源完整路径:" + name);

                EditorGUILayout.BeginHorizontal();
                _json[name]["Level"].AsInt = EditorGUILayout.Popup("加载等级:", _json[name]["Level"].AsInt, new string[] { "System", "Scene", "Custom" });
                _json[name]["Max"].AsInt = EditorGUILayout.IntField("最大保留数:", _json[name]["Max"].AsInt);
                _json[name]["Min"].AsInt = EditorGUILayout.IntField("最小保留数(一般建议0):", _json[name]["Min"].AsInt);
                _json[name]["StartDesTime"].AsFloat = EditorGUILayout.FloatField("自动销毁开始时间:", _json[name]["StartDesTime"].AsFloat);
                _json[name]["DesInterval"].AsFloat = EditorGUILayout.FloatField("自动销毁间隔:", _json[name]["DesInterval"].AsFloat);
                EditorGUILayout.EndHorizontal();

                GUI.color = Color.white;
            }

            if (GUILayout.Button("refresh"))
            {
                _searchStr = "";

                RefreshResList();

                Debug.Log("刷新资源列表");
            }

            if (GUILayout.Button("execute"))
            {
                _json.SaveToFile(Application.dataPath + _configPath);

                AssetDatabase.Refresh();

                Debug.Log("输出完毕");
            }

            if (GUILayout.Button("aaa"))
            {
                DeleteNoUseJson();
            }

            GUILayout.EndScrollView();
        }
    }

    private void DeleteNoUseJson()
    {
        List<JSONNode> deletes = new List<JSONNode>();
        foreach(JSONNode node in _json.Childs)
        {
            Object obj = AssetDatabase.LoadAssetAtPath(node["Path"], typeof(Object));

            if (obj == null)
                deletes.Add(node);
        }

        foreach(JSONNode node in deletes)
        {
            _json.Remove(node);
        }
    }

    private void RefreshResList()
    {
        _targets.Clear();

        foreach (string path in _publishPaths)
        {
            string[] filepaths = Directory.GetFiles(Application.dataPath + path);

            foreach (var filepath in filepaths)
            {
                if (filepath.EndsWith(".prefab"))
                {
                    string targetpath = filepath.Replace(Application.dataPath, "Assets");

                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(targetpath);

                    if (go != null)
                    {
                        if (_json[targetpath] == null)
                            CreateNewNode(_json, targetpath);
                    }

                    _targets.Add(go);
                }
            }
        }
    }

    private void CreateNewNode(JSONClass json,string name)
    {
        json[name]["Path"] = name;
        json[name]["Level"].AsInt = 1;
        json[name]["Max"].AsInt = 3;
        json[name]["Min"].AsInt = 0;
        json[name]["StartDesTime"].AsFloat = 60f;
        json[name]["DesInterval"].AsFloat = 5f;

        //待扩展
    }
}
