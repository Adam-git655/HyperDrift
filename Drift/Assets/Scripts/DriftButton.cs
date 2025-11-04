using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftButton : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        gameObject.SetActive(false);
#elif UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
#endif
    }

}
