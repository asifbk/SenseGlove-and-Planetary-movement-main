using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UIBarGraphWithLabels : MaskableGraphic
{
    [Header("Bar Colors")]
    public Color thumbColor = Color.red;
    public Color indexColor = Color.green;
    public Color wristColor = Color.blue;
    public Color totalColor = Color.yellow;

    [Header("Grid Settings")]
    public Color gridColor = new Color(1, 1, 1, 0.2f);
    public int gridX = 10;
    public int gridY = 5;

    [Header("Labels")]
    public Font labelFont;
    public int labelFontSize = 14;
    public Color labelColor = Color.white;

    [Header("Graph Settings")]
    public int maxPoints = 10;
    public float scrollSpeed = 10f;

    private List<float> thumbValues = new List<float>();
    private List<float> indexValues = new List<float>();
    private List<float> wristValues = new List<float>();
    private List<float> totalValues = new List<float>();
    private float barOffset = 0f;

    protected override void Awake()
    {
        if (color.a == 0) color = new Color(0, 0, 0, 0.001f);
    }

    /// <summary>
    /// Add vibration data. If all values are zero, the graph will still scroll showing zeros.
    /// </summary>
    public void AddData(float thumb, float index, float wrist)
    {
        if (float.IsNaN(thumb) || float.IsNaN(index) || float.IsNaN(wrist))
        {
            ClearData();
            return;
        }

        thumb = Mathf.Max(0f, thumb);
        index = Mathf.Max(0f, index);
        wrist = Mathf.Max(0f, wrist);

        float total = thumb + index + wrist;

        thumbValues.Add(thumb);
        indexValues.Add(index);
        wristValues.Add(wrist);
        totalValues.Add(total);

        if (thumbValues.Count > maxPoints) thumbValues.RemoveAt(0);
        if (indexValues.Count > maxPoints) indexValues.RemoveAt(0);
        if (wristValues.Count > maxPoints) wristValues.RemoveAt(0);
        if (totalValues.Count > maxPoints) totalValues.RemoveAt(0);

        SetVerticesDirty();
    }

    public void ClearData()
    {
        thumbValues.Clear();
        indexValues.Clear();
        wristValues.Clear();
        totalValues.Clear();
        SetVerticesDirty();
    }

    private void Update()
    {
        barOffset += scrollSpeed * Time.deltaTime;
        if (barOffset >= rectTransform.rect.width / maxPoints)
            barOffset = 0f;

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        DrawGrid(vh);

        int totalBars = 4; // Thumb, Index, Wrist, Total

        DrawBars(vh, thumbValues, thumbColor, 0, totalBars);
        DrawBars(vh, indexValues, indexColor, 1, totalBars);
        DrawBars(vh, wristValues, wristColor, 2, totalBars);
        DrawBars(vh, totalValues, totalColor, 3, totalBars);

        DrawLabels();
    }

    private void DrawGrid(VertexHelper vh)
    {
        Vector2 size = rectTransform.rect.size;
        for (int i = 0; i <= gridX; i++)
        {
            float x = (i / (float)gridX) * size.x;
            AddUILine(vh, new Vector2(x, 0), new Vector2(x, size.y), gridColor, 1f);
        }
        for (int j = 0; j <= gridY; j++)
        {
            float y = (j / (float)gridY) * size.y;
            AddUILine(vh, new Vector2(0, y), new Vector2(size.x, y), gridColor, 1f);
        }
    }

    private void DrawBars(VertexHelper vh, List<float> values, Color color, int barPositionIndex, int totalBars)
    {
        if (values.Count == 0) return;

        Vector2 size = rectTransform.rect.size;
        float pointWidth = size.x / maxPoints;
        float barWidth = pointWidth / totalBars;

        for (int i = 0; i < values.Count; i++)
        {
            float xCenter = i * pointWidth - barOffset + pointWidth / 2;
            xCenter += (barPositionIndex - (totalBars - 1) / 2f) * barWidth;

            float height = Mathf.Clamp01(values[i]); // ensure 0-1 range
            Vector2 p1 = new Vector2(xCenter - barWidth / 2, 0);
            Vector2 p2 = new Vector2(xCenter + barWidth / 2, height * size.y);

            AddRect(vh, p1, p2, color);
        }
    }

    private void DrawLabels()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Label_"))
                Destroy(child.gameObject);
        }

        Vector2 size = rectTransform.rect.size;

        for (int j = 0; j <= gridY; j++)
        {
            float y = (j / (float)gridY) * size.y;
            CreateText("Label_Y" + j, new Vector2(-25, y), (j / (float)gridY).ToString("0.0"));
        }
    }

    private void CreateText(string name, Vector2 anchoredPos, string text)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(50, 20);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Text txt = go.AddComponent<Text>();
        txt.font = labelFont != null ? labelFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = labelFontSize;
        txt.color = labelColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = text;
    }

    private void AddRect(VertexHelper vh, Vector2 p1, Vector2 p2, Color color)
    {
        int index = vh.currentVertCount;
        vh.AddVert(new Vector2(p1.x, p1.y), color, Vector2.zero);
        vh.AddVert(new Vector2(p1.x, p2.y), color, Vector2.zero);
        vh.AddVert(new Vector2(p2.x, p2.y), color, Vector2.zero);
        vh.AddVert(new Vector2(p2.x, p1.y), color, Vector2.zero);
        vh.AddTriangle(index + 0, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index + 0);
    }

    private void AddUILine(VertexHelper vh, Vector2 p1, Vector2 p2, Color color, float thickness)
    {
        Vector2 dir = (p2 - p1).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * thickness;
        int index = vh.currentVertCount;
        vh.AddVert(p1 - perp, color, Vector2.zero);
        vh.AddVert(p1 + perp, color, Vector2.zero);
        vh.AddVert(p2 + perp, color, Vector2.zero);
        vh.AddVert(p2 - perp, color, Vector2.zero);
        vh.AddTriangle(index + 0, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index + 0);
    }
}
