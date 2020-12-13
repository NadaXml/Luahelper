using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Cell样式模板
public class ItemTemplate : MonoBehaviour
{
    //item模板
    public List<GameObject> itemTemplate = new List<GameObject>();

    public GameObject GetItemTemplate(int nType)
    {
        return itemTemplate[nType];
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

    #region pool相关

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
        return GameObject.Instantiate<GameObject>(itemTemplate[nType-1]);
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