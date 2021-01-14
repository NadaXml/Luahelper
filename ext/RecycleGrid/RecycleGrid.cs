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
        //节点的y位置（上方对齐）
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
        //控制content的长度
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
        //设置当前滑动的位置
        virtualPos = content.anchoredPosition.y;
        checkCurrentVisiable(ref allBeginLine, ref allEndLine);
        bool upTodown = false;
        //true 向上划
        //false 向下划
        upTodown = content.anchoredPosition.y > virtualPos;
        //向上划
        if (upTodown)
        {
            bool onylEnd = allEndLine != -1
            int tempIndex = onylEnd ? allEndLine : lastEnd;
            fillToBottom(tempIndex, onylEnd, ref allBeginLine, ref allEndLine);
        }
        //向下划
        else
        {
            bool onylEnd = allBeginLine != -1
            int tempIndex = onylEnd ? allBeginLine : lastBegin;
            fillToTop(tempIndex, onylEnd, ref allBeginLine, allEndLine);
        }
        if (lastEnd != allEndLine || lastBegin != allBeginLine)
        {
            Debug.Log(allBeginLine + " " + allEndLine);
            drawView();
        }
    }

    public int fillToBottom(int searchIndex, bool onlyEnd, ref int outBegin, ref int outEnd)
    {
        outEnd = lineDataList.Count - 1;
        bool flag = true;
        for (int i = searchIndex + 1; i < lineDataList.Count; i++)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (!onlyEnd && flag)
                {
                    outBegin = i;
                    flag = false;
                } 
            }
            else
            {
                if ( onlyEnd )
                {
                    outEnd = i - 1;
                }
                break;
            }
        }
    }

    public int fillToTop(int searchIndex, bool onlyEnd, ref int outBegin, ref int outEnd)
    {
        outBegin = 0;
        bool flag = true;
        for (int i = searchIndex - 1; i >= 0; i--)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (!onlyEnd && flag)
                {
                    outEnd = i;
                    flag = false;
                }
            }
            else
            {
                if ( onlyEnd )
                {
                    outBegin = i + 1;
                }
                break;
            }
        }
    }

    //检查当前显示区域的合法性
    public void checkCurrentVisiable(ref int beginIndex, ref int endIndex)
    {
        int tempBegin = beginIndex;
        int tempEnd = endIndex;
        beginIndex = -1;
        endIndex = -1;
        bool flag = false;
        for (int i = tempBegin; i <= tempEnd; i++)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (flag)
                {
                    beginIndex = i;
                    flag = true;
                }
                else
                {
                    endIndex = i;
                }   
            }
            else
            {
                recycleLine(lineDataList[i]);
            }
        }
    }

    public void recycleLine(lineData)
    {
        foreach (var s in lineData.list)
        {
            if (s.item != null)
            {
                itemTemplate.recycleItem(s.nType, s.item);
                s.item = null;
            }
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
            recycleLine(lineDataList[i]);
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
                virtualPos = tempData.childLineY.posy;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, virtualPos);
                break;
            }
        }

        //如果剩余不够填满，直接拉到最低
        if ( content.sizeDelta.y - virtualPos < viewHeight )
        {
            //设置位置，向上查找
        }
        else
        {
            fillToBottom()
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
