using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigManager : MonoBehaviour {

    private GameManager gameManager;

    public Slider dashTime;
    public Slider dashImpulse;
    public Slider blockPushBackForce;
    public Slider blockAttackPushBackForce;
    public Slider damagePushBackForce;
    public Slider attackPushBackForce;
    public Slider splitForce;
    public Slider K;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Use this for initialization
    void Start() {
        dashTime.value = gameManager.dashTime;
        dashImpulse.value = gameManager.dashImpulse;
        blockPushBackForce.value = gameManager.blockPushBackForce;
        blockAttackPushBackForce.value = gameManager.blockAttackPushBackForce;
        damagePushBackForce.value = gameManager.damagePushBackForce;
        attackPushBackForce.value = gameManager.attackPushBackForce;
        splitForce.value = gameManager.splitForce;
        K.value = gameManager.K;
    }

    // Update is called once per frame
    void Update() {

    }

    public void SaveConfig()
    {
        gameManager.dashTime = dashTime.value;
        gameManager.dashImpulse = dashImpulse.value;
        gameManager.blockPushBackForce = blockPushBackForce.value;
        gameManager.blockAttackPushBackForce = blockAttackPushBackForce.value;
        gameManager.damagePushBackForce = damagePushBackForce.value;
        gameManager.attackPushBackForce = attackPushBackForce.value;
        gameManager.splitForce = splitForce.value;
        gameManager.K = K.value;
    }
}
