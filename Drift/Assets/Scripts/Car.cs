using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Globals
{
    public static float maxCarHealth = 100.0f;
    public static float attackModeDuration = 10f;
    public static int totalGears = 0;
}

public class Car : MonoBehaviour
{
    private CarControls controls;
    public SteeringWheelController steeringWheelController;

    private float steerInput;
    private bool driftPressed;
    private bool attackModePressed;

    public GameObject attackModeButtonUI;

    public bool autoThrottleEnabled = true;

    public float carHealth = 100f;

    public GameObject GameOverPanel;
    public Text timeSurvivedCount;
    public Text gearsCollectedCount;
    public Tilemap tilemap;

    public Slider HealthBarSlider;

    public ParticleSystem ElectricShockFx;

    public Text GearsCountText;
    public int gears = 0;

    public float maxSpeed = 10f;
    public float accelerationTime = 1f;
    public float friction = 1.2f;
    public float rotateDegreesPerSec = 220f;
    private bool canMove = true;

    public float turnInput;

    public float driftMaxAngle = 60f;
    public float driftAngleBuildSpeed = 10f;
    public float driftAngleReturnSpeed = 5f;
    public float maxDriftSpeed = 2.5f;
    public float driftSpeedBoost = 2.5f;
    public float driftSpeedDecay = 5.0f;
    private float currentDriftSpeed = 1.0f;

    public bool isInAttackMode = false;
    private float attackModeTimer = 0f;
    public GameObject shieldAuraVfx;

    public bool isDrifting = false;
    public Slider driftMeterSlider;
    private float driftChargeMeter = 0f;
    private float maxDriftCharge = 100f;
    private float driftChargePerSecond = 25f;

    public float trackSegLength = .15f;
    public int trackSegCount = 100;
    public Transform[] wheels;
    public Material trailMaterial;

    Rigidbody2D m_RigidBody;
    private Transform m_VelDir;
    float m_AppliedSpeed = 0;
    private List<WheelTrack> m_WheelTracks;
    private Vector3 m_LastPos;

    public Sprite BlueEnergyBarSprite;
    public Sprite GreyEnergyBarSprite;

    public AudioSource EngineAudioSource;
    public AudioSource DriftAudioSource;

    private bool isGameOver = false;

    private void Awake()
    {
        controls = new CarControls();

        controls.Driving.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += ctx => steerInput = 0f;

        controls.Driving.Drift.performed += ctx => driftPressed = true;
        controls.Driving.Drift.canceled += ctx => driftPressed = false;

        controls.Driving.AttackMode.performed += ctx => attackModePressed = true;

    }
    private void OnEnable()
    {
        controls.Driving.Enable();
    }

    private void OnDisable()
    {
        controls.Driving.Disable();
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        isGameOver = false;
        m_RigidBody = GetComponent<Rigidbody2D>();
        HealthBarSlider.maxValue = Globals.maxCarHealth;
        carHealth = Globals.maxCarHealth;
        driftMeterSlider.maxValue = maxDriftCharge;
        m_VelDir = new GameObject("VelocityDirection").transform;
        m_VelDir.parent = transform;
        m_VelDir.localPosition = Vector3.zero;
        m_VelDir.localEulerAngles = Vector3.zero;
        m_LastPos = transform.position;
        m_WheelTracks = new List<WheelTrack>();
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelTrack wheel = new WheelTrack();
            wheel.Init(wheels[i], trailMaterial, trackSegCount);
            m_WheelTracks.Add(wheel);
        }
        ElectricShockFx.Pause();
        ElectricShockFx.gameObject.SetActive(false);
        attackModeButtonUI.SetActive(false);
        GameOverPanel.SetActive(false);
    }

    void Update()
    {
        HealthBarSlider.value = carHealth;

        if (carHealth <= 0f && !isGameOver)
        {
            isGameOver = true;
            GameOverPanel.SetActive(true);
            tilemap.color = Color.gray;
            Globals.totalGears += gears;
            int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
            int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad % 60f);
            timeSurvivedCount.text = $"{minutes}m {seconds}s";
            gearsCollectedCount.text = gears.ToString();
            Time.timeScale = 0f;
        }

        GearsCountText.text = gears.ToString();

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        turnInput = canMove ? steerInput : 0f;

