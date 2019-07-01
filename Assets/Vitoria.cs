using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vitoria : MonoBehaviour {

    private GameManager gameManager;
    private Image winnerImage;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        winnerImage = GameObject.Find("Winner Sprite").GetComponent<Image>();

        if (gameManager.p1Score > gameManager.p2Score)
        {
            winnerImage.sprite = gameManager.p1NeutralSprite;
        }
        else
        {
            winnerImage.sprite = gameManager.p2NeutralSprite;
        }
    }
	
}
