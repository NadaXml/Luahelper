using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using SoFunny.UI;
using System.Collections.Generic;

namespace UnityEditor.UI
{

    [CreateAssetMenu(fileName = "MockListData.asset", menuName = "UGUI/MultiList数据/生成")]
    public class MockListData : ScriptableObject
    {
        public GridDataLocator data;
    }

    [CustomEditor(typeof(RecycleGrid), true)]
    [CanEditMultipleObjects]
    public class RecycleGridEditor : Editor
    {
        RecycleGrid rg;
        //定位到第几个
        SerializedProperty sp_testIndex;

        MockListData dataObj;

        protected virtual void OnEnable()
        {
            sp_testIndex = serializedObject.FindProperty("testIndex");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            rg = (RecycleGrid)target;
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("预览", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.resetData();
                rg.initData();
                rg.setViewIndex(0);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp_testIndex, true);
            if (GUILayout.Button("定位到", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.setViewIndex(rg.testIndex);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            dataObj = EditorGUILayout.ObjectField(dataObj, typeof(MockListData), true) as MockListData;
            if (GUILayout.Button("模拟数据", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.dataLocator.loadPrefab(dataObj.data.source);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
        private GUIStyle createStyle()
        {
            GUIStyle style = new GUIStyle();
            style.richText = true;
            return style;
        }
    }
}
