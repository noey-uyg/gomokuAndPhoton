using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseManager : MonoBehaviour
{
    GameManager gameManager;
    
    private Tilemap tilemap;
    public GameObject[,] imagepool;
    public GameObject[] cursorImage;
    private Sprite[] cursorSprites;

    public int currentTurn = 0;

    int boardSizeX;
    int boardSizeY;

    private void Start()
    {
        gameManager = GameManager.Instance;
        cursorSprites = new Sprite[cursorImage.Length];
        for (int i = 0; i < cursorImage.Length; i++)
        {
            cursorSprites[i] = cursorImage[i].GetComponent<SpriteRenderer>().sprite;
        }

        int[] board = gameManager.GetBoardSize();
        tilemap = gameManager.GetTileMap();
        imagepool = gameManager.poolManager.imagepool;
        boardSizeX = board[0];
        boardSizeY = board[1];
        
    }


    private void Update()
    {
        currentTurn = gameManager.GetCurrentPlayer();

        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // 모든 이미지를 비활성화
        for (int i = 0; i < boardSizeX; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                if(gameManager.GetBoardValue(i,j) == -1)
                {
                    imagepool[i, j].SetActive(false);
                }
            }
        }

        // 마우스가 올라가 있는 셀의 이미지를 활성화
        if (tilemap.HasTile(cellPosition))
        {
            Vector2Int index = GetImageIndexByCellPosition(cellPosition);
            int indexX = index.x;
            int indexY = index.y;

            if (indexX >=0 && indexY >=0)
            {
                // 이미지 활성화 및 이미지 풀 관리
                if(gameManager.GetBoardValue(indexX,indexY) == -1)
                {
                    imagepool[indexX, indexY].GetComponent<SpriteRenderer>().sprite = cursorSprites[currentTurn];
                    imagepool[indexX, indexY].SetActive(true);
                }
                
                // 클릭 시 이미지 고정 및 턴 변경
                if (Input.GetMouseButtonDown(0) && gameManager.GetBoardValue(indexX, indexY) == -1 && !gameManager.IsGameOver())
                {
                    // 클릭 위치에 돌 놓기
                    gameManager.SetBoardValue(indexX, indexY, currentTurn);

                    // 턴 변경
                    int nextturn = (currentTurn + 1) % cursorImage.Length;

                    gameManager.SetCurrentPlayer(nextturn);
                    if(gameManager.CheckWin() != -1)
                    {
                        Debug.Log(gameManager.CheckWin()+"플레이어 승리");
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
