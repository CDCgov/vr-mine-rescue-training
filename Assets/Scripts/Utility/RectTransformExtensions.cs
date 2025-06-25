
using UnityEngine;
using System.Collections.Generic;

 public static class RectTransformExtensions
{
    public static readonly Vector2 centerFixedAnchor = new Vector2(0.5f, 0.5f);
    public static readonly Vector2 centerStretchedAnchorMin = new Vector2(0,0);
    public static readonly Vector2 centerStretchedAnchorMax = new Vector2(1, 1);

    
    public static void SetLeftOffset(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRightOffset(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTopOffset(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottomOffset(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
    
   
    /*
    public static float GetTop(this RectTransform rt)
    {
        return rt.position.y + (rt.sizeDelta.y / 2);
    }
    public static float GetBottom(this RectTransform rt)
    {
        return rt.position.y - (rt.sizeDelta.y / 2);
    }
    public static float GetRight(this RectTransform rt)
    {
        return rt.position.x + (rt.sizeDelta.x / 2);
    }
    public static float GetLeft(this RectTransform rt)
    {
        return rt.position.x - (rt.sizeDelta.x / 2);
    }
    */

    public static void SnapAndResizeWindow(this RectTransform.Edge edge, RectTransform rt, RectTransform targetRT)
    {
        //snap edges together
        SetEdgeToOppositeEdgeInCanvasSpace(edge,  rt, targetRT);

        // figure out other edges to reposition
        var edgeA = RectTransform.Edge.Bottom;
        var edgeB = RectTransform.Edge.Bottom;

        if(edge == RectTransform.Edge.Left || edge == RectTransform.Edge.Right)
        {
            edgeA = RectTransform.Edge.Top; 
            edgeB = RectTransform.Edge.Bottom;
            
        }
        else
        {
            edgeA = RectTransform.Edge.Right;
            edgeB = RectTransform.Edge.Left;
        }

        //resize same edges to match
        SetEdgeToSameEdgeInCanvasSpace(edgeA,  rt, targetRT);
        SetEdgeToSameEdgeInCanvasSpace(edgeB,  rt, targetRT);

        // TODO we should scale/move opposite edge to retain space
    }

    /// <summary>
    /// Set the edge of RT to the same edge of the target. Useful for repositioning after snapping adjacent edges together
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="canvasRT"></param>
    /// <param name="rt"></param>
    /// <param name="targetRT"></param>
    public static void SetEdgeToSameEdgeInCanvasSpace(this RectTransform.Edge edge,  RectTransform rt, RectTransform targetRT)
    {
        //edges should be the same
        RectTransform.Edge targetEdge = edge;

        //get target position and reset pivot
        float targetEdgePosition = GetEdgeInCanvasSpace(targetEdge,  targetRT, true);

        // set the RT edge to target edge position
        SetEdgeInCanvasSpace(edge,  rt, targetEdgePosition);
    }

    /// <summary>
    /// Set the edge of RT to the opposite edge of the target. Useful for snapping edges together
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="canvasRT"></param>
    /// <param name="rt"></param>
    /// <param name="targetRT"></param>
    public static void SetEdgeToOppositeEdgeInCanvasSpace(this RectTransform.Edge edge,  RectTransform rt, RectTransform targetRT)
    {
        // our root edges must be opposite to the main edge we are trying to move
        RectTransform.Edge targetEdge = GetOppositeEdge(edge);
      

        //get target position and reset pivot
        float targetEdgePosition = GetEdgeInCanvasSpace(targetEdge,  targetRT, true);

        SetEdgeInCanvasSpace(edge,  rt, targetEdgePosition);
    }

    public static void SetEdgeInCanvasSpace(this RectTransform.Edge edge,  RectTransform rt,  float target)
    {
        // the edge we pivot to and grow from
        RectTransform.Edge rootEdge = GetOppositeEdge(edge);
       

        // the position to calculate the difference and grow the edge from
        float root = GetEdgeInCanvasSpace(rootEdge,  rt, false);
        SetPivotOfEdge(rt, rootEdge);

        float difference = Mathf.Abs(target - root);

        // set size and pivot to extend edge to target position;
        if (edge == RectTransform.Edge.Bottom || edge == RectTransform.Edge.Top) rt.sizeDelta = new Vector2(rt.sizeDelta.x, difference);
        else rt.sizeDelta = new Vector2(difference, rt.sizeDelta.y);

        //reset pivot
        ResetPivot(rt);
    }

    /// <summary>
    /// returns the Y position of top and bottom edges and the X position of left and right edges in canvas space
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="canvasRT"></param>
    /// <param name="rt"></param>
    /// <returns></returns>
    public static float GetEdgeInCanvasSpace(this RectTransform.Edge edge,  RectTransform rt, bool resetPivot = true)
    {
        
        //var canvasBounds = canvasRT.sizeDelta / 2;
        var canvasBounds = Vector2.zero;
       // var lastPivot = rt.pivot;
        Vector2 tempPivot = Vector2.zero;
        float solution = 0;

        switch (edge)
        {
            case RectTransform.Edge.Bottom:
                //set pivot to get accurate anchor position
                tempPivot = new Vector2(0.5f, 0f);
                SetPivot(rt, tempPivot);
                //to get canvas position, subtract anchored position - canvas rect /2 
                solution = rt.anchoredPosition.y - canvasBounds.y;
                break;
            case RectTransform.Edge.Top:
                //set pivot to get accurate anchor position
                tempPivot = new Vector2(0.5f, 1f);
                SetPivot(rt, tempPivot);
                //to get canvas position, subtract anchored position - canvas rect /2 
                solution = rt.anchoredPosition.y - canvasBounds.y;
                break;
            case RectTransform.Edge.Right:
                //set pivot to get accurate anchor position
                tempPivot = new Vector2(1f, 0.5f);
                SetPivot(rt, tempPivot);
                //to get canvas position, subtract anchored position - canvas rect /2 
                solution = rt.anchoredPosition.x - canvasBounds.x;
                
                //Debug.Log("anchoredPosition " + rt.anchoredPosition.x);
                //Debug.Log("bounds " + canvasBounds.x);
                break;
            case RectTransform.Edge.Left:
                //set pivot to get accurate anchor position
                tempPivot = new Vector2(0f, 0.5f);
                SetPivot(rt, tempPivot);
                //to get canvas position, subtract anchored position - canvas rect /2 
                solution = rt.anchoredPosition.x - canvasBounds.x;
                
                
                //Debug.Log("anchoredPosition " + rt.anchoredPosition.x);
                //Debug.Log("bounds " + canvasBounds.x);
                //Debug.Log("solution" + solution);
                break;
        }
        
        //set pivot back to what it was
        if(resetPivot)ResetPivot(rt);

        return solution;
    }

    /// <summary>
    /// Get the standard pivot values for an edge
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    static Vector2 GetPivotOfEdge(this RectTransform.Edge edge)
    {
        Vector2 pivot = Vector2.zero;
        switch (edge)
        {
            case RectTransform.Edge.Bottom:
                pivot = new Vector2(0.5f, 0f);
                break;
            case RectTransform.Edge.Top:
                pivot = new Vector2(0.5f, 1f);
                break;
            case RectTransform.Edge.Left:
                pivot = new Vector2(0f, 0.5f);
                break;
            case RectTransform.Edge.Right:
                pivot = new Vector2(1f, 0.5f);
                break;

        }
        return pivot;
    }
    
    /// <summary>
    ///  get an edges opposite
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static RectTransform.Edge GetOppositeEdge(this RectTransform.Edge edge)
    {
        RectTransform.Edge oppositeEdge = RectTransform.Edge.Bottom;
        // target edge and root edge should be opposite of the edge we want to move
        switch (edge)
        {

            case RectTransform.Edge.Bottom:
                oppositeEdge = RectTransform.Edge.Top;
                break;
            case RectTransform.Edge.Top:
                oppositeEdge = RectTransform.Edge.Bottom;
                break;
            case RectTransform.Edge.Left:
                oppositeEdge = RectTransform.Edge.Right;
                break;
            case RectTransform.Edge.Right:
                oppositeEdge = RectTransform.Edge.Left;
                break;

        }
        return oppositeEdge;
    }
    
    /// <summary>
    /// Set our pivot to an edge for a given rect transform
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="edge"></param>
    public static void SetPivotOfEdge(this RectTransform rt, RectTransform.Edge edge)
    {
        SetPivot(rt, GetPivotOfEdge(edge));
    }
   
    /// <summary>
    /// Sets pivot without adjusting position 
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="pivot"></param>
    public static void SetPivot(this RectTransform rt, Vector2 pivot)
    {
        Vector2 size = rt.rect.size;
        Vector2 deltaPivot = rt.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rt.pivot = pivot;
        rt.localPosition -= deltaPosition;
    }
    /// <summary>
    /// Reset Pivot to center
    /// </summary>
    /// <param name="rt"></param>
    static public void ResetPivot(this RectTransform rt)
    {
        SetPivot(rt, new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Set anchor to fixed (true) for movemement, or streched (false) when placed. Maintians size.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="state"></param>
    static public void SetAnchoringFixedWithSize(this RectTransform rt, bool state)
    {
        // unity will corrupt size of UI when changing anchor, so cache size and reset after changing anchoring

        //fixed
        if (state)
        {
            var strechedHeight = rt.rect.height;
            var stretchedWidth = rt.rect.width;

            rt.anchorMin = centerFixedAnchor;
            rt.anchorMax = centerFixedAnchor;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, stretchedWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, strechedHeight);
        }
        //Streched
        else
        {
            var fixedSize = rt.sizeDelta;

            rt.anchorMin = centerStretchedAnchorMin;
            rt.anchorMax = centerStretchedAnchorMax;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedSize.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fixedSize.y);
        }

    }
    
    static public void ResizeParentForChildren(RectTransform parent, Vector2 padding )
    {
        //RectTransform children = parent.GetComponentInChildren<RectTransform>();
        
        float min_x, max_x, min_y, max_y;
        min_x = max_x = parent.localPosition.x;
        min_y = max_y = parent.localPosition.y;

        foreach (RectTransform child in parent)
        {
            Vector2 scale = child.sizeDelta;
            float temp_min_x, temp_max_x, temp_min_y, temp_max_y;

            temp_min_x = child.localPosition.x - (scale.x / 2);
            temp_max_x = child.localPosition.x + (scale.x / 2);
            temp_min_y = child.localPosition.y - (scale.y / 2);
            temp_max_y = child.localPosition.y + (scale.y / 2);

            if (temp_min_x < min_x)
                min_x = temp_min_x;
            if (temp_max_x > max_x)
                max_x = temp_max_x;

            if (temp_min_y < min_y)
                min_y = temp_min_y;
            if (temp_max_y > max_y)
                max_y = temp_max_y;
            
        }
        Debug.Log("Resize for children ");

        // apply padding
        max_x += padding.x;
        min_x -= padding.x;
        max_y += padding.y;
        min_y -= padding.y;

        parent.sizeDelta = new Vector2(max_x - min_x, max_y - min_y);
    }
    
}

