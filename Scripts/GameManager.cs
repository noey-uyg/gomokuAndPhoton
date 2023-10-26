using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    //싱글톤 패턴 적용
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

    /*변수 선언부*/
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
        // 기타 초기화 작업
        InitBoard(); 
    }

    //오브젝트 풀과 보드판 초기화 함수
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

    // 승리 조건 검사 함수
    public int CheckWin()
    {
        // 가로, 세로, 대각선(왼쪽 상단에서 오른쪽 하단으로), 대각선(왼쪽 하단에서 오른쪽 상단으로) 방향으로 검사합니다.
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int currentPlayer = board[x, y];
                if (currentPlayer == -1) continue; // 빈 셀은 스킵

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 1; // 현재 위치에서 시작하는 돌의 개수 (자기 자신)

                    // 해당 방향으로 연속적인 돌의 개수를 센다.
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
                        return currentPlayer; // 5개의 연속된 돌이 있는 경우 승리
                    }
                }
            }
        }
        return -1; // 아직 승리자가 없는 경우
    }

    public bool IsGameOver()
    {
        return gameover;
    }


    /*값 주고 받는 함수들 선언부*/
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
        return -1; // 유효하지 않은 인덱스
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
