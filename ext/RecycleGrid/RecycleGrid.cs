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

    //所有行的信息
    public List<LineData> lineDataList = new List<LineData>();

    public void datasource()
    {

    }

    public void initMap()
    {
        mmap[1] = proxyTemplate1;
        mmap[2] = proxyTemplate2;
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

        public float posx = 0;

        public bool inLine(int index)
        {
            return beginIndex <= index && endIndex >= index;
        }

        public bool isInView(float virtualPos, float viewHeight)
        {
            float delta = childLineY.posy - virtualPos;
            return delta + lineHeight > 0 && delta < viewHeight;
        }

        public ShowCellData obtainShowCellData(float x, LineY ld, int itemType)
        {
            ShowCellData scd = new ShowCellData();
            scd.posX = x;
            scd.ld = ld;
            scd.nType = itemType;
            return scd;
        }
        public bool AddCell(RectTransform rect, int index, float viewWidth, ref float lineHeightDelta)
        {
            float ft = posx + rect.rect.width;
            if ( ft > viewWidth)
            {
                return false;
            }
            else
            {
                posx = ft;
                ShowCellData scd = obtainShowCellData(posx- rect.rect.width, childLineY, 1);
                float heightDelta = Mathf.Max(0, rect.rect.height - lineHeight);
                lineHeightDelta = Mathf.Max(lineHeight, rect.rect.height);
                endIndex = index;
                list.Add(scd);
                return true;
            }
        }
        
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

    //视口的高度
    public float ViewHeight
    {
        get
        {
            return viewport.rect.height;
        }
    }
    //视口的宽度
    public float ViewWidth
    {
        get
        {
            return viewport.rect.width;
        }
    }
    //当前滑动的位置
    public float virtualPos = 0;
    public int allBeginLine;
    public int allEndLine;

    public void initData()
    {
        lineDataList.Clear();
        float tempPosY = 0;
        for (int i = 0; i < proxy.Count; i++)
        {
            RectTransform rect = proxy[i].GetComponent<RectTransform>();
            LineData ld = GetLine(tempPosY, i, rect.rect.height);
            float heightDelta = 0;
            bool addSucess = ld.AddCell(rect, i, ViewWidth, ref heightDelta);
            if (!addSucess)
            {
                //滑动位置
                tempPosY = tempPosY + ld.lineHeight;

                //新增一行
                ld = NewLine(tempPosY, i, rect.rect.height);

                addSucess = ld.AddCell(rect, i, ViewWidth, ref heightDelta);
                if (!addSucess)
                {
                    Debug.LogError("Item的大小不合适窗口的大小");
                    break;
                }
                else
                {
                    lineDataList.Add(ld);
                }
            }
        }
    }

    public void OnScorll(float x, float y)
    {
        virtualPos = virtualPos + y;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + y );
        int lastInVisiable = -1;
        for (int i = allBeginLine; i <= allEndLine; i++)
        {
            if ( lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                lastInVisiable = i;
                break;
            }
            else
            {
                foreach (var s in lineDataList[i].list)
                {
                    if (s.item != null)
                    {
                        recycleItem(s.nType, s.item);
                    }
                }
            }
        }
        //全部不可见,从allEnLine开始找可见到不可见
        if (lastInVisiable == -1)
        {
            bool firstVisiable = false;
            for (int i = allEndLine+1; i < lineDataList.Count; i++)
            {
                if (!lineDataList[i].isInView(virtualPos, ViewHeight))
                {
                    if (firstVisiable)
                    {
                        allEndLine = i-1;
                        break;
                    }
                }
                else
                {
                    allBeginLine = i;
                    firstVisiable = true;
                }
            }
        }
        else
        {
            int tempBegin = -1;
            //有可见
            if (lastInVisiable == -1)
            {
                tempBegin = allEndLine+1;
            }
            else
            {
                allBeginLine = lastInVisiable;
                tempBegin = allBeginLine + 1;
            }
            
            for(int i = tempBegin; i < lineDataList.Count; i++)
            {
                if( !lineDataList[i].isInView(virtualPos, ViewHeight))
                {
                    allEndLine = i-1;
                    break;
                }
            }
        }
        
    }

    public void setViewIndex(int index)
    {
        int findLine = -1;
        int beginLine = -1;
        int endLine = -1;
        for (int i = 0; i < lineDataList.Count; i++)
        {
            LineData tempData = lineDataList[i];
            if (tempData.inLine(index))
            {
                if (i == 0)
                {
                    findLine = 0;
                    virtualPos = 0;
                }
                else
                {
                    if ( tempData.childLineY.posy - (ViewHeight - tempData.lineHeight) < 0 )
                    {
                        findLine = 0;
                        virtualPos = 0;
                    }
                    else
                    {
                        findLine = i;
                    }
                }
                break;
            }
        }

        //第一行，查找下方
        if (findLine == 0)
        {
            beginLine = findLine;
            endLine = lineDataList.Count;
            for (int i = 0; i < lineDataList.Count; i++)
            {
                if ( !lineDataList[i].isInView(virtualPos, ViewHeight) )
                {
                    endLine = i-1;
                    break;
                }
            }
        }
        else
        //向上查找行，上方不够，再显示下方
        {
            beginLine = 0;
            endLine = findLine;
            for (int i = findLine - 1; i > 0; i--)
            {
                if ( !lineDataList[i].isInView(virtualPos, ViewHeight) )
                {
                    beginLine = i;
                    break;
                }                
            }
        }

        if (beginLine != -1 && endLine != - 1)
        {
            allBeginLine = beginLine;
            allEndLine = endLine;
            Debug.Log("print find "+ beginLine + "  " + endLine);
        }

    }

    private LineData GetLine(float deltaY, int i, float originHeight)
    {
        //生成一行
        LineData ld;
        if (lineDataList.Count == 0)
        {
            ld = NewLine(deltaY, i, originHeight);
            lineDataList.Add(ld);
        }
        else
        {
            ld = lineDataList[lineDataList.Count - 1];
        }

        return ld;
    }

    private LineData NewLine(float deltaY, int i, float originHeight)
    {
        //生成一行
        LineData ld; 
        ld = obtainLineData(i, originHeight, deltaY);
        return ld;
    }

    private void stepLineData(float dStep, List<LineData> lineHeight)
    {
        for (int j = 0; j < lineHeight.Count; j++)
        {
            LineData topLine = lineHeight[j];
            topLine.childLineY.posy += dStep;
        }
    }

    public void drawView()
    {
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, virtualPos);
        for (int i = allBeginLine; i <= allEndLine; i++)
        {
            var lineData = lineDataList[i];
            for (int j = 0; j < lineData.list.Count; j++)
            {
                var scd = lineData.list[j];
                if (scd.item == null )
                {
                    scd.item = findItem(scd.nType);
                }
                GameObject set = scd.item;
                Vector2 v2 = new Vector2(scd.posX, -scd.ld.posy);
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
