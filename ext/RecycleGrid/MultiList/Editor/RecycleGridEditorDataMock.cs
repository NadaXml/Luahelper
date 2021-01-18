using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI
{

    [CreateAssetMenu(fileName = "MockListData.asset", menuName = "UGUI/MultiList数据/生成")]
    public class MockListData : ScriptableObject
    {
        public GridDataLocator data;
    }
}
