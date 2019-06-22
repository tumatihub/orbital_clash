using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    private Attractor attractor;
    private SpriteRenderer sprite;
    private GameManager gameManager;

    public Camera cam;

    public GameObject border;

    public Color blockingColor;
    public Color dashingColor;
    public Color neutralColor;

    private Vector2 touchStart;
    private float minTouchDistance = 10f;
    [Range(0,1)] public float touchMinX;
    [Range(0,1)] public float touchMaxX;

    private Vector2 lastMousePos;

	// Use this for initialization
	void Start () {
        attractor = GetComponent<Attractor>();
        sprite = attractor.GetComponent<SpriteRenderer>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {

        if (gameManager.gameState == GameManager.GameState.PAUSE) return;

        #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (Input.GetButtonDown(attractor.atk) && !attractor.dashing && !attractor.blocking)
        {
            attractor.Dash();
        }

        if (Input.GetButtonDown(attractor.def))
        {
            attractor.Block();
        }

        if (Input.GetButtonUp(attractor.def))
        {
            attractor.ReleaseBlock();
        }

        // Simular swipe com as setas do teclado
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            attractor.Dodge(new Vector2(0, 1));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            attractor.Dodge(new Vector2(1, 0));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            attractor.Dodge(new Vector2(0, -1));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            attractor.Dodge(new Vector2(-1, 0));
        }

        #elif UNITY_IOS || UNITY_ANDROID
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            GetSwipeDir(touch);
        }

        
        #endif

        UpdateColor();
    }
    
    private void GetSwipeDir(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            touchStart = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 touchEnd = touch.position;
            Vector2 viewportTouch = cam.ScreenToViewportPoint(touchEnd);
            if (viewportTouch.x >= touchMinX && viewportTouch.x < touchMaxX)
            {
                float xDist = Mathf.Abs(touchEnd.x - touchStart.x);
                float yDist = Mathf.Abs(touchEnd.y - touchStart.y);
                if (xDist >= minTouchDistance || yDist >= minTouchDistance)
                {
                    float x = Mathf.Sign(touchEnd.x - touchStart.x);
                    float y = Mathf.Sign(touchEnd.y - touchStart.y);

                    if (xDist > yDist)
                    {
                        y = 0;
                    }
                    else
                    {
                        x = 0;
                    }
                    attractor.Dodge(new Vector2(x, y));
                }
            }
        }
    }

    public void Dash()
    {
        attractor.Dash();
    }

    public void Block()
    {
        attractor.Block();
    }

    public void ReleaseBlock()
    {
        attractor.ReleaseBlock();
    }

    private void UpdateColor()
    {
        if (attractor.takingDamage || attractor.dodging) return;

        if (attractor.blocking)
        {
            sprite.color = blockingColor;
        }
        else if (attractor.dashing)
        {
            sprite.color = dashingColor;
        }
        else if (attractor.stunned)
        {
            sprite.color = Color.black;
        }
        else
        {
            sprite.color = neutralColor;
        }
    }

    private void OnDrawGizmos()
    {
        Collider2D[] colliders = border.GetComponents<Collider2D>();

        float distance = 0;
        Vector2 closest = Vector2.zero;

        foreach (var borderCollider in colliders)
        {
            Vector2 tmpClosest = borderCollider.bounds.ClosestPoint(transform.position);
            float tmpDistance = (tmpClosest - (Vector2)transform.position).magnitude;
            if (tmpDistance < distance || distance == 0)
            {
                distance = tmpDistance;
                closest = tmpClosest;
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(closest, 1.0f);
        if (cam != null)
        {
            Gizmos.DrawLine(cam.ViewportToWorldPoint(new Vector3(touchMinX, 0.5f, 0)), cam.ViewportToWorldPoint(new Vector3(touchMaxX, 0.5f, 0)));

        }
    }
}
