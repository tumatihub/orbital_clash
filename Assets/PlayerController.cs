using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Attractor attractor;

    public GameObject border;

	// Use this for initialization
	void Start () {
        attractor = GetComponent<Attractor>();    		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown(attractor.atk) && !attractor.dashing)
        {
            attractor.dashing = true;
            attractor.dashCooldown = attractor.dashTime;

            Vector2 direction = attractor.enemy.transform.position - transform.position;
            attractor.rb.AddForce(direction.normalized * attractor.dashImpulse, ForceMode2D.Impulse);
        }

        if (Input.GetButtonDown(attractor.def))
        {
            attractor.blocking = true;
        }

        if (Input.GetButtonUp(attractor.def))
        {
            attractor.blocking = false;
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

    }
}
