using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniBoss : Enemy
{
    public GameObject bulletPrefab;

    public Slider healthBarSlider;

    public float timeBetweenPatterns = 2.0f;
    public int radialBulletCount = 16;

    public float moveSpeed = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Car car;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
        car = player.GetComponent<Car>();

        healthBarSlider = GameObject.Find("Canvas").transform.GetChild(4).GetComponent<Slider>();
        healthBarSlider.gameObject.SetActive(true);
        healthBarSlider.maxValue = health;

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
            SoundManager.PlaySound(SoundType.BulletFire);
            float angle = i * (360f / radialBulletCount);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(bulletPrefab, transform.position, rotation);
        }
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        healthBarSlider.value = health;

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
                TakeDamage(car.stats.Damage.Value);
            else
                car.TakeDamage(7f);
        }
    }
}