#elif UNITY_ANDROID || UNITY_IOS
        turnInput = canMove && steeringWheelController != null ? steeringWheelController.steerInput : 0f;
#endif

        isDrifting = canMove && driftPressed;
        
        ManageSound();

        if (autoThrottleEnabled && canMove)
        { 
            m_AppliedSpeed += maxSpeed * Time.deltaTime * accelerationTime;
        }
        else
        {
            m_AppliedSpeed -= friction * Time.deltaTime;
        }

        m_AppliedSpeed = Mathf.Clamp(m_AppliedSpeed, 0f, maxSpeed);

        if (m_AppliedSpeed < .5f)
            m_VelDir.localEulerAngles = Vector3.zero;

        float zVal = transform.eulerAngles.z;

        if (m_AppliedSpeed > 0.1f && Mathf.Abs(turnInput) > 0.1f)
        {
            if (!isDrifting)
                zVal += rotateDegreesPerSec * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / maxSpeed);
            else
                zVal += rotateDegreesPerSec * 2.3f * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / maxSpeed);
        }
        else
        {
            // Aggressive straighten out towards velocity direction
            zVal = Mathf.LerpAngle(transform.eulerAngles.z, m_VelDir.eulerAngles.z, Time.deltaTime * 5f);
        }

        transform.eulerAngles = new Vector3(0f, 0f, zVal);

        float targetDriftAngle = 0f;

        if (isDrifting && Mathf.Abs(turnInput) > 0.1f)
        {
            targetDriftAngle = turnInput * driftMaxAngle;
            currentDriftSpeed = Mathf.Min(currentDriftSpeed + driftSpeedBoost * Time.deltaTime, maxDriftSpeed);

            if (!isInAttackMode)
            {
                driftChargeMeter += driftChargePerSecond * Time.deltaTime;
                driftChargeMeter = Mathf.Min(driftChargeMeter, maxDriftCharge);
            }
        }
        else
        {
            currentDriftSpeed = Mathf.Lerp(currentDriftSpeed, 1f, Time.deltaTime * driftSpeedDecay);
        }

        driftMeterSlider.value = driftChargeMeter;

        if (driftChargeMeter >= maxDriftCharge && !isInAttackMode)
        {
            driftMeterSlider.fillRect.GetComponent<Image>().sprite = BlueEnergyBarSprite;

#if UNITY_ANDROID || UNITY_IOS
            attackModeButtonUI.SetActive(true);
#endif

            if (attackModePressed && canMove)
            {
                ActivateAttackMode();
                attackModePressed = false;
                attackModeButtonUI.SetActive(false);
            }
        }

        if (isInAttackMode)
        {
            attackModeTimer -= Time.deltaTime;
            driftChargeMeter = (attackModeTimer / Globals.attackModeDuration) * 100f;
            if (attackModeTimer <= 0f)
            {
                isInAttackMode = false;
                driftChargeMeter = 0f;
                driftMeterSlider.fillRect.GetComponent<Image>().sprite = GreyEnergyBarSprite;
                shieldAuraVfx.SetActive(false);
                Debug.Log("ATTACK MODE DISABLED :(");
            }
        }
        else
        {
            maxDriftSpeed = 1f;
        }

        currentDriftSpeed = Mathf.Clamp(currentDriftSpeed, 1.0f, maxDriftSpeed);

        float driftAngleLerpSpeed = isDrifting ? driftAngleBuildSpeed : driftAngleReturnSpeed;

        m_VelDir.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(m_VelDir.localEulerAngles.z, targetDriftAngle, Time.deltaTime * driftAngleLerpSpeed));

        if (Vector3.Distance(transform.position, m_LastPos) > trackSegLength)
        {
            m_LastPos = transform.position;

            float angleOffset = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, m_VelDir.eulerAngles.z)) / 90f;
            Color newAlpha = new Color(0, 0, 0, Mathf.Min(angleOffset, .5f));

            foreach (WheelTrack wheel in m_WheelTracks)
            {
                wheel.AddSegment(newAlpha);
            }
        }
    }


    void ManageSound()
    {
        // Engine sound control
        if (GetComponent<Rigidbody2D>().velocity.sqrMagnitude > 5f && !isGameOver)
        {
            if (!EngineAudioSource.isPlaying)
            {
                EngineAudioSource.Play();
            }
        }
        else
        {
            if (EngineAudioSource.isPlaying)
            {
                EngineAudioSource.Stop();
            }
        }

        // Drift sound control with volume fading
        if (isDrifting && Mathf.Abs(turnInput) > 0.5f && !isGameOver)
        {
            if (!DriftAudioSource.isPlaying)
            {
                DriftAudioSource.Play();
            }

            // Smoothly fade in drift sound volume
            DriftAudioSource.volume = Mathf.Lerp(DriftAudioSource.volume, 0.5f, Time.deltaTime * 5f);
        }
        else
        {
            // Smoothly fade out drift sound volume
            DriftAudioSource.volume = Mathf.Lerp(DriftAudioSource.volume, 0.0f, Time.deltaTime * 5f);

            // Stop playing when volume is very low to free resources
            if (DriftAudioSource.isPlaying && DriftAudioSource.volume < 0.01f)
            {
                DriftAudioSource.Stop();
            }
        }
    }


    private void ActivateAttackMode()
    {
        isInAttackMode = true;
        shieldAuraVfx.SetActive(true);
        attackModeTimer = Globals.attackModeDuration;
        Debug.Log("ATTACK MODE BABY");
    }

    public void GetElectrocuted()
    {
        canMove = false;
        carHealth -= 5;
        ElectricShockFx.gameObject.SetActive(true);
        ElectricShockFx.Play();
        StartCoroutine(RegainControlsAfterElectricShock());
    }

    IEnumerator RegainControlsAfterElectricShock()
    {
        yield return new WaitForSeconds(2f);
        ElectricShockFx.gameObject.SetActive(false);
        ElectricShockFx.Pause();
        canMove = true;
    }

    public void OnMainMenuButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitGameButtonPressed()
    {
        Application.Quit();
    }

    private void FixedUpdate()
    {
        Vector2 forwardVelocity = Vector2.Dot(m_RigidBody.velocity, transform.up) * transform.up;
        Vector2 sideVelocity = Vector2.Dot(m_RigidBody.velocity, transform.right) * transform.right;

        // Kill lateral velocity faster for tighter drifts
        m_RigidBody.velocity = forwardVelocity + sideVelocity * 0.01f;

        // Apply velocity in the drift direction
        m_RigidBody.velocity = currentDriftSpeed * m_AppliedSpeed * m_VelDir.up;
    }

    private float Remap(float val, float srcMin, float srcMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(srcMin, srcMax, val));
    }

    private class WheelTrack
    {
        private List<Transform> lines;
        private Vector3 lastPos;
        private Transform transform;
        private Transform lineHolder;
        private int lineIndex = 0;
        private Material mat;
        private int segmentCount;

        public void Init(Transform tf, Material material, int segCount)
        {
            lines = new List<Transform>();
            transform = tf;
            mat = material;
            segmentCount = segCount;
            lineHolder = new GameObject(transform.name + "Tracks").transform;
            lastPos = tf.position;
        }

        public void AddSegment(Color color)
        {
            if (lines.Count < segmentCount)
            {
                GameObject go = new GameObject();
                go.transform.parent = lineHolder;
                LineRenderer newLine = go.AddComponent<LineRenderer>();
                newLine.material = mat;
                newLine.startWidth = newLine.endWidth = .05f;
                newLine.positionCount = 2;
                lines.Add(go.transform);
            }

            LineRenderer line = lines[lineIndex].GetComponent<LineRenderer>();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, lastPos);
            line.startColor = line.endColor = color;

            lastPos = transform.position;
            lineIndex = (lineIndex + 1) % segmentCount;
        }
    }
}
