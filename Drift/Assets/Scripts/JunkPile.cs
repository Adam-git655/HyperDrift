using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunkPile : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            if (collision.gameObject.GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 75f)
            {
                collision.gameObject.GetComponent<Car>().carHealth -= 4f;
            }
        }
    }
}
