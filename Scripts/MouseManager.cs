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
        //초기화 작업
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

    //착수
    void Placement()
    {
        if (gameManager.IsGameOver()) return;

        currentTurn = gameManager.GetCurrentPlayer();

        if (gameManager.GetMyTurn() == currentTurn)
        {
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

            // 타일맵에 마우스가 올라가있을 때
            if (tilemap.HasTile(cellPosition))
            {
                Vector2Int index = GetImageIndexByCellPosition(cellPosition);
                int indexX = index.x;
                int indexY = index.y;

                if (indexX >= 0 && indexY >= 0)
                {
                    // 마우스가 올라가 있는 셀의 이미지를 활성화
                    if (gameManager.GetBoardValue(indexX, indexY) == -1)
                    {
                        imagepool[indexX, indexY].GetComponent<SpriteRenderer>().sprite = cursorSprites[currentTurn];
                        imagepool[indexX, indexY].SetActive(true);
                    }

                    // 클릭 시 턴 변경
                    if (Input.GetMouseButtonDown(0) && gameManager.GetBoardValue(indexX, indexY) == -1)
                    {
                        // 클릭 위치에 돌 놓기
                        pv.RPC("SetGameTime", RpcTarget.All, 0f);

                        gameManager.SetBoardValue(indexX, indexY, currentTurn);

                        string stone = currentTurn == 0 ? "흑돌" : "백돌";
                        string temp = PhotonNetwork.NickName + "님이 " + stone + "을 착수했습니다.";

                        gameManager.networkManager.pv.RPC("NoticeRPC", RpcTarget.All, temp);

                        // 턴 변경
                        int nextturn = (currentTurn + 1) % 2;
                        gameManager.SetCurrentPlayer(nextturn);

                        if (gameManager.CheckWin() != -1)
                        {
                            string tempWin = "<b>"+PhotonNetwork.NickName + "님이 승리하였습니다.</b>";
                            gameManager.GameOver(tempWin);
  
                        }
                    }
                }
            }
        }
    }

    // 이미지 활성화 및 이미지 풀 관리
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


    // 이미지와 셀 위치를 연결하는 함수
    private Vector2Int GetImageIndexByCellPosition(Vector3Int cellPosition)
    {
        for (int i = 0; i < imagepool.GetLength(0); i++)
        {
            for (int j = 0; j < imagepool.GetLength(1); j++)
            {
                Vector3Int imageCellPosition = tilemap.WorldToCell(imagepool[i, j].transform.position);
                if (imageCellPosition == cellPosition)
                {
                    return new Vector2Int(i, j); // 이미지의 인덱스 반환
                }
            }
        }
        return new Vector2Int(-1, -1); // 해당 셀 위치와 일치하는 이미지가 없음
    }


}
