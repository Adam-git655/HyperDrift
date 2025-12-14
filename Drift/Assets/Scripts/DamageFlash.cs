using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Material material;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;
    }

    public IEnumerator PlayDamageFlash()
    {
        //Set Max default flash color
        material.SetColor("_FlashColor", flashColor);

        //Lerp flash color to 0
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / flashTime));
            material.SetFloat("_FlashAmount", currentFlashAmount);

            yield return null;
        }

        material.SetFloat("_FlashAmount", 0f);
    }
}
