using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PoolManager : MonoBehaviour
{
    GameManager gameManager;

    public GameObject cursorImage;
    public GameObject[,] imagepool;

    private int boardSizeX;
    private int boardSizeY;
    private Tilemap tilemap;
    private BoundsInt bounds;

    private void Start()
    {
        gameManager = GameManager.Instance;
        tilemap = gameManager.GetTileMap();

        InitObjectPool();
    }

    //오브젝트 풀 초기화 함수
    private void InitObjectPool()
    {
        bounds = tilemap.cellBounds;
        int[] boardSize = gameManager.GetBoardSize();
        boardSizeX = boardSize[0];
        boardSizeY = boardSize[1];

        imagepool = new GameObject[boardSizeX, boardSizeY];
        for (int i = 0; i < boardSizeX; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                Vector3Int cellPosition = new Vector3Int(bounds.x + i, bounds.y + j, 0);
                Vector3 cellCenterWorld = tilemap.GetCellCenterWorld(cellPosition);
                imagepool[i, j] = Instantiate(cursorImage, transform);
                imagepool[i, j].transform.position = cellCenterWorld;
                imagepool[i, j].SetActive(false);
            }
        }
    }
}
