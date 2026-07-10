using UnityEngine;

namespace CozyAR.AR.Procedural
{
    public class LookAroundController : MonoBehaviour
    {
        public float rotationRange = 4.0f;
        public float lookSpeed = 0.5f;

        private Quaternion initialRotation;
        private Vector3 initialPosition;
        private float randomOffset;

        private void Start()
        {
            initialRotation = transform.localRotation;
            initialPosition = transform.localPosition;
            randomOffset = Random.Range(0f, 100f);
        }

        public void Initialize(float rotationRangeDegrees)
        {
            rotationRange = rotationRangeDegrees;
        }

        private void Update()
        {
            // Slow sway simulating head turns and neck tilt
            float timeFactor = Time.time * lookSpeed + randomOffset;
            float angleZ = Mathf.Sin(timeFactor) * rotationRange;
            float shiftX = Mathf.Cos(timeFactor * 0.7f) * (rotationRange * 0.002f); // subtle horizontal translation

            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, angleZ);
            transform.localPosition = initialPosition + new Vector3(shiftX, 0, 0);
        }
    }
}
