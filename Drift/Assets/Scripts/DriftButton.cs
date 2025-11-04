using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftButton : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        gameObject.SetActive(false);
#elif UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
#endif
    }

}
