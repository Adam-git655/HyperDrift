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
        buttonText.SetActive(false);
    }

    public void OnHover()
    {
        transform.localScale = hoverScale;
        buttonText.SetActive(true);
    }

    public void OnHoverExit()
    {
        transform.localScale = originalScale; 
        buttonText.SetActive(false);
    }
}
