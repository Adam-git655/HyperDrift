using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> AllEnemies = new List<Enemy>();

    [Header("Enemy Settings")]
    public float health;
    public GameObject gearPrefab;
    public GameObject floatingTextPrefab;
    public GameObject salvageCorePrefab;
    public GameObject explosionEffectPrefab;
    [SerializeField] protected int gearsToSpawnOnDeath = 1;
    [SerializeField] protected float salvageCoreDropChance = 0.1f;
    [SerializeField] protected int salvageCoresToSpawnOnDeath = 1;
    [SerializeField] protected float spawnRadiusOnDeath = 0.2f;

    protected Transform player;
    protected DamageFlash damageFlash;

    protected bool canMove = true;

    private void OnEnable()
    {
        AllEnemies.Add(this);
    }

    private void OnDisable()
    {
        AllEnemies.Remove(this);
    }

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
        SoundManager.PlaySound(SoundType.EnemyHit, 1.5f);
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

    public virtual IEnumerator Stun(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    protected void ShowFloatingText(float damage)
    {
        GameObject text = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
        text.GetComponent<TMP_Text>().text = damage.ToString();
    }

    protected virtual IEnumerator DoFinalFlashAndDie()
    {
        yield return StartCoroutine(damageFlash.PlayDamageFlash());

        //Add Explosion Effect
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        SoundManager.PlaySound(SoundType.Explosion, 0.7f);

        //spawn gears
        for (int i = 0; i < gearsToSpawnOnDeath; i++)
            Instantiate(gearPrefab, transform.position + new Vector3(Random.Range(-spawnRadiusOnDeath, spawnRadiusOnDeath), Random.Range(-spawnRadiusOnDeath, spawnRadiusOnDeath), transform.position.z), transform.rotation);

        //spawn salvage cores
        if (Random.value <= salvageCoreDropChance)
        {
            for (int i = 0; i < salvageCoresToSpawnOnDeath; i++)
                Instantiate(salvageCorePrefab, transform.position + new Vector3(Random.Range(-spawnRadiusOnDeath, spawnRadiusOnDeath), Random.Range(-spawnRadiusOnDeath, spawnRadiusOnDeath), transform.position.z), transform.rotation);
        }

        Globals.enemiesKilled++;
        Destroy(gameObject);
    }
}
