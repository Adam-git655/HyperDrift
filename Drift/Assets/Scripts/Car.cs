using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public static class Globals
{
    public static float gameplayTime = 0f;
    public static bool gameplayTimerRunning = false;

    public static PlayerMetaProgressionStats playerMeta = new PlayerMetaProgressionStats();
    public static int totalSalvageCores = 0;

    public static int enemiesKilled = 0;

    public static void StartGameplayTimer()
    {
        gameplayTime = 0f;
        gameplayTimerRunning = true;
    }
}

public class Car : MonoBehaviour
{
    //Stats
    public PlayerBaseStats baseStats;
    public PlayerStatsRuntime stats;
    public int salvageCores = 0;

    [Header("Speed")]
    [Tooltip("seconds to reach max speed")]
    public float accelerationTime = 1.2f;

    [Tooltip("seconds to reach 0 speed")]
    public float decelRate = 4f;

    [Tooltip("Number of degrees car's nose turns every second during normal driving")]
    public float rotateDegreesPerSec = 220f;

    [Header("Drifting")]
    [Tooltip("Maximum sideways angle car can point inwards")]
    public float driftMaxAngle = 60f;

    [Tooltip("Speed of nose swinging away from default")]
    public float driftAngleBuildSpeed = 10f;

    [Tooltip("Speed of nose swining back to default")]
    public float driftAngleReturnSpeed = 5f;

    [Tooltip("Smoothing factor, easing the car in to the drift (avoid jerk when starting drift)")]
    public float driftRampIn = 3.5f;

    [Tooltip("Speed Limit when you are drifting")]
    public float maxDriftSpeed = 1f;

    [Tooltip("Extra speed added to the car when drifiting")]
    public float driftSpeedBoost = 2.5f;

    [Tooltip("How fast you lose built up speed once drift ends")]
    public float driftSpeedDecay = 5.0f;

    [Tooltip("How many seconds you can turn in one direction constantly before overheating")]
    public float hyperDriftInstabilityChargeTime = 5f;

    [Tooltip("How many seconds your car overheats for")]
    public float hyperDriftInstabilityTime = 3f;

    [Header("Drift Control")]
    [Tooltip("How fast the drift tightens from wide to sharp (0 to 1 in X seconds)")]
    public float driftTightenRate = 0.8f;

    [Tooltip("The starting percentage of the drift angle (0.3 = starts at 30% of max angle)")]
    public float minDriftAngleRatio = 0.3f;

    [Tooltip("Rotation multiplier at the START of a drift (Wide loop)")]
    public float startDriftRotateMult = 1.1f;

    [Tooltip("Rotation multiplier at MAX tightness (Tight loop)")]
    public float maxDriftRotateMult = 2.4f;

    [Header("Vignette")]
    [SerializeField] private float maxVignetteIntesity = 0.5f;
    [SerializeField] private float pulseStartThreshold = 0.25f;
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float pulseStrength = 0.25f;
    [SerializeField] private Color hyperDriftVignetteColor;

    [Header("References")]
    public CinemachineVirtualCamera followCam;
    public GameObject attackModeButtonUI;
    public GameObject GameOverPanel;
    public Text timeSurvivedCount;
    public Text salvageCoresCollectedCountOnDeathText;
    public Tilemap tilemap;
    public Slider HealthBarSlider;
    public ParticleSystem ElectricShockFx;
    public Text salvageCoresCount;
    public GameObject shieldAuraVfx;
    public Slider driftMeterSlider;
    public Transform[] wheels;
    public Material trailMaterial;
    private DamageFlash damageFlash;
    public Sprite BlueEnergyBarSprite;
    public Sprite GreyEnergyBarSprite;
    public AudioSource EngineAudioSource;
    public AudioSource DriftAudioSource;
    public PlayerWeapons weaponController;
    public Volume postProccesingVolume;
    public SteeringWheelController steeringWheelController;
    private Vignette vignette;

    //Internals/States/Values
    private Rigidbody2D rb;
    private Transform velDir;
    private float m_AppliedSpeed = 0;
    private float steerInput;
    public float turnInput;
    private float hyperDriftInstabilityTimer = 0f;

    private float carHealth;
    public bool canTakeDamage = true;
    private bool isGameOver = false;

    private bool attackModePressed;
    public bool canMove = true;

    public bool isInAttackMode = false;
    private float attackModeTimer = 0f;

    private bool driftPressed;
    private float currentDriftSpeed = 1.0f;
    public bool isDrifting = false;
    private float driftChargeMeter = 0f;
    private readonly float maxDriftCharge = 100f;
    private float driftInfluence = 0f;
    private float driftTightness = 0f; // Internal tracker (0.0 to 1.0)
    private float lastDriftDir = 0f;

