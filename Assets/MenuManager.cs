using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public void LoadSinglePlayer()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadMultiPlayer()
    {
        SceneManager.LoadScene(2);
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
        SceneManager.LoadScene(0);
    }
}
