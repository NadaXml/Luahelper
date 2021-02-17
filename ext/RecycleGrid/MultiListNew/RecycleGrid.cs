using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示用的数据
/// </summary>
[System.Serializable]
[SLua.CustomLuaClass]
public class GridDataSource
{
    public int templateId;
}

/// <summary>
/// 列表显示的数据源
/// </summary>
[System.Serializable]
public class GridDataLocator
{
    //所有数据
    public List<GridDataSource> source;

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

/// <summary>
/// 初始化后，列表格子的显示数据
/// </summary>
[SLua.CustomLuaClass]
public class LineData
{
    //按照从上到下，从左到右填充
    //每一个节点对应UI显示数据
    [SLua.CustomLuaClass]
    public class ShowCellData
    {
        //节点的x位置
        public float posX;
        //节点的y位置（上方对齐）
        public LineData ld;
        //节点的显示模板类型
        public int dataIndex;
        //item模板类型
        public int nType;
        //itemKey
        public int itemKey;

        private static Vector2 tempPos;
        /// <summary>
        /// 放置位置
        /// </summary>
        public Vector2 calcuatePos()
        {
            tempPos.x = posX;
            tempPos.y = -ld.childLineY;
            return tempPos;
        }
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
        ld.childLineY = childPosy;
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
        LineData ld;
        ld = obtainLineData(i, originHeight, deltaY);
        return ld;
    }

    /// <summary>
    /// 清理行数据
    /// </summary>
    public void clearLine()
    {
        lineHeight = 0;
        beginIndex = 0;
        endIndex = 0;
        childLineY = 0;
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
    public float childLineY;
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
        float delta = childLineY - virtualPos;
        return delta + lineHeight > 0 && delta < viewHeight;
    }
    /// <summary>
    /// New一个节点
    /// </summary>
    /// <param name="x"></param>
    /// <param name="ld"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public ShowCellData obtainShowCellData(float x, LineData ld, int dataIndex)
    {
        ShowCellData scd = new ShowCellData();
        scd.posX = x;
        scd.ld = ld;
        scd.dataIndex = dataIndex;
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
    public bool AddCell(GridDataSource source, int index, WllTemplate templateInfo, RecycleGrid rg, ref float lineHeightDelta)
    {
        float delta = templateInfo.width;
        float ft = posx;
        if (list.Count > 0)
        {
            delta = delta + rg.padding.spacex;
        }
        else
        {
            ft = ft + rg.padding.left;
        }
        ft = ft + delta;
        if (ft > rg.ViewWidth)
        {
            return false;
        }
        else
        {
            posx = ft;
            ShowCellData scd = obtainShowCellData(posx - templateInfo.width, this, index);
            scd.nType = source.templateId;
            lineHeightDelta = Mathf.Max(lineHeight, templateInfo.height);
            lineHeight = lineHeightDelta;
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
            topLine.childLineY += dStep;
        }
    }
}

[SLua.CustomLuaClass]
public class RecycleGrid : MonoBehaviour
{
    //视口位置
    public RectTransform viewport;
    //整个内容区域
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
        //public float right;//垂直排列下没有用；
        public float spacey;
        public float spacex;
    }
    public Padding padding;

    //cell的显示信息
    public List<LineData> lineDataList = new List<LineData>();

    //cell的数据源
    public GridDataLocator dataLocator = new GridDataLocator();
    
    /// <summary>
    /// 用于cell子预制的模板
    /// 用于Editor下预览
    /// </summary>
    public ItemTemplate itemTemplate;

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

    //当前滑动的位置
    public float virtualPos = 0;
    /// <summary>
    /// 如果都不可见，allBeginLine == allEndLine == INVALID_POS
    /// 只有一行可见 allBeginLine == allEndLine
    /// </summary>
    //当前可见的行起始
    public int allBeginLine = RecycleGrid.INVALID_POS;
    //当前可见的行结束
    public int allEndLine = RecycleGrid.INVALID_POS;
    

    private Vector2 tempSize = Vector2.zero;
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

    //事件
    //当cell被渲染的时候
    public delegate void OnCellRenderHandler(int dataIndex, int nType, int itemKey, Vector2 pos, LineData.ShowCellData sData);
    public OnCellRenderHandler onCellRenderHandler;
    //当cell被隐藏的时候
    public delegate void OnCellRecycleHandler(int dataIndex, int nType, int itemKey);
    public OnCellRecycleHandler onCellRecycleHandler;
    //当cell第一次被创建的时候
    public delegate int OnCellInitHandler(string path, int nType, RectTransform content);
    public OnCellInitHandler onCellInitHandler;