    public float trackSegLength = .15f;
    private List<WheelTrack> m_WheelTracks;
    private Vector3 m_LastPos;

    //Controls/Input
    private CarControls controls;

    private void Awake()
    {
        //set current stats to runtime stats
        stats = new PlayerStatsRuntime(baseStats, Globals.playerMeta);

        controls = new CarControls();

        controls.Driving.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
        controls.Driving.Steer.canceled += ctx => steerInput = 0f;

        controls.Driving.Drift.performed += ctx => driftPressed = true;
        controls.Driving.Drift.canceled += ctx => driftPressed = false;

        controls.Driving.AttackMode.performed += ctx => attackModePressed = true;

        stats.MaxHealth.OnValueChanged += OnMaxHealthChanged;
        stats.MaxSpeed.OnValueChanged += OnMaxSpeedChanged;
        stats.DriftSegmentCount.OnValueChanged += OnDriftSegmentCountChanged;
    }
    private void OnEnable()
    {
        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
    }

    public void EnableInput()
    {
        canMove = true;
        controls.Driving.Enable();
    }

    public void DisableInput()
    {
        canMove = false;
        controls.Driving.Disable();

        steerInput = 0f;
        driftPressed = false;
        attackModePressed = false;
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        isGameOver = false;

        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponentInChildren<DamageFlash>();
        weaponController = GetComponent<PlayerWeapons>();
        HealthBarSlider.maxValue = stats.MaxHealth.Value;
        carHealth = stats.MaxHealth.Value;
        driftMeterSlider.maxValue = maxDriftCharge;
        trailMaterial.color = Color.black;

        if (!postProccesingVolume.profile.TryGet(out vignette))
            Debug.LogError("Vignette effect not found in global volume");

        velDir = new GameObject("VelocityDirection").transform;
        velDir.parent = transform;
        velDir.localPosition = Vector3.zero;
        velDir.localEulerAngles = Vector3.zero;

        m_LastPos = transform.position;
        m_WheelTracks = new List<WheelTrack>();
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelTrack wheel = new WheelTrack();
            wheel.Init(wheels[i], trailMaterial, (int)stats.DriftSegmentCount.Value);
            m_WheelTracks.Add(wheel);
        }

        ElectricShockFx.Pause();
        ElectricShockFx.gameObject.SetActive(false);
        attackModeButtonUI.SetActive(false);
        GameOverPanel.SetActive(false);

