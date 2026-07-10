using UnityEngine;
using CozyAR.Data;

namespace CozyAR.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const string BackendUrlKey = "CozyAR_BackendUrl";
        private const string DefaultBackendUrl = "http://192.168.86.63:3000";

        public string BackendUrl { get; private set; }
        public CharacterDetails SelectedCharacter { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        public void SaveBackendUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) url = DefaultBackendUrl;
            
            // Clean URL trailing slash
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            BackendUrl = url;
            PlayerPrefs.SetString(BackendUrlKey, url);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            BackendUrl = PlayerPrefs.GetString(BackendUrlKey, DefaultBackendUrl);
            if (BackendUrl.Contains("localhost"))
            {
                BackendUrl = DefaultBackendUrl;
            }
        }
    }
}
