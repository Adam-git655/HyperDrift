using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Globals
{
    public static int totalGears = 0;
}

public class Car : MonoBehaviour
{
    //Stats
    public PlayerBaseStats baseStats;
    public PlayerStatsRuntime stats;
    public int gears = 0;

    [Header("Speed")]
    public float accelerationTime = 1.2f; //seconds to reach max speed
    public float decelRate = 4f;
    public float rotateDegreesPerSec = 220f;

    [Header("Drifting")]
    public float driftMaxAngle = 60f;
    public float driftAngleBuildSpeed = 10f;
    public float driftAngleReturnSpeed = 5f;

    public float maxDriftSpeed = 1f;
    public float driftSpeedBoost = 2.5f;
    public float driftSpeedDecay = 5.0f;

    [Header("References")]
    public GameObject attackModeButtonUI;
    public GameObject GameOverPanel;
    public Text timeSurvivedCount;
    public Text gearsCollectedCount;
    public Tilemap tilemap;
    public Slider HealthBarSlider;
    public ParticleSystem ElectricShockFx;
    public Text GearsCountText;
    public GameObject shieldAuraVfx;
    public Slider driftMeterSlider;
    public Transform[] wheels;
    public Material trailMaterial;
    private DamageFlash damageFlash;
    public Sprite BlueEnergyBarSprite;
    public Sprite GreyEnergyBarSprite;
    public AudioSource EngineAudioSource;
    public AudioSource DriftAudioSource;

    //Internals/States/Values
    private Rigidbody2D rb;
    private Transform velDir;
    private float m_AppliedSpeed = 0;
    private float steerInput;
    public float turnInput;

    private float carHealth;
    private bool isGameOver = false;

    private bool attackModePressed;
    public bool canMove = true;

    public bool isInAttackMode = false;
    private float attackModeTimer = 0f;

    private bool driftPressed;
    private float currentDriftSpeed = 1.0f;
    public bool isDrifting = false;
    private float driftChargeMeter = 0f;
    private float maxDriftCharge = 100f;
    private float driftChargePerSecond = 25f;

    public float trackSegLength = .15f;
    public int trackSegCount = 100;
    private List<WheelTrack> m_WheelTracks;
    private Vector3 m_LastPos;

    //Controls/Input
    private CarControls controls;
    public SteeringWheelController steeringWheelController;

    private void Awake()
    {
        //set current stats to runtime stats
        stats = new PlayerStatsRuntime(baseStats);

        controls = new CarControls();

        controls.Driving.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += ctx => steerInput = 0f;

        controls.Driving.Drift.performed += ctx => driftPressed = true;
        controls.Driving.Drift.canceled += ctx => driftPressed = false;

        controls.Driving.AttackMode.performed += ctx => attackModePressed = true;

        stats.MaxHealth.OnValueChanged += OnMaxHealthChanged;
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

        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponentInChildren<DamageFlash>();
        HealthBarSlider.maxValue = stats.MaxHealth.Value;
        carHealth = stats.MaxHealth.Value;
        driftMeterSlider.maxValue = maxDriftCharge;

        velDir = new GameObject("VelocityDirection").transform;
        velDir.parent = transform;
        velDir.localPosition = Vector3.zero;
        velDir.localEulerAngles = Vector3.zero;

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

        //On death 
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

        //MOVEMENT
        turnInput = canMove ? steerInput : 0f;

//For steering wheel support

//#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
//        turnInput = canMove ? steerInput : 0f;

//#elif UNITY_ANDROID || UNITY_IOS
//        turnInput = canMove && steeringWheelController != null ? steeringWheelController.steerInput : 0f;
//#endif

        isDrifting = canMove && driftPressed;
        
        ManageSound();

        //Movement
        float accel = stats.MaxSpeed.Value / accelerationTime;
        float targetSpeed = canMove ? stats.MaxSpeed.Value : 0f;
        float rate = canMove ? accel : decelRate;

        m_AppliedSpeed = Mathf.MoveTowards(
            m_AppliedSpeed,
            targetSpeed,
            rate * Time.deltaTime
        );

        m_AppliedSpeed = Mathf.Clamp(m_AppliedSpeed, 0f, stats.MaxSpeed.Value);

        if (m_AppliedSpeed < .5f)
            velDir.localEulerAngles = Vector3.zero;

        //Rotation
        float zVal = transform.eulerAngles.z;

        if (m_AppliedSpeed > 0.1f && Mathf.Abs(turnInput) > 0.1f)
        {
            if (!isDrifting)
                zVal += rotateDegreesPerSec * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / stats.MaxSpeed.Value);
            else
                zVal += rotateDegreesPerSec * 2.3f * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / stats.MaxSpeed.Value);
        }
        else
        {
            // Aggressive straighten out towards velocity direction
            zVal = Mathf.LerpAngle(transform.eulerAngles.z, velDir.eulerAngles.z, Time.deltaTime * 5f);
        }

        transform.eulerAngles = new Vector3(0f, 0f, zVal);


        //Drift Direction
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
            driftChargeMeter = (attackModeTimer / stats.AttackModeDuration.Value) * 100f;
            if (attackModeTimer <= 0f)
            {
                isInAttackMode = false;
                driftChargeMeter = 0f;
                driftMeterSlider.fillRect.GetComponent<Image>().sprite = GreyEnergyBarSprite;
                shieldAuraVfx.SetActive(false);
                Debug.Log("ATTACK MODE DISABLED :(");
            }
        }

        currentDriftSpeed = Mathf.Clamp(currentDriftSpeed, 1.0f, maxDriftSpeed);

        float driftAngleLerpSpeed = isDrifting ? driftAngleBuildSpeed : driftAngleReturnSpeed;

        velDir.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(velDir.localEulerAngles.z, targetDriftAngle, Time.deltaTime * driftAngleLerpSpeed));

        if (Vector3.Distance(transform.position, m_LastPos) > trackSegLength)
        {
            m_LastPos = transform.position;

            float angleOffset = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, velDir.eulerAngles.z)) / 90f;
            Color newAlpha = new Color(0, 0, 0, Mathf.Min(angleOffset, .5f));

            foreach (WheelTrack wheel in m_WheelTracks)
            {
                wheel.AddSegment(newAlpha);
            }
        }
    }

    private void FixedUpdate()
    {
        // Apply velocity in the drift direction
        rb.velocity = currentDriftSpeed * m_AppliedSpeed * velDir.up;
    }

    private void OnMaxHealthChanged(float newValue)
    {
        //Reset health to full
        carHealth = newValue;

        //Update ui
        HealthBarSlider.maxValue = newValue;
        HealthBarSlider.value = carHealth;
    }

    public void TakeDamage(float damage)
    {
        carHealth -= damage;
        SoundManager.PlaySound(SoundType.CarDamage);
        StartCoroutine(damageFlash.PlayDamageFlash());
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
        attackModeTimer = stats.AttackModeDuration.Value;
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
