using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//显示数据
public class GridDataSource
{
    public float Width;
    public float Height;
    public int templateId;
}

//数据绑定
public class GridDataLocator
{
    //所有数据
    public List<GridDataSource> source;

    public void TestMock()
    {
        source = new List<GridDataSource>();
        GridDataSource lambda(int templateId)
        {
            GridDataSource gd = new GridDataSource();
            gd.Width = 110;
            gd.Height = 110;
            gd.templateId = templateId;
            return gd;
        }
        GridDataSource tempGd;
        for (int i = 0; i < 20; i++)
        {
            tempGd = lambda(i%2);
            source.Add(tempGd);
        }
    }

    public List<GridDataSource> GetDataSource()
    {
        return source;
    }
}

//临时保存的每一行的数据
public class LineData
{
    //Grid 显示数据
    public class LineY
    {
        public float posy;
    }

    //按照从上到下，从左到右填充
    //每一个节点对应UI显示数据
    public class ShowCellData
    {
        //节点的x位置
        public float posX;
        //节点的高度ld
        public LineY ld;
        //节点的显示模板类型
        public int nType;
        //节点目前使用的GamoObject
        public GameObject item;
    }

    /// <summary>
    /// New一行
    /// </summary>
    /// <param name="beginIndex"></param>
    /// <param name="lineHeight"></param>
    /// <param name="childPosy"></param>
    /// <returns></returns>
    static public LineData obtainLineData(int beginIndex, float lineHeight, float childPosy)
    {
        LineData ld = new LineData();
        ld.beginIndex = beginIndex;
        ld.lineHeight = lineHeight;
        ld.childLineY = new LineY();
        ld.childLineY.posy = childPosy;
        return ld;
    }

    /// <summary>
    /// 新生成一行
    /// </summary>
    /// <param name="deltaY"></param>
    /// <param name="i"></param>
    /// <param name="originHeight"></param>
    /// <returns></returns>
    static public LineData NewLine(float deltaY, int i, float originHeight)
    {
        //生成一行
        LineData ld;
        ld = obtainLineData(i, originHeight, deltaY);
        return ld;
    }

    //行高度
    public float lineHeight;
    //行开始的索引
    public int beginIndex;
    //行结束的索引
    public int endIndex;
    //行Y坐标
    public LineY childLineY;
    //行拥有的Cell
    public List<ShowCellData> list = new List<ShowCellData>();
    //行的X坐标（结尾的位置）
    public float posx = 0;
    /// <summary>
    /// index是否在行内
    /// </summary>
    /// <param name="index">cell的Index</param>
    /// <returns>true：cell在行内 false：cell不在行内</returns>
    public bool inLine(int index)
    {
        return beginIndex <= index && endIndex >= index;
    }
    /// <summary>
    /// 当前行是否在ViewPort中
    /// </summary>
    /// <param name="virtualPos"></param>
    /// <param name="viewHeight"></param>
    /// <returns></returns>
    public bool isInView(float virtualPos, float viewHeight)
    {
        float delta = childLineY.posy - virtualPos;
        return delta + lineHeight > 0 && delta < viewHeight;
    }
    /// <summary>
    /// New一个节点
    /// </summary>
    /// <param name="x"></param>
    /// <param name="ld"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public ShowCellData obtainShowCellData(float x, LineY ld, int itemType)
    {
        ShowCellData scd = new ShowCellData();
        scd.posX = x;
        scd.ld = ld;
        scd.nType = itemType;
        return scd;
    }
    /// <summary>
    /// 添加节点到这一行
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index"></param>
    /// <param name="viewWidth"></param>
    /// <param name="lineHeightDelta"></param>
    /// <returns></returns>
    public bool AddCell(GridDataSource rect, int index, float viewWidth, ref float lineHeightDelta)
    {
        float ft = posx + rect.Width;
        if (ft > viewWidth)
        {
            return false;
        }
        else
        {
            posx = ft;
            ShowCellData scd = obtainShowCellData(posx - rect.Width, childLineY, rect.templateId);
            float heightDelta = Mathf.Max(0, rect.Height - lineHeight);
            lineHeightDelta = Mathf.Max(lineHeight, rect.Height);
            endIndex = index;
            list.Add(scd);
            return true;
        }
    }

    /// <summary>
    /// 数据初始化的时候更新当前行的高度
    /// </summary>
    /// <param name="dStep"></param>
    /// <param name="lineHeight"></param>
    private void stepLineData(float dStep, List<LineData> lineHeight)
    {
        for (int j = 0; j < lineHeight.Count; j++)
        {
            LineData topLine = lineHeight[j];
            topLine.childLineY.posy += dStep;
        }
    }
}

public class RecycleGrid : MonoBehaviour
{
    //视口位置
    public RectTransform viewport;
    //内容区域
    public RectTransform content;
    //滑动区域
    public ScrollRect scrollRect;
    //偏移
    struct padding
    {
        public float top;
        public float bottom;
        public float left;
        public float right;
    }

    //所有行的信息
    public List<LineData> lineDataList = new List<LineData>();

    //数据源
    public GridDataLocator dataLocator = new GridDataLocator();

    //Cell模板
    public ItemTemplate itemTemplate;

