using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    private static GameManager instance = null;
    public enum GameMode { MULTIPLAYER, SINGLE }
    public GameMode gameMode;
    public enum GameState { PLAY, PAUSE }
    public GameState gameState = GameState.PAUSE;

    public GameObject playerPrefab;
    public GameObject p1;
    public GameObject p2;

    public MenuManager menuManager;

    private GameObject targetGroup;

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

    public Color p1NeutralColor;
    public Color p1DashColor;
    public Color p1BlockColor;

    public Color p2NeutralColor;
    public Color p2DashColor;
    public Color p2BlockColor;

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

    public void SetupGameMode()
    {
        Destroy(p1);
        Destroy(p2);

        gameState = GameState.PAUSE;

        Transform p1Start = GameObject.Find("P1Start").transform;
        Transform p2Start = GameObject.Find("P2Start").transform;

        p1 = Instantiate(playerPrefab, p1Start.position, Quaternion.identity);
        p2 = Instantiate(playerPrefab, p2Start.position, Quaternion.identity);

        SetupPlayer1Controller();

        Attractor p1Attractor = p1.GetComponent<Attractor>();
        Attractor p2Attractor = p2.GetComponent<Attractor>();

        // LifeBar
        Slider p1LifeBar = GameObject.Find("LifeBar_P1").GetComponent<Slider>();
        Slider p2LifeBar = GameObject.Find("LifeBar_P2").GetComponent<Slider>();

        p1Attractor.lifeBar = p1LifeBar;
        p2Attractor.lifeBar = p2LifeBar;

        p1Attractor.lifeBarColor = p1NeutralColor;
        p2Attractor.lifeBarColor = p2NeutralColor;

        p1Attractor.enemy = p2Attractor;
        p2Attractor.enemy = p1Attractor;

        // Inputs
        p1Attractor.atk = "P1_atk";
        p1Attractor.def = "P1_def";

        p2Attractor.atk = "P2_atk";
        p2Attractor.def = "P2_def";

        // Target Group for cinemachine
        targetGroup = GameObject.Find("Target Group");
        var group = targetGroup.GetComponent<CinemachineTargetGroup>();
        group.m_Targets[0].target = p1.transform;
        group.m_Targets[1].target = p2.transform;

        SetupButtons(p1Attractor, "Control_P1");

        if (gameMode == GameMode.SINGLE)
        {
            SetupAIController();
            DeactivateButtons("Control_P2");
            print("Mode: SINGLEPLAYER");
        }

        if (gameMode == GameMode.MULTIPLAYER)
        {
            SetupPlayer2Controller();
            SetupButtons(p2Attractor, "Control_P2");
            print("Mode: MULTIPLAYER");
        }
    }

    private void SetupButtons(Attractor attractor, string buttonContainer)
    {
        // Button and event trigger
        EventTrigger pAtkEventTrigger = GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<EventTrigger>();
        EventTrigger pDefEventTrigger = GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<EventTrigger>();

        EventTrigger.Entry entryDash = new EventTrigger.Entry();
        entryDash.eventID = EventTriggerType.PointerDown;
        entryDash.callback.AddListener((data) => { attractor.Dash(); });
        pAtkEventTrigger.triggers.Add(entryDash);

        EventTrigger.Entry entryBlock = new EventTrigger.Entry();
        entryBlock.eventID = EventTriggerType.PointerDown;
        entryBlock.callback.AddListener((data) => { attractor.Block(); });
        pDefEventTrigger.triggers.Add(entryBlock);

        EventTrigger.Entry entryReleaseBlock = new EventTrigger.Entry();
        entryReleaseBlock.eventID = EventTriggerType.PointerUp;
        entryReleaseBlock.callback.AddListener((data) => { attractor.ReleaseBlock(); });
        pDefEventTrigger.triggers.Add(entryReleaseBlock);

        
    }

    public void DeactivateButtons(string buttonContainer)
    {
        Color imageColor;
        GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<EventTrigger>().enabled = false;
        GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<EventTrigger>().enabled = false;
        Button pAtkButton = GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<Button>();
        Button pDefButton = GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<Button>();
        pAtkButton.enabled = false;
        pDefButton.enabled = false;
        Image pAtkImage = GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<Image>();
        imageColor = pAtkImage.color;
        imageColor.a = .1f;
        pAtkImage.color = imageColor;
        Image pDefImage = GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<Image>();
        imageColor = pDefImage.color;
        imageColor.a = .1f;
        pDefImage.color = imageColor;
    }

    public void ActivateButtons(string buttonContainer)
    {
        Color imageColor;
        GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<EventTrigger>().enabled = true;
        GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<EventTrigger>().enabled = true;
        Button pAtkButton = GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<Button>();
        Button pDefButton = GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<Button>();
        pAtkButton.enabled = true;
        pDefButton.enabled = true;
        Image pAtkImage = GameObject.Find(buttonContainer).transform.Find("Atk").GetComponent<Image>();
        imageColor = pAtkImage.color;
        imageColor.a = 1f;
        pAtkImage.color = imageColor;
        Image pDefImage = GameObject.Find(buttonContainer).transform.Find("Def").GetComponent<Image>();
        imageColor = pDefImage.color;
        imageColor.a = 1f;
        pDefImage.color = imageColor;
    }

    private void SetupPlayer1Controller()
    {
        PlayerController p1Controller = p1.AddComponent<PlayerController>();

        // Setup Color
        p1Controller.neutralColor = p1NeutralColor;
        p1Controller.dashingColor = p1DashColor;
        p1Controller.blockingColor = p1BlockColor;
        p1.GetComponent<SpriteRenderer>().color = p1NeutralColor;

        // Border
        GameObject border = GameObject.Find("Border");
        p1Controller.border = border;

        // Touch area
        p1Controller.cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        p1Controller.touchMinX = .5f;
        p1Controller.touchMaxX = .9f;
    }

    private void SetupPlayer2Controller()
    {
        PlayerController p2Controller = p2.AddComponent<PlayerController>();

        // Setup Color
        p2Controller.neutralColor = p2NeutralColor;
        p2Controller.dashingColor = p2DashColor;
        p2Controller.blockingColor = p2BlockColor;
        p2.GetComponent<SpriteRenderer>().color = p2NeutralColor;

        // Border
        GameObject border = GameObject.Find("Border");
        p2Controller.border = border;

        // Touch area
        p2Controller.cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        p2Controller.touchMinX = .1f;
        p2Controller.touchMaxX = .5f;
    }

    private void SetupAIController()
    {
        AIController p2Controller = p2.AddComponent<AIController>();

        // Setup Color
        p2Controller.neutralColor = p2NeutralColor;
        p2Controller.dashingColor = p2DashColor;
        p2Controller.blockingColor = p2BlockColor;
        p2.GetComponent<SpriteRenderer>().color = p2NeutralColor;

        // Border
        GameObject border = GameObject.Find("Border");
        p2Controller.border = border;
    }

    public void StartLevel()
    {
        StartCoroutine("StartLevelCountDown");
    }

    private IEnumerator StartLevelCountDown()
    {
        GameObject counterObj = GameObject.Find("Counter");

        var counter = counterObj.GetComponent<TextMeshProUGUI>();
        counter.enabled = true;

        for (var i = 3; i>=0; i--)
        {
            print(i); // TODO: Transformar em UI
            if (i != 0)
            {
                counter.SetText(i.ToString());
            }
            else
            {
                counter.SetText("GO!");
            }
            yield return new WaitForSeconds(1);
        }

        counter.enabled = false;
        
        print("Start!!!");

        ActivateButtons("Control_P1");

        if (gameMode == GameMode.MULTIPLAYER)
        {
            ActivateButtons("Control_P2");
        }

        gameState = GameState.PLAY;
    }

    public void EndLevel()
    {
        IEnumerator coroutine = Slowmotion(2f, .1f);
        StartCoroutine(coroutine);
        StartCoroutine("KO");
    }

    private IEnumerator KO()
    {
        GameObject counterObj = GameObject.Find("Counter");

        var counter = counterObj.GetComponent<TextMeshProUGUI>();
        counter.enabled = true;

        counter.SetText("K.");
        yield return new WaitForSecondsRealtime(1f);

        counter.SetText("K.O.!");
        yield return new WaitForSecondsRealtime(1f);

        RestartLevel();
    }

    private IEnumerator Slowmotion(float duration, float scale)
    {
        float durationCount = duration;
        float oldFixed = Time.fixedDeltaTime;
        Time.fixedDeltaTime = scale * 0.02f;
        Time.timeScale = scale;

        gameState = GameState.PAUSE;

        while(durationCount > 0)
        {
            yield return new WaitForSecondsRealtime(.1f);
            Time.timeScale += .1f;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0, 1);
            durationCount -= .1f;
        }

        Time.fixedDeltaTime = oldFixed;

    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
