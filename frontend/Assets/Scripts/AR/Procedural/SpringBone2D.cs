using UnityEngine;

namespace CozyAR.AR.Procedural
{
    public class SpringBone2D : MonoBehaviour
    {
        public float springForce = 150f;
        public float damping = 10f;
        public float inertiaInfluence = 400f;
        public float maxAngleLimit = 40f;

        private float currentAngle = 0f;
        private float angularVelocity = 0f;
        
        private Vector3 lastParentPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            initialRotation = transform.localRotation;
            lastParentPosition = transform.parent != null ? transform.parent.position : transform.position;
        }

        public void Initialize(float stiffness, float damp)
        {
            // Map values from database config
            springForce = stiffness * 500f; // Scale to comfortable C# force levels
            damping = damp * 20f;
        }

        private void LateUpdate()
        {
            Vector3 parentPos = transform.parent != null ? transform.parent.position : transform.position;
            Vector3 localMovement = parentPos - lastParentPosition;
            lastParentPosition = parentPos;

            // Project horizontal movement onto the spring's rotation plane (X-axis movement induces Z-rotation)
            float externalForce = -localMovement.x * inertiaInfluence;

            // Simple spring-mass-damper integration loop
            float springTargetAngle = 0f;
            float displacement = springTargetAngle - currentAngle;
            float restoringForce = displacement * springForce;
            
            // Damping force resists motion
            float dampingForce = -angularVelocity * damping;

            // Sum forces
            float totalAcceleration = restoringForce + dampingForce + externalForce;

            // Integrate
            angularVelocity += totalAcceleration * Time.deltaTime;
            currentAngle += angularVelocity * Time.deltaTime;

            // Clamp rotation bounds
            currentAngle = Mathf.Clamp(currentAngle, -maxAngleLimit, maxAngleLimit);

            // Apply rotation around the Z axis
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, currentAngle);
        }
    }
}
