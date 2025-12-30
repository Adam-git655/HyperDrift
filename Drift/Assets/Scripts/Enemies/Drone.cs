using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class Drone : Enemy
{
    public float moveSpeed = 3f;

    private readonly float attackRange = 0.7f;
    public float attackCooldown = 3f;
    private bool isAttacking;
    private float attackTimer;

    private Rigidbody2D rb;
    Car car;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        car = player.GetComponent<Car>();

        isAttacking = false;
    }

    private void Update()
    {
        if (player == null || !canMove) return;

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceToPlayer > 0.2f)
        {
            Vector3 dir = (player.position - transform.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.rotation = angle + 90f;

            rb.velocity = dir * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (distanceToPlayer <= attackRange && !car.isInAttackMode)
        {
            isAttacking = true;
        }    
        else
        {
            isAttacking = false;
            attackTimer = 0f;
        }

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                car.TakeDamage(1f);
                attackTimer = 0f;
            }
        }
    }

    public override IEnumerator Stun(float duration)
    {
        rb.velocity = Vector2.zero;
        return base.Stun(duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (car.isInAttackMode && car.isDrifting && car.canMove)
            {
                TakeDamage(car.stats.Damage.Value);
            }
            else
            {
                car.TakeDamage(1.5f);
            }
        }
    }
}
