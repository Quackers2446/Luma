using System;
using System.Collections;
using UnityEngine;
using CozyAR.Data;
using CozyAR.Network;

namespace CozyAR.AR
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer; // Legacy single-sprite reference (kept for fallback compatibility)

        private CharacterDetails characterDetails;

        public void Initialize(CharacterDetails details)
        {
            characterDetails = details;
            
            if (characterDetails.metadata != null && characterDetails.metadata.layers != null)
            {
                StartCoroutine(AssembleCharacterLayers());
            }
            else
            {
                // Fallback to legacy single-sprite rendering
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
                SetExpression("idle");
            }
        }

        private IEnumerator AssembleCharacterLayers()
        {
            // Clear any legacy child visual objects
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            var layers = characterDetails.metadata.layers;
            var config = characterDetails.metadata.config;

            if (layers == null || config == null)
            {
                Debug.LogError("No layered configurations defined for this character!");
                yield break;
            }

            Sprite bodySprite = null;
            Sprite headSprite = null;
            Sprite leftArmSprite = null;
            Sprite rightArmSprite = null;
            Sprite hairSprite = null;
            Sprite eyesOpenSprite = null;
            Sprite eyesClosedSprite = null;
            Sprite mouthSprite = null;
            Sprite mouthHappySprite = null;
            Sprite mouthSadSprite = null;
            Sprite shadowSprite = null;
            Sprite accessorySprite = null;

            bool isDone = false;
            int loadedCount = 0;
            int totalExpected = 9; // Body, Head, LeftArm, RightArm, Hair, EyesOpen, EyesClosed, Mouth, Shadow
            if (!string.IsNullOrEmpty(layers.mouth_happy)) totalExpected++;
            if (!string.IsNullOrEmpty(layers.mouth_sad)) totalExpected++;
            if (!string.IsNullOrEmpty(layers.accessory)) totalExpected++;

            Action checkDone = () => {
                loadedCount++;
                if (loadedCount >= totalExpected) isDone = true;
            };

            // Download layers in parallel
            StartCoroutine(LoadLayer(layers.body, (s) => bodySprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.head, (s) => headSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.left_arm, (s) => leftArmSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.right_arm, (s) => rightArmSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.hair, (s) => hairSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.eyes_open, (s) => eyesOpenSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.eyes_closed, (s) => eyesClosedSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.mouth, (s) => mouthSprite = s, checkDone));
            StartCoroutine(LoadLayer(layers.shadow, (s) => shadowSprite = s, checkDone));
            
            if (!string.IsNullOrEmpty(layers.mouth_happy))
            {
                StartCoroutine(LoadLayer(layers.mouth_happy, (s) => mouthHappySprite = s, checkDone));
            }
            if (!string.IsNullOrEmpty(layers.mouth_sad))
            {
                StartCoroutine(LoadLayer(layers.mouth_sad, (s) => mouthSadSprite = s, checkDone));
            }
            if (!string.IsNullOrEmpty(layers.accessory))
            {
                StartCoroutine(LoadLayer(layers.accessory, (s) => accessorySprite = s, checkDone));
            }

            // Wait until downloading completes
            while (!isDone)
            {
                yield return null;
            }

            // Load shared character material if available
            Material mat = Resources.Load<Material>("Materials/CharacterSpriteMaterial");
            if (mat == null) mat = new Material(Shader.Find("Sprites/Default"));

            // Compile layers dynamically inside 3D space with correct depth offsets
            // 1. Shadow (Base ground layer)
            GameObject shadowGo = CreateLayerGo("Shadow", shadowSprite, mat, new Vector3(0, 0, 0.05f));

            // 2. Left Arm (Behind body)
            GameObject leftArmGo = CreateLayerGo("LeftArm", leftArmSprite, mat, new Vector3(0, 0, 0.02f));

            // 3. Body (Anchor center)
            GameObject bodyGo = CreateLayerGo("Body", bodySprite, mat, new Vector3(0, 0, 0f));

            // 4. Head (Slightly in front of body)
            GameObject headGo = CreateLayerGo("Head", headSprite, mat, new Vector3(0, 0, -0.01f));

            // 5. Hair (Child of Head)
            GameObject hairGo = CreateLayerGo("Hair", hairSprite, mat, new Vector3(0, 0, -0.005f), headGo.transform);

            // 6. Mouth (Child of Head)
            GameObject mouthGo = CreateLayerGo("Mouth", mouthSprite, mat, new Vector3(0, 0, -0.01f), headGo.transform);

            // 7. Eyes Open (Child of Head)
            GameObject eyesOpenGo = CreateLayerGo("EyesOpen", eyesOpenSprite, mat, new Vector3(0, 0, -0.015f), headGo.transform);

            // 8. Eyes Closed (Child of Head)
            GameObject eyesClosedGo = CreateLayerGo("EyesClosed", eyesClosedSprite, mat, new Vector3(0, 0, -0.015f), headGo.transform);

            // 9. Right Arm (In front of body)
            GameObject rightArmGo = CreateLayerGo("RightArm", rightArmSprite, mat, new Vector3(0, 0, -0.03f));

            // 10. Accessory (In front of arm)
            GameObject accessoryGo = null;
            if (accessorySprite != null)
            {
                accessoryGo = CreateLayerGo("Accessory", accessorySprite, mat, new Vector3(0, 0, -0.04f));
            }

            // Adjust physics bounds using body outline dimensions
            AdjustColliderSize(bodySprite);

            // Instantiate Procedural Component Motion Controllers
            var bob = gameObject.AddComponent<Procedural.IdleBobController>();
            bob.Initialize(config.idleBobAmplitude, config.idleBobSpeed);

            var breathe = gameObject.AddComponent<Procedural.BreathingController>();
            breathe.Initialize(config.breathingScale, config.idleBobSpeed);

            var look = headGo.AddComponent<Procedural.LookAroundController>();
            look.Initialize(config.headRotationRange);

            var blink = headGo.AddComponent<Procedural.BlinkController>();
            blink.Initialize(config.blinkIntervalMin, config.blinkIntervalMax);
            blink.eyesOpenGo = eyesOpenGo;
            blink.eyesClosedGo = eyesClosedGo;

            // Instantiate Secondary Physics springs on loose transforms
            if (hairGo != null)
            {
                var spring = hairGo.AddComponent<Procedural.SpringBone2D>();
                spring.Initialize(config.hairSpring, config.hairDamping);
            }
            if (leftArmGo != null)
            {
                var spring = leftArmGo.AddComponent<Procedural.SpringBone2D>();
                spring.Initialize(config.hairSpring * 0.7f, config.hairDamping * 1.1f);
            }
            if (rightArmGo != null)
            {
                var spring = rightArmGo.AddComponent<Procedural.SpringBone2D>();
                spring.Initialize(config.hairSpring * 0.7f, config.hairDamping * 1.1f);
            }
            if (accessoryGo != null)
            {
                var spring = accessoryGo.AddComponent<Procedural.SpringBone2D>();
                spring.Initialize(config.hairSpring * 1.1f, config.hairDamping * 0.9f);
            }

            // Initialize State Machine
            var stateMachine = gameObject.AddComponent<CharacterStateMachine>();
            stateMachine.Initialize(
                bob, 
                breathe, 
                blink, 
                look, 
                leftArmGo, 
                rightArmGo, 
                eyesOpenGo, 
                eyesClosedGo, 
                mouthGo, 
                mouthSprite, 
                mouthHappySprite, 
                mouthSadSprite
            );
        }

        private IEnumerator LoadLayer(string url, Action<Sprite> onSuccess, Action onComplete)
        {
            yield return StartCoroutine(BackendService.Instance.LoadSprite(url, (sprite) => {
                onSuccess?.Invoke(sprite);
                onComplete?.Invoke();
            }, (err) => {
                Debug.LogError($"Error loading character layer texture ({url}): {err}");
                onComplete?.Invoke(); // Always run complete callback to prevent lockups on download errors!
            }));
        }

        private GameObject CreateLayerGo(string name, Sprite sprite, Material mat, Vector3 localPos, Transform parent = null)
        {
            if (sprite == null) return null;

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : transform, false);
            go.transform.localPosition = localPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.material = mat;

            return go;
        }

        public void SetExpression(string expressionName)
        {
            var stateMachine = GetComponent<CharacterStateMachine>();
            if (stateMachine != null)
            {
                switch (expressionName.ToLower())
                {
                    case "happy":
                        stateMachine.SetState(CharacterState.Happy);
                        break;
                    case "sad":
                    case "sleep":
                        stateMachine.SetState(CharacterState.Sleep);
                        break;
                    case "wave":
                        stateMachine.SetState(CharacterState.Wave);
                        break;
                    case "walk":
                        stateMachine.SetState(CharacterState.Walk);
                        break;
                    case "idle":
                    default:
                        stateMachine.SetState(CharacterState.Idle);
                        break;
                }
            }
            else
            {
                // Fallback to legacy single sprite expression mapping
                if (characterDetails == null || spriteRenderer == null) return;
                string url = GetLegacyExpressionUrl(expressionName);
                if (!string.IsNullOrEmpty(url))
                {
                    StartCoroutine(BackendService.Instance.LoadSprite(url, (sprite) =>
                    {
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sprite = sprite;
                            AdjustColliderSize(sprite);
                        }
                    }));
                }
            }
        }

        private string GetLegacyExpressionUrl(string expressionName)
        {
            if (characterDetails == null || characterDetails.metadata == null || characterDetails.metadata.expressions == null)
            {
                return null;
            }

            switch (expressionName.ToLower())
            {
                case "happy":
                    return characterDetails.metadata.expressions.happy;
                case "sad":
                    return characterDetails.metadata.expressions.sad;
                case "wave":
                case "waving":
                    return characterDetails.metadata.expressions.wave;
                case "idle":
                default:
                    return characterDetails.metadata.expressions.idle;
            }
        }

        private void AdjustColliderSize(Sprite sprite)
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null && sprite != null)
            {
                Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
                boxCollider.size = new Vector3(spriteSize.x, spriteSize.y, 0.1f);
                boxCollider.center = new Vector3(0, spriteSize.y / 2f - sprite.pivot.y / sprite.pixelsPerUnit, 0);
            }
        }
    }
}
