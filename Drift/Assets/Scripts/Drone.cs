using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Transform player;
    public GameObject Gear;
    public float moveSpeed = 3f;

    private readonly float attackRange = 0.7f;
    public float attackCooldown = 3f;
    private bool isAttacking = false;
    private float attackTimer;

    private Rigidbody2D rb;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isAttacking = false;
        car = player.GetComponent<Car>();
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (player != null && distanceToPlayer > 0.2f)
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
                car.carHealth -= 1;
                attackTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (car.isInAttackMode && car.isDrifting && (Mathf.Abs(car.turnInput) > 0.5f || collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 60f))
            {
                Destroy(gameObject);
                Instantiate(Gear, transform.position, transform.rotation);
            }
            else
            {
                car.carHealth -= 1.5f;
            }
        }
    }
}
