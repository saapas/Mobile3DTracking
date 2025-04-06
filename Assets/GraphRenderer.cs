using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RealtimeGraphRenderer : MonoBehaviour // Class is used for the graph rendering, it was made with DeepSeek AI. 
// May or may not end up in the final version of the project
{
    public RectTransform graphContainer;
    public Sprite pointSprite;
    public Color lineColor = Color.white;
    public float lineThickness = 2f;
    public int maxDataPoints = 20; // Maximum number of points to display
    public float timeWindow = 10f; // Time window in seconds to display

    public GameObject gridLinePrefab; // Prefab for grid lines
    public GameObject labelPrefab; // Prefab for labels
    public int yAxisLabelCount = 5; // Number of labels on the Y-axis
    public int xAxisLabelCount = 5; // Number of labels on the X-axis

    private Queue<(float time, float value)> dataPoints = new Queue<(float, float)>();
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> labels = new List<GameObject>();

    private float minValue = 0f; // Minimum Y value
    private float maxValue = 0f; // Maximum Y value

    private void Start()
    {
        InvokeRepeating("AddDataPoint", 0f, 0.1f); // Add a point every 0.1 seconds
    }

    private void AddDataPoint()
    {
        float newValue = 5; //StepDetector.smoothedPitch; // Get the new Y-axis value
        float currentTime = Time.time; // Use the current time as the X-axis value

        // Add the new point to the queue
        dataPoints.Enqueue((currentTime, newValue));

        // Remove points that are outside the time window
        while (dataPoints.Count > 0 && currentTime - dataPoints.Peek().time > timeWindow)
        {
            dataPoints.Dequeue();
        }

        // Redraw the graph
        UpdateGraph();
    }

    private void UpdateGraph()
    {
        // Clear previous lines, points, grid lines, and labels
        ClearGraph();

        // Normalize data points to fit within the graph container
        Vector2[] normalizedPoints = NormalizeDataPoints(dataPoints);

        // Draw lines between points
        for (int i = 0; i < normalizedPoints.Length - 1; i++)
        {
            DrawLine(normalizedPoints[i], normalizedPoints[i + 1]);
        }

        // Draw points
        foreach (var point in normalizedPoints)
        {
            DrawPoint(point);
        }

        // Draw grid and labels
        DrawGridAndLabels();
    }

    private void ClearGraph()
    {
        foreach (var line in lines)
        {
            Destroy(line);
        }
        foreach (var point in points)
        {
            Destroy(point);
        }
        foreach (var gridLine in gridLines)
        {
            Destroy(gridLine);
        }
        foreach (var label in labels)
        {
            Destroy(label);
        }
        lines.Clear();
        points.Clear();
        gridLines.Clear();
        labels.Clear();
    }

    private Vector2[] NormalizeDataPoints(Queue<(float time, float value)> dataPoints)
    {
        Vector2[] normalizedPoints = new Vector2[dataPoints.Count];
        float maxTime = Time.time; // Current time
        float minTime = maxTime - timeWindow; // Oldest time to display

        // Find minimum and maximum Y values
        minValue = float.MaxValue;
        maxValue = float.MinValue;
        foreach (var point in dataPoints)
        {
            if (point.value < minValue) minValue = point.value;
            if (point.value > maxValue) maxValue = point.value;
        }

        // Ensure the Y-axis range is symmetric around 0
        float absMaxValue = Mathf.Max(Mathf.Abs(minValue), Mathf.Abs(maxValue));
        minValue = -absMaxValue;
        maxValue = absMaxValue;

        // Normalize points to fit within the graph container
        int index = 0;
        foreach (var point in dataPoints)
        {
            float normalizedX = (point.time - minTime) / timeWindow * graphContainer.rect.width;
            float normalizedY = (point.value - minValue) / (maxValue - minValue) * graphContainer.rect.height;
            normalizedPoints[index] = new Vector2(normalizedX, normalizedY);
            index++;
        }

        return normalizedPoints;
    }

    private void DrawLine(Vector2 pointA, Vector2 pointB)
    {
        GameObject line = new GameObject("Line", typeof(Image));
        line.transform.SetParent(graphContainer, false);
        line.GetComponent<Image>().color = lineColor;

        RectTransform rectTransform = line.GetComponent<RectTransform>();
        Vector2 direction = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(distance, lineThickness);
        rectTransform.anchoredPosition = pointA + direction * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        lines.Add(line);
    }

    private void DrawPoint(Vector2 position)
    {
        GameObject point = new GameObject("Point", typeof(Image));
        point.transform.SetParent(graphContainer, false);
        point.GetComponent<Image>().sprite = pointSprite;
        point.GetComponent<Image>().color = Color.red;

        RectTransform rectTransform = point.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(10, 10); // Size of the point

        points.Add(point);
    }

    private void DrawGridAndLabels()
    {
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;

        // Draw Y-axis grid lines and labels
        for (int i = 0; i <= yAxisLabelCount; i++)
        {
            float normalizedY = (float)i / yAxisLabelCount;
            float yPosition = normalizedY * graphHeight;

            // Draw grid line
            GameObject gridLine = Instantiate(gridLinePrefab, graphContainer);
            RectTransform gridLineRect = gridLine.GetComponent<RectTransform>();
            gridLineRect.anchorMin = new Vector2(0, normalizedY);
            gridLineRect.anchorMax = new Vector2(1, normalizedY);
            gridLineRect.sizeDelta = new Vector2(graphWidth, 1f);
            gridLineRect.anchoredPosition = Vector2.zero;
            gridLines.Add(gridLine);

            // Draw label
            GameObject label = Instantiate(labelPrefab, graphContainer);
            label.GetComponent<Text>().text = Mathf.Lerp(minValue, maxValue, normalizedY).ToString("F1"); // Adjust for your Y-axis range
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0); // Anchor to bottom-left
            labelRect.anchorMax = new Vector2(0, 0); // Anchor to bottom-left
            labelRect.anchoredPosition = new Vector2(-40f, yPosition); // Position to the left of the graph
            labels.Add(label);
        }

        // Draw X-axis grid lines and labels
        for (int i = 0; i <= xAxisLabelCount; i++)
        {
            float normalizedX = (float)i / xAxisLabelCount;
            float xPosition = normalizedX * graphWidth;

            // Draw grid line
            GameObject gridLine = Instantiate(gridLinePrefab, graphContainer);
            RectTransform gridLineRect = gridLine.GetComponent<RectTransform>();
            gridLineRect.anchorMin = new Vector2(normalizedX, 0);
            gridLineRect.anchorMax = new Vector2(normalizedX, 1);
            gridLineRect.sizeDelta = new Vector2(1f, graphHeight);
            gridLineRect.anchoredPosition = Vector2.zero;
            gridLines.Add(gridLine);

            // Draw label
            GameObject label = Instantiate(labelPrefab, graphContainer);
            label.GetComponent<Text>().text = Mathf.Lerp(0f, timeWindow, normalizedX).ToString("F1"); // Time labels
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0); // Anchor to bottom-left
            labelRect.anchorMax = new Vector2(0, 0); // Anchor to bottom-left
            labelRect.anchoredPosition = new Vector2(xPosition, -30f); // Position below the graph
            labels.Add(label);
        }
    }
}
