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

        // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // ��� �̹����� ��Ȱ��ȭ
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

        // ���콺�� �ö� �ִ� ���� �̹����� Ȱ��ȭ
        if (tilemap.HasTile(cellPosition))
        {
            Vector2Int index = GetImageIndexByCellPosition(cellPosition);
            int indexX = index.x;
            int indexY = index.y;

            if (indexX >=0 && indexY >=0)
            {
                // �̹��� Ȱ��ȭ �� �̹��� Ǯ ����
                if(gameManager.GetBoardValue(indexX,indexY) == -1)
                {
                    imagepool[indexX, indexY].GetComponent<SpriteRenderer>().sprite = cursorSprites[currentTurn];
                    imagepool[indexX, indexY].SetActive(true);
                }
                
                // Ŭ�� �� �̹��� ���� �� �� ����
                if (Input.GetMouseButtonDown(0) && gameManager.GetBoardValue(indexX, indexY) == -1 && !gameManager.IsGameOver())
                {
                    // Ŭ�� ��ġ�� �� ����
                    gameManager.SetBoardValue(indexX, indexY, currentTurn);

                    // �� ����
                    int nextturn = (currentTurn + 1) % cursorImage.Length;

                    gameManager.SetCurrentPlayer(nextturn);
                    if(gameManager.CheckWin() != -1)
                    {
                        Debug.Log(gameManager.CheckWin()+"�÷��̾� �¸�");
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
