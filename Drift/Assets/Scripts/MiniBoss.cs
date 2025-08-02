using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoss : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public GameObject gearPrefab;

    public int health = 10;

    public float timeBetweenPatterns = 2.0f;
    public int radialBulletCount = 16;

    public float moveSpeed = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
        player = GameObject.FindGameObjectWithTag("Player").transform;
        car = player.GetComponent<Car>();

        StartCoroutine(AttackPatternLoop());
    }

    IEnumerator AttackPatternLoop()
    {
        while(true)
        {
            FireRadialBurst();
            yield return new WaitForSeconds(timeBetweenPatterns);
        }
    }

    void FireRadialBurst()
    {
        for (int i = 0; i < radialBulletCount; i++)
        {
            float angle = i * (360f / radialBulletCount);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(bulletPrefab, transform.position, rotation);
        }
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (health <= 0)
        {
            Destroy(gameObject);
            for (int i = 0; i < 10; i++)
                Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);
        }

        if (player != null && distanceToPlayer > 0.2f)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.rotation = angle + 90f;

            rb.velocity = dir * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Car car = collision.GetComponent<Car>();
            if (car.isInAttackMode && car.isDrifting && (Mathf.Abs(car.turnInput) > 0.5f || collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 60f))
                health -= 1;
            else
                car.carHealth -= 7f;
        }
    }
}
