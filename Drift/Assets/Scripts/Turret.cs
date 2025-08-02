using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform player;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject gearPrefab;
    [SerializeField] private float rotationSpeed = 200.0f;
    [SerializeField] private float lockOnTime = 1.5f;
    [SerializeField] private float burstCount = 5f;
    [SerializeField] private float burstInterval = 0.2f;

    private float lockOnTimer = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
        {
            lockOnTimer += Time.deltaTime;
            if (lockOnTimer >= lockOnTime)
            {
                StartCoroutine(FireBurst());
                lockOnTimer = 0f;
            }
        }
        else
        {
            lockOnTimer = 0f;
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Car car = collision.GetComponent<Car>();
            if (car.isInAttackMode && car.isDrifting && (Mathf.Abs(car.turnInput) > 0.5f || collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 75f))
            {
                car.DriftSpeedBoost();
                Destroy(gameObject);
                Instantiate(gearPrefab, transform.position, transform.rotation);
            }
            else
                car.carHealth -= 3;
        }
    }
}