    public void Awake()
    {
        scrollRect.onValueChanged.AddListener(OnScorll);
    }

    /// <summary>
    /// 初始化所有cell数据
    /// </summary>
    public void initData()
    {
        resetData();
        itemTemplate.Reset();
        List<GridDataSource> source = dataLocator.GetDataSource();
        fillData(0, source);
    }

    /// <summary>
    /// 列表回收
    /// </summary>
    public void resetData()
    {
        clearUseList();
        allBeginLine = RecycleGrid.INVALID_POS;
        allEndLine = RecycleGrid.INVALID_POS;
    }

    /// <summary>
    /// lua传入数据方法
    /// </summary>
    /// <returns></returns>
    public List<GridDataSource> getDataList()
    {
        if (dataLocator.source == null )
        {
            dataLocator.source = new List<GridDataSource>();
        }
        return dataLocator.source;
    }

    /// <summary>
    /// 回收格子中无用的Cell
    /// </summary>
    public void clearUseList()
    {
        foreach (var temp in lineDataList)
        {
            recycleLine(temp);
        }
        lineDataList.Clear();
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
        //向上划
        if (upTodown)
        {
            bool onylEnd = allEndLine != INVALID_POS;
            int tempIndex = onylEnd ? allEndLine : lastEnd;
            if (onylEnd)
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
            if (onylEnd)
            {
                fillToTop_end(tempIndex, ref allBeginLine);
            }
            else
            {
                fillToTop_tr(tempIndex, ref allBeginLine, ref allEndLine);
            }
        }

        Debug.Log("print find xx " + allBeginLine + "  " + allEndLine + " " + lastEnd + " " + lastBegin);

        if (lastEnd != allEndLine || lastBegin != allBeginLine)
        {
            //drawView();
            drawDirtyView(lastBegin, lastEnd, allBeginLine, allEndLine);
        }
    }

    /// <summary>
    /// 定位显示到某一行
    /// </summary>
    /// <param name="index"></param>
    public void setViewIndex(int index, bool force)
    {
        //空列表
        if (lineDataList.Count == 0)
        {
            return;
        }

        //不合法索引
        if (index < 0 || index >= dataLocator.source.Count)
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
                virtualPos = tempData.childLineY;
                ContentAnchorY = virtualPos;
                break;
            }
        }

        //如果定位的行不够填充整个视口，直接拉到最低
        float tempContentHeight = ContentHeight;
        if (tempContentHeight - virtualPos < ViewHeight)
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

    #region 显示数据初始化

