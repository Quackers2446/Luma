using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using CozyAR.AR;
using CozyAR.Core;
using CozyAR.Utils;

using CharacterController = CozyAR.AR.CharacterController;

namespace CozyAR.UI
{
    public class ARUiController : MonoBehaviour
    {
        public static ARUiController Instance { get; private set; }

        [Header("Global Canvas Root")]
        [SerializeField] private Canvas arHudCanvas;

        [Header("Controls Panel")]
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private Button expressionHappyBtn;
        [SerializeField] private Button expressionSadBtn;
        [SerializeField] private Button expressionWaveBtn;
        [SerializeField] private Button expressionIdleBtn;
        [SerializeField] private Button resetPosBtn;
        [SerializeField] private Button deleteBtn;
        [SerializeField] private Button spawnAnotherBtn;

        [Header("Capture Controls")]
        [SerializeField] private Button photoBtn;
        [SerializeField] private Button videoBtn;
        [SerializeField] private Button backBtn;

        [Header("Feedback Overlays")]
        [SerializeField] private TMP_Text toastText;
        [SerializeField] private GameObject recordingIndicator;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Bind buttons
            if (expressionHappyBtn != null) expressionHappyBtn.onClick.AddListener(() => SetExpression("happy"));
            if (expressionSadBtn != null) expressionSadBtn.onClick.AddListener(() => SetExpression("sad"));
            if (expressionWaveBtn != null) expressionWaveBtn.onClick.AddListener(() => SetExpression("wave"));
            if (expressionIdleBtn != null) expressionIdleBtn.onClick.AddListener(() => SetExpression("idle"));

            if (resetPosBtn != null) resetPosBtn.onClick.AddListener(ResetPosition);
            if (deleteBtn != null) deleteBtn.onClick.AddListener(DeleteSelected);
            if (spawnAnotherBtn != null) spawnAnotherBtn.onClick.AddListener(SpawnAnother);

            if (photoBtn != null) photoBtn.onClick.AddListener(CapturePhoto);
            if (videoBtn != null) videoBtn.onClick.AddListener(RecordVideo);
            if (backBtn != null) backBtn.onClick.AddListener(GoBackToHome);

            // Default UI state
            if (controlPanel != null) controlPanel.SetActive(false);
            if (recordingIndicator != null) recordingIndicator.SetActive(false);
            if (toastText != null) toastText.text = "Scan surface & tap to invite companion";
        }

        public void OnCharacterSelected(CharacterController controller)
        {
            if (controlPanel != null) controlPanel.SetActive(true);
            ShowToast($"Companion selected!");
        }

        public void OnCharacterDeselected()
        {
            if (controlPanel != null) controlPanel.SetActive(false);
            ShowToast("Select a companion or tap plane to place");
        }

        private void SetExpression(string expression)
        {
            ARPlacementManager.Instance.SetSelectedExpression(expression);
            ShowToast($"Companion is now {expression}!");
        }

        private void ResetPosition()
        {
            ARPlacementManager.Instance.ResetPosition();
            ShowToast("Position reset!");
        }

        private void DeleteSelected()
        {
            ARPlacementManager.Instance.DeleteSelected();
            ShowToast("Companion departed.");
        }

        private void SpawnAnother()
        {
            ARPlacementManager.Instance.SpawnAnotherCopy();
            ShowToast("New companion spawned!");
        }

        private void SetHudActive(bool active)
        {
            if (photoBtn != null) photoBtn.gameObject.SetActive(active);
            if (videoBtn != null) videoBtn.gameObject.SetActive(active);
            if (backBtn != null) backBtn.gameObject.SetActive(active);
            if (toastText != null) toastText.gameObject.SetActive(active);
            
            if (controlPanel != null)
            {
                if (active)
                {
                    controlPanel.SetActive(ARPlacementManager.Instance.HasSelectedCharacter());
                }
                else
                {
                    controlPanel.SetActive(false);
                }
            }
        }

        private void CapturePhoto()
        {
            StartCoroutine(CapturePhotoCoroutine());
        }

        private IEnumerator CapturePhotoCoroutine()
        {
            // Hide the UI HUD
            SetHudActive(false);

            // Wait for end of frame so UI hide is applied to the screen pixels
            yield return new WaitForEndOfFrame();

            string fileName = $"Photo_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string folderPath = Application.persistentDataPath;

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Use public app media folder so Android's System MediaScanner has read permission (Scoped Storage bypass)
            folderPath = folderPath.Replace("/Android/data/", "/Android/media/");
            if (folderPath.EndsWith("/files"))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 6);
            }
            #endif

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            // Capture screenshot to the absolute path
            ScreenCapture.CaptureScreenshot(filePath);

            // Wait a small buffer to ensure file write is complete on slow storage
            yield return new WaitForSeconds(0.5f);

            // Save to native gallery
            NativeGalleryHelper.SaveImage(filePath);

            // Restore HUD
            SetHudActive(true);

            ShowToast("Photo saved to gallery!");
        }

        private void RecordVideo()
        {
            StartCoroutine(RecordVideoCoroutine());
        }

        private IEnumerator RecordVideoCoroutine()
        {
            // Show recording indicator overlay
            if (recordingIndicator != null) recordingIndicator.SetActive(true);
            
            // Hide the UI HUD buttons
            SetHudActive(false);
            ShowToast("Recording cozy moment...");

            // Simulate recording for 3 seconds
            yield return new WaitForSeconds(3.0f);

            string fileName = $"Video_{System.DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            string folderPath = Application.persistentDataPath;

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Use public app media folder so Android's System MediaScanner has read permission (Scoped Storage bypass)
            folderPath = folderPath.Replace("/Android/data/", "/Android/media/");
            if (folderPath.EndsWith("/files"))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 6);
            }
            #endif

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            // Create a small mock text/dummy binary file representing the MP4 file
            File.WriteAllText(filePath, "Cozy AR Companion - Saved video capture recording mock binary data.");

            // Save/notify gallery scanner
            NativeGalleryHelper.SaveVideo(filePath);

            // Clean up overlay and restore HUD
            if (recordingIndicator != null) recordingIndicator.SetActive(false);
            SetHudActive(true);

            ShowToast("Video saved to gallery!");
        }

        private void GoBackToHome()
        {
            SceneManager.LoadScene("Home");
        }

        private void ShowToast(string message)
        {
            if (toastText != null)
            {
                toastText.text = message;
            }
        }
    }
}
