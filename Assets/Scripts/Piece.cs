using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public Vector3Int position { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    // TETRIS ARR AND DAS VARIABLES
    private bool isDirectionKeyHeld = false;
    private float dasTimer;
    private float arrTimer;
    private bool dasCompleted;
    private Vector2 lastInput = Vector2.zero;

    // default puyo tetris 2 value
    // source: https://www.terasol.co.jp/%E3%83%97%E3%83%AD%E3%82%B0%E3%83%A9%E3%83%9F%E3%83%B3%E3%82%B0/6892
    public float arrMs = 33f;
    public float dasMs = 183f;
    public float arrSeconds;
    public float dasSeconds;

    private float stepTime;
    private float lockTime;

    InputAction moveAction;
    InputAction hardDropAction;
    InputAction rotateAction;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        lockTime = 0f;

        arrSeconds = arrMs / 1000f;
        dasSeconds = dasMs / 1000f;

        if (cells == null)
            cells = new Vector3Int[data.cells.Length];

        for (int i = 0; i < data.cells.Length; i++)
            cells[i] = (Vector3Int)data.cells[i];
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        hardDropAction = InputSystem.actions.FindAction("HardDrop");
        rotateAction = InputSystem.actions.FindAction("Rotate");
    }

    private void Update()
    {
        board.Clear(this);
        lockTime += Time.deltaTime;

        Vector2 moveValue = moveAction.ReadValue<Vector2>();

// Detect direction switch or new press
        bool directionChanged = moveValue.x != 0 && moveValue.x != lastInput.x;

        if (directionChanged || moveAction.WasPressedThisFrame())
        {
            if (moveValue.x < 0)
            {
                Move(Vector2Int.left);
                lastInput = Vector2.left;
            }
            else if (moveValue.x > 0)
            {
                Move(Vector2Int.right);
                lastInput = Vector2.right;
            }

            if (moveValue.y < 0)
            {
                Move(Vector2Int.down);
                lastInput = Vector2.down;
            }

            // Reset DAS/ARR after immediate move
            dasTimer = 0f;
            arrTimer = 0f;
            dasCompleted = false;
            isDirectionKeyHeld = true;

            board.Set(this);
        }

// Handle release
        if (moveValue.x == 0 && lastInput.x != 0)
        {
            lastInput = Vector2.zero;
            dasTimer = 0f;
            arrTimer = 0f;
            dasCompleted = false;
            isDirectionKeyHeld = false;
        }


// DAS/ARR logic
        if (isDirectionKeyHeld && lastInput.x != 0)
        {
            if (!dasCompleted)
            {
                dasTimer += Time.deltaTime;
                if (dasTimer >= dasSeconds)
                {
                    dasCompleted = true;
                    arrTimer = arrSeconds; // trigger first repeat immediately
                }
            }
            else
            {
                arrTimer += Time.deltaTime;
                if (arrTimer >= arrSeconds)
                {
                    bool moved = Move(Vector2Int.RoundToInt(lastInput));
                    if (moved)
                    {
                        arrTimer = 0f;
                    }
                }
            }
        }


        if (hardDropAction.WasPressedThisFrame())
        {
            HardDrop();
        }

        if (rotateAction.WasPressedThisFrame())
        {
            float value = rotateAction.ReadValue<float>();
            // 1 is clockwise, -1 is counterclockwise
            if (value is 1)
                Rotate(1);
            else if (value is 2)
                Rotate(-1);
        }

        if (Time.time >= stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);
        if (lockTime >= lockDelay)
            Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Rotate(int value)
    {
        int originalRotation = rotationIndex;
        rotationIndex += Wrap(rotationIndex + value, 0, 4);
        ApplyRotate(value);

        if (!TestWallKicks(rotationIndex, value))
        {
            rotationIndex = originalRotation;
            ApplyRotate(-value);
        }
    }

    private void ApplyRotate(int value)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];
            int x;
            int y;
            switch (data.tetromino)
            {
                case (Tetromino.I):
                case (Tetromino.O):
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * value) +
                                        (cell.y * Data.RotationMatrix[1] * value));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * value) +
                                        (cell.y * Data.RotationMatrix[3] * value));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * value) +
                                         (cell.y * Data.RotationMatrix[1] * value));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * value) +
                                         (cell.y * Data.RotationMatrix[3] * value));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
            return max - (min - input) % (max - min);
        else
            return min + (input - min) % (max - min);
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
            wallKickIndex--;
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position + (Vector3Int)translation;
        bool valid = board.IsValidPosition(this, newPosition);

        if (valid && newPosition != position)
        {
            position = newPosition;
            lockTime = 0f;
            return true;
        }

        return false;
    }
}