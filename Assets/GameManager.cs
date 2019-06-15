using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private static GameManager instance = null;

    public float dashTime = .5f;
    public float dashImpulse = 100;
    public float blockPushBackForce = 50;
    public float blockAttackPushBackForce = 100;
    public float damagePushBackForce = 140;
    public float attackPushBackForce = 20;
    public float dashOnDashPushBack = 150;
    public float splitForce = 100;
    public float K = 10;

    public float maxLife = 500;
    public float dashDamage = 50;
    public float edgeDamage = 50;

    // Use this for initialization
    void Awake () {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
	}

    public void RestartLevel()
    {
        print("Starting Coroutine");
        StartCoroutine("RestartLevelCountDown");
    }

    private IEnumerator RestartLevelCountDown()
    {
        yield return new WaitForSeconds(1);
        print("Reloading");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
