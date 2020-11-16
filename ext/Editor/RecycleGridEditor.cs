using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class AnimData
{
    public Vector2 d;
    public float gap;
    public Vector2 w;
}

[CreateAssetMenu(fileName = "LoadForceDataObject.asset", menuName = "生成/加载数据")]
public class LoadForceDataObject : ScriptableObject
{
    public AnimData dataList;
}



public class RecycleGridEditorWindow : EditorWindow
{

    [MenuItem("Tools/DataTable")]
    static void CreateWindow()
    {
        EditorWindow window = EditorWindow.GetWindow<RecycleGridEditorWindow>("数据配制", true);
        window.position = new Rect(200, 300, 600, 400);
        window.maxSize = new Vector2(600, 400);
        window.Show();
    }

    public LoadForceDataObject dataObj;

    public void OnGUI()
    {
        dataObj = EditorGUILayout.ObjectField(dataObj, typeof(LoadForceDataObject), true) as LoadForceDataObject;
        if ( GUILayout.Button("按下"))
        {
            Debug.Log(dataObj.dataList.d);
        }
    }
}

[CustomEditor(typeof(RecycleGrid))]
public class RecycleGridEditor : Editor
{
    public string drawIndex = "";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        drawIndex = GUILayout.TextField(drawIndex);
        if ( GUILayout.Button("按下"))
        {
            GameObject s = Selection.activeGameObject;
            RecycleGrid rg = s.GetComponent<RecycleGrid>();
            rg.initMap();
            rg.recycleOldItem();
            rg.setViewIndex(Convert.ToInt32(drawIndex));
            rg.drawView();
        }

        if ( GUILayout.Button("清理"))
        {
            GameObject s = Selection.activeGameObject;
            RecycleGrid rg = s.GetComponent<RecycleGrid>();
            rg.resetData();
        }
    }
}
