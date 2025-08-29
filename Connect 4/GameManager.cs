using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> PlayerSwitched;
    public event Action<int, List<Vector2Int>> PlayerWon;
    public event Action GameDraw;

    [Header("Grid Settings")]
    [SerializeField] int rows = 6;
    [SerializeField] int columns = 7;
    private int[,] grid;
    private GameObject[,] pieceGrid;
    public GameObject[,] PieceGrid => pieceGrid;

    [Header("Pieces")]
    [SerializeField] GameObject piecePrefab;
    [SerializeField] float dropImpulse = 5f;
    [SerializeField] float dropLockTime = 0.5f;
    private bool isDropping;

    [Header("ColumnPoints")]
    [SerializeField] Transform[] columnPoints;
    public Transform[] ColumnPoints => columnPoints;

    [HideInInspector] public bool gameOver;
    [HideInInspector] public int currentPlayer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        InitializeGrid();
    }

    void InitializeGrid()
    {
        grid = new int[rows, columns];
        pieceGrid = new GameObject[rows, columns];
        gameOver = false;
        currentPlayer = 1;
    }

    public void PlacePiece(int column)
    {
        if (gameOver || isDropping) return;
        
        isDropping = true;

        int row = GetAvailableRow(column);
        if (row < 0) { Debug.Log("Column full"); return; }

        // Update model
        grid[row, column] = currentPlayer;

        // Spawn visual piece
        Vector3 spawnPos = columnPoints[column].position;
        GameObject piece = Instantiate(piecePrefab, spawnPos, Quaternion.identity);
        piece.GetComponent<Rigidbody>().AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
        pieceGrid[row, column] = piece;
        Material mat = UIManager.Instance.GetCurrentPlayerMaterial();
        foreach (var rend in piece.GetComponentsInChildren<Renderer>())
            rend.material = mat;

        if (CheckForWin(row, column, out var winCells))
        {
            gameOver = true;
            PlayerWon?.Invoke(currentPlayer, winCells);
            return;
        }

        if (IsBoardFull())
        {
            gameOver = true;
            GameDraw?.Invoke();
            return;
        }

        // Switch player and notify UI
        currentPlayer = 3 - currentPlayer;
        PlayerSwitched?.Invoke(currentPlayer);
        StartCoroutine(ResetDropLock());
    }

    public void DestroyPieces()
    {
        for (int r = 0; r < PieceGrid.GetLength(0); r++)
        {
            for (int c = 0; c < PieceGrid.GetLength(1); c++)
            {
                var piece = PieceGrid[r, c];
                if (piece == null) continue;
                Destroy(piece);
            }
        }
    }

    IEnumerator ResetDropLock()
    {
        yield return new WaitForSeconds(dropLockTime);
        isDropping = false;
    }

    int GetAvailableRow(int column)
    {
        for (int r = 0; r < rows; r++)
            if (grid[r, column] == 0) 
                return r;
        return -1;
    }

    bool IsBoardFull()
    {
        for (int c = 0; c < columns; c++)
            if (grid[rows - 1, c] == 0) 
                return false;
        return true;
    }

    public bool ColumnHasSpace(int column)
    {
        return GetAvailableRow(column) != -1;
    }

    private bool InBounds(int r, int c)
    {
        return r >= 0 && r < rows && c >= 0 && c < columns;
    }

    bool CheckForWin(int placedRow, int placedCol, out List<Vector2Int> winCells)
    {
        winCells = new List<Vector2Int>();
        Vector2Int[] dirs = {
            new Vector2Int(1, 0),   // horizontal
            new Vector2Int(0, 1),   // vertical
            new Vector2Int(1, 1),   // diag up-right
            new Vector2Int(1, -1)   // diag down-right
        };

        foreach (var d in dirs)
        {
            var temp = new List<Vector2Int> {
                new Vector2Int(placedRow, placedCol)
            };

            // forward up to 3 steps
            for (int i = 1; i < 4; i++)
            {
                int r = placedRow + d.x * i;
                int c = placedCol + d.y * i;
                if (InBounds(r, c) && grid[r, c] == currentPlayer)
                    temp.Add(new Vector2Int(r, c));
                else
                    break;
            }

            // backward up to 3 steps
            for (int i = 1; i < 4; i++)
            {
                int r = placedRow - d.x * i;
                int c = placedCol - d.y * i;
                if (InBounds(r, c) && grid[r, c] == currentPlayer)
                    temp.Add(new Vector2Int(r, c));
                else
                    break;
            }

            if (temp.Count >= 4)
            {
                winCells = temp.Take(4).ToList();
                return true;
            }
        }

        return false;
    }

}
