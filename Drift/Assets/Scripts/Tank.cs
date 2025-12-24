using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public Transform player;
    public GameObject gearPrefab;
    public GameObject floatingTextPrefab;
    public float moveSpeed = 1f;
    public float health = 15f;
    [SerializeField] private int gearsToSpawnOnDeath = 2;

    public float chargeSpeed = 10f;
    private float detectionRange = 2f;
    public float chargeCooldown = 5f;
    public float chargeDuration = 1f;
    private bool isCharging = false;
    private float chargeTimer;

    private Rigidbody2D rb;

    private DamageFlash damageFlash;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponent<DamageFlash>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isCharging = false;
        car = player.GetComponent<Car>();
        chargeTimer = 0f;
    }


    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        chargeTimer -= Time.deltaTime;

        if (!isCharging)
        {
            if (distanceToPlayer <= detectionRange)
            {
                rb.velocity = Vector2.zero;

                if (chargeTimer <= 0f)
                    StartCoroutine(ChargeTowardsPlayer());
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        //Move Towards Player 
        Vector3 dir = (player.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rb.rotation = angle + 90f;

        rb.velocity = dir * moveSpeed;
    }

    private IEnumerator ChargeTowardsPlayer()
    {
        isCharging = true;

        Vector2 chargeDir = (player.position - transform.position).normalized;

        float chargeTime = 0f;

        while (chargeTime < chargeDuration)
        {
            rb.velocity = chargeDir * chargeSpeed;
            chargeTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        chargeTimer = chargeCooldown;
        isCharging = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (car.isInAttackMode && car.isDrifting && (Mathf.Abs(car.turnInput) > 0.5f || collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 60f))
            {
                TakeDamage(car.stats.Damage.Value);
            }
            else if (isCharging)
            {
                car.TakeDamage(5f);
            }
            else
            {
                car.TakeDamage(1f);
            }
        }
    }

    private void TakeDamage(float amount)
    {
        SoundManager.PlaySound(SoundType.EnemyHit);
        health -= amount;

        if (floatingTextPrefab != null)
            ShowFloatingText(Mathf.RoundToInt(Random.Range(amount - 2, amount + 2)));

        if (health <= 0f)
        {
            StartCoroutine(DoFinalFlashAndDie());
        }
        else
        {
            StartCoroutine(damageFlash.PlayDamageFlash());
        }
    }

    private void ShowFloatingText(float damage)
    {
        GameObject text = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
        text.GetComponent<TMP_Text>().text = damage.ToString();
    }

    private IEnumerator DoFinalFlashAndDie()
    {
        yield return StartCoroutine(damageFlash.PlayDamageFlash());

        for (int i = 0; i < gearsToSpawnOnDeath; ++i)
            Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);
        
        Destroy(gameObject);
    }
}
