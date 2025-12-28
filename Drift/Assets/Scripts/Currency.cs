using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Currency : MonoBehaviour
{
    private bool followPlayer = false;
    protected GameObject player = null;
    public float followSpeed = 6f;
    private bool canBePickedUp = false;

    protected virtual void Start()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(InitializeTimer());
    }

    private IEnumerator InitializeTimer()
    {
        yield return new WaitForSeconds(0.5f);
        GetComponent<CircleCollider2D>().enabled = true;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            followPlayer = true;
            canBePickedUp = false;
            player = collision.gameObject;
            StartCoroutine(WaitBeforePickUp());
        }
    }

    protected virtual void Update()
    {
        if (followPlayer && player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            transform.position += followSpeed * Time.deltaTime * dir;
        }

        if (canBePickedUp)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < player.GetComponent<Car>().stats.PickupRange.Value)
            {
                followPlayer = false;
                canBePickedUp = false;
                OnPickUp();
            }
        }
    }

    protected abstract void OnPickUp();

    private IEnumerator WaitBeforePickUp()
    {
        yield return new WaitForSeconds(0.3f);
        canBePickedUp = true;
    }
}
