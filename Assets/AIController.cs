using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

    private Attractor attractor;

    public GameObject border;
    public float distaceToAct = 15;
    private float delayBetweenAction = .3f;
    private float countDown = 0;
    private float chanceToBlock = .3f;
    private float distanceToGoAgress = 20;

    // Use this for initialization
    void Start () {
        attractor = GetComponent<Attractor>();
    }
	
	// Update is called once per frame
	void Update () {
        if (countDown <= 0)
        {
            print("Acting!");
            float dist = GetDistanceToPlayer(attractor);
            if (dist <= distaceToAct)
            {
                ChooseAction();
                countDown = delayBetweenAction;
            }
        }
        countDown -= Time.deltaTime;

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
        if (dist <= distanceToGoAgress)
        {
            chance = .01f;
        }
        else
        {
            chance = chanceToBlock;
        }

        float rand = Random.value;
        if (rand <= chanceToBlock)
        {
            attractor.blocking = true;
            attractor.dashing = false;
            print("Blocking");
        }
        else
        {
            attractor.blocking = false;
            attractor.dashing = true;
            print("Dashing");
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
        Gizmos.DrawWireSphere(closest, 1.0f);

    }
}
