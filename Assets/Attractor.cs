using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour {

    public Rigidbody2D rb;
    public Attractor enemy;

    
    public float minForce = 100, maxForce = 140;

    public float G = 6;

    private void FixedUpdate()
    {
        Attract(enemy);
    }

    void Attract(Attractor objToAttract)
    {
        Rigidbody2D rbToAttract = objToAttract.rb;

        Vector2 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        float forceMagnitude = G * (rb.mass * rbToAttract.mass) / distance;
        //print(forceMagnitude);
        Vector2 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D enemyRb = collision.rigidbody;

        Vector2 direction = rb.position - enemyRb.position;
        float forceMagnitude = minForce;
        if (Random.value > .8)
        {
            forceMagnitude = maxForce;
        }

        float rotationAddX = Random.Range(-0.1f, 0.1f);
        float rotationAddY = Random.Range(-0.1f, 0.1f);

        direction = new Vector2(direction.x + rotationAddX, direction.y + rotationAddY);
        print(forceMagnitude);

        
        rb.AddForce(direction.normalized * forceMagnitude, ForceMode2D.Impulse);
    }
}