    public WllTemplate getTemplateInfo(GridDataSource source)
    {
        return itemTemplate.GetWllItemTemplate(source.templateId);
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
            WllTemplate wllt = itemTemplate.GetWllItemTemplate(data.templateId);

            LineData ld = GetLine(tempPosY, i, wllt.height);
            float heightDelta = 0;
            bool addSucess = ld.AddCell(data, i, wllt, this, ref heightDelta);
            if (!addSucess)
            {
                //滑动位置
                tempPosY = tempPosY + ld.lineHeight + padding.spacey;
                //新增一行
                ld = LineData.NewLine(tempPosY, i, wllt.height);

                addSucess = ld.AddCell(data, i, wllt, this, ref heightDelta);
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
        ContentHeight = last.childLineY + last.lineHeight + padding.bottom;
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
    #endregion 显示数据初始化

    #region cell位置显隐计算
    /// <summary>
    /// 从指定可见位置，向下找结束行
    /// </summary>
    /// <param name="searchIndex">开始查找的行索引</param>
    /// <param name="outEnd">查找到的结束行</param>
    public void fillToBottom_end(int searchIndex, ref int outEnd)
    {
        outEnd = searchIndex;
        searchIndex = CalcuateToBottomSearchIndex(searchIndex);
        for (int i = searchIndex; i < lineDataList.Count; i++)
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
        searchIndex = CalcuateToBottomSearchIndex(searchIndex);
        for (int i = searchIndex; i < lineDataList.Count; i++)
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

    private static int CalcuateToBottomSearchIndex(int searchIndex)
    {
        if (searchIndex == INVALID_POS)
        {
            searchIndex = 0;
        }
        else
        {
            searchIndex = searchIndex + 1;
        }

        return searchIndex;
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
        searchIndex = CalcuateToTopSearchIndex(searchIndex);
        for (int i = searchIndex; i >= 0; i--)
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
        searchIndex = CalcuateToTopSearchIndex(searchIndex);
        for (int i = searchIndex; i >= 0; i--)
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

    private int CalcuateToTopSearchIndex(int searchIndex)
    {
        if (searchIndex == INVALID_POS)
        {
            searchIndex = lineDataList.Count - 1;
        }
        else
        {
            searchIndex = searchIndex - 1;
        }

        return searchIndex;
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
        }
        //只有一行可见，索引重合
        if (beginIndex != INVALID_POS && endIndex == INVALID_POS)
        {
            endIndex = beginIndex;
        }
    }

    #endregion

    #region Cell显示回收方法

    /// <summary>
    /// 根据和上次行数的队伍，回收或者渲染cell
    /// </summary>
    /// <param name="lastBegin"></param>
    /// <param name="lastEnd"></param>
    /// <param name="curBegin"></param>
    /// <param name="curEnd"></param>
    public void drawDirtyView(int lastBegin, int lastEnd, int curBegin, int curEnd)
    {
        //集合重合
        if (lastBegin == curBegin && lastEnd == curEnd)
        {
            return;
        }

        //所有元素消失
        if (curBegin == INVALID_POS && curEnd == INVALID_POS)
        {
            for (int i = lastBegin; i <= lastEnd; i++)
            {
                recycleLine(lineDataList[i]);
            }
        }
        //没有交集
        else if (lastEnd > curBegin || curEnd > lastBegin)
        {
            if (lastBegin != INVALID_POS && lastBegin != INVALID_POS)
            {
                for (int i = lastBegin; i <= lastEnd; i++)
                {
                    recycleLine(lineDataList[i]);
                }
            }
            for (int i = curBegin; i <= curEnd; i++)
            {
                renderLine(lineDataList[i]);
            }
        }
        //有交集
        else
        {
            //比较起点
            if (lastBegin > curBegin)
            {
                for (int i = curBegin + 1; i <= lastBegin; i++)
                {
                    recycleLine(lineDataList[i]);
                }
            }
            else
            {
                for (int i = lastBegin + 1; i <= curBegin; i++)
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

    /// <summary>
    /// 回收一行Item
    /// </summary>
    /// <param name="lineData"></param>
    public void recycleLine(LineData lineData)
    {
        foreach (var s in lineData.list)
        {
            if (s.itemKey != 0)
            {
                if (onCellRecycleHandler != null)
                {
                    onCellRecycleHandler.Invoke(s.dataIndex, s.nType, s.itemKey);
                }
                else
                {
                    itemTemplate.recycleItem(s.nType, s.itemKey);
                }
                s.itemKey = 0;
            }
        }
    }

    /// <summary>
    /// render一行
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
    /// 渲染视口内的cell
    /// </summary>
    /// <param name="showData"></param>
    public void renderCell(LineData.ShowCellData showData)
    {
        LineData.ShowCellData scd = showData;
        if (scd.itemKey == 0)
        {
            if (onCellInitHandler != null)
            {
                WllTemplate wt = itemTemplate.GetWllItemTemplate(scd.nType);
                if (string.IsNullOrEmpty(wt.path))
                {
                    Debug.Log("没有Item路径");
                    return;
                }
                scd.itemKey = onCellInitHandler.Invoke(wt.path, scd.nType, content);
            }
            else
            {
                scd.itemKey = itemTemplate.findItem(scd.nType, content);
            }
        }
        Vector2 vec = scd.calcuatePos();
        Debug.Log("render " + scd.dataIndex);
        onCellRenderHandler.Invoke(scd.dataIndex, scd.nType, scd.itemKey, vec, scd);
    }

    #endregion

    #region 测试中
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
        if (end < 0 || end >= dataCount)
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
        bool needClear = false;
        int fillBeginIndex = 0;
        for (int i = lineDataList.Count - 1; i >= 0; i--)
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
    #endregion

    #region Editor下预览方法

    /// <summary>
    /// Editor下传入预览格子方法
    /// </summary>
    /// <param name="lb"></param>
    public void BindData(List<GridDataSource> lb)
    {
        dataLocator.loadPrefab(lb);
    }

#if UNITY_EDITOR
    public int testIndex = 0;
#endif

    #endregion
}