        StartCoroutine(PressAnyKeyBeforeStarting());
    }

    private IEnumerator PressAnyKeyBeforeStarting()
    {
        DisableInput();
        yield return new WaitUntil(() => Keyboard.current.anyKey.wasPressedThisFrame
                                 || Mouse.current.leftButton.wasPressedThisFrame
                                 || Gamepad.current?.buttonSouth.wasPressedThisFrame == true);
        EnableInput();
    }

    void Update()
    {
        HealthBarSlider.value = carHealth;

        if (Globals.gameplayTimerRunning)
        {
            Globals.gameplayTime += Time.deltaTime;
        }

        //On death 
        if (carHealth <= 0f && !isGameOver)
        {
            isGameOver = true;
            GameOverPanel.SetActive(true);
            DriftAudioSource.Stop();
            tilemap.color = Color.gray;
            Globals.totalSalvageCores += salvageCores;
            int minutes = Mathf.FloorToInt(Globals.gameplayTime / 60f);
            int seconds = Mathf.FloorToInt(Globals.gameplayTime % 60f);
            timeSurvivedCount.text = $"{minutes} min {seconds} sec";
            salvageCoresCollectedCountOnDeathText.text = salvageCores.ToString();
            Time.timeScale = 0f;
        }

        salvageCoresCount.text = salvageCores.ToString();

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

        //Speed
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

        //record current dir
        float currentDir = Mathf.Abs(turnInput) > 0.1f ? Mathf.Sign(turnInput) : 0f;

        // If we switched directions (e.g., went from Left to Right) while drifting
        if (isDrifting && currentDir != 0 && currentDir != lastDriftDir)
        {
            // Reset tightness to 0 so the new direction starts wide
            driftTightness = 0f;
        }

        // Update the last direction tracker
        if (currentDir != 0) lastDriftDir = currentDir;

        // Logic for ramping tightness up or down
        float targetTightness = (isDrifting && currentDir != 0) ? 1f : 0f;
        float currentRampSpeed = (targetTightness > driftTightness) ? driftTightenRate : driftTightenRate * 2.0f;
        driftTightness = Mathf.MoveTowards(driftTightness, targetTightness, currentRampSpeed * Time.deltaTime);

        float currentRotateMult = Mathf.Lerp(startDriftRotateMult, maxDriftRotateMult, driftTightness);
        float currentAngleRatio = Mathf.Lerp(minDriftAngleRatio, 1f, driftTightness);

        //Rotation
        float zVal = transform.eulerAngles.z;

        if (m_AppliedSpeed > 0.1f && Mathf.Abs(turnInput) > 0.1f)
        {
            if (!isDrifting)
                zVal += rotateDegreesPerSec * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / stats.MaxSpeed.Value);
            else
                zVal += rotateDegreesPerSec * currentRotateMult * Time.deltaTime * -turnInput * Mathf.Clamp01(m_AppliedSpeed / stats.MaxSpeed.Value);
        }
        else
        {
            // Aggressive straighten out towards velocity direction
            zVal = Mathf.LerpAngle(transform.eulerAngles.z, velDir.eulerAngles.z, Time.deltaTime * 5f);
        }

        transform.eulerAngles = new Vector3(0f, 0f, zVal);
        

        //HyperDrift instability
        if (turnInput == 1f || turnInput == -1f)
        {
            hyperDriftInstabilityTimer += Time.deltaTime;
            if (hyperDriftInstabilityTimer >= hyperDriftInstabilityChargeTime)
            {
                StartCoroutine(HyperDriftInstability());
                hyperDriftInstabilityTimer = 0f;
            }
        }
        else
        {
            hyperDriftInstabilityTimer = 0f;
        }

        //Drift Direction
        float targetDriftAngle = 0f;

        //Heavier drift entry
        float targetDriftInfluence = isDrifting ? 1f : 0f;
        driftInfluence = Mathf.Lerp(driftInfluence, targetDriftInfluence, driftRampIn * Time.deltaTime);

        if (isDrifting && Mathf.Abs(turnInput) > 0.1f)
        {
            //angle
            targetDriftAngle = turnInput * (driftMaxAngle * currentAngleRatio) * driftInfluence;

            //drift speed boost
            currentDriftSpeed = Mathf.Min(currentDriftSpeed + driftSpeedBoost * Time.deltaTime, maxDriftSpeed);
        }
        else
        {
            //drift speed decay
            currentDriftSpeed = Mathf.Lerp(currentDriftSpeed, 1f, Time.deltaTime * driftSpeedDecay);
        }

        //clamp speed
        currentDriftSpeed = Mathf.Clamp(currentDriftSpeed, 1.0f, maxDriftSpeed);

        //lerp drift angle
        float driftAngleLerpSpeed = isDrifting ? driftAngleBuildSpeed : driftAngleReturnSpeed; 
        velDir.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(velDir.localEulerAngles.z, targetDriftAngle, Time.deltaTime * driftAngleLerpSpeed));

        HandleHyperDriftLogic();

        //Handle drift trail generation
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
        // Apply velocity with directional inertia in the drift direction
        Vector2 desiredVelocity = currentDriftSpeed * m_AppliedSpeed * velDir.up;
        float velocityResponsiveness = isDrifting ? 9f : 12f;
        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, velocityResponsiveness * Time.fixedDeltaTime);
    }

    private void OnMaxHealthChanged(float oldValue, float newValue)
    {
        //Reset health to full
        carHealth = newValue;

        //Update ui
        HealthBarSlider.maxValue = newValue;
        HealthBarSlider.value = carHealth;
    }

    private void OnMaxSpeedChanged(float oldValue, float newValue)
    { 
        float speedFractionalChange = ((newValue - oldValue) / oldValue);
        stats.Damage.Multiply(1 + speedFractionalChange * 1.8f);
    }

    private void OnDriftSegmentCountChanged(float oldValue, float newValue)
    {
        foreach (var wheel in m_WheelTracks)
        {
            wheel.SetSegmentCount((int)newValue);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!canTakeDamage)
            return;
        
        float reductionPercent = stats.DamageReduction.Value;
        reductionPercent = Mathf.Clamp(reductionPercent, 0f, 0.9f);
        float finalDamage = damage * (1f - reductionPercent);

        carHealth -= finalDamage;
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

    private void HandleHyperDriftLogic()
    {
        if (isDrifting && Mathf.Abs(turnInput) > 0.1f)
        {
            //drift charge meter icnrease
            if (!isInAttackMode)
            {
                driftChargeMeter += stats.DriftChargeRate.Value * Time.deltaTime;
                driftChargeMeter = Mathf.Min(driftChargeMeter, maxDriftCharge);
            }
        }
        driftMeterSlider.value = driftChargeMeter;


        //when drift charge meter is full
        if (driftChargeMeter >= maxDriftCharge && !isInAttackMode)
        {
            driftMeterSlider.fillRect.GetComponent<Image>().sprite = BlueEnergyBarSprite;

#if UNITY_ANDROID || UNITY_IOS
            attackModeButtonUI.SetActive(true);
#endif
            //if space pressed when drift charge full then activate hyperdrift
            if (attackModePressed && canMove)
            {
                ActivateAttackMode();
                attackModePressed = false;
                attackModeButtonUI.SetActive(false);
            }
        }
        
        //When in hyperdrift do this
        if (isInAttackMode)
        {
            attackModeTimer -= Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(attackModeTimer / stats.AttackModeDuration.Value);

            float vignetteIntensity = normalizedTime * maxVignetteIntesity;

            if (normalizedTime <= pulseStartThreshold)
            {
                float pulseT = 1f - (normalizedTime / pulseStartThreshold);

                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;

                vignetteIntensity += pulse * pulseStrength * pulseT;
            }
            vignette.intensity.value = Mathf.Clamp01(vignetteIntensity);

            driftChargeMeter = normalizedTime * 100f;

            if (attackModeTimer <= 0f)
            {
                isInAttackMode = false;
                driftChargeMeter = 0f;
                vignette.intensity.value = 0f;
                driftMeterSlider.fillRect.GetComponent<Image>().sprite = GreyEnergyBarSprite;
                shieldAuraVfx.SetActive(false);

                if (weaponController.ActiveWeapons.TryGetValue(WeaponType.InertiaShield, out Weapon value))
                {
                    InertiaShieldWeapon weapon = value as InertiaShieldWeapon;
                    StartCoroutine(weapon.Activate());
                }
            }
        }

        //avoid double input
        attackModePressed = false;
    }

    private void ActivateAttackMode()
    {
        isInAttackMode = true;
        shieldAuraVfx.SetActive(true);
        vignette.color.Override(hyperDriftVignetteColor);
        vignette.intensity.value = 0.5f;
        attackModeTimer = stats.AttackModeDuration.Value;
    }

    private IEnumerator HyperDriftInstability()
    {
        canMove = false;
        CameraNoise(1f, 3f);
        vignette.color.Override(Color.red);
        vignette.intensity.value = 0.3f;
        SoundManager.PlaySound(SoundType.OverHeat);

        yield return new WaitForSeconds(hyperDriftInstabilityTime);

        canMove = true;
        CameraNoise(0f, 0f);
        vignette.color.Override(hyperDriftVignetteColor);
        vignette.intensity.value = 0f;
    }

    public void CameraNoise(float amplitudeGain, float frequencyGain)
    {
        CinemachineBasicMultiChannelPerlin noise = followCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;
    }

    public void GetElectrocuted()
    {
        canMove = false;
        carHealth -= 5;
        ElectricShockFx.gameObject.SetActive(true);
        ElectricShockFx.Play();
        SoundManager.PlayLoopSound(SoundType.ElectricShock);
        StartCoroutine(RegainControlsAfterElectricShock());
    }

    IEnumerator RegainControlsAfterElectricShock()
    {
        yield return new WaitForSeconds(2f);
        ElectricShockFx.gameObject.SetActive(false);
        ElectricShockFx.Pause();
        if (SoundManager.IsLooping(SoundType.ElectricShock))
            SoundManager.StopLoopSound(SoundType.ElectricShock);
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

    public void SetTrailColor(Color color)
    {
        trailMaterial.color = color;
    }

    public List<Vector2> GetAllTrailPoints()
    {
        List<Vector2> allPoints = new List<Vector2>();
        foreach (var wheel in m_WheelTracks)
            allPoints.AddRange(wheel.GetPoints());

        return allPoints;
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

        public void SetSegmentCount(int segCount)
        {
            segmentCount = segCount;
        }

        public void AddSegment(Color alphaColor)
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
            line.startColor = line.endColor = alphaColor;

            lastPos = transform.position;
            lineIndex = (lineIndex + 1) % segmentCount;
        }

        public List<Vector2> GetPoints()
        {
            List<Vector2> points = new List<Vector2>();
            foreach (var line in lines)
            {
                LineRenderer lr = line.GetComponent<LineRenderer>();
                points.Add(lr.GetPosition(0));
                points.Add(lr.GetPosition(1));
            }
            return points;
        }
    }
}
