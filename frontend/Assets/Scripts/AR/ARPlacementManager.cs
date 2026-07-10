using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using CozyAR.Core;
using CozyAR.Data;
using CozyAR.UI;

namespace CozyAR.AR
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARPlacementManager : MonoBehaviour
    {
        public static ARPlacementManager Instance { get; private set; }

        [SerializeField]
        private GameObject characterPrefab; // Assigned dynamically by Setup script or Inspector

        private ARRaycastManager arRaycastManager;
        private List<ARRaycastHit> hits = new List<ARRaycastHit>();

        private GameObject selectedObject;
        private List<GameObject> spawnedCharacters = new List<GameObject>();

        // Gesture state
        private float prevTouchDistance;
        private float prevTouchAngle;

        private void Awake()
        {
            Instance = this;
            arRaycastManager = GetComponent<ARRaycastManager>();
        }

        private void Start()
        {
            // Set the prefab reference if we have it in resources
            if (characterPrefab == null)
            {
                characterPrefab = Resources.Load<GameObject>("Prefabs/CharacterPrefab");
            }
        }

        private void Update()
        {
            // Handle touches
            int touchCount = Input.touchCount;
            if (touchCount == 0) return;

            if (touchCount == 1)
            {
                HandleSingleTouch();
            }
            else if (touchCount == 2)
            {
                HandleDoubleTouch();
            }
        }

        private void HandleSingleTouch()
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Raycast from camera to screen touch to check if we hit an existing character
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Hit something with a collider
                    GameObject hitObj = hit.collider.gameObject;
                    
                    // If it is a character or child of a character
                    CharacterController controller = hitObj.GetComponentInParent<CharacterController>();
                    if (controller != null)
                    {
                        selectedObject = controller.gameObject;
                        Debug.Log($"Selected character: {selectedObject.name}");
                        ARUiController.Instance.OnCharacterSelected(controller);
                        return;
                    }
                }

                // If we didn't hit a character, try to raycast against detected AR planes to spawn/reposition
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    
                    if (selectedObject == null && spawnedCharacters.Count == 0)
                    {
                        // Spawn first character
                        SpawnCharacter(hitPose.position);
                    }
                    else if (selectedObject != null)
                    {
                        // Reposition selected character
                        selectedObject.transform.position = hitPose.position;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // Drag selected character along planes
                if (selectedObject != null)
                {
                    if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                    {
                        selectedObject.transform.position = hits[0].pose.position;
                    }
                }
            }
        }

        private void HandleDoubleTouch()
        {
            if (selectedObject == null) return;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                // Initialize scale/rotate metrics
                prevTouchDistance = Vector2.Distance(t0.position, t1.position);
                Vector2 diff = t1.position - t0.position;
                prevTouchAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                return;
            }

            if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                // 1. Pinch to Scale
                float currentTouchDistance = Vector2.Distance(t0.position, t1.position);
                if (prevTouchDistance > 0)
                {
                    float scaleFactor = currentTouchDistance / prevTouchDistance;
                    Vector3 newScale = selectedObject.transform.localScale * scaleFactor;
                    
                    // Clamp scale between 0.1f and 3.0f for safety
                    newScale.x = Mathf.Clamp(newScale.x, 0.1f, 3.0f);
                    newScale.y = Mathf.Clamp(newScale.y, 0.1f, 3.0f);
                    newScale.z = Mathf.Clamp(newScale.z, 0.1f, 3.0f);
                    
                    selectedObject.transform.localScale = newScale;
                }
                prevTouchDistance = currentTouchDistance;

                // 2. Two-finger Rotate (updates rotation offset in the billboard script)
                Vector2 currentDiff = t1.position - t0.position;
                float currentTouchAngle = Mathf.Atan2(currentDiff.y, currentDiff.x) * Mathf.Rad2Deg;
                float angleDiff = currentTouchAngle - prevTouchAngle;
                
                CharacterBillboard billboard = selectedObject.GetComponent<CharacterBillboard>();
                if (billboard != null)
                {
                    // Rotate the billboard direction around vertical axis
                    billboard.rotationOffset += angleDiff;
                }
                prevTouchAngle = currentTouchAngle;
            }
        }

        public void SpawnCharacter(Vector3 position)
        {
            if (characterPrefab == null)
            {
                Debug.LogError("CharacterPrefab is not set!");
                return;
            }

            GameObject go = Instantiate(characterPrefab, position, Quaternion.identity);
            go.name = $"{GameManager.Instance.SelectedCharacter.name}_{spawnedCharacters.Count + 1}";
            
            CharacterController controller = go.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.Initialize(GameManager.Instance.SelectedCharacter);
            }

            spawnedCharacters.Add(go);
            selectedObject = go;
            ARUiController.Instance.OnCharacterSelected(controller);
            Debug.Log($"Spawned companion: {go.name}");
        }

        public void SpawnAnotherCopy()
        {
            if (GameManager.Instance.SelectedCharacter == null) return;
            
            // Spawn in front of the camera
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            
            // Raycast down to find floor plane if possible, otherwise spawn floaty
            Ray ray = new Ray(spawnPos, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                spawnPos = hit.point;
            }
            else
            {
                spawnPos.y = Camera.main.transform.position.y - 0.5f; // Float at nice level
            }

            SpawnCharacter(spawnPos);
        }

        public void ResetPosition()
        {
            if (selectedObject == null) return;
            
            // Reset to look-at camera position directly in front
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            spawnPos.y = Camera.main.transform.position.y - 0.5f;
            
            selectedObject.transform.position = spawnPos;
            selectedObject.transform.localScale = Vector3.one;

            CharacterBillboard billboard = selectedObject.GetComponent<CharacterBillboard>();
            if (billboard != null)
            {
                billboard.rotationOffset = 0f;
            }
        }

        public bool HasSelectedCharacter()
        {
            return selectedObject != null;
        }

        public void DeleteSelected()
        {
            if (selectedObject == null) return;

            spawnedCharacters.Remove(selectedObject);
            Destroy(selectedObject);
            selectedObject = null;

            ARUiController.Instance.OnCharacterDeselected();
        }

        public void SetSelectedExpression(string expression)
        {
            if (selectedObject != null)
            {
                CharacterController controller = selectedObject.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.SetExpression(expression);
                }
            }
        }
    }
}
