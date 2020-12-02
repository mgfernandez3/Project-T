﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnemyHealth;

public abstract class EnemyController : MonoBehaviour {

    public float movingSpeed = 1f;
    
    protected Transform playerTransform;

    protected bool isStunned;

    protected Animator animator;
    private Rigidbody2D enemyRigidbody;
    private Collider2D enemyCollider;

    private SpriteManager spriteManager;

    private EnemyHealthController healthController;

    private void Awake() {
        this.playerTransform = GameObject.FindGameObjectsWithTag("Player")[0].transform;

        this.animator = GetComponent<Animator>();
        this.enemyRigidbody = GetComponent<Rigidbody2D>();
        this.enemyCollider = GetComponent<Collider2D>();
        this.healthController = GetComponent<EnemyHealthController>();

        this.spriteManager = new SpriteManager(GetComponent<SpriteRenderer>());
        spriteManager.SetMainColor(healthController.GetColor());

    }

    protected void Start() {
        this.isStunned = false;
        this.animator.SetBool("Moving", true);
    }

    protected void Update() {
        if (!isStunned) {
            RotateEnemyToPlayer();
            Move();
            SecondaryActions();
        }
    }

    private void RotateEnemyToPlayer() {
        Vector2 directionToPlayer = playerTransform.position - this.transform.position;
        float angleToPlayer = Vector2.SignedAngle(Vector2.right, directionToPlayer);
        this.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
    }

    private void Move() {
        Vector3 deltaVector = CalculateDirection().normalized * this.movingSpeed * Time.deltaTime;
        this.transform.Translate(deltaVector, Space.World);
    }

    protected abstract Vector2 CalculateDirection();

    protected abstract void SecondaryActions();



    public void StartInvulnerabilityTime() {
        StartCoroutine(InvulnerabilityTime());
    }

    protected IEnumerator InvulnerabilityTime() {
        Stun();

        yield return StartCoroutine(this.spriteManager.HitFlash());
        yield return StartCoroutine(this.spriteManager.InvulnerabilityFlash(1f));
        this.spriteManager.ResetColor();

        Unstun();
    }

    protected virtual void Stun(){
        this.isStunned = true;

        //Change animation
        this.animator.SetBool("Moving", false);

        //Disable physics
        enemyRigidbody.isKinematic = true;
        enemyRigidbody.velocity = Vector2.zero;
        enemyRigidbody.angularVelocity = 0f;

        //Disable collisions
        this.enemyCollider.enabled = false;
    }

    protected virtual void Unstun(){
        this.isStunned = false;

        //Change animation
        this.animator.SetBool("Moving", true);

        //Enable physics back
        enemyRigidbody.isKinematic = false;

        //Enable collisions back
        this.enemyCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.CompareTag("Player")) {
            collision.collider.SendMessageUpwards("AddDamage", 1);
        }
    }
}