    public void resetData()
    {
        itemTemplate.Reset();
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


    public void Awake()
    {
        initData();
        setViewIndex(Convert.ToInt32(1));
        drawView();

        scrollRect.onValueChanged.AddListener(OnScorll);
    }

    /// <summary>
    /// 初始化所有cell数据
    /// </summary>
    public void initData()
    {
        dataLocator.TestMock();

        List<GridDataSource> source = dataLocator.source;
        lineDataList.Clear();
        float tempPosY = 0;
        for (int i = 0; i < source.Count; i++)
        {
            GridDataSource data = source[i];
            LineData ld = GetLine(tempPosY, i, data.Height);
            float heightDelta = 0;
            bool addSucess = ld.AddCell(data, i, ViewWidth, ref heightDelta);
            if (!addSucess)
            {
                //滑动位置
                tempPosY = tempPosY + ld.lineHeight;

                //新增一行
                ld = LineData.NewLine(tempPosY, i, data.Height);

                addSucess = ld.AddCell(data, i, ViewWidth, ref heightDelta);
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
        LineData last = lineDataList[lineDataList.Count - 1];
        content.sizeDelta = new Vector2(content.sizeDelta.x, last.childLineY.posy + last.lineHeight);
    }

    /// <summary>
    /// 列表发生滑动
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void OnScorll(Vector2 v2)
    {
        int lastBegin = allBeginLine;
        int lastEnd = allEndLine;
        int visiableBegin = -1;
        int visiableEnd = -1;

        bool upTodown = false;
        upTodown = content.anchoredPosition.y > virtualPos;
        
        //true 向上划
        //false 向下划
        virtualPos = content.anchoredPosition.y;
        for (int i = allBeginLine; i <= allEndLine; i++)
        {
            if ( lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (visiableBegin == -1 )
                {
                    visiableBegin = i;
                }
                else
                {
                    visiableEnd = i;
                }   
            }
            else
            {
                foreach (var s in lineDataList[i].list)
                {
                    if (s.item != null)
                    {
                        itemTemplate.recycleItem(s.nType, s.item);
                        s.item = null;
                    }
                }
            }
        }

        Debug.Log("  upTodown  " + upTodown);
        //向上划
        if (upTodown)
        {
            int temp = 0;
            bool hasVisiable = visiableEnd != -1;
            if (hasVisiable)
            {
                allBeginLine = visiableBegin;
                temp = visiableEnd;
            }
            else
            {
                temp = allEndLine;
            }
            for (int i = temp + 1; i < lineDataList.Count; i++)
            {
                if (lineDataList[i].isInView(virtualPos, ViewHeight))
                {
                    if (!hasVisiable)
                    {
                        allBeginLine = i;
                    }
                }
                else
                {
                    allEndLine = i;
                    break;
                }
            }
        }
        //向下划
        else
        {
            int temp = 0;
            bool hasVisiable = visiableBegin != -1;
            if (hasVisiable)
            {
                allEndLine = visiableEnd;
                temp = visiableBegin;
            }
            else
            {
                temp = allBeginLine;
            }
            for (int i = temp - 1; i >= 0; i--)
            {
                if (lineDataList[i].isInView(virtualPos, ViewHeight))
                {
                    if (!hasVisiable)
                    {
                        allEndLine = i;
                    }
                }
                else
                {
                    allBeginLine = i;
                    break;
                }
            }
        }
        if (lastEnd != allEndLine || lastBegin != allBeginLine)
        {
            Debug.Log(allBeginLine + " " + allEndLine);
            drawView();
        }
    }

    /// <summary>
    /// 定位到某一行
    /// </summary>
    /// <param name="index"></param>
    public void setViewIndex(int index)
    {
        for (int i = allBeginLine; i <= allEndLine; i++)
        {
            foreach(var cell in lineDataList[i].list)
            {
                if ( cell.item != null )
                {
                    itemTemplate.recycleItem(cell.nType, cell.item);
                    cell.item = null;
                }
            }
        }

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

    /// <summary>
    /// 得到某一行的大小数据,根据行号
    /// </summary>
    /// <param name="deltaY"></param>
    /// <param name="i"></param>
    /// <param name="originHeight"></param>
    /// <returns></returns>
    private LineData GetLine(float deltaY, int i, float originHeight)
    {
        //生成一行
        LineData ld;
        if (lineDataList.Count == 0)
        {
            ld = LineData.NewLine(deltaY, i, originHeight);
            lineDataList.Add(ld);
        }
        else
        {
            ld = lineDataList[lineDataList.Count - 1];
        }

        return ld;
    }

    /// <summary>
    /// 绘制当前视口的数据
    /// </summary>
    public void drawView()
    {
        for (int i = allBeginLine; i <= allEndLine; i++)
        {
            var lineData = lineDataList[i];
            for (int j = 0; j < lineData.list.Count; j++)
            {
                var scd = lineData.list[j];
                if (scd.item == null )
                {
                    scd.item = itemTemplate.findItem(scd.nType);
                }
                GameObject set = scd.item;
                Vector2 v2 = new Vector2(scd.posX, -scd.ld.posy);
                set.transform.SetParent(content);
                set.SetActive(true);
                set.GetComponent<RectTransform>().anchoredPosition = v2;
            }
        }
    }
}
