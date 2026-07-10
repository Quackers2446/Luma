using UnityEngine;

namespace CozyAR.AR
{
    public class CharacterBillboard : MonoBehaviour
    {
        public float rotationOffset = 0f;

        private Transform mainCameraTransform;

        private void Start()
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (mainCameraTransform == null)
            {
                if (Camera.main != null)
                {
                    mainCameraTransform = Camera.main.transform;
                }
                return;
            }

            // Billboard effect: Make the quad face the camera, but stay upright vertically
            Vector3 targetPosition = mainCameraTransform.position;
            targetPosition.y = transform.position.y; // Keep vertical orientation straight

            transform.LookAt(targetPosition);
            
            // Reorient by 180 degrees + user rotation offset around vertical axis
            transform.Rotate(0, 180f + rotationOffset, 0);
        }
    }
}
