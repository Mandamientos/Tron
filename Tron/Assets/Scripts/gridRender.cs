using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gridRender : MonoBehaviour
{
    public int cols = 100;
    public int rows = 100;
    public float cellSize = 100f;
    public LineRenderer lineRenderer;
    void Start()
    {
        drawGrid();
    }

    void drawGrid() {
        lineRenderer.positionCount = (cols + 1) * 2 + (rows + 1) * 2;
        Vector3[] positions = new Vector3[lineRenderer.positionCount];

        int index = 0;

        // Draw horizontal lines
        for (int i = 0; i <= rows; i++)
        {
            positions[index++] = new Vector3(0, i * cellSize, 0);
            positions[index++] = new Vector3(cols * cellSize, i * cellSize, 0);
        }

        // Draw vertical lines
        for (int j = 0; j <= cols; j++)
        {
            positions[index++] = new Vector3(j * cellSize, 0, 0);
            positions[index++] = new Vector3(j * cellSize, rows * cellSize, 0);
        }

        lineRenderer.SetPositions(positions);
    }
}
