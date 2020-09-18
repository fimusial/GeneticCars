using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    public class Car : MonoBehaviour
    {
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private float m_MaximumSteerAngle = 30f;
        [Range(0, 1)] [SerializeField] private float m_SteerHelper = 0.71f; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)] [SerializeField] private float m_TractionControl = 1f; // 0 is no traction control, 1 is full interference
        [SerializeField] private float m_FullTorqueOverAllWheels = 2200f;
        [SerializeField] private float m_Downforce = 150f;
        [SerializeField] private float m_Topspeed = 130;
        [SerializeField] private int NoOfGears = 5;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit = 0.35f;
        [SerializeField] private float m_BrakeTorque = 20000f;

        private Quaternion[] m_WheelMeshLocalRotations;
        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_Mph = 2.23693629f;

        public bool Skidding { get; private set; }
        public bool HasCollidedWithWall { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get { return m_SteerAngle; } }
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * k_Mph; } }
        public float MaxSpeed { get { return m_Topspeed; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        private void Start()
        {
            HasCollidedWithWall = false;
            m_WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }

            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
        }

        void OnCollisionEnter(Collision collision)
        {
            HasCollidedWithWall = true;
        }

        void OnCollisionStay(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }
        }

        // both inputs are clamped to <-1, 1> range
        public void Move(float steering, float accel)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            float steeringClamped = Mathf.Clamp(steering, -1, 1);
            float accelClamped = Mathf.Clamp(accel, 0, 1);
            float brakeClamped = -1 * Mathf.Clamp(accel, -1, 0);
            AccelInput = accelClamped;
            BrakeInput = brakeClamped;

            m_SteerAngle = steeringClamped * m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(accelClamped, brakeClamped);
            ApplySpeedCap();
            CalculateRevs();
            GearChanging();
            AddDownForce();
            TractionControl();
        }

        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);

                if (wheelhit.normal == Vector3.zero)
                {
                    return;
                }
            }

            // this if block is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }

        private void ApplyDrive(float accel, float footbrake)
        {
            float thrustTorque = accel * (m_CurrentTorque / 4f);
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].motorTorque = thrustTorque;
            }

            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
        }

        private void ApplySpeedCap()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            speed *= k_Mph;
            if (speed > m_Topspeed)
            {
                m_Rigidbody.velocity = (m_Topspeed / k_Mph) * m_Rigidbody.velocity.normalized;
            }
        }

        private void CalculateRevs()
        {
            CalculateGearFactor();
            var gearNumFactor = m_GearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
            float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
            }
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        private void CalculateGearFactor()
        {
            float f = (1 / (float)NoOfGears);
            var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }

        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce * m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }

        private void TractionControl()
        {
            WheelHit wheelHit;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                if (m_CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }
    }
}