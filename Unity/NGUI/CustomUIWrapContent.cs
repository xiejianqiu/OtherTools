using UnityEngine;
using System.Collections.Generic;

public class CustomUIWrapContent : UIWrapContent
{
    public UICenterOnChild centerChild = null;

    private int MinLimit = 1;
    private int MaxLimit = 81;
    private int CurLimit = 1;

    protected override void Start()
    {
        Reset();
    }

    protected override void ResetChildPositions()
    {
        for (int i = 0, imax = mChildren.Count; i < imax; ++i)
        {
            Transform t = mChildren[i];
            t.localPosition = mHorizontal ? new Vector3(i * itemSize, 0f, 0f) : new Vector3(0f, -i * itemSize, 0f);
            Vector3 pos = t.localPosition;
            pos.y += -(CurLimit - MinLimit) * itemSize; 
            t.localPosition = pos;
            UpdateItem(t, i);
        }
    }

    private void SetAllChild(bool nActive)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(nActive);
        }
    }

    public void SetLimit(int minLimit, int maxLimit, int curLimit)
    {
        MinLimit = minLimit;
        MaxLimit = maxLimit;
        CurLimit = curLimit;
    }

    public void Reset()
    {
        if (centerChild == null)
        {
            centerChild = gameObject.GetComponent<UICenterOnChild>();
        }
        SetAllChild(true);
        mFirstTime = true;
        SortBasedOnScrollMovement();
        mScroll.ResetPosition();
        mPanel.clipOffset = Vector2.zero;
        Vector3 pos = mPanel.transform.localPosition;
        pos = (mScroll.movement == UIScrollView.Movement.Vertical) ? new Vector3(pos.x, 0, 0) : new Vector3(0, pos.y, 0);
        mPanel.transform.localPosition = pos;
        GfxUtils.SetScrollViewOffset(mScroll, (CurLimit - MinLimit) * itemSize, false);
        WrapContent();
        if (mScroll != null) mScroll.GetComponent<UIPanel>().onClipMove = OnMove;
        mFirstTime = false;
    }
}
