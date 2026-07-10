using System.Collections;
using UnityEngine;

namespace CozyAR.AR.Procedural
{
    public class BlinkController : MonoBehaviour
    {
        public GameObject eyesOpenGo;
        public GameObject eyesClosedGo;

        public float blinkIntervalMin = 2.5f;
        public float blinkIntervalMax = 6.0f;
        public float blinkDuration = 0.12f;

        private void Start()
        {
            if (eyesOpenGo != null) eyesOpenGo.SetActive(true);
            if (eyesClosedGo != null) eyesClosedGo.SetActive(false);

            StartCoroutine(BlinkLoop());
        }

        public void Initialize(float minInt, float maxInt)
        {
            blinkIntervalMin = minInt;
            blinkIntervalMax = maxInt;
        }

        private IEnumerator BlinkLoop()
        {
            while (true)
            {
                // Wait for a random interval between blinks
                float delay = Random.Range(blinkIntervalMin, blinkIntervalMax);
                yield return new WaitForSeconds(delay);

                // Perform the blink
                if (eyesOpenGo != null && eyesClosedGo != null)
                {
                    eyesOpenGo.SetActive(false);
                    eyesClosedGo.SetActive(true);

                    yield return new WaitForSeconds(blinkDuration);

                    eyesOpenGo.SetActive(true);
                    eyesClosedGo.SetActive(false);
                }
            }
        }
    }
}
