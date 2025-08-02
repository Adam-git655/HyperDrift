using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float carHealth = 100f;
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
    public float attackModeDuration = 8f;
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

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
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
    }

    void Update()
    {
        HealthBarSlider.value = carHealth;
        if (carHealth <= 0f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        GearsCountText.text = gears.ToString();

        float throttle = Input.GetKey(KeyCode.W) && canMove ? 1f : 0f;

        if (throttle > 0f)
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

        turnInput = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        isDrifting = Input.GetKey(KeyCode.LeftShift) && canMove;

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
            driftMeterSlider.fillRect.GetComponent<Image>().color = Color.cyan;
            if (Input.GetKeyDown(KeyCode.Space) && canMove)
            {
                ActivateAttackMode();
            }
        }

        if (isInAttackMode)
        {
            attackModeTimer -= Time.deltaTime;
            driftChargeMeter = (attackModeTimer / attackModeDuration) * 100f;
            if (attackModeTimer <= 0f)
            {
                isInAttackMode = false;
                driftChargeMeter = 0f;
                driftMeterSlider.fillRect.GetComponent<Image>().color = Color.gray;
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

        // Chaos Boost when exiting drift
        if (!isDrifting && Mathf.Abs(turnInput) < 0.1f && m_AppliedSpeed > 0.2f && throttle > 0f)
        {
            m_AppliedSpeed = Mathf.Min(m_AppliedSpeed + Time.deltaTime * 5f, maxSpeed * 1.2f);
        }
    }

    private void ActivateAttackMode()
    {
        isInAttackMode = true;
        shieldAuraVfx.SetActive(true);
        attackModeTimer = attackModeDuration;
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
