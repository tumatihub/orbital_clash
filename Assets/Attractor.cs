using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attractor : MonoBehaviour {

    public enum AttackResult { BLOCK, ATTACK, DAMAGED };

    public Rigidbody2D rb;
    public Attractor enemy;
    public GameObject swordPrefab;
    public GameObject sword;

    public Slider lifeBar;
    public Color lifeBarColor;

    private float life;

    private GameManager gameManager;

    public string atk;
    public string def;

    public float dashTime = .5f;
    public float dashImpulse = 100;
    [HideInInspector] public bool dashing = false;
    [HideInInspector] public float dashCooldown = 0;

    [HideInInspector] public bool blocking = false;

    public float minForce = 100, maxForce = 140;
    public float blockPushBackForce = 50;
    public float blockAttackPushBackForce = 100;
    public float damagePushBackForce = 140;
    public float attackPushBackForce = 20;
    public float dashOnDashPushBack;
    public float splitForce = 100;

    public float G = 6;
    public float K = 10;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Setup

        dashTime = gameManager.dashTime;
        dashImpulse = gameManager.dashImpulse;
        blockPushBackForce = gameManager.blockPushBackForce;
        blockAttackPushBackForce = gameManager.blockAttackPushBackForce;
        damagePushBackForce = gameManager.damagePushBackForce;
        attackPushBackForce = gameManager.attackPushBackForce;
        dashOnDashPushBack = gameManager.dashOnDashPushBack;
        splitForce = gameManager.splitForce;
        K = gameManager.K;

    }

    private void Start()
    {
        sword = Instantiate(swordPrefab);
        sword.transform.position = transform.position;
        sword.transform.parent = transform;

        // Setup lifeBar
        lifeBar.fillRect.GetComponent<Image>().color = lifeBarColor;
        life = gameManager.maxLife;
        UpdateLifeBar();
    }

    private void Update()
    {
        //Lookat
        transform.right = enemy.transform.position - transform.position;

        if (dashCooldown >= 0)
        {
            dashCooldown -= Time.deltaTime;
        } else
        {
            dashing = false;
        }
        
    }

    private void FixedUpdate()
    {
        Attract(enemy);
        
        //transform.LookAt(enemy.transform);
    }

    void UpdateLifeBar()
    {
        lifeBar.value = life / gameManager.maxLife;
    } 

    void Attract(Attractor objToAttract)
    {
        Rigidbody2D rbToAttract = objToAttract.rb;

        Vector2 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        //float forceMagnitude = G * (rb.mass * rbToAttract.mass) / distance;
        float forceMagnitude = K * distance;

        Vector2 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }

    public void Dash()
    {
        if (!dashing && !blocking)
        {
            dashing = true;
            dashCooldown = dashTime;

            Vector2 direction = enemy.transform.position - transform.position;
            rb.AddForce(direction.normalized * dashImpulse, ForceMode2D.Impulse);
        }
    }

    public void Block()
    {
        blocking = true;
    }

    public void ReleaseBlock()
    {
        blocking = false;
    }

    public AttackResult Attacked()
    {
        if (dashing)
        {
            DashOnDashPushBack();
            return AttackResult.ATTACK;
        }
        if (blocking)
        {
            BlockAttack();
            return AttackResult.BLOCK;
        } else
        {
            DamagePushBack();
            ChangeLife(gameManager.dashDamage);
            return AttackResult.DAMAGED;
        }
    }

    private void ChangeLife(float amount)
    {
        float newLifeValue = life - amount;
        life = Mathf.Clamp(newLifeValue, 0, gameManager.maxLife);
        UpdateLifeBar();
        if (life == 0)
        {
            gameManager.RestartLevel();
        }
    }


    private void DamagePushBack()
    {
        rb.velocity = Vector2.zero;
        float forceMagnitude = damagePushBackForce;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
    }

    private void DashOnDashPushBack()
    {
        rb.velocity = Vector2.zero;
        float forceMagnitude = dashOnDashPushBack;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
    }

    private void BlockAttack()
    {
        rb.velocity = Vector2.zero;
        float forceMagnitude = blockAttackPushBackForce;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("Split");

            if (dashing)
            {
                //dashing = false;
                var result = enemy.Attacked();

                if (result == AttackResult.BLOCK)
                {
                    BlockPushBack();
                }
                else if (result == AttackResult.DAMAGED)
                {
                    AttackPushBack();
                }
                else if (result == AttackResult.ATTACK)
                {
                    DashOnDashPushBack();
                }
            }
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        StopCoroutine("Split");
    }
    

    private IEnumerator Split()
    {
        float forceMagnitude = splitForce;
        yield return new WaitForSeconds(1);
        rb.velocity = Vector2.zero;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
        StopCoroutine("Split");
    }

    private void AttackPushBack()
    {
        float forceMagnitude = attackPushBackForce;
        rb.velocity = Vector2.zero;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
    }

    private void BlockPushBack()
    {
        rb.velocity = Vector2.zero;
        float forceMagnitude = blockPushBackForce;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
    }

    private Vector2 GetDir()
    {
        Vector2 direction = transform.position - enemy.transform.position;
        
        float rotationAddX = Random.Range(-0.1f, 0.1f);
        float rotationAddY = Random.Range(-0.1f, 0.1f);

        direction = new Vector2(direction.x + rotationAddX, direction.y + rotationAddY);

        return direction;
    }
}
