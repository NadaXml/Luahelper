using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecycleGridSample : MonoBehaviour
{
    public RecycleGrid comp;

    public void Awake()
    {
        if ( comp == null)
        {
            return;
        }

        List<GridDataSource> dataList = new List<GridDataSource>();
        for (int i = 0; i < 10; i++)
        {
            dataList.Add(new GridDataSource());
        }

        comp.BindData(dataList);
        //comp.onRenderCellHandler = TestOnRender;
        comp.initData();
        comp.setViewIndex(1, true);
    }
    public void TestOnRender(int dataIndex, int nType, int itemKey, LineData.ShowCellData sData)
    {
        //Transform tmp = sData.Item.transform.Find("num");
        //if (tmp != null)
        //{
        //    Text tx = tmp.GetComponent<Text>();
        //    tx.text = dataIndex.ToString();
        //}
    }
}
