using UnityEngine;
using CozyAR.Data;
using CozyAR.Network;

namespace CozyAR.AR
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private CharacterDetails characterDetails;

        public void Initialize(CharacterDetails details)
        {
            characterDetails = details;
            
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            // Load default idle sprite
            SetExpression("idle");
        }

        public void SetExpression(string expressionName)
        {
            if (characterDetails == null || spriteRenderer == null) return;

            string url = GetExpressionUrl(expressionName);
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($"Expression URL not found for: {expressionName}");
                return;
            }

            // Start downloading/loading from cache
            StartCoroutine(BackendService.Instance.LoadSprite(url, (sprite) =>
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = sprite;
                    AdjustColliderSize(sprite);
                }
            }, (error) =>
            {
                Debug.LogError($"Error loading expression {expressionName}: {error}");
            }));
        }

        private string GetExpressionUrl(string expressionName)
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
            // Dynamically adjust box collider size based on sprite dimensions to make raycast hit detection accurate
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
