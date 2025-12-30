using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPPulseWeapon : Weapon
{
    private EMPPulseWeaponData EMPpulseWeaponData;
    private Material material;
    private static int waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private float stunTime;

    protected override void Awake()
    {
        base.Awake();
        material = GetComponent<SpriteRenderer>().material;
        material.SetFloat(waveDistanceFromCenter, -0.1f);

        EMPpulseWeaponData = (EMPPulseWeaponData)data;
        stunTime = EMPpulseWeaponData.stunTime;
    }

    protected override void Fire()
    {
        //Shockwave every cooldown seconds
        ApplyEMP();
        StartCoroutine(ShockWaveAction(-0.1f, 1f));
        SoundManager.PlaySound(SoundType.ShockWave);
    }

    private void ApplyEMP()
    {
        foreach (Enemy enemy in Enemy.AllEnemies)
        {
            if (IsOnScreen(enemy))
            {
                StartCoroutine(enemy.Stun(stunTime));
            }
        }
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        material.SetFloat(waveDistanceFromCenter, startPos);

        float lerpedAmount = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < EMPpulseWeaponData.shockWaveAnimTime)
        {
            elapsedTime += Time.deltaTime;
            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / EMPpulseWeaponData.shockWaveAnimTime));
            material.SetFloat(waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
    }

    protected override void ApplyLevelUp(int level)
    {
        stunTime *= 1.15f;
        cooldown *= 0.85f;
    }

    private bool IsOnScreen(Enemy enemy)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(enemy.transform.position);
        return vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
    }
}
