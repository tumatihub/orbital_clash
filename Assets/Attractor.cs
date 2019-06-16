using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attractor : MonoBehaviour {

    public enum AttackResult { BLOCK, ATTACK, DAMAGED, PARRY};

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
    [HideInInspector] public bool parrying = false;
    public float parryingFXTime = .2f;
    private float parryingFXCountdown = 0f;
    public float parryingCooldownTime = 1f;
    private float parryingCooldown = 0f;
    private SpriteRenderer parrySprite;

    public bool stunned = false;
    private float stunTime = 1.5f;
    private float stunCountdown = 0;

    private bool collidingWithPlayer = false;

    private Vector2 previousPos = Vector2.zero;
    [SerializeField]
    private float stopOrbitForce = 1000;

    [Header("Force/Impulse Parameters")]
    public float minForce = 100; 
    public float maxForce = 140;
    public float blockPushBackForce = 50;
    public float blockAttackPushBackForce = 100;
    public float damagePushBackForce = 140;
    public float attackPushBackForce = 20;
    public float dashOnDashPushBack;
    public float splitForce = 100;

    public float G = 1f;
    public float K = 10;

    [Header("CamShake Parameters")]
    public GameObject camController;
    private CamShake camShake;
    public float blockCamShake_Amp = 2f;
    public float blockCamShake_Freq = 1f;
    public float blockCamShake_Dur = .2f;
    public float DashOnDashCamShake_Amp = 2f;
    public float DashOnDashCamShake_Freq = 2f;
    public float DashOnDashCamShake_Dur = .2f;

    [Header("Particles")]
    private ParticleSystem blockParticles;
    private ParticleSystem dashParticles;
    private ParticleSystem stunParticles;
    public ParticleSystem dashOnDashParticlesPrefab;


    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        camShake = camController.GetComponent<CamShake>();

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
        
        parrySprite = transform.Find("Parry").GetComponent<SpriteRenderer>();

        // Setup lifeBar
        lifeBar.fillRect.GetComponent<Image>().color = lifeBarColor;
        life = gameManager.maxLife;
        UpdateLifeBar();

        // Setup particles
        blockParticles = transform.Find("BlockParticles").GetComponent<ParticleSystem>();
        dashParticles = transform.Find("DashParticles").GetComponent<ParticleSystem>();
        stunParticles = transform.Find("StunParticles").GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        //Lookat
        transform.right = enemy.transform.position - transform.position;

        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
        }
        else
        {
            dashing = false;
        }

        if (parryingFXCountdown > 0)
        {
            parryingFXCountdown -= Time.deltaTime;
            StartParryingEffect();
            parrying = true;
        }
        else
        {
            EndParryingEffect();
            parrying = false;
        }

        if (parryingCooldown > 0)
        {
            parryingCooldown -= Time.deltaTime;
        }

        if (stunCountdown > 0)
        {
            stunCountdown -= Time.deltaTime;
        }
        else
        {
            stunned = false;
        }
        
    }

    private void FixedUpdate()
    {
        Attract(enemy);
        StopOrbit();
        //transform.LookAt(enemy.transform);
    }

    void UpdateLifeBar()
    {
        lifeBar.value = life / gameManager.maxLife;
    } 

    void StopOrbit()
    {
        if (previousPos == Vector2.zero)
        {
            previousPos = GetDirFromEnemy();
            return;
        }

        Vector2 actualPos = GetDirFromEnemy();
        Vector2 diffPos = previousPos - actualPos;

        Vector2 resultForce = diffPos - (Vector2.Dot(diffPos, actualPos) / Vector2.Dot(actualPos, actualPos)) * actualPos;

        if (resultForce.magnitude >= .2f)
        {
            rb.AddForce(resultForce * stopOrbitForce);
        }
        previousPos = actualPos;
    }

    void Attract(Attractor objToAttract)
    {
        Rigidbody2D rbToAttract = objToAttract.rb;

        Vector2 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        float forceMagnitude = K * distance;

        Vector2 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }

    public void Dodge(Vector2 dir)
    {
        if (stunned || collidingWithPlayer) return;

        Vector2 dirToEnemy = GetDirToEnemy();
        rb.velocity = Vector2.zero;
        Vector2 direction = dir - (Vector2.Dot(dir, dirToEnemy)/ Vector2.Dot(dirToEnemy, dirToEnemy) * dirToEnemy);
        rb.AddForce(direction.normalized * 50, ForceMode2D.Impulse);
    }

    public void Dash()
    {
        if (!dashing && !blocking && !stunned && !collidingWithPlayer)
        {
            dashing = true;
            dashCooldown = dashTime;

            Vector2 direction = enemy.transform.position - transform.position;
            rb.AddForce(direction.normalized * dashImpulse, ForceMode2D.Impulse);
            dashParticles.Play();
        }
    }

    public void Block()
    {
        if (!stunned && !collidingWithPlayer)
        {
            blocking = true;
            if (parryingCooldown <= 0)
            {
                parrying = true;
                parryingCooldown = parryingCooldownTime;
                parryingFXCountdown = parryingFXTime;
            }
        }
    }

    private void StartParryingEffect()
    {
        parrySprite.enabled = true;
    }

    private void EndParryingEffect()
    {
        parrySprite.enabled = false;
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
            var part = Instantiate(dashOnDashParticlesPrefab, transform.position, transform.rotation);
            Destroy(part.gameObject, 1f);
            camShake.Shake(DashOnDashCamShake_Dur, DashOnDashCamShake_Amp, DashOnDashCamShake_Freq);
            return AttackResult.ATTACK;
        }
        if (parrying)
        {
            BlockAttack();
            return AttackResult.PARRY;
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

    private void Stunned()
    {
        stunned = true;
        stunCountdown = stunTime;
        dashing = false;
        ReleaseBlock();
        stunParticles.Play();
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
        camShake.Shake(blockCamShake_Dur, blockCamShake_Amp, blockCamShake_Freq);
        blockParticles.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("Split");
            collidingWithPlayer = true;

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
                else if (result == AttackResult.PARRY)
                {
                    BlockPushBack();
                    Stunned();
                }
            }
        }

        if (collision.gameObject.tag == "Borda")
        {
            TakeEdgeDamage();
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        StopCoroutine("Split");
        collidingWithPlayer = false;
    }
    

    private IEnumerator Split()
    {
        float forceMagnitude = splitForce;
        yield return new WaitForSeconds(1);
        rb.velocity = Vector2.zero;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
        StopCoroutine("Split");
    }

    private void TakeEdgeDamage()
    {
        ChangeLife(gameManager.edgeDamage);
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
    private Vector2 GetDirFromEnemy()
    {
        Vector2 direction = transform.position - enemy.transform.position;

        return direction;
    }
    private Vector2 GetDirToEnemy()
    {
        Vector2 direction = enemy.transform.position - transform.position;

        return direction;
    }
}
