using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    private GameManager gameManager;
    private Color neonColor = Color.white;
    public SpriteRenderer neonSprite;
    private Attractor p1Attractor;
    private Attractor p2Attractor;
    private float increment = .1f;
    private float actualSaturation = 0;
    private float h, s, v;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.SetupGameMode();

        gameManager.DeactivateButtons("Control_P1");
        gameManager.DeactivateButtons("Control_P2");

        gameManager.StartLevel();

        p1Attractor = gameManager.p1.GetComponent<Attractor>();
        p2Attractor = gameManager.p2.GetComponent<Attractor>();
    }

    private void Update()
    {
        UpdateNeonLines();
    }

    void UpdateNeonLines()
    {
        if (p1Attractor.life > p2Attractor.life)
        {
            neonColor = p1Attractor.lifeBarColor;
        } else if (p2Attractor.life > p1Attractor.life)
        {
            neonColor = p2Attractor.lifeBarColor;
        } else
        {
            neonColor = Color.white;
        }
        Color.RGBToHSV(neonColor, out h, out s, out v);
        if (actualSaturation >= 1)
        {
            increment = -0.1f;
        } else if (actualSaturation <= 0)
        {
            increment = 0.1f;
        }
        
        neonSprite.color = Color.HSVToRGB(h, actualSaturation, v);

        actualSaturation += increment * Time.deltaTime + (actualSaturation/20)*Mathf.Sign(increment);
        actualSaturation = Mathf.Clamp(actualSaturation, 0, 1);
    }
	
}
