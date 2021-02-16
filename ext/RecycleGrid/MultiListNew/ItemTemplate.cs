using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WllTemplate
{
    public string path;
    //用来生成了路径用
    public GameObject pathObj;
    public float width;
    public float height;
}

//Cell模板
[System.Serializable]
public class ItemTemplate : MonoBehaviour
{
    /// <summary>
    /// wll子预制路径
    /// </summary>
    public List<WllTemplate> subItemTemplate = new List<WllTemplate>();

    public WllTemplate GetWllItemTemplate(int nType)
    {
        return subItemTemplate[nType];
    }

    public void Reset()
    {
        foreach( var list in poolMap )
        {
            foreach(var item in list.Value.keyMap)
            {
                if ( item.Value != null )
                {
                    GameObject.DestroyImmediate(item.Value);
                }
            }
            list.Value.keyMap.Clear();
            list.Value.inUseList.Clear();
            list.Value.unUseList.Clear();
        }
        poolMap.Clear();
    }

    public void OnCellRenderHandler(int dataIndex, int nType, int itemKey, Vector2 pos, LineData.ShowCellData sData)
    {
        PooList pl;
        if ( poolMap.TryGetValue(nType, out pl) )
        {
            GameObject tmp = pl.keyMap[itemKey];
            if (tmp != null)
            {
                tmp.transform.localPosition = pos;
                Text tx = tmp.transform.Find("num").GetComponent<Text>();
                tx.text = dataIndex.ToString();
            }
        }
    }

    #region Item回收

    /// <summary>
    /// pool
    /// </summary>
    public class PooList
    {
        public Dictionary<int, GameObject> keyMap = new Dictionary<int, GameObject>();
        public Stack<int> unUseList = new Stack<int>();
        public List<int> inUseList = new List<int>();

        public GameObject UnUseItem(int itemKey)
        {
            inUseList.Remove(itemKey);
            unUseList.Push(itemKey);
            return keyMap[itemKey];
        }

        public void AppendItem(int itemKey, GameObject cItem)
        {
            inUseList.Add(itemKey);
            keyMap[itemKey] = cItem;
        }

        public int GetItem()
        {
            int itemKey = unUseList.Pop();
            inUseList.Add(itemKey);
            return itemKey;
        }
    }
    public Dictionary<int, PooList> poolMap = new Dictionary<int, PooList>();

    public int allIdKey = 0;

    public int findItem(int nType, RectTransform content)
    {
        int itemKey;
        GameObject cItem;
        PooList pl;
        if (poolMap.TryGetValue(nType, out pl))
        {
            if (pl.unUseList.Count > 0)
            {
                itemKey = pl.GetItem();
                cItem = pl.keyMap[itemKey];
            }
            else
            {
                cItem = createItem(nType);
                itemKey = ++allIdKey;
                pl.AppendItem(itemKey, cItem);
                cItem.transform.SetParent(content);
            }
        }
        else
        {
            pl = new PooList();
            cItem = createItem(nType);
            itemKey = ++allIdKey;
            pl.AppendItem(itemKey, cItem);
            cItem.transform.SetParent(content);
            poolMap[nType] = pl;
        }
        cItem.SetActive(true);
        return itemKey;
    }

    public GameObject createItem(int nType)
    {
        return addPrefab_wllEidtor(nType);
    }

    //private const string Path_Prev = "Assets/Lib/";
    private const string Path_Prev = "Assets/MultiList/Editor/";

    public GameObject addPrefab_wllEidtor(int nType)
    {
        WllTemplate wt = GetWllItemTemplate(nType);

        string itemTempletePath = wt.path;
        // 创建新的
        string path = Path_Prev + itemTempletePath + ".prefab";
        GameObject load = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
        GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(load) as GameObject;
        obj.transform.SetParent(transform);
        obj.transform.localScale = load.transform.localScale;
        obj.transform.localPosition = load.transform.localPosition;
        obj.transform.localRotation = load.transform.localRotation;
        RectTransform objR = obj.GetComponent<RectTransform>();
        if (objR != null)
        {
            RectTransform rect2 = load.GetComponent<RectTransform>();
            objR.anchoredPosition = rect2.anchoredPosition;
            objR.offsetMin = rect2.offsetMin;
            objR.offsetMax = rect2.offsetMax;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(objR);
        return obj;
    }

    public void recycleItem(int nType, int itemKey)
    {
        PooList pl;
        if (poolMap.TryGetValue(nType, out pl))
        {
            GameObject cItem = pl.UnUseItem(itemKey);
            cItem.SetActive(false);
        }
        else
        {
            Debug.Log("这个不可能为空");
        }
    }
    #endregion
}