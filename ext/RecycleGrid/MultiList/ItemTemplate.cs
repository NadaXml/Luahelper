using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WllTemplate
{
    public string path;
    public float width;
    public float height;
#if UNITY_EDITOR
    public GameObject tItem;
    #endif
}

//Cell模板
public class ItemTemplate : MonoBehaviour
{
    public enum sType
    {
        t = 1,
        eidtor = 2
    }  
    public sType templateType = sType.t;

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
            foreach(var item in list.Value.list)
            {
                if ( item != null )
                {
                    GameObject.Destroy(item);
                }
            }

        }
        poolMap.Clear();
    }

    #region Item回收

    /// <summary>
    /// pool
    /// </summary>
    public class PooList
    {
        public Stack<GameObject> list = new Stack<GameObject>();
    }

    public Dictionary<int, PooList> poolMap = new Dictionary<int, PooList>();

    public GameObject findItem(int nType)
    {
        GameObject fd;
        PooList pl;
        if (poolMap.TryGetValue(nType, out pl))
        {
            if (pl.list.Count > 0)
            {
                fd = pl.list.Pop();
                if ( fd == null )
                {
                    fd = createItem(nType);
                }
            }
            else
            {
                fd = createItem(nType);
            }
        }
        else
        {
            fd = createItem(nType);
        }
        fd.SetActive(true);
        return fd;
    }

    public GameObject createItem(int nType)
    {
        if ( templateType == sType.eidtor) 
        {
            return addPrefab_wllEidtor(nType);
        }       
        else
        {
#if UNITY_EDITOR
            WllTemplate template = GetWllItemTemplate(nType);
            return GameObject.Instantiate<GameObject>(template.tItem);
#else
            return null;
#endif
        }
    }

    public GameObject addPrefab_wllEidtor(int nType)
    {
        WllTemplate wt = GetWllItemTemplate(nType);

        string itemTempletePath = wt.path;
        // 创建新的
        string path = "Assets/Lib/" + itemTempletePath + ".prefab";
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

    public void recycleItem(int nType, GameObject item)
    {
        PooList pl;
        item.SetActive(false);
        if (poolMap.TryGetValue(nType, out pl))
        {
            pl.list.Push(item);
        }
        else
        {
            pl = new PooList();
            pl.list.Push(item);
            poolMap[nType] = pl;
        }
    }
    #endregion
}