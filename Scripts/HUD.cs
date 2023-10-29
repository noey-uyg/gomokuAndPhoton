using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    GameManager gameManager;
    public Image myStoneImage;
    public Image currentStoneImage;
    public Text countDown;
    public Sprite[] cursorSprites;


    void Start()
    {
        //초기화 작업
        gameManager = GameManager.Instance;
        countDown.text = string.Format("{0:D2}초", Mathf.FloorToInt(gameManager.placeMentTime));
        currentStoneImage.sprite = cursorSprites[0];
        myStoneImage.sprite = cursorSprites[gameManager.GetMyTurn()];
    }

    private void LateUpdate()
    {
        myStoneImage.sprite = cursorSprites[gameManager.GetMyTurn()];
        countDown.text = string.Format("{0:D2}초", Mathf.FloorToInt(gameManager.placeMentTime - gameManager.gameTime));
        currentStoneImage.sprite = cursorSprites[gameManager.GetCurrentPlayer()];
    }
}
