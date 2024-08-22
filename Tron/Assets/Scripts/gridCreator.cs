using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node {
    public Vector2 pos;
    public Node Up, Down, Left, Right;

    public Node(Vector2 pos) {
        this.pos = pos;
        Up = Down = Left = Right = null;
    }
}

public class gridCreator : MonoBehaviour
{
    public int cols = 100;
    public int rows = 100;
    public float cellSize = 1f;
    public float nodeRadius = 1f;
    public Node[,] grid;
    void Start()
    {
        createGrid();
    }

    void createGrid() {
        grid = new Node[rows, cols];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector2 pos = new Vector2(j * cellSize, i * cellSize);
                grid[i, j] = new Node(pos);
            }
        }

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                if (i > 0) {
                    grid[i, j].Down = grid[i - 1, j];
                }
                if (j < cols - 1) {
                    grid[i, j].Right = grid[i, j + 1];
                }
                if (i < rows - 1) {
                    grid[i, j].Up = grid[i + 1, j];
                }
                if (j > 0) {
                    grid[i, j].Left = grid[i, j - 1];
                }
            }
        }
    }

    public Node obtainNode (Vector2 pos) {
        int col = Mathf.RoundToInt(pos.x / cellSize);
        int row = Mathf.RoundToInt(pos.y / cellSize);

        if (col >= 0 && row >= 0 && col < cols && row < rows) {
            return grid[col, row];
        }
        return null;

    }
}
