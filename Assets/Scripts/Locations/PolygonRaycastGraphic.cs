using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PolygonRaycastGraphic : Graphic, ICanvasRaycastFilter
{
    private readonly List<Vector2> normalizedPoints =
        new List<Vector2>();

    private bool hovered;
    private bool debugVisible;

    private Color normalFillColor =
        new Color(0.1f, 0.35f, 0.65f, 0.18f);

    private Color normalOutlineColor =
        new Color(0.3f, 0.7f, 1f, 0.75f);

    private Color hoverFillColor =
        new Color(0.2f, 0.65f, 1f, 0.32f);

    private Color hoverOutlineColor =
        new Color(0.65f, 0.9f, 1f, 1f);

    private Color debugFillColor =
        new Color(1f, 0.1f, 0.05f, 0.35f);

    private Color debugOutlineColor =
        new Color(1f, 0.85f, 0.05f, 1f);

    private float normalOutlineThickness = 2f;
    private float hoverOutlineThickness = 4f;
    private float debugOutlineThickness = 4f;

    public void SetPoints(IReadOnlyList<Vector2> points)
    {
        normalizedPoints.Clear();

        if (points != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
                normalizedPoints.Add(points[i]);
            }
        }

        raycastTarget = true;
        SetVerticesDirty();
    }

    public void SetNormalDisplay(
        Color fillColor,
        Color outlineColor,
        float outlineThickness)
    {
        normalFillColor = fillColor;
        normalOutlineColor = outlineColor;
        normalOutlineThickness =
            Mathf.Max(0f, outlineThickness);

        SetVerticesDirty();
    }

    public void SetHoverDisplay(
        Color fillColor,
        Color outlineColor,
        float outlineThickness)
    {
        hoverFillColor = fillColor;
        hoverOutlineColor = outlineColor;
        hoverOutlineThickness =
            Mathf.Max(0f, outlineThickness);

        SetVerticesDirty();
    }

    public void SetHovered(bool value)
    {
        if (hovered == value)
            return;

        hovered = value;
        SetVerticesDirty();
    }

    public void SetDebugDisplay(
        bool visible,
        Color fillColor,
        Color outlineColor,
        float outlineThickness)
    {
        debugVisible = visible;
        debugFillColor = fillColor;
        debugOutlineColor = outlineColor;
        debugOutlineThickness =
            Mathf.Max(0f, outlineThickness);

        SetVerticesDirty();
    }

    public bool IsRaycastLocationValid(
        Vector2 screenPoint,
        Camera eventCamera)
    {
        if (!isActiveAndEnabled)
            return false;

        if (normalizedPoints.Count < 3)
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                eventCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = rectTransform.rect;

        if (rect.width <= 0f ||
            rect.height <= 0f)
        {
            return false;
        }

        Vector2 normalizedPoint =
            new Vector2(
                Mathf.InverseLerp(
                    rect.xMin,
                    rect.xMax,
                    localPoint.x),
                Mathf.InverseLerp(
                    rect.yMin,
                    rect.yMax,
                    localPoint.y));

        return IsPointInsidePolygon(
            normalizedPoint);
    }

    protected override void OnPopulateMesh(
        VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        if (normalizedPoints.Count < 3)
            return;

        Rect rect = rectTransform.rect;

        if (rect.width <= 0f ||
            rect.height <= 0f)
        {
            return;
        }

        Color fillColor;
        Color outlineColor;
        float outlineThickness;

        if (debugVisible)
        {
            fillColor =
                debugFillColor;

            outlineColor =
                debugOutlineColor;

            outlineThickness =
                debugOutlineThickness;
        }
        else if (hovered)
        {
            fillColor =
                hoverFillColor;

            outlineColor =
                hoverOutlineColor;

            outlineThickness =
                hoverOutlineThickness;
        }
        else
        {
            fillColor =
                normalFillColor;

            outlineColor =
                normalOutlineColor;

            outlineThickness =
                normalOutlineThickness;
        }

        BuildFill(
            vertexHelper,
            rect,
            fillColor);

        BuildOutline(
            vertexHelper,
            rect,
            outlineColor,
            outlineThickness);
    }

    private void BuildFill(
        VertexHelper vertexHelper,
        Rect rect,
        Color fillColor)
    {
        if (fillColor.a <= 0f)
            return;

        Vector2 center =
            CalculatePolygonCenter();

        int centerIndex =
            vertexHelper.currentVertCount;

        AddVertex(
            vertexHelper,
            NormalizedToLocal(
                center,
                rect),
            fillColor);

        int firstPointIndex =
            vertexHelper.currentVertCount;

        for (int i = 0;
             i < normalizedPoints.Count;
             i++)
        {
            AddVertex(
                vertexHelper,
                NormalizedToLocal(
                    normalizedPoints[i],
                    rect),
                fillColor);
        }

        for (int i = 0;
             i < normalizedPoints.Count;
             i++)
        {
            int next =
                (i + 1) %
                normalizedPoints.Count;

            vertexHelper.AddTriangle(
                centerIndex,
                firstPointIndex + i,
                firstPointIndex + next);
        }
    }

    private void BuildOutline(
        VertexHelper vertexHelper,
        Rect rect,
        Color outlineColor,
        float thickness)
    {
        if (outlineColor.a <= 0f ||
            thickness <= 0f)
        {
            return;
        }

        for (int i = 0;
             i < normalizedPoints.Count;
             i++)
        {
            int next =
                (i + 1) %
                normalizedPoints.Count;

            Vector2 start =
                NormalizedToLocal(
                    normalizedPoints[i],
                    rect);

            Vector2 end =
                NormalizedToLocal(
                    normalizedPoints[next],
                    rect);

            AddLine(
                vertexHelper,
                start,
                end,
                thickness,
                outlineColor);
        }
    }

    private void AddLine(
        VertexHelper vertexHelper,
        Vector2 start,
        Vector2 end,
        float thickness,
        Color lineColor)
    {
        Vector2 direction =
            end - start;

        if (direction.sqrMagnitude <=
            Mathf.Epsilon)
        {
            return;
        }

        direction.Normalize();

        Vector2 perpendicular =
            new Vector2(
                -direction.y,
                direction.x);

        Vector2 offset =
            perpendicular *
            (thickness * 0.5f);

        int startIndex =
            vertexHelper.currentVertCount;

        AddVertex(
            vertexHelper,
            start - offset,
            lineColor);

        AddVertex(
            vertexHelper,
            start + offset,
            lineColor);

        AddVertex(
            vertexHelper,
            end + offset,
            lineColor);

        AddVertex(
            vertexHelper,
            end - offset,
            lineColor);

        vertexHelper.AddTriangle(
            startIndex,
            startIndex + 1,
            startIndex + 2);

        vertexHelper.AddTriangle(
            startIndex + 2,
            startIndex + 3,
            startIndex);
    }

    private void AddVertex(
        VertexHelper vertexHelper,
        Vector2 position,
        Color vertexColor)
    {
        UIVertex vertex =
            UIVertex.simpleVert;

        vertex.position = position;
        vertex.color = vertexColor;
        vertex.uv0 = Vector2.zero;

        vertexHelper.AddVert(vertex);
    }

    private Vector2 CalculatePolygonCenter()
    {
        Vector2 center =
            Vector2.zero;

        if (normalizedPoints.Count == 0)
            return center;

        for (int i = 0;
             i < normalizedPoints.Count;
             i++)
        {
            center +=
                normalizedPoints[i];
        }

        return center /
               normalizedPoints.Count;
    }

    private Vector2 NormalizedToLocal(
        Vector2 normalizedPoint,
        Rect rect)
    {
        return new Vector2(
            Mathf.Lerp(
                rect.xMin,
                rect.xMax,
                normalizedPoint.x),
            Mathf.Lerp(
                rect.yMin,
                rect.yMax,
                normalizedPoint.y));
    }

    private bool IsPointInsidePolygon(
        Vector2 point)
    {
        bool inside = false;

        int previousIndex =
            normalizedPoints.Count - 1;

        for (int currentIndex = 0;
             currentIndex < normalizedPoints.Count;
             currentIndex++)
        {
            Vector2 current =
                normalizedPoints[currentIndex];

            Vector2 previous =
                normalizedPoints[previousIndex];

            bool crossesHorizontalLine =
                (current.y > point.y) !=
                (previous.y > point.y);

            if (crossesHorizontalLine)
            {
                float denominator =
                    previous.y -
                    current.y;

                if (Mathf.Abs(denominator) >
                    Mathf.Epsilon)
                {
                    float intersectionX =
                        ((previous.x -
                          current.x) *
                         (point.y -
                          current.y) /
                         denominator) +
                        current.x;

                    if (point.x <
                        intersectionX)
                    {
                        inside = !inside;
                    }
                }
            }

            previousIndex =
                currentIndex;
        }

        return inside;
    }
}