using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AttackModeButton : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;        // Speed of pulse
    public float pulseAmount = 0.1f;     // Scale multiplier (10% bigger)

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    void OnEnable()
    {
        // When enabled, store scale and start pulsing
        originalScale = transform.localScale;
        pulseCoroutine = StartCoroutine(PulseEffect());
    }

    void OnDisable()
    {
        // Stop pulsing when disabled
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        transform.localScale = originalScale;
    }

    private IEnumerator PulseEffect()
    {
        while (true)
        {
            // Grow
            yield return ScaleTo(originalScale * (1 + pulseAmount), pulseSpeed);
            // Shrink
            yield return ScaleTo(originalScale, pulseSpeed);
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float speed)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
            yield return null;
        }
    }
}
