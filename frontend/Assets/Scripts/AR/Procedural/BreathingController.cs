using UnityEngine;

namespace CozyAR.AR.Procedural
{
    public class BreathingController : MonoBehaviour
    {
        public float amplitude = 0.015f;
        public float speed = 1.2f;

        private Vector3 initialScale;
        private float randomOffset;

        private void Start()
        {
            initialScale = transform.localScale;
            randomOffset = Random.Range(0f, 100f);
        }

        public void Initialize(float amp, float spd)
        {
            amplitude = amp;
            // Link breathing speed to general bob speed or keep distinct
            speed = spd * 1.1f;
        }

        private void Update()
        {
            float wave = Mathf.Sin((Time.time * speed) + randomOffset) * amplitude;
            
            // Out of phase scaling: scaling up on Y squashes X to conserve visual volume
            float scaleX = initialScale.x * (1f - wave * 0.7f);
            float scaleY = initialScale.y * (1f + wave);
            
            transform.localScale = new Vector3(scaleX, scaleY, initialScale.z);
        }
    }
}
