using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecycleGrid : MonoBehaviour
{
    public Dictionary<int, GameObject> mmap = new Dictionary<int, GameObject>();
    public void Awake()
    {
    }

    public List<GameObject> proxy;
    public GameObject proxyTemplate1;
    public GameObject proxyTemplate2;
    public RectTransform viewport;
    public RectTransform content;

    public ScrollRect scrollRect;

    struct padding
    {
        public float top;
        public float bottom;
        public float left;
        public float right;
    }
  
    public Vector2 spacing;

    public class ShowCellData
    {
        public float posX;
        public LineY ld;
        public int nType;
        public GameObject item;
    }

    //视口中所有行的信息
    public List<LineData> lineDataList = new List<LineData>();

    public void datasource()
    {

    }

    public void initMap()
    {
        mmap[1] = proxyTemplate1;
        mmap[2] = proxyTemplate2;
    }

    public void scrollDelta(float yDelta)
    {
        if (yDelta > 0 )
        {

        }
    }

    public void resetData()
    {
        poolMap.Clear();
    }

    public class LineY
    {
        public float posy;
    }
    //临时保存的每一行的数据
    public class LineData
    {
        public float lineHeight;
        public int beginIndex;
        public int endIndex;
        public LineY childLineY;
        public List<ShowCellData> list = new List<ShowCellData>();
    }

    public LineData obtainLineData(int beginIndex, float lineHeight, float childPosy)
    {
        LineData ld = new LineData();
        ld.beginIndex = beginIndex;
        ld.lineHeight = lineHeight;
        ld.childLineY = new LineY();
        ld.childLineY.posy = childPosy;
        return ld;
    }

    public void setViewIndex(int tarIndex)
    {
        recycleOldItem();
        //y向计算
        //视口的大小
        float viewWidth = viewport.rect.width;
        float viewHeight = -viewport.rect.height;
        //计算中间值
        float deltaX = 0;
        float deltaY = 0;
        //换行
        bool yPlus = false; 
        //找到Item的位置
        bool findItem = false;

        for (int i= 0; i < proxy.Count; i++)
        {
            if (tarIndex == i )
            {
                //找到合适的Item，向下继续填满viewport;
                //向下未填满，目前是留出空白
                //继续向下填充，到结束，可以提早结束
                findItem = true;
            }

            RectTransform rect = proxy[i].GetComponent<RectTransform>();
            //增加X的长度
            deltaX += rect.rect.width;

            LineData ld;
            LineData newLine;
            if (lineDataList.Count == 0)
            {
                ld = obtainLineData(i, rect.rect.height, deltaY);
                lineDataList.Add(ld);
            }
            else
            {
                ld = lineDataList[lineDataList.Count - 1];
            }

            //超过宽度，换行，收下宽度
            if ( deltaX > viewWidth )
            {
                deltaX -= rect.rect.width;
                yPlus = true;
            }

            //换行
            if (yPlus)
            {
                deltaY -= rect.rect.height;
                deltaX = rect.rect.width;

                ld = obtainLineData(i, rect.rect.height, deltaY);
                lineDataList.Add(ld);
                yPlus = false;
            }
            else
            {
                //计算当前行高度高度
                ld.lineHeight = Mathf.Max(ld.lineHeight, rect.rect.height);
            }

            //超过下方视口
            if ( deltaY < viewHeight )
            {
                //第一行移动，只有一行,直接结束
                if (lineDataList.Count == 1)
                {
                    break;
                }
                else
                {
                    //找到Item之后，绘制到视口结束
                    if (findItem)
                    {
                        lineDataList.RemoveAt(lineDataList.Count-1);
                        break;
                    }
                    else
                    {
                        //未找到，需要将上方列表一处，下方列表补上
                        LineData toremove = lineDataList[0];
                        deltaY += toremove.lineHeight;
                        lineDataList.RemoveAt(0);
                        stepLineData(toremove.lineHeight, lineDataList);
                    }
                }
            }
            //生成某个Cell
            float showDataX = deltaX - rect.rect.width;
            int itemType = 1;
            if (i == 1)
            {
                itemType = 2;
            }
            else
            {
                itemType = 1;
            }
            ShowCellData scd = obtainShowCellData(showDataX, ld.childLineY, itemType);
            ld.list.Add(scd);
        }
    }

    private void stepLineData(float dStep, List<LineData> lineHeight)
    {
        for (int j = 0; j < lineHeight.Count; j++)
        {
            LineData topLine = lineHeight[j];
            topLine.childLineY.posy += dStep;
        }
    }

    public ShowCellData obtainShowCellData(float x, LineY ld, int itemType)
    {
        ShowCellData scd = new ShowCellData();
        scd.posX = x;
        scd.ld = ld;
        scd.nType = itemType;
        return scd;
    }

    public void recycleOldItem()
    {
        for (int i = 0; i< lineDataList.Count; i++)
        {
            var lineData = lineDataList[i];
            for (int j = 0; j < lineData.list.Count; j++)
            {
                var showdata = lineData.list[j];
                recycleItem(showdata.nType, showdata.item);
            }
        }
        lineDataList.Clear();

    }

    public void drawView()
    {
        for (int i = 0; i < lineDataList.Count; i++)
        {
            var lineData = lineDataList[i];
            for (int j = 0; j < lineData.list.Count; j++)
            {
                var scd = lineData.list[j];
                GameObject set = findItem(scd.nType);
                scd.item = set;
                Vector2 v2 = new Vector2(scd.posX, scd.ld.posy);
                set.transform.SetParent(content);
                set.SetActive(true);
                set.GetComponent<RectTransform>().anchoredPosition = v2;
            }
        }
    }

    public class PooList
    {
        public Stack<GameObject> list = new Stack<GameObject>();
    }
    public Dictionary<int, PooList> poolMap = new Dictionary<int, PooList>();

    public GameObject findItem(int nType)
    {
        GameObject fd;
        PooList pl;
        if ( poolMap.TryGetValue(nType, out pl) )
        {
            if ( pl.list.Count > 0 )
            {
                fd = pl.list.Pop();
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
        return GameObject.Instantiate<GameObject>(mmap[nType]);
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
}
