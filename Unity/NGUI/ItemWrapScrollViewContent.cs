using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemWrapScrollViewContent :ComponentBase
{
    public int itemSizeX = 100;
    public int itemSizeY = 100;

    public delegate void OnItemPosChange(int nRealIndex, GameObject go);
    private OnItemPosChange GotoEnd;
    private OnItemPosChange GotoStart;

    private Transform mTrans = null;
    private UIScrollView mScroll = null;
    private UIPanel mPanel = null;
    private UIGrid mGrid = null;
    private Vector3[] corners = null;

    public int mStartIndex = 0;
    private int mStartRealIndex = 0;
    private int mTotalCount = 0;
    private int mChildCount = 0;
    private Vector3 mLastLocalPos;
    private List<Transform> mChildList = null;

    private AttributeList list;

    public bool isResetting = false;

    public override void Awake()
    {
        //mTotalCount = 0;
        //mStartIndex = 0;
        CacheScrollView();
        mLastLocalPos = mPanel.transform.localPosition;
        mPanel.onClipMove = OnMove;
    }
    //public override void Dispose()
    //{
    //    mTrans = null;
    //    mScroll = null;
    //    mPanel = null;
    //    corners = null;
    //}

    public void SetLastPosition(Vector3 pos)
    {
        mLastLocalPos = pos;
    }
    public void Init(int nSizeX, int nTotalCount, int nChildCount, OnItemPosChange start, OnItemPosChange end, int nSizeY = -1, bool isHorzational = false, int startRealIndex = 0)
    {
        itemSizeX = nSizeX;
        if (nSizeY == -1)
        {
            nSizeY = nSizeX;
        }
        mStartRealIndex = startRealIndex;
        itemSizeY = nSizeY;
        mTotalCount = nTotalCount;
        mChildCount = nChildCount;
        GotoStart = start;
        GotoEnd = end;

        if (isHorzational)
        {
            ResetHorizontalPosition(startRealIndex);
        }
        else
        {
            ResetPosition(startRealIndex);
        }
    }
    public void CalChild(Transform t)
    {
        for (int i = 0; i < t.childCount; ++i)
        {
            mChildList.Add(t.GetChild(i));
        }
    }
    public void CalChildForCard(Transform t)
    {
        mChildList.Clear();
        for (int i = 0; i < t.childCount; ++i)
        {
            if(t.GetChild(i).gameObject.activeSelf)
                mChildList.Add(t.GetChild(i));
        }
    }
    public void SetTotalSize(int nTotalSize)
    {
        mTotalCount = nTotalSize;
    }

    public void CacheScrollView()
    {
        if (list != null)
        {
            return;
        }
        list = gameObject.GetComponent<AttributeList>();
        mScroll = list.m_lstGameObj[0].GetComponent<UIScrollView>();
        mPanel = list.m_lstGameObj[0].GetComponent<UIPanel>();
        mTrans = list.m_lstGameObj[1].transform;
        mGrid = mTrans.GetComponent<UIGrid>();

        corners = new Vector3[4];
        mChildList = new List<Transform>();
    }

    public void OnMove(UIPanel UIpanel)
    {
        if (!isResetting)
        {
            Vector3 delta = UIpanel.transform.localPosition - mLastLocalPos;
            WrapContent(delta);
            mLastLocalPos = UIpanel.transform.localPosition;
        }
    }

    public void WrapContent(Vector3 delta)
    {
        if (mPanel == null)
        {
            return;
        }

        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = mPanel.worldCorners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Bounds b = mScroll.bounds;
        Vector3 constraint = mPanel.CalculateConstrainOffset(b.min, b.max);

        float min = corners[0].y - itemSizeY - constraint.y;
        float max = corners[2].y + itemSizeY - constraint.y;

        float X_min = corners[1].x - itemSizeX - constraint.x;
        float X_max = corners[3].x + itemSizeX - constraint.x;

        int index = 0;
        if (delta.y > 0)
        {
            for (int i = 0; i < mChildList.Count; ++i)
            {
                Transform t = mChildList[i];

                Vector3 center = t.localPosition;
                ++index;
                if (center.y > max)
                {
                    if (mStartRealIndex + mChildCount >= mTotalCount)
                    {
                        continue;
                    }

                    int realindex = mStartRealIndex + mChildCount;
                    t.localPosition = GetPositionByIndex(realindex);
                    GotoEnd(realindex, t.gameObject);
                    ++mStartIndex;
                    if (mStartIndex >= mChildCount)
                    {
                        mStartIndex -= mChildCount;
                    }
                    ++mStartRealIndex;
                }
            }
        }
        else if (delta.y < 0)
        {
            index = 0;
            for (int i = mChildList.Count - 1; i >= 0; --i)
            {
                Transform t = mChildList[i];
                Vector3 center = t.localPosition;
                ++index;
                if (center.y < min)
                {
                    if (mStartRealIndex <= 0)
                    {
                        continue;
                    }

                    int realindex = mStartRealIndex - 1;
                    t.localPosition = GetPositionByIndex(realindex);
                    GotoStart(realindex, t.gameObject);
                    --mStartIndex;
                    if (mStartIndex < 0)
                    {
                        mStartIndex += mChildCount;
                    }
                    --mStartRealIndex;
                }
            }
        }
        else if(delta.x > 0)
        {
            index = 0;
            for (int i = mChildList.Count - 1; i >= 0; --i)
            {
                Transform t = mChildList[i];

                Vector3 center = t.localPosition;
                ++index;
                if (center.x > X_max)
                {
                    if (mStartRealIndex <= 0)
                    {
                        continue;
                    }

                    int realindex = mStartRealIndex - 1;
                    t.localPosition = GetHorizontalPositionByIndex(realindex);
                    GotoStart(realindex, t.gameObject);

                    --mStartIndex;
                    if (mStartIndex < 0)
                    {
                        mStartIndex += mChildCount;
                    }
                    --mStartRealIndex;
                }
            }
        }
        else if(delta.x < 0)
        {
            index = 0;
            for (int i = 0; i < mChildList.Count; ++i)
            {
                Transform t = mChildList[i];
                Vector3 center = t.localPosition;
                ++index;
                if (center.x < X_min)
                {
                    if (mStartRealIndex + mChildCount >= mTotalCount)
                    {
                        continue;
                    }

                    int realindex = mStartRealIndex + mChildCount;
                    t.localPosition = GetHorizontalPositionByIndex(realindex);
                    GotoEnd(realindex, t.gameObject);
                    ++mStartIndex;
                    if (mStartIndex >= mChildCount)
                    {
                        mStartIndex -= mChildCount;
                    }
                    ++mStartRealIndex;
                }
            }
        }

        mScroll.InvalidateBounds();
    }

    public void ResetPosition(int startIndex = 0)
    {
        CacheScrollView();
        mStartIndex = 0;
        mStartRealIndex = startIndex;
        mGrid.Reposition();
        if (mScroll != null && startIndex == 0)
        {
            mScroll.ResetPosition();
        }
        for (int i = 0; i < mChildList.Count; i++)
        {
            Transform t = mChildList[i];
            int realIndex = i + mStartRealIndex;
            t.localPosition = GetPositionByIndex(realIndex);
            GotoEnd(realIndex, t.gameObject);
        }
        //ReWrapContent();
    }

    public void ResetHorizontalPosition(int startIndex = 0)
    {
        CacheScrollView();
        mStartIndex = 0;
        mStartRealIndex = startIndex;
        if (startIndex == 0)
            mGrid.Reposition();
        if (mScroll != null && startIndex == 0)
        {
            mScroll.ResetPosition();
        }
        for (int i = 0; i < mChildList.Count; i++)
        {
            Transform t = mChildList[i];
            int realIndex = i + mStartRealIndex;
            t.localPosition = GetHorizontalPositionByIndex(realIndex);
            GotoEnd(realIndex, t.gameObject);
        }
        //ReWrapContent();
    }

    public void ReWrapContent()
    {
        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = mPanel.worldCorners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Bounds b = mScroll.bounds;
        Vector3 constraint = mPanel.CalculateConstrainOffset(b.min, b.max);
        Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);

        float min = corners[0].y - itemSizeY - constraint.y;

        for (int i = 0; i < mChildList.Count; ++i)
        {
            Transform t = mChildList[i];
            if (center.y < min)
            {
                if (mStartRealIndex + mChildCount >= mTotalCount)
                {
                    continue;
                }

                int realindex = mStartRealIndex + mChildCount;
                t.localPosition = GetPositionByIndex(realindex);
                GotoEnd(realindex, t.gameObject);

                ++mStartIndex;
                if (mStartIndex >= mChildCount)
                {
                    mStartIndex -= mChildCount;
                }
                ++mStartRealIndex;
            }
            else
            {
                int index = i - mStartIndex;
                if (index < 0)
                {
                    index += mChildCount;
                }
                int real = mStartRealIndex + index;
                GotoEnd(real, t.gameObject);
            }
        }
    }

    public void WrapRealIndex(int nRealIndex)
    {
        for (int i = 0; i < mChildList.Count; ++i)
        {
            int index = i - mStartIndex;
            if (index < 0)
            {
                index += mChildCount;
            }
            int real = mStartRealIndex + index;
            if (real == nRealIndex)
            {
                var tf = mChildList[i];
                if (tf == null)
                    continue;
                GotoEnd(real, tf.gameObject);
                break;
            }
        }
    }

    public Vector3 GetPositionByIndex(int nIndex)
    {
        int y = nIndex / mGrid.maxPerLine;
        int x = nIndex % mGrid.maxPerLine;

        float posX = mGrid.cellWidth * (x + (1 - mGrid.maxPerLine) / 2.0f) - mGrid.transform.localPosition.x;
        float posY = -mGrid.cellHeight * y;
        return new Vector3(posX, posY, 0);
    }

    public Vector3 GetHorizontalPositionByIndex(int nIndex)
    {
        int x = nIndex / mGrid.maxPerLine;
        int y = nIndex % mGrid.maxPerLine;
        
        float posX = mGrid.cellWidth * x;
        float posY = mGrid.cellHeight * ((mGrid.maxPerLine - y) - 3 / 2.0f) - mGrid.transform.localPosition.y;
        return new Vector3(posX, posY, 0);
    }

    public void ClearChildList()
    {
        if (mChildList != null && mChildList.Count > 0)
        {
            mChildList.Clear();
        }
    }
}
