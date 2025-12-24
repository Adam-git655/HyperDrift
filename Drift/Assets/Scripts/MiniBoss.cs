using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniBoss : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public GameObject gearPrefab;

    public float health = 100f;
    public Slider healthBarSlider;

    [SerializeField] private int gearsToSpawnOnDeath = 10;

    public float timeBetweenPatterns = 2.0f;
    public int radialBulletCount = 16;

    public float moveSpeed = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private DamageFlash damageFlash;

    Car car;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
        player = GameObject.FindGameObjectWithTag("Player").transform;
        car = player.GetComponent<Car>();
        damageFlash = GetComponent<DamageFlash>();

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

    private void TakeDamage(float amount)
    {
        Debug.Log(amount);
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

        Destroy(gameObject);
        healthBarSlider.gameObject.SetActive(false);

        for (int i = 0; i < gearsToSpawnOnDeath; i++)
            Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);
    }
}
