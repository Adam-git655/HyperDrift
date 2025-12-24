using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Transform player;
    public GameObject gearPrefab;
    public float moveSpeed = 3f;
    public float health = 10f;
    [SerializeField] private int gearsToSpawnOnDeath = 1;

    private readonly float attackRange = 0.7f;
    public float attackCooldown = 3f;
    private bool isAttacking = false;
    private float attackTimer;

    private Rigidbody2D rb;

    private DamageFlash damageFlash;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isAttacking = false;
        car = player.GetComponent<Car>();
        damageFlash = GetComponent<DamageFlash>();
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
                car.TakeDamage(1f);
                attackTimer = 0f;
            }
        }
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

    private void TakeDamage(float amount)
    {
        SoundManager.PlaySound(SoundType.EnemyHit);
        health -= amount;

        if (health <= 0f)
        {
            StartCoroutine(DoFinalFlashAndDie());
        }
        else
        {
            StartCoroutine(damageFlash.PlayDamageFlash());
        }
    }

    private IEnumerator DoFinalFlashAndDie()
    {
        yield return StartCoroutine(damageFlash.PlayDamageFlash());

        for (int i = 0; i < gearsToSpawnOnDeath; i++)
            Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);
        
        Destroy(gameObject);
    }
}
