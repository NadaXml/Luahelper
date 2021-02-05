using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

//数据格式
[System.Serializable]
public class GridDataSource
{
    //引用的Id
    public int templateId;
    public WllTemplate wllt;
}

//数据绑定
[System.Serializable]
public class GridDataLocator
{
    //数据源
    public List<GridDataSource> source;

    //加载数据
    public void loadPrefab(List<GridDataSource> source)
    {
        this.source = new List<GridDataSource>();
        foreach ( var temp in source)
        {
            this.source.Add(temp);
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
        //y方向的坐标
        //节点的y位置（上方对齐）
        public float posy;
    }

    //按照从上到下，从左到右填充
    //每一个节点对应UI显示数据
    public class ShowCellData
    {
        //节点的x位置
        public float posX;
        //节点公用的行数据
        public LineY ld;
        //节点的显示模板类型
        public int dataIndex;
        //item模板类型
        public int nType;
        //节点目前使用的GamoObject
        private GameObject item;
        private RectTransform rect;
        //引用的子预制
        public GameObject Item
        {
            set
            {
                if (value == null)
                {
                    item = null;
                    rect = null;
                }
                else
                {
                    item = value;
                    rect = item.GetComponent<RectTransform>();
                }
            }
            get
            {
                return item;
            }
        }

        private static Vector2 tempPos;
        public void renderPos()
        {
            tempPos.x = posX;
            tempPos.y = -ld.posy;
            rect.anchoredPosition = tempPos;
        }

        public void render(GridDataSource gds)
        {
            Debug.Log("render " + dataIndex);

#if UNITY_EDITOR
            Transform tmp = rect.transform.Find("num");
            if (tmp != null)
            {
                Text tx = tmp.GetComponent<Text>();
                tx.text = dataIndex.ToString();
            }
#endif

        }
    }

    /// <summary>
    /// create行的显示数据
    /// </summary>
    /// <param name="beginIndex">cell起始索引</param>
    /// <param name="lineHeight">行高度</param>
    /// <param name="childPosy">垂直方向坐标</param>
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
    /// <param name="deltaY">垂直方向坐标</param>
    /// <param name="i">行索引</param>
    /// <param name="originHeight">行初始高度</param>
    /// <returns></returns>
    static public LineData NewLine(float deltaY, int i, float originHeight)
    {
        //生成一行
        LineData ld;
        ld = obtainLineData(i, originHeight, deltaY);
        return ld;
    }

    /// <summary>
    /// 清理显示行数据（没有回收Item）
    /// </summary>
    public void clearLine()
    {
        lineHeight = 0;
        beginIndex = 0;
        endIndex = 0;
        childLineY.posy = 0;
        list.Clear();
        posx = 0;
    }

    //行高度
    public float lineHeight;
    //行cell开始的索引
    public int beginIndex;
    //行cell结束的索引
    public int endIndex;
    //行Y坐标
    public LineY childLineY;
    //行拥有的Cell
    public List<ShowCellData> list = new List<ShowCellData>();
    //行的X坐标（结尾的位置）
    public float posx = 0;
    /// <summary>
    /// cell的索引是否在行内
    /// </summary>
    /// <param name="index">cell的Index</param>
    /// <returns>true：cell在行内 false：cell不在行内</returns>
    public bool inLine(int index)
    {
        return beginIndex <= index && endIndex >= index;
    }
    /// <summary>
    /// 显示的行是否在viewport中
    /// </summary>
    /// <param name="virtualPos">contet区域的y方向坐标</param>
    /// <param name="viewHeight">视口的高度</param>
    /// <returns>true: 行在视口内 false:行不在视口内</returns>
    public bool isInView(float virtualPos, float viewHeight)
    {
        float delta = childLineY.posy - virtualPos;
        return delta + lineHeight > 0 && delta < viewHeight;
    }

    /// <summary>
    /// create新的cell数据
    /// </summary>
    /// <param name="x">x的坐标</param>
    /// <param name="ld">y的坐标</param>
    /// <param name="dataIndex">datasource的索引</param>
    /// <returns></returns>
    public ShowCellData obtainShowCellData(float x, LineY ld, int dataIndex)
    {
        ShowCellData scd = new ShowCellData();
        scd.posX = x;
        scd.ld = ld;
        scd.dataIndex = dataIndex;
        return scd;
    }

    /// 
    /// <summary>
    /// 添加cell到这一行
    /// </summary>
    /// <param name="source">cell的数据</param>
    /// <param name="index">数据列表的下标</param>
    /// <param name="viewWidth">视口宽度</param>
    /// <param name="lineHeightDelta">行高度</param>
    /// <returns></returns>
    public bool AddCell(GridDataSource source, int index, RecycleGrid grid, ref float lineHeightDelta)
    {
        float ft = posx + source.wllt.width;
        if (list.Count == 0)
        {
            ft = ft + grid.padding.left;
        }
        else
        {
            ft = ft + grid.padding.spacex;
        }
        if (ft > grid.ViewWidth)
        {
            return false;
        }
        else
        {
            posx = ft;
            ShowCellData scd = obtainShowCellData(posx - source.wllt.width, childLineY, index);
            scd.nType = source.templateId;
            lineHeightDelta = Mathf.Max(lineHeight, source.wllt.height);
            lineHeight = lineHeightDelta;
            endIndex = index;
            list.Add(scd);
            return true;
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
    [Serializable]
    public class Padding
    {
        public float top;
        public float bottom;
        public float left;
        public float spacex;
        public float spacey;
    }
    public Padding padding;
    //所有行的信息
    public List<LineData> lineDataList = new List<LineData>();
    //数据源
    public GridDataLocator dataLocator = new GridDataLocator();
    //Cell模板
    public ItemTemplate itemTemplate;

    public void resetData()
    {
        itemTemplate.Reset();
        allBeginLine = INVALID_POS;
        allEndLine = INVALID_POS;
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
    static public int INVALID_POS = -1;
    /// <summary>
    /// content滑动的位置，正数
    /// </summary>
    public float virtualPos = 0;
    
    /// <summary>
    /// 当前显示的行起始
    /// </summary>
    public int allBeginLine = RecycleGrid.INVALID_POS;
    /// <summary>
    /// 当前显示的行起始
    /// </summary>
    public int allEndLine = RecycleGrid.INVALID_POS;

    private Vector2 tempSize = Vector2.zero;
    /// <summary>
    /// 内容区域长度
    /// </summary>
    public float ContentHeight
    {
        get
        {
            return content.sizeDelta.y;
        }
        set
        {
            tempSize.x = content.sizeDelta.x;
            tempSize.y = value;
            content.sizeDelta = tempSize;
        }
    }
    private Vector2 tempAnchor = Vector2.zero;
    /// <summary>
    /// content
    /// </summary>
    public float ContentAnchorY
    {
        get
        {
            return content.anchoredPosition.y;
        }
        set
        {
            tempAnchor.x = content.anchoredPosition.x;
            tempAnchor.y = value;
            content.anchoredPosition = tempAnchor;
        }
    }

    public void Awake()
    {
        scrollRect.onValueChanged.AddListener(OnScorll);
    }

    /// <summary>
    /// 初始化所有cell数据
    /// </summary>
    public void initData()
    {
        List<GridDataSource> source = dataLocator.GetDataSource();
        foreach( var temp in lineDataList)
        {
            recycleLine(temp);
        }
        lineDataList.Clear();
        fillData(0, source);
    }

    public int removeBegin = 0;
    public int removeEnd = 0;
    //目前只支持删除可见区域内的数据
    public void removeData(int begin, int end)
    {
        int dataCount = dataLocator.source.Count;
        if (begin < 0 || begin >= dataCount)
        {
            Debug.Log("索引不合法");
            return;
        }
        if ( end < 0 || end >= dataCount)
        {
            Debug.Log("索引不合法");
            return;
        }
        //删除数据
        for (int i = end; i >= begin; i--)
        {
            dataLocator.source.RemoveAt(i);
        }
        //删除这个位置后面的数据
        int fillBeginIndex = 0;
        for (int i = lineDataList.Count -1 ; i >= 0; i--)
        {
            LineData ld = lineDataList[i];
            fillBeginIndex = ld.beginIndex;
            recycleLine(ld);
            lineDataList.RemoveAt(i);
            if (ld.inLine(begin))
            {
                break;
            }           
        }
        List<GridDataSource> source = dataLocator.GetDataSource();
        fillData(fillBeginIndex, source);
        setViewIndex(fillBeginIndex, true);
    }

    /// <summary>
    /// 计算显示cell需要的数据，按行存储
    /// </summary>
    /// <param name="beginIndex">开始计算的cell索引</param>
    /// <param name="source">数据源</param>
    public void fillData(int beginIndex, List<GridDataSource> source)
    {
        float tempPosY = padding.top;
        for (int i = beginIndex; i < source.Count; i++)
        {
            GridDataSource data = source[i];
            data.wllt = itemTemplate.GetWllItemTemplate(data.templateId);

            LineData ld = GetLine(tempPosY, i, data.wllt.height);
            float heightDelta = 0;
            bool addSucess = ld.AddCell(data, i, this, ref heightDelta);
            if (!addSucess)
            {
                //滑动位置
                tempPosY = tempPosY + ld.lineHeight + padding.spacey;
                //新增一行
                ld = LineData.NewLine(tempPosY, i, data.wllt.height);

                addSucess = ld.AddCell(data, i, this, ref heightDelta);
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
        ContentHeight = last.childLineY.posy + last.lineHeight + padding.bottom;
    }
    /// <summary>
    /// 列表发生滑动，新增显示行，减少显示行
    /// </summary>
    /// <param name="v2"></param>
    public void OnScorll(Vector2 v2)
    {
        int lastBegin = allBeginLine;
        int lastEnd = allEndLine;
        float tempContentY = ContentAnchorY;
        //true 向上划
        //false 向下划
        bool upTodown = false;
        upTodown = tempContentY > virtualPos;
        //设置当前滑动的位置
        virtualPos = tempContentY;
        checkCurrentVisiable(ref allBeginLine, ref allEndLine);

        if(allBeginLine == INVALID_POS && allEndLine == INVALID_POS)
        {
            return;
        }
        //向上划
        if (upTodown)
        { 
            bool onylEnd = allEndLine != INVALID_POS;
            int tempIndex = onylEnd ? allEndLine : lastEnd;
            if ( onylEnd )
            {
                fillToBottom_end(tempIndex, ref allEndLine);
            }
            else
            {
                fillToBottom_tr(tempIndex, ref allBeginLine, ref allEndLine);
            }
        }
        //向下划
        else
        {
            bool onylEnd = allBeginLine != INVALID_POS;
            int tempIndex = onylEnd ? allBeginLine : lastBegin;
            if ( onylEnd )
            {
                fillToTop_end(tempIndex, ref allBeginLine);
            }
            else
            {
                fillToTop_tr(tempIndex, ref allBeginLine, ref allEndLine);
            }
        }
        if (allEndLine == INVALID_POS && allBeginLine == INVALID_POS)
        {
            return;
        }
        if (lastEnd != allEndLine || lastBegin != allBeginLine)
        {
            Debug.Log(allBeginLine + " " + allEndLine);
            //drawView();
            drawDirtyView(lastBegin, lastEnd, allBeginLine, allEndLine);
        }
    }

    /// <summary>
    /// 从指定可见位置，向下找结束行
    /// </summary>
    /// <param name="searchIndex">开始查找的行索引</param>
    /// <param name="outEnd">查找到的结束行</param>
    public void fillToBottom_end(int searchIndex, ref int outEnd)
    {
        outEnd = searchIndex;
        for (int i = searchIndex + 1; i < lineDataList.Count; i++)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                outEnd = i;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 从指定不可见位置，向下找可见的开始行和结束行
    /// </summary>
    /// <param name="searchIndex">开始查找的行索引</param>
    /// <param name="outBegin">查找到的开始行</param>
    /// <param name="outEnd">查找到的结束行</param>
    public void fillToBottom_tr(int searchIndex, ref int outBegin, ref int outEnd)
    {
        outBegin = INVALID_POS;
        outEnd = INVALID_POS;
        bool flag = true;
        for (int i = searchIndex + 1; i < lineDataList.Count; i++)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (flag)
                {
                    outBegin = i;
                    outEnd = i;
                    flag = false;
                }
                else
                {
                    outEnd = i;
                }
            }
            else
            {
                break;
            }
        }
    }


    /// <summary>
    /// 从指定不可见位置，向上可见的开始行和结束行
    /// </summary>
    /// <param name="searchIndex">开始查找的行索引</param>
    /// <param name="outBegin"></param>
    /// <param name="outEnd"></param>
    public void fillToTop_tr(int searchIndex, ref int outBegin, ref int outEnd)
    {
        outBegin = INVALID_POS;
        outEnd = INVALID_POS;
        bool flag = true;
        for (int i = searchIndex - 1; i >= 0; i--)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (flag)
                {
                    outEnd = i;
                    outBegin = i;
                    flag = false;
                }
                else
                {
                    outBegin = i;
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 从指定可见位置，向上找开始行
    /// </summary>
    /// <param name="searchIndex"></param>
    /// <param name="outBegin"></param>
    public void fillToTop_end(int searchIndex, ref int outBegin)
    {
        outBegin = searchIndex;
        for (int i = searchIndex - 1; i >= 0; i--)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                outBegin = i;
            }
            else
            {
                break;
            }
        }
    }


    /// <summary>
    /// 检查当前显示区域的合法性
    /// </summary>
    /// <param name="beginIndex">返回可显示的开始行</param>
    /// <param name="endIndex">返回可显示的结束行</param>
    public void checkCurrentVisiable(ref int beginIndex, ref int endIndex)
    {
        int tempBegin = beginIndex;
        int tempEnd = endIndex;
        beginIndex = INVALID_POS;
        endIndex = INVALID_POS;
        int dataCount = lineDataList.Count;
        if (tempBegin < 0 || tempBegin >= dataCount)
        {
            return;
        }
        if (tempEnd < 0 || tempEnd >= dataCount)
        {
            return;
        }

        bool flag = true;
        for (int i = tempBegin; i <= tempEnd; i++)
        {
            if (lineDataList[i].isInView(virtualPos, ViewHeight))
            {
                if (flag)
                {
                    beginIndex = i;
                    flag = false;
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
        //只有一行可见，索引重合
        if (beginIndex != INVALID_POS && endIndex == INVALID_POS)
        {
            endIndex = beginIndex;
        }
    }

    /// <summary>
    /// 回收这一行的cell
    /// </summary>
    /// <param name="lineData"></param>
    public void recycleLine(LineData lineData)
    {
        foreach (var s in lineData.list)
        {
            if (s.Item != null)
            {
                itemTemplate.recycleItem(s.nType, s.Item);
                s.Item = null;
            }
        }
    }

    /// <summary>
    /// render这一行的cell
    /// </summary>
    /// <param name="lineData"></param>
    public void renderLine(LineData lineData)
    {
        for (int j = 0; j < lineData.list.Count; j++)
        {
            renderCell(lineData.list[j]);
        }
    }

    /// <summary>
    /// Editor测试变量
    /// </summary>
    public int testIndex = 0;
    /// <summary>
    /// 定位显示到某一行
    /// </summary>
    /// <param name="index"></param>
    public void setViewIndex(int index, bool force)
    {
        //空列表
        if ( lineDataList.Count == 0 )
        {
            return;
        }

        //不合法索引
        if (index < 0 || index >= dataLocator.source.Count )
        {
            Debug.Log("索引不合法");
            return;
        }

        int lastBegin = allBeginLine;
        int lastEnd = allEndLine;
        
        int findLine = INVALID_POS;
        for (int i = 0; i < lineDataList.Count; i++)
        {
            LineData tempData = lineDataList[i];
            if (tempData.inLine(index))
            {
                findLine = i;
                virtualPos = tempData.childLineY.posy;
                ContentAnchorY = virtualPos;
                break;
            }
        }

        //如果定位的行不够填充整个视口，直接拉到最低
        float tempContentHeight = ContentHeight;
        if (tempContentHeight - virtualPos < ViewHeight )
        {
            //设置位置，向上查找
            if (tempContentHeight >= ViewHeight)
            {
                virtualPos = tempContentHeight - ViewHeight;
            }
            else
            {
                virtualPos = 0;
            }
            ContentAnchorY = virtualPos;
            //向上填充
            fillToTop_end(findLine, ref allBeginLine);
            //向下填充
            fillToBottom_end(findLine, ref allEndLine);
        }
        else
        {
            allBeginLine = findLine;
            //向下填充
            fillToBottom_end(findLine, ref allEndLine);
        }
        
        if (lastBegin != allBeginLine || lastEnd != allEndLine)
        {
            Debug.Log("print find "+ allBeginLine + "  " + allEndLine);
            if (lastBegin == INVALID_POS || lastEnd == INVALID_POS || force)
            {
                drawView();
            }
            else
            {
                drawDirtyView(lastBegin, lastEnd, allBeginLine, allEndLine);
            }
        }
    }

    /// <summary>
    /// 得到一行的显示数据
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

    private Vector2 tmpPos = Vector2.zero;
    public void drawDirtyView(int lastBegin, int lastEnd, int curBegin, int curEnd)
    {
        //集合重合
        if (lastBegin == curBegin && lastEnd == curEnd)
        {
            return;
        }

        //没有交集
        if (lastEnd > curBegin || curEnd > lastBegin)
        {
            for (int i = lastBegin; i <= lastEnd; i++)
            {
                recycleLine(lineDataList[i]);
            }

            for(int i = curBegin; i <= curEnd; i++)
            {
                renderLine(lineDataList[i]);
            }
        }
        //有交集
        else
        {
            //比较起点
            if(lastBegin > curBegin)
            {
                for(int i = curBegin + 1; i <= lastBegin; i++)
                {
                    recycleLine(lineDataList[i]);  
                }
            }
            else
            {
                for(int i = lastBegin + 1; i <= curBegin; i++)
                {
                    renderLine(lineDataList[i]);
                }
            }

            //比较终点
            if (lastEnd > curEnd)
            {
                for (int i = curEnd + 1; i <= lastEnd; i++)
                {
                    recycleLine(lineDataList[i]);
                }
            }
            else
            {
                for (int i = lastEnd + 1; i <= curEnd; i++)
                {
                    renderLine(lineDataList[i]);
                }
            }
        }
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
                renderCell(lineData.list[j]);
            }
        }
    }

    public void renderCell(LineData.ShowCellData showData)
    {
        LineData.ShowCellData scd = showData;
        if (scd.Item == null)
        {
            scd.Item = itemTemplate.findItem(scd.nType);
            scd.Item.transform.SetParent(content);
            scd.Item.SetActive(true);
        }
        scd.renderPos();
        GridDataSource gds = dataLocator.source[scd.dataIndex];
        scd.render(gds);
    }
}
