using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    private bool followPlayer = false;
    GameObject player = null;
    public float followSpeed = 6f;
    private bool canBePickedUp = false;

    private void Start()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(InitializeTimer());
    }

    IEnumerator InitializeTimer()
    {
        yield return new WaitForSeconds(0.5f);
        GetComponent<CircleCollider2D>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            followPlayer = true;
            canBePickedUp = false;
            player = collision.gameObject;
            StartCoroutine(WaitBeforePickUp());
        }
    }

    private void Update()
    {
        if (followPlayer && player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            transform.position += followSpeed * Time.deltaTime * dir;
        }

        if (canBePickedUp)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < 2f)
            {
                followPlayer = false;
                canBePickedUp = false;
                player.GetComponent<Car>().gears += 1;
                Destroy(gameObject);
            }
        }
    }

    IEnumerator WaitBeforePickUp()
    {
        yield return new WaitForSeconds(0.3f);
        canBePickedUp = true;
    }
}
