using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    
    //private static MenuManager instance = null;
    private GameManager gameManager;

    void Awake()
    {

        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("J_START"))
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                LoadMultiPlayer();
            }
        }
        else if (Input.GetButtonDown("J_BACK"))
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                LoadSinglePlayer();
        }
        else if (Input.GetButtonDown("J_LB"))
        {
            MainMenu();
        }
    }

    public void LoadSinglePlayer()
    {
        gameManager.gameMode = GameManager.GameMode.SINGLE;
        ChangeToFightMusic();
        SceneManager.LoadScene(4);
    }

    public void LoadMultiPlayer()
    {
        gameManager.gameMode = GameManager.GameMode.MULTIPLAYER;
        ChangeToFightMusic();
        SceneManager.LoadScene(4);
    }

    public void LoadConfig()
    {
        SceneManager.LoadScene(3);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        gameManager.audioSource.Stop();
        gameManager.audioSource.clip = gameManager.musicaMenu;
        gameManager.audioSource.PlayDelayed(.5f);
        gameManager.ResetScore();
        SceneManager.LoadScene(0);
    }

    private void ChangeToFightMusic()
    {
        gameManager.audioSource.Stop();
        gameManager.audioSource.clip = gameManager.musicaLuta;
        gameManager.audioSource.PlayDelayed(.2f);
    }
}
