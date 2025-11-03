using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector2.up);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Car car = collision.gameObject.GetComponent<Car>();
            if (car.isInAttackMode && car.isDrifting && Mathf.Abs(car.turnInput) > 0.5f)
            {
                Destroy(gameObject);
            }
            else
            {
                car.carHealth -= 1;
                Destroy(gameObject);
            }     
        }

        else if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
