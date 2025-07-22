using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Piece currentPiece { get; private set; }
    public Piece nextPiece { get; private set; }
    public Piece holdPiece { get; private set; }
    public Tilemap tilemap { get; private set; }
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int previewPosition = new Vector3Int(-1, 12, 0);
    public Vector3Int holdPosition = new Vector3Int(-1, 16, 0);
    public Vector2Int boardSize = new Vector2Int(BoardWidth, BoardHeight);
    private TakenPieceArray takenPiece;

    public const int BoardWidth = 10;
    public const int BoardHeight = 20;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        currentPiece = GetComponentInChildren<Piece>();
        nextPiece = gameObject.AddComponent<Piece>();
        nextPiece.enabled = false;
        holdPiece = gameObject.AddComponent<Piece>();
        holdPiece.enabled = false;

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        takenPiece = new TakenPieceArray();
        SetNextPiece();
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        if (currentPiece.hasPiece)
        {
            Clear(currentPiece);
        }
        
        currentPiece.Initialize(this, spawnPosition, nextPiece.data);

        if (IsValidPosition(currentPiece, spawnPosition))
        {
            Set(currentPiece);
        }
        else
        {
            GameOver();
            return; // Exit if game over
        }

        SetNextPiece();
    }

    /// <summary>
    /// Helper method to initialize the next Piece with a random piece, and initializes it
    /// in the previewPosition.
    /// </summary>
    private void SetNextPiece()
    {
        // Clear the existing piece from the board
        if (nextPiece.cells != null)
        {
            Clear(nextPiece);
        }

        // Pick a random tetromino to use
        TetrominoData data = GetRandomPiece();

        // Initialize the next piece with the random data
        // Draw it at the "preview" position on the board
        nextPiece.Initialize(this, previewPosition, data);
        Set(nextPiece);
    }

    private TetrominoData GetRandomPiece()
    {
        if (takenPiece.Count() == 7)
            takenPiece.Clear();
        int random = Random.Range(0, tetrominoes.Length);

        while (takenPiece.Contains(random))
            random = Random.Range(0, tetrominoes.Length);

        TetrominoData data = tetrominoes[random];
        takenPiece.Add(random);
        return data;
    }

    private void GameOver()
    {
        // make it show a game over screen (scene) with retry and back to main menu button?
        // for now ill make it print log
        Debug.Log("Game Over");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Draws the given Piece in the game
    /// Example: If the given Piece is I, it uses (-1, 1), (0, 1), (1, 1), and (2, 1)
    /// it loops through that cell's array, gets the position and sets each tile with the passed in tile
    /// in this case, the passed in tile is the cyan block, since cyan block usually represents the I block.
    /// </summary>
    /// <param name="piece">The Piece to draw</param>
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            if (tilemap.HasTile(tilePosition))
                return false;
            if (!bounds.Contains((Vector2Int)tilePosition))
                return false;
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(position))
                return false;
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase tileAbove = tilemap.GetTile(position);
                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, tileAbove);
            }

            row++;
        }
    }

    public void SwapPiece()
    {
        // First, clear the current piece from the board
        Clear(currentPiece);

        // Scenario 1: There is already a piece in hold.
        if (holdPiece.hasPiece)
        {
            // Clear the existing hold piece from the board
            Clear(holdPiece);

            // Temporarily store the data of the current piece
            TetrominoData tempCurrentPieceData = currentPiece.data;

            // Initialize currentPiece with the data from holdPiece
            currentPiece.Initialize(this, spawnPosition, holdPiece.data);

            // Initialize holdPiece with the data from the previously current piece
            holdPiece.Initialize(this, holdPosition, tempCurrentPieceData);

            // Set both the new current piece and the new hold piece on the board
            Set(currentPiece);
            Set(holdPiece);
        }
        // Scenario 2: There is NO piece in hold.
        else
        {
            // Initialize holdPiece with the data from the current piece
            holdPiece.Initialize(this, holdPosition, currentPiece.data);
            Set(holdPiece); // Draw the new hold piece

            // Spawn a completely new piece (which will use the 'nextPiece' and generate a new 'nextPiece')
            SpawnPiece();
        }
    }
}