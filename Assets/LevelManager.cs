using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    private GameManager gameManager;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.SetupGameMode();

        gameManager.DeactivateButtons("Control_P1");
        gameManager.DeactivateButtons("Control_P2");

        gameManager.StartLevel();
    }
	
}
