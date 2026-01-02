using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    private Vector3 originalScale;
    public Vector3 hoverScale;
    public GameObject buttonText;

    private void Start()
    {
        originalScale = transform.localScale;

        if (buttonText != null )
            buttonText.SetActive(false);
    }

    public void OnHover()
    {
        transform.localScale = hoverScale;

        if (buttonText != null)
            buttonText.SetActive(true);
    }

    public void OnHoverExit()
    {
        transform.localScale = originalScale;

        if (buttonText != null)
            buttonText.SetActive(false);
    }
}
