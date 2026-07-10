using UnityEngine;

namespace CozyAR.AR.Procedural
{
    public class IdleBobController : MonoBehaviour
    {
        public float amplitude = 0.02f;
        public float speed = 1.1f;
        
        private Vector3 initialLocalPosition;
        private float randomOffset;

        private void Start()
        {
            initialLocalPosition = transform.localPosition;
            // Add a randomized offset so multiple characters don't float in perfect sync
            randomOffset = Random.Range(0f, 100f);
        }

        public void Initialize(float amp, float spd)
        {
            amplitude = amp;
            speed = spd;
        }

        private void Update()
        {
            float bobY = Mathf.Sin((Time.time * speed) + randomOffset) * amplitude;
            transform.localPosition = initialLocalPosition + new Vector3(0, bobY, 0);
        }
    }
}
