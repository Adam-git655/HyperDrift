using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float health;
    public GameObject gearPrefab;
    public GameObject floatingTextPrefab;
    public GameObject salvageCorePrefab;
    [SerializeField] protected int gearsToSpawnOnDeath = 1;
    [SerializeField] protected float salvageCoreDropChance = 0.1f;
    [SerializeField] protected int salvageCoresToSpawnOnDeath = 1;

    protected Transform player;
    protected DamageFlash damageFlash;

    protected virtual void Awake()
    {
        damageFlash = GetComponent<DamageFlash>();
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
            player = playerObj.transform;
    }

    public virtual void TakeDamage(float amount)
    {
        SoundManager.PlaySound(SoundType.EnemyHit);
        health -= amount;

        if (floatingTextPrefab != null)
        {
            int damageToShow = Mathf.RoundToInt(Random.Range(amount - 2, amount + 2));

            if (damageToShow <= 0)
                damageToShow = 1;

            ShowFloatingText(damageToShow);
        }
            

        if (health <= 0f)
        {
            StartCoroutine(DoFinalFlashAndDie());
        }
        else
        {
            StartCoroutine(damageFlash.PlayDamageFlash());
        }
    }

    protected void ShowFloatingText(float damage)
    {
        GameObject text = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
        text.GetComponent<TMP_Text>().text = damage.ToString();
    }

    protected virtual IEnumerator DoFinalFlashAndDie()
    {
        yield return StartCoroutine(damageFlash.PlayDamageFlash());

        //spawn gears
        for (int i = 0; i < gearsToSpawnOnDeath; i++)
            Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);

        //spawn salvage cores
        if (Random.value <= salvageCoreDropChance)
        {
            for (int i = 0; i < salvageCoresToSpawnOnDeath; i++)
                Instantiate(salvageCorePrefab, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), transform.position.z), transform.rotation);
        }
            
        Destroy(gameObject);
    }
}
