using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public Transform player;
    public GameObject Gear;
    public float moveSpeed = 1f;

    public float chargeSpeed = 10f;
    private float detectionRange = 2f;
    public float chargeCooldown = 5f;
    public float chargeDuration = 1f;
    private bool isCharging = false;
    private float chargeTimer;

    private Rigidbody2D rb;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
            if (car.isInAttackMode && car.isDrifting && (Mathf.Abs(car.turnInput) > 0.5f || collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 75f))
            {
                car.DriftSpeedBoost();
                Destroy(gameObject);
                Instantiate(Gear, transform.position, Quaternion.identity);
            }
            else if (isCharging)
            {
                car.carHealth -= 5f;
            }
            else
            {
                car.carHealth -= 1f;
            }
        }
    }
}
