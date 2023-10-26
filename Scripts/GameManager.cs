using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    //�̱��� ���� ����
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject gameManagerObject = new GameObject("GameManager");
                    instance = gameManagerObject.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    /*���� �����*/
    public PoolManager poolManager;
    public MouseManager mouseManager;

    private int[,] board;
    private int boardSizeX;
    private int boardSizeY;
    public Tilemap tilemap;
    private BoundsInt bounds;

    private int turn = 0;
    private bool gameover = false;

    private void Awake()
    {
        // ��Ÿ �ʱ�ȭ �۾�
        InitBoard(); 
    }

    //������Ʈ Ǯ�� ������ �ʱ�ȭ �Լ�
    private void InitBoard()
    {
        bounds = tilemap.cellBounds;
        boardSizeX = bounds.size.x;
        boardSizeY = bounds.size.y;
        board = new int[boardSizeX, boardSizeY];

        for (int i = 0; i < boardSizeX; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                board[i, j] = -1;
            }
        }
    }

    // �¸� ���� �˻� �Լ�
    public int CheckWin()
    {
        // ����, ����, �밢��(���� ��ܿ��� ������ �ϴ�����), �밢��(���� �ϴܿ��� ������ �������) �������� �˻��մϴ�.
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int currentPlayer = board[x, y];
                if (currentPlayer == -1) continue; // �� ���� ��ŵ

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 1; // ���� ��ġ���� �����ϴ� ���� ���� (�ڱ� �ڽ�)

                    // �ش� �������� �������� ���� ������ ����.
                    for (int step = 1; step < 5; step++)
                    {
                        int newX = x + step * dx[direction];
                        int newY = y + step * dy[direction];

                        if (newX >= 0 && newX < boardSizeX && newY >= 0 && newY < boardSizeY && board[newX, newY] == currentPlayer)
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (count == 5)
                    {
                        gameover = true;
                        return currentPlayer; // 5���� ���ӵ� ���� �ִ� ��� �¸�
                    }
                }
            }
        }
        return -1; // ���� �¸��ڰ� ���� ���
    }

    public bool IsGameOver()
    {
        return gameover;
    }


    /*�� �ְ� �޴� �Լ��� �����*/
    public int[] GetBoardSize()
    {
        int[] boardSize = { boardSizeX, boardSizeY };
        return boardSize;
    }

    public int GetBoardValue(int x, int y)
    {
        if (x >= 0 && x < boardSizeX && y >= 0 && y < boardSizeY)
        {
            return board[x, y];
        }
        return -1; // ��ȿ���� ���� �ε���
    }

    public void SetBoardValue(int x, int y, int value)
    {
        if (x >= 0 && x < boardSizeX && y >= 0 && y < boardSizeY)
        {
            board[x, y] = value;
        }
    }

    public int GetCurrentPlayer()
    {
        return turn;
    }

    public void SetCurrentPlayer(int value)
    {
        turn = value;
    }

    public Tilemap GetTileMap()
    {
        return tilemap;
    }
}
