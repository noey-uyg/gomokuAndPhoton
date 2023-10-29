using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;
using Photon.Realtime;

public class MouseManager : MonoBehaviourPun
{
    GameManager gameManager;

    public PhotonView pv;
    private Tilemap tilemap;
    public GameObject[,] imagepool;
    public GameObject[] cursorImage;
    private Sprite[] cursorSprites;

    public int currentTurn = 0;

    int boardSizeX;
    int boardSizeY;

    private void Start()
    {
        //�ʱ�ȭ �۾�
        gameManager = GameManager.Instance;
        cursorSprites = new Sprite[cursorImage.Length];
        for (int i = 0; i < cursorImage.Length; i++)
        {
            cursorSprites[i] = cursorImage[i].GetComponent<SpriteRenderer>().sprite;
        }

        int[] board = gameManager.GetBoardSize();
        tilemap = gameManager.GetTileMap();
        boardSizeX = board[0];
        boardSizeY = board[1];

        imagepool = gameManager.poolManager.imagepool;

    }


    private void Update()
    {
        UpdateImage();
        Placement();

    }

    //����
    void Placement()
    {
        if (gameManager.IsGameOver()) return;

        currentTurn = gameManager.GetCurrentPlayer();

        if (gameManager.GetMyTurn() == currentTurn)
        {
            // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

            // Ÿ�ϸʿ� ���콺�� �ö����� ��
            if (tilemap.HasTile(cellPosition))
            {
                Vector2Int index = GetImageIndexByCellPosition(cellPosition);
                int indexX = index.x;
                int indexY = index.y;

                if (indexX >= 0 && indexY >= 0)
                {
                    // ���콺�� �ö� �ִ� ���� �̹����� Ȱ��ȭ
                    if (gameManager.GetBoardValue(indexX, indexY) == -1)
                    {
                        imagepool[indexX, indexY].GetComponent<SpriteRenderer>().sprite = cursorSprites[currentTurn];
                        imagepool[indexX, indexY].SetActive(true);
                    }

                    // Ŭ�� �� �� ����
                    if (Input.GetMouseButtonDown(0) && gameManager.GetBoardValue(indexX, indexY) == -1)
                    {
                        // Ŭ�� ��ġ�� �� ����
                        pv.RPC("SetGameTime", RpcTarget.All, 0f);

                        gameManager.SetBoardValue(indexX, indexY, currentTurn);

                        string stone = currentTurn == 0 ? "�浹" : "�鵹";
                        string temp = PhotonNetwork.NickName + "���� " + stone + "�� �����߽��ϴ�.";

                        gameManager.networkManager.pv.RPC("NoticeRPC", RpcTarget.All, temp);

                        // �� ����
                        int nextturn = (currentTurn + 1) % 2;
                        gameManager.SetCurrentPlayer(nextturn);

                        if (gameManager.CheckWin() != -1)
                        {
                            string tempWin = "<b>"+PhotonNetwork.NickName + "���� �¸��Ͽ����ϴ�.</b>";
                            gameManager.GameOver(tempWin);
  
                        }
                    }
                }
            }
        }
    }

    // �̹��� Ȱ��ȭ �� �̹��� Ǯ ����
    public void UpdateImage()
    {
        if (gameManager.IsGameOver())
        {
            for (int i = 0; i < boardSizeX; i++)
            {
                for (int j = 0; j < boardSizeY; j++)
                {
                    if (gameManager.GetBoardValue(i, j) == -1)
                    {
                        imagepool[i, j].SetActive(false);
                    }
                    else
                    {
                        imagepool[i, j].GetComponent<SpriteRenderer>().sprite = cursorSprites[gameManager.GetBoardValue(i, j)];
                        imagepool[i, j].SetActive(true);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < boardSizeX; i++)
            {
                for (int j = 0; j < boardSizeY; j++)
                {
                    if (gameManager.GetBoardValue(i, j) == -1)
                    {
                        imagepool[i, j].SetActive(false);
                    }
                    else
                    {
                        imagepool[i, j].GetComponent<SpriteRenderer>().sprite = cursorSprites[gameManager.GetMyTurn()];
                        imagepool[i, j].SetActive(true);
                    }
                }
            }   
        }
    } 


    // �̹����� �� ��ġ�� �����ϴ� �Լ�
    private Vector2Int GetImageIndexByCellPosition(Vector3Int cellPosition)
    {
        for (int i = 0; i < imagepool.GetLength(0); i++)
        {
            for (int j = 0; j < imagepool.GetLength(1); j++)
            {
                Vector3Int imageCellPosition = tilemap.WorldToCell(imagepool[i, j].transform.position);
                if (imageCellPosition == cellPosition)
                {
                    return new Vector2Int(i, j); // �̹����� �ε��� ��ȯ
                }
            }
        }
        return new Vector2Int(-1, -1); // �ش� �� ��ġ�� ��ġ�ϴ� �̹����� ����
    }


}
