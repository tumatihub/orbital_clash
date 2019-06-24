using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

    private Attractor attractor;
    private SpriteRenderer sprite;
    private GameManager gameManager;

    public GameObject border;
    public float distanceToAct = 15;
    private float delayBetweenAction = .7f;
    private float countDown = 0;
    public float chanceToBlock = .4f;
    public float distanceToGoAgress = 20;

    public Color blockingColor;
    public Color dashingColor;
    public Color neutralColor;

    public Sprite neutralSprite;
    public Sprite dashingSprite;
    public Sprite blockingSprite;
    public Sprite stunSprite;

    // Use this for initialization
    void Start () {
        attractor = GetComponent<Attractor>();
        sprite = attractor.GetComponent<SpriteRenderer>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {

        if (gameManager.gameState == GameManager.GameState.PAUSE) return;

        if (countDown <= 0)
        {;
            float dist = GetDistanceToPlayer(attractor);
            if (dist <= distanceToAct)
            {
                ChooseAction();
                countDown = delayBetweenAction;
            }
        }
        countDown -= Time.deltaTime;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (attractor.takingDamage || attractor.dodging) return;

        if (attractor.blocking)
        {
            sprite.sprite = blockingSprite;
        }
        else if (attractor.dashing)
        {
            sprite.sprite = dashingSprite;
        }
        else if (attractor.stunned)
        {
            sprite.sprite = stunSprite;
        }
        else
        {
            sprite.sprite = neutralSprite;
        }
    }

    private float GetClosestDistanceToBorder()
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
        return distance;
    }

    private void ChooseAction()
    {
        float chance = chanceToBlock;
        float dist = GetClosestDistanceToBorder();
        if (dist <= distanceToGoAgress || attractor.stunned)
        {
            chance = .01f;
        }
        else
        {
            chance = chanceToBlock;
        }

        float rand = Random.value;
        if (rand <= chance)
        {
            attractor.Block();
        }
        else
        {
            attractor.ReleaseBlock();
            attractor.Dash();
        }
    }

    private float GetDistanceToPlayer(Attractor attractor)
    {
        return (transform.position - attractor.enemy.transform.position).magnitude;
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
        if (distance <= distanceToGoAgress)
        {
            Gizmos.DrawLine(transform.position, closest);
        }
        if (attractor != null && GetDistanceToPlayer(attractor) <= distanceToAct)
        {
            Gizmos.DrawLine(transform.position, attractor.enemy.transform.position);
        }

        Gizmos.DrawWireSphere(closest, 1.0f);

    }
}
