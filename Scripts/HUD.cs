using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    GameManager gameManager;
    public Image currentStoneImage;
    public Sprite[] cursorSprites;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        currentStoneImage.sprite = cursorSprites[0];
    }

    // Update is called once per frame
    void Update()
    {
        currentStoneImage.sprite = cursorSprites[gameManager.GetCurrentPlayer()];
    }
}
