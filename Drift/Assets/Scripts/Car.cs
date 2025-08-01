using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float accelerationTime = 1f;
    public float friction = 1.2f;
    public float rotateDegreesPerSec = 220f;

    public float driftMaxAngle = 60f;
    public float driftAngleBuildSpeed = 10f;
    public float driftAngleReturnSpeed = 5f;
    public float maxDriftSpeed = 2.5f;
    public float driftSpeedBoost = 2.5f;
    public float driftSpeedDecay = 5.0f;
    private float currentDriftSpeed = 1.0f;

    public bool isInAttackMode = false;
    public float attackModeDuration = 3f;
    private float attackModeTimer = 0f;

    private float driftChargeMeter = 0f;
    private float maxDriftCharge = 100f;
    private float driftChargePerSecond = 20f;

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
    }

    void Update()
    {
        float throttle = Input.GetKey(KeyCode.W) ? 1f : 0f;

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

        float turnInput = Input.GetAxisRaw("Horizontal");
        bool isDrifting = Input.GetKey(KeyCode.LeftShift);

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

        if (driftChargeMeter >= maxDriftCharge && !isInAttackMode)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ActivateAttackMode();
            }
        }

        if (isInAttackMode)
        {
            attackModeTimer -= Time.deltaTime;
            if (attackModeTimer <= 0f)
            {
                isInAttackMode = false;
                Debug.Log("ATTACK MODE DISABLED :(");
            }
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
        attackModeTimer = attackModeDuration;
        driftChargeMeter = 0f;

        Debug.Log("ATTACK MODE BABY");
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
