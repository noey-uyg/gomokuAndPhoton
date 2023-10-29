using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
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
    public NetworkManager networkManager;

    public PhotonView pv;

    private int[,] board;
    private int boardSizeX;
    private int boardSizeY;
    public float placeMentTime = 3 * 10f;
    public float gameTime = 0f;

    public Tilemap tilemap;
    private BoundsInt bounds;

    private int myTurn;
    private int currentPlayer = 0;
    private bool gameover = true;

    private void Awake()
    {
        //초기화 작업
        Screen.SetResolution(1024, 768, false);

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            InitBoard();
            myTurn = IsMasterClient() ? 0 : 1;
        }
    }

    private void Update()
    {
        if (gameover) return;
        gameTime += Time.deltaTime;

        if (gameTime > placeMentTime)
        {
            pv.RPC("SetGameTime", RpcTarget.All, 0f);
            int nextturn = (currentPlayer + 1) % 2;
            SetCurrentPlayer(nextturn);
        }
    }

    //보드판 초기화 함수
    [PunRPC]
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

    //게임 시작
    public void GameStart()
    {
        if (IsMasterClient() && PhotonNetwork.CurrentRoom.PlayerCount != 1)
        {
            if (gameover)
            {
                pv.RPC("InitBoard", RpcTarget.All);

                pv.RPC("SyncStart", RpcTarget.All);

                Room room = PhotonNetwork.CurrentRoom;
                room.CustomProperties["GameState"] = !gameover;
                room.SetCustomProperties(room.CustomProperties);

                string temp = "<b>게임이 시작되었습니다.</b>";
                networkManager.pv.RPC("NoticeRPC", RpcTarget.All, temp);

                SetCurrentPlayer(0);
            }
            else
            {
                string temp = "<color=#FF0000FF><b>게임이 진행중입니다.</b></color>";
                networkManager.SendMessage(temp, networkManager.noticeText);
            }
            
        }
        else if (!IsMasterClient())
        {
            string temp = "<color=#FF0000FF><b>방장이 아닙니다.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
        else
        {
            string temp = "<color=#FF0000FF><b>혼자 게임을 시작할 수 없습니다.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
    }

    //게임 종료
    public void GameOver(string msg)
    {
        pv.RPC("SyncOver", RpcTarget.All);
        Room room = PhotonNetwork.CurrentRoom;
        room.CustomProperties["GameState"] = !gameover;
        room.SetCustomProperties(room.CustomProperties);

        networkManager.pv.RPC("NoticeRPC", RpcTarget.All, msg);
    }


    // 승리 조건 검사 함수
    public int CheckWin()
    {
        // 가로, 세로, 대각선(왼쪽 상단에서 오른쪽 하단으로), 대각선(왼쪽 하단에서 오른쪽 상단으로) 방향으로 검사한다.
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int CheckStone = board[x, y];
                if (CheckStone == -1) continue; // 빈 셀은 스킵

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 1; // 현재 위치에서 시작하는 돌의 개수 (자기 자신)

                    // 해당 방향으로 연속적인 돌의 개수를 센다.
                    for (int step = 1; step < 5; step++)
                    {
                        int newX = x + step * dx[direction];
                        int newY = y + step * dy[direction];

                        if (newX >= 0 && newX < boardSizeX && newY >= 0 && newY < boardSizeY && board[newX, newY] == CheckStone)
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (count >= 5)
                    {
                        return CheckStone; // 5개의 연속된 돌이 있는 경우 승리
                    }
                }
            }
        }
        return -1; // 아직 승리자가 없는 경우
    }


    //씬 변경 함수
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    //돌 바꾸기 함수
    public void ChangeMyTurn()
    {
        if (IsMasterClient() && PhotonNetwork.CurrentRoom.PlayerCount != 1)
        {
            if (gameover)
            {
                pv.RPC("SetMyTurn", RpcTarget.All);
            }
            else
            {
                string temp = "<color=#FF0000FF><b>게임이 진행중입니다.</b></color>";
                networkManager.SendMessage(temp, networkManager.noticeText);
            }
        }
        else if(!IsMasterClient())
        {
            string temp = "<color=#FF0000FF><b>방장이 아닙니다.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
        else
        {
            string temp = "<color=#FF0000FF><b>혼자 돌을 바꿀 수 없습니다.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
    }


    /*값 주고 받는 함수*/
    public bool IsGameOver()
    {
        return gameover;
    }

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
            pv.RPC("SyncBoard", RpcTarget.All, x, y, value);
        }
    }

    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void SetCurrentPlayer(int value)
    {
        pv.RPC("SyncTurn", RpcTarget.All, value);
    }

    public Tilemap GetTileMap()
    {
        return tilemap;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public int GetMyTurn()
    {
        return myTurn;
    }

    [PunRPC]
    void TimerController()
    {
        gameTime += Time.deltaTime;
        pv.RPC("SetGameTime", RpcTarget.All, gameTime);

        if (gameTime > placeMentTime)
        {
            pv.RPC("SetGameTime", RpcTarget.All, 0f);
            int nextturn = (currentPlayer + 1) % 2;
            SetCurrentPlayer(nextturn);
        }
    }

    [PunRPC]
    void SetGameTime(float gt)
    {
        gameTime = gt;
    }

    [PunRPC]
    void SyncBoard(int x, int y, int value)
    {
        board[x, y] = value;
    }

    [PunRPC]
    void SyncTurn(int turn)
    {
        currentPlayer = turn;
    }

    [PunRPC]
    void SyncStart()
    {
        gameover = false;
    }

    [PunRPC]
    void SyncOver()
    {
        gameover = true;
    }

    [PunRPC]
    void SetMyTurn()
    {
        myTurn = myTurn == 0 ? 1 : 0;
    }




}
