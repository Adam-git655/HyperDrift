using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grinder : MonoBehaviour
{
    public float damage;
    public float rotationSpeed;

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.forward);  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
}
