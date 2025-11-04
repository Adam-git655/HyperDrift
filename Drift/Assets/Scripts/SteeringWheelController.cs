using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteeringWheelController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    public float maxSteeringAngle = 270f; // ±135° of rotation
    public float returnSpeed = 300f;      // speed to recenter

    [Header("Output")]
    [Range(-1f, 1f)] public float steerInput = 0f;

    private RectTransform wheelRect;
    private float currentAngle = 0f;
    private bool isHolding = false;

    private Vector2 wheelCenter;
    private float startAngle;

    private void Awake()
    {
        wheelRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        gameObject.SetActive(false);
#elif UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
#endif
        wheelCenter = wheelRect.position;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        UpdateWheel(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isHolding)
            UpdateWheel(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }
    private void Update()
    {
        if (!isHolding)
        {
            // Return wheel smoothly to center
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
            steerInput = -currentAngle / (maxSteeringAngle / 2f);
            wheelRect.localEulerAngles = new Vector3(0f, 0f, currentAngle);
        }
    }

    private void UpdateWheel(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - wheelCenter;

        // Current absolute angle from up
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);

        // Calculate difference from previous frame, handling wrap-around
        float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);

        // Accumulate rotation and clamp
        currentAngle += deltaAngle;
        currentAngle = Mathf.Clamp(currentAngle, -maxSteeringAngle / 2f, maxSteeringAngle / 2f);

        // Inverted for car input
        steerInput = -currentAngle / (maxSteeringAngle / 2f);

        // Rotate wheel visually
        wheelRect.localEulerAngles = new Vector3(0f, 0f, currentAngle);
    }

}
