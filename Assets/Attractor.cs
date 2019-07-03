using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attractor : MonoBehaviour {

    public enum AttackResult { BLOCK, ATTACK, DAMAGED, PARRY};

    public Rigidbody2D rb;
    public Attractor enemy;

    [HideInInspector]
    public Sprite neutralSprite;

    private AudioSource audioSource;

    public Slider lifeBar;
    public Color lifeBarColor;
    
    public float life;

    private GameManager gameManager;
    private SpriteRenderer sprite;
    private Color spriteColor;

    // Inputs
    public string atk;
    public string def;
    public string up;
    public string down;
    public string left;
    public string right;

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

    [HideInInspector]
    public bool dodging = false;
    private float dodgeCooldown = 2f;
    private float dodgeCountdown = 0;
    private float dodgeFXTime = .5f;
    private float dodgeFXCountdown = 0;
    [SerializeField]
    private float minDistToSlowDodge = 2f;

    [HideInInspector]
    public bool takingDamage = false;
    public float damageTime = .5f;
    public float damageCountdown = 0;

    private Vector2 previousPos = Vector2.zero;
    [SerializeField]
    private float stopOrbitForce = 40000;

    [Header("Force/Impulse Parameters")]
    public float minForce = 100; 
    public float maxForce = 140;
    public float blockPushBackForce = 50;
    public float blockAttackPushBackForce = 100;
    public float damagePushBackForce = 140;
    public float attackPushBackForce = 20;
    public float dashOnDashPushBack;
    public float splitForce = 100;
    public float dodgeImpulse = 100;

    public float G = 5f;
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
    public float CollidingCamShake_Amp = 1f;
    public float CollidingCamShake_Freq = 5f;
    public float BorderCamShake_Amp = 2f;
    public float BorderCamShake_Freq = 3f;
    public float BorderCamShake_Dur = .2f;

    [Header("Particles")]
    private ParticleSystem blockParticles;
    private ParticleSystem dashParticles;
    private ParticleSystem stunParticles;
    private ParticleSystem collidingParticles;
    public ParticleSystem dashOnDashParticlesPrefab;
    public ParticleSystem borderParticlesPrefab;

    [Header("SFX")]
    public AudioClip[] dashSounds;
    [Range(0, 1)] public float dashVolume = 1;
    public AudioClip blockSound;
    [Range(0, 1)] public float blockVolume = 1;
    public AudioClip stunBlockSound;
    [Range(0, 1)] public float stunBlockVolume = 1;
    public AudioClip dashOnDashSound;
    [Range(0, 1)] public float dashOnDashVolume = 1;
    public AudioClip edgeSound;
    [Range(0, 1)] public float edgeVolume = 1;
    public AudioClip stunSound;
    [Range(0, 1)] public float stunVolume = 1;
    public AudioClip damageSound;
    [Range(0, 1)] public float damageVolume = 1;
    public AudioClip collidingSound;
    [Range(0, 1)] public float collidingVolume = 1;
    public AudioClip dodgeSound;
    [Range(0, 1)] public float dodgeVolume = 1;
    

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
        
        parrySprite = transform.Find("Parry").GetComponent<SpriteRenderer>();
        sprite = GetComponent<SpriteRenderer>();
        spriteColor = lifeBarColor;

        camController = GameObject.Find("CamShake");
        camShake = camController.GetComponent<CamShake>();

        // Setup lifeBar
        lifeBar.fillRect.GetComponent<Image>().color = lifeBarColor;
        life = gameManager.maxLife;
        UpdateLifeBar();

        // Setup particles
        blockParticles = transform.Find("BlockParticles").GetComponent<ParticleSystem>();
        dashParticles = transform.Find("DashParticles").GetComponent<ParticleSystem>();
        stunParticles = transform.Find("StunParticles").GetComponent<ParticleSystem>();
        collidingParticles = transform.Find("CollidingParticles").GetComponent<ParticleSystem>();

        //Audio
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (gameManager.gameState == GameManager.GameState.PAUSE) return;

        //Lookat
        transform.right = enemy.transform.position - transform.position;

        if (dashing)
        {
            if (dashCooldown > 0)
            {
                dashCooldown -= Time.deltaTime;
            }
            else
            {
                dashing = false;
            }
        }

        if (parrying)
        {
            if (parryingFXCountdown > 0)
            {
                parryingFXCountdown -= Time.deltaTime;
                StartParryingEffect();
            }
            else
            {
                EndParryingEffect();
                parrying = false;
            }
        }

        if (parryingCooldown > 0)
        {
            parryingCooldown -= Time.deltaTime;
        }

        if (stunned)
        {
            if (stunCountdown > 0)
            {
                stunCountdown -= Time.deltaTime;
            }
            else
            {
                stunned = false;
            }
        }
        
        if (takingDamage)
        {
            if (damageCountdown > 0)
            {
                damageCountdown -= Time.deltaTime;
            }
            else
            {
                takingDamage = false;
                StopCoroutine("TakingDamageFX");
                sprite.color = Color.white;
            }
        }

        if (dodging)
        {
            if (dodgeCountdown > 0)
            {
                dodgeCountdown -= Time.deltaTime;
            }
            else
            {
                dodging = false;
            }
            if (dodgeFXCountdown > 0)
            {
                dodgeFXCountdown -= Time.deltaTime;
                if (GetDirToEnemy().magnitude <= minDistToSlowDodge && !gameManager.slow)
                {
                    IEnumerator coroutine = gameManager.Slowmotion(1f, .05f);
                    StartCoroutine(coroutine);
                }
            }
            else
            {
                sprite.color = Color.white;
            } 
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.gameState == GameManager.GameState.PAUSE) return;

        Attract(enemy);
        StopOrbit();
        //transform.LookAt(enemy.transform);
    }

    void UpdateLifeBar()
    {
        lifeBar.value = life / gameManager.maxLife;
        StartCoroutine("LifeBarFX");
    }

    private IEnumerator LifeBarFX()
    {
        Image fill = lifeBar.fillRect.GetComponent<Image>();
        fill.color = Color.white;
        yield return new WaitForSeconds(.1f);
        fill.color = lifeBarColor;
    }

    void PlaySound(AudioSource source, AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
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
            rb.AddForce(resultForce * stopOrbitForce * Time.fixedDeltaTime);
        }
        previousPos = actualPos;
    }

    void Attract(Attractor objToAttract)
    {
        Rigidbody2D rbToAttract = objToAttract.rb;

        Vector2 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        float forceMagnitude = K * distance;

        if (collidingWithPlayer)
        {
            //forceMagnitude += G * (rb.mass + enemy.rb.mass) / distance;
        }

        Vector2 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }

    public void Dodge(Vector2 dir)
    {
        if (stunned || collidingWithPlayer || takingDamage || dodging) return;

        dodging = true;

        dodgeCountdown = dodgeCooldown;
        
        // Dodge FX
        Color dodgingColor = spriteColor;
        dodgingColor.a = 0.3f;
        sprite.color = dodgingColor;
        dodgeFXCountdown = dodgeFXTime;

        PlaySound(audioSource, dodgeSound, dodgeVolume);
        Vector2 dirToEnemy = GetDirToEnemy();
        rb.velocity = Vector2.zero;
        Vector2 direction = dir - (Vector2.Dot(dir, dirToEnemy)/ Vector2.Dot(dirToEnemy, dirToEnemy) * dirToEnemy);
        rb.AddForce(direction.normalized * dodgeImpulse, ForceMode2D.Impulse);
    }

    public void Dash()
    {
        if (!dashing && !blocking && !stunned && !collidingWithPlayer && !takingDamage)
        {
            dashing = true;
            dashCooldown = dashTime;

            Vector2 direction = enemy.transform.position - transform.position;
            rb.AddForce(direction.normalized * dashImpulse, ForceMode2D.Impulse);
            PlaySound(audioSource, dashSounds[Random.Range(0, dashSounds.Length - 1)], dashVolume);
            dashParticles.Play();
        }
    }

    public void Block()
    {
        if (!stunned && !collidingWithPlayer && !takingDamage)
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
            PlaySound(audioSource, dashOnDashSound, dashOnDashVolume);
            return AttackResult.ATTACK;
        }
        if (parrying)
        {
            BlockAttack();
            PlaySound(audioSource, stunBlockSound, stunBlockVolume);
            return AttackResult.PARRY;
        }
        if (blocking)
        {
            BlockAttack();
            PlaySound(audioSource, blockSound, blockVolume);
            return AttackResult.BLOCK;
        } else
        {
            DamagePushBack();
            takingDamage = true;
            damageCountdown = damageTime;
            StartCoroutine("TakingDamageFX");
            ChangeLife(gameManager.dashDamage);
            PlaySound(audioSource, damageSound, damageVolume);
            return AttackResult.DAMAGED;
        }
    }

    private IEnumerator TakingDamageFX()
    {
        sprite.sprite = neutralSprite;
        for (; ; )
        {
            if (sprite.color == Color.white)
                sprite.color = spriteColor;
            else
                sprite.color = Color.white;
            yield return new WaitForSeconds(.2f);
        }
    }

    private void ChangeLife(float amount)
    {
        if (life > 0)
        {
            float newLifeValue = life - amount;
            life = Mathf.Clamp(newLifeValue, 0, gameManager.maxLife);
            UpdateLifeBar();
            if (life == 0)
            {
                gameManager.EndLevel(gameObject);
            }
        }
    }

    private void Stunned()
    {
        stunned = true;
        stunCountdown = stunTime;
        dashing = false;
        ReleaseBlock();
        PlaySound(audioSource, stunSound, stunVolume);
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
            else
            {
                StartCoroutine("Split");
                
            }
        }

        if (collision.gameObject.tag == "Borda")
        {
            TakeEdgeDamage();
            EdgeDamageFX(collision);
            PlaySound(audioSource, edgeSound, edgeVolume);
        }

    }

    private void EdgeDamageFX(Collision2D collision)
    {
        Vector2 point = collision.contacts[0].point;
        Vector2 pointNormal = collision.contacts[0].normal;
        Quaternion rot = Quaternion.LookRotation(pointNormal);
        var obj = Instantiate(borderParticlesPrefab, point, rot);
        camShake.Shake(BorderCamShake_Dur, BorderCamShake_Amp, BorderCamShake_Freq);
        Destroy(obj.gameObject, 1f);
        return;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        StopCoroutine("Split");
        collidingParticles.Stop();
        camShake.StopCamShaking();
        collidingWithPlayer = false;
    }
    

    private IEnumerator Split()
    {
        yield return new WaitForSeconds(.1f);
        collidingWithPlayer = true;
        rb.velocity = Vector2.zero;
        enemy.rb.velocity = Vector2.zero;
        collidingParticles.Play();
        PlaySound(audioSource, collidingSound, collidingVolume);
        camShake.StartCamShaking(CollidingCamShake_Amp, CollidingCamShake_Freq);
        float forceMagnitude = splitForce;
        yield return new WaitForSeconds(1);
        rb.velocity = Vector2.zero;
        rb.AddForce(GetDir().normalized * forceMagnitude, ForceMode2D.Impulse);
        StopCoroutine("Split");
        collidingParticles.Stop();
        camShake.StopCamShaking();
    }

    private void TakeEdgeDamage()
    {
        ChangeLife(gameManager.edgeDamage);
        takingDamage = true;
        damageCountdown = damageTime;
        StartCoroutine("TakingDamageFX");
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
