using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

namespace UnityEditor.UI
{

    [CustomEditor(typeof(RecycleGrid), true)]
    [CanEditMultipleObjects]
    public class RecycleGridEditor : Editor
    {
        RecycleGrid rg;
        //定位到第几个
        SerializedProperty sp_scrollRect;
        SerializedProperty sp_viewport;
        SerializedProperty sp_content;
        SerializedProperty sp_dataLocator;
        SerializedProperty sp_itemTemplate;
        SerializedProperty sp_virtualPos;
        SerializedProperty sp_allBeginLine;
        SerializedProperty sp_allEndLine;
        SerializedProperty sp_testIndex;
        SerializedProperty sp_removeBegin;
        SerializedProperty sp_removeEnd;

        MockListData dataObj;

        ScrollRect scrollRect;

        protected virtual void OnEnable()
        {
            sp_scrollRect = serializedObject.FindProperty("scrollRect");
            sp_viewport = serializedObject.FindProperty("viewport");
            sp_content = serializedObject.FindProperty("content");
            sp_dataLocator = serializedObject.FindProperty("dataLocator");
            sp_itemTemplate = serializedObject.FindProperty("itemTemplate");
            sp_virtualPos = serializedObject.FindProperty("virtualPos");
            sp_allBeginLine = serializedObject.FindProperty("allBeginLine");
            sp_allEndLine = serializedObject.FindProperty("allEndLine");
            sp_removeBegin = serializedObject.FindProperty("removeBegin");
            sp_removeEnd = serializedObject.FindProperty("removeEnd");
            sp_testIndex = serializedObject.FindProperty("testIndex");
        }

        public override void OnInspectorGUI()
        {
            rg = (RecycleGrid)target;
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(sp_scrollRect, true);
            EditorGUILayout.PropertyField(sp_viewport, true);
            EditorGUILayout.PropertyField(sp_content, true);
            EditorGUILayout.PropertyField(sp_dataLocator, true);
            EditorGUILayout.PropertyField(sp_itemTemplate, true);
            EditorGUILayout.PropertyField(sp_allBeginLine, true);
            EditorGUILayout.PropertyField(sp_allEndLine, true);

            if (GUILayout.Button("预览", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.resetData();
                rg.initData();
                rg.setViewIndex(0, true);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp_testIndex, true);
            if (GUILayout.Button("定位到", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.setViewIndex(rg.testIndex, false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            dataObj = EditorGUILayout.ObjectField(dataObj, typeof(MockListData), true) as MockListData;
            if (GUILayout.Button("模拟数据", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.dataLocator.loadPrefab(dataObj.data.source);
            }
            EditorGUILayout.EndHorizontal();

            
            EditorGUILayout.PropertyField(sp_removeBegin, true);
            EditorGUILayout.PropertyField(sp_removeEnd, true);
            if (GUILayout.Button("删除数据", createStyle(), GUILayout.Width(24), GUILayout.Height(16)))
            {
                rg.removeData(rg.removeBegin, rg.removeEnd);
            }
            
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
