using UnityEngine;
using System.Collections;

namespace CozyAR.AR
{
    public enum CharacterState
    {
        Idle,
        Wave,
        Happy,
        Walk,
        Sleep
    }

    public class CharacterStateMachine : MonoBehaviour
    {
        private CharacterState currentState = CharacterState.Idle;
        
        private Procedural.IdleBobController bobController;
        private Procedural.BreathingController breathingController;
        private Procedural.BlinkController blinkController;
        private Procedural.LookAroundController lookAroundController;

        private GameObject leftArmGo;
        private GameObject rightArmGo;
        private GameObject eyesOpenGo;
        private GameObject eyesClosedGo;
        private GameObject mouthGo;

        private Sprite mouthIdleSprite;
        private Sprite mouthHappySprite;
        private Sprite mouthSadSprite;
        private SpriteRenderer mouthRenderer;

        private Vector3 initialLeftArmRot;
        private Vector3 initialRightArmRot;
        
        private float defaultBobSpeed;
        private float defaultBobAmp;
        private float defaultBreatheSpeed;
        private float defaultBreatheAmp;

        public void Initialize(
            Procedural.IdleBobController bob,
            Procedural.BreathingController breathing,
            Procedural.BlinkController blink,
            Procedural.LookAroundController look,
            GameObject leftArm,
            GameObject rightArm,
            GameObject eyesOpen,
            GameObject eyesClosed,
            GameObject mouth,
            Sprite mouthIdle,
            Sprite mouthHappy,
            Sprite mouthSad)
        {
            bobController = bob;
            breathingController = breathing;
            blinkController = blink;
            lookAroundController = look;

            leftArmGo = leftArm;
            rightArmGo = rightArm;
            eyesOpenGo = eyesOpen;
            eyesClosedGo = eyesClosed;
            mouthGo = mouth;

            mouthIdleSprite = mouthIdle;
            mouthHappySprite = mouthHappy;
            mouthSadSprite = mouthSad;

            if (mouthGo != null)
            {
                mouthRenderer = mouthGo.GetComponent<SpriteRenderer>();
            }

            if (leftArmGo != null) initialLeftArmRot = leftArmGo.transform.localEulerAngles;
            if (rightArmGo != null) initialRightArmRot = rightArmGo.transform.localEulerAngles;

            if (bobController != null)
            {
                defaultBobSpeed = bobController.speed;
                defaultBobAmp = bobController.amplitude;
            }
            if (breathingController != null)
            {
                defaultBreatheSpeed = breathingController.speed;
                defaultBreatheAmp = breathingController.amplitude;
            }

            SetState(CharacterState.Idle);
        }

        public void SetState(CharacterState newState)
        {
            currentState = newState;
            StopAllCoroutines();

            // Reset controller states
            if (bobController != null)
            {
                bobController.enabled = true;
                bobController.speed = defaultBobSpeed;
                bobController.amplitude = defaultBobAmp;
            }
            if (breathingController != null)
            {
                breathingController.enabled = true;
                breathingController.speed = defaultBreatheSpeed;
                breathingController.amplitude = defaultBreatheAmp;
            }
            if (blinkController != null)
            {
                blinkController.enabled = true;
            }
            if (lookAroundController != null)
            {
                lookAroundController.enabled = true;
            }

            if (eyesOpenGo != null) eyesOpenGo.SetActive(true);
            if (eyesClosedGo != null) eyesClosedGo.SetActive(false);

            // Reset mouth sprite
            if (mouthRenderer != null && mouthIdleSprite != null)
            {
                mouthRenderer.sprite = mouthIdleSprite;
            }

            if (leftArmGo != null) leftArmGo.transform.localEulerAngles = initialLeftArmRot;
            if (rightArmGo != null) rightArmGo.transform.localEulerAngles = initialRightArmRot;

            // Re-enable spring physics on arms in case they were disabled by states
            var leftSpring = leftArmGo != null ? leftArmGo.GetComponent<Procedural.SpringBone2D>() : null;
            if (leftSpring != null) leftSpring.enabled = true;

            var rightSpring = rightArmGo != null ? rightArmGo.GetComponent<Procedural.SpringBone2D>() : null;
            if (rightSpring != null) rightSpring.enabled = true;

            switch (currentState)
            {
                case CharacterState.Idle:
                    break;

                case CharacterState.Wave:
                    StartCoroutine(WaveCoroutine());
                    break;

                case CharacterState.Happy:
                    StartCoroutine(HappyCoroutine());
                    break;

                case CharacterState.Sleep:
                    if (blinkController != null) blinkController.enabled = false;
                    if (lookAroundController != null) lookAroundController.enabled = false;
                    if (eyesOpenGo != null) eyesOpenGo.SetActive(false);
                    if (eyesClosedGo != null) eyesClosedGo.SetActive(true);

                    // Swap to sad mouth shape for sleep/sad
                    if (mouthRenderer != null && mouthSadSprite != null)
                    {
                        mouthRenderer.sprite = mouthSadSprite;
                    }
                    
                    // Slower, deeper breathing for sleep
                    if (breathingController != null)
                    {
                        breathingController.speed = defaultBreatheSpeed * 0.5f;
                        breathingController.amplitude = defaultBreatheAmp * 1.6f;
                    }
                    break;

                case CharacterState.Walk:
                    // Faster bobbing when moving/walking
                    if (bobController != null)
                    {
                        bobController.speed = defaultBobSpeed * 1.8f;
                        bobController.amplitude = defaultBobAmp * 1.4f;
                    }
                    break;
            }
        }

        private IEnumerator WaveCoroutine()
        {
            float duration = 2.5f;
            float elapsed = 0f;

            // Disable spring bone temporarily on waving arm so physics does not override wave rotation in LateUpdate
            var spring = rightArmGo != null ? rightArmGo.GetComponent<Procedural.SpringBone2D>() : null;
            if (spring != null) spring.enabled = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (rightArmGo != null)
                {
                    // Rapid waving motion: Rotate Z angle back and forth
                    float angle = Mathf.Sin(Time.time * 16f) * 30f;
                    rightArmGo.transform.localEulerAngles = initialRightArmRot + new Vector3(0, 0, angle);
                }
                yield return null;
            }

            if (spring != null) spring.enabled = true;
            SetState(CharacterState.Idle);
        }

        private IEnumerator HappyCoroutine()
        {
            float duration = 1.8f;
            float elapsed = 0f;

            // Swap to happy open mouth
            if (mouthRenderer != null && mouthHappySprite != null)
            {
                mouthRenderer.sprite = mouthHappySprite;
            }

            // Quickly bob character up and down excitedly
            if (bobController != null)
            {
                bobController.speed = defaultBobSpeed * 2.5f;
                bobController.amplitude = defaultBobAmp * 2.2f;
            }

            if (blinkController != null) blinkController.enabled = false;
            if (eyesOpenGo != null) eyesOpenGo.SetActive(false);
            if (eyesClosedGo != null) eyesClosedGo.SetActive(true); // Close eyes happily

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetState(CharacterState.Idle);
        }
    }
}
