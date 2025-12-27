using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FireTrailDamage : MonoBehaviour
{
    List<Vector2> trailPoints = new List<Vector2>();

    private float damage;
    private float tickCooldown;
    private float cooldownTimer;
    private float radius;

    public LayerMask enemyLayer;

    public void Init(float damage, float cooldown, float radius)
    {
        this.damage = damage;
        this.radius = radius;
        tickCooldown = cooldown;
        cooldownTimer = 0f;
    }

    public void SetStats(float damage, float cooldown)
    {
        this.damage = damage;
        tickCooldown = cooldown;
    }

    public void UpdateTrail(List<Vector2> points)
    {
        trailPoints.Clear();
        trailPoints.AddRange(points);
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer < 0f)
        {
            ApplyDamage();
            cooldownTimer = tickCooldown;
        }
    }

    private void ApplyDamage()
    {
        HashSet<Enemy> alreadyDamaged = new HashSet<Enemy>();
        for (int i = 0; i < trailPoints.Count; i+=2)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(trailPoints[i], radius, enemyLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Enemy enemy))
                {
                    if (!alreadyDamaged.Add(enemy))
                        continue;

                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
