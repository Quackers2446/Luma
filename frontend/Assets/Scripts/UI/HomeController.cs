using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using CozyAR.Core;
using CozyAR.Data;
using CozyAR.Network;

namespace CozyAR.UI
{
    public class HomeController : MonoBehaviour
    {
        [Header("Server Configuration")]
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private Button connectButton;

        [Header("UI Views")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private GameObject slotTemplatePrefab;

        [Header("Cozy UI Styling")]
        [SerializeField] private Color successColor = new Color(0.4f, 0.76f, 0.64f); // Cozy soft green
        [SerializeField] private Color errorColor = new Color(0.9f, 0.45f, 0.45f);   // Cozy soft red

        private List<GameObject> activeSlots = new List<GameObject>();

        private void Start()
        {
            // Set up default/saved IP value in Input Field
            if (ipInputField != null)
            {
                ipInputField.text = GameManager.Instance.BackendUrl;
            }

            if (connectButton != null)
            {
                connectButton.onClick.AddListener(OnConnectClicked);
            }

            // Perform initial load on start
            RefreshCharacterList();
        }

        private void OnConnectClicked()
        {
            if (ipInputField != null)
            {
                GameManager.Instance.SaveBackendUrl(ipInputField.text);
            }
            RefreshCharacterList();
        }

        public void RefreshCharacterList()
        {
            // Clear existing slots
            foreach (var slot in activeSlots)
            {
                Destroy(slot);
            }
            activeSlots.Clear();

            UpdateStatus("Loading cute companions...", Color.gray);

            string url = GameManager.Instance.BackendUrl;
            
            // Check if BackendService exists, otherwise create it (fallback if scene started directly)
            if (BackendService.Instance == null)
            {
                GameObject serviceGo = new GameObject("BackendService");
                serviceGo.AddComponent<BackendService>();
            }

            StartCoroutine(BackendService.Instance.GetCharacters(url, 
                onSuccess: (characters) => {
                    if (characters.Count == 0)
                    {
                        UpdateStatus("No characters found on the server.", errorColor);
                    }
                    else
                    {
                        UpdateStatus("Select a companion to begin!", successColor);
                        PopulateSlots(characters);
                    }
                },
                onError: (error) => {
                    UpdateStatus($"Connection error: {error}\nVerify your server URL.", errorColor);
                }
            ));
        }

        private void PopulateSlots(List<CharacterSummary> characters)
        {
            if (slotTemplatePrefab == null || slotsParent == null)
            {
                Debug.LogError("UI References are missing in HomeController!");
                return;
            }

            foreach (var charSummary in characters)
            {
                GameObject slotGo = Instantiate(slotTemplatePrefab, slotsParent);
                slotGo.SetActive(true);
                activeSlots.Add(slotGo);

                // Set Character Name
                TMP_Text nameText = slotGo.transform.Find("NameText")?.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    nameText.text = charSummary.name;
                }

                // Set Button Callback
                Button slotButton = slotGo.GetComponent<Button>();
                if (slotButton != null)
                {
                    int id = charSummary.id;
                    slotButton.onClick.AddListener(() => OnCharacterSelected(id));
                }

                // Download and display Thumbnail Sprite
                Image iconImage = slotGo.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null && !string.IsNullOrEmpty(charSummary.thumbnail_url))
                {
                    StartCoroutine(BackendService.Instance.LoadSprite(charSummary.thumbnail_url, (sprite) =>
                    {
                        if (iconImage != null) iconImage.sprite = sprite;
                    }));
                }
            }
        }

        private void OnCharacterSelected(int id)
        {
            UpdateStatus("Inviting your companion...", successColor);

            StartCoroutine(BackendService.Instance.GetCharacterDetails(
                GameManager.Instance.BackendUrl, 
                id,
                onSuccess: (details) =>
                {
                    GameManager.Instance.SelectedCharacter = details;
                    SceneManager.LoadScene("AR");
                },
                onError: (error) =>
                {
                    UpdateStatus($"Failed to load character details: {error}", errorColor);
                }
            ));
        }

        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
    }
}
