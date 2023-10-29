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
        //�ʱ�ȭ �۾�
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

    //������ �ʱ�ȭ �Լ�
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

    //���� ����
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

                string temp = "<b>������ ���۵Ǿ����ϴ�.</b>";
                networkManager.pv.RPC("NoticeRPC", RpcTarget.All, temp);

                SetCurrentPlayer(0);
            }
            else
            {
                string temp = "<color=#FF0000FF><b>������ �������Դϴ�.</b></color>";
                networkManager.SendMessage(temp, networkManager.noticeText);
            }
            
        }
        else if (!IsMasterClient())
        {
            string temp = "<color=#FF0000FF><b>������ �ƴմϴ�.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
        else
        {
            string temp = "<color=#FF0000FF><b>ȥ�� ������ ������ �� �����ϴ�.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
    }

    //���� ����
    public void GameOver(string msg)
    {
        pv.RPC("SyncOver", RpcTarget.All);
        Room room = PhotonNetwork.CurrentRoom;
        room.CustomProperties["GameState"] = !gameover;
        room.SetCustomProperties(room.CustomProperties);

        networkManager.pv.RPC("NoticeRPC", RpcTarget.All, msg);
    }


    // �¸� ���� �˻� �Լ�
    public int CheckWin()
    {
        // ����, ����, �밢��(���� ��ܿ��� ������ �ϴ�����), �밢��(���� �ϴܿ��� ������ �������) �������� �˻��Ѵ�.
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int CheckStone = board[x, y];
                if (CheckStone == -1) continue; // �� ���� ��ŵ

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 1; // ���� ��ġ���� �����ϴ� ���� ���� (�ڱ� �ڽ�)

                    // �ش� �������� �������� ���� ������ ����.
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
                        return CheckStone; // 5���� ���ӵ� ���� �ִ� ��� �¸�
                    }
                }
            }
        }
        return -1; // ���� �¸��ڰ� ���� ���
    }


    //�� ���� �Լ�
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    //�� �ٲٱ� �Լ�
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
                string temp = "<color=#FF0000FF><b>������ �������Դϴ�.</b></color>";
                networkManager.SendMessage(temp, networkManager.noticeText);
            }
        }
        else if(!IsMasterClient())
        {
            string temp = "<color=#FF0000FF><b>������ �ƴմϴ�.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
        else
        {
            string temp = "<color=#FF0000FF><b>ȥ�� ���� �ٲ� �� �����ϴ�.</b></color>";
            networkManager.SendMessage(temp, networkManager.noticeText);
        }
    }


    /*�� �ְ� �޴� �Լ�*/
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
        return -1; // ��ȿ���� ���� �ε���
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
