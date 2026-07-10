using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using CozyAR.Data;

namespace CozyAR.Network
{
    [Serializable]
    public class CharacterSummaryListWrapper
    {
        public List<CharacterSummary> items;
    }

    public class BackendService : MonoBehaviour
    {
        public static BackendService Instance { get; private set; }

        // Cache downloaded textures to make expression swapping instant
        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Fetches all characters (array wrapped for JsonUtility helper)
        public IEnumerator GetCharacters(string baseUrl, Action<List<CharacterSummary>> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/characters";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Wrap array json for Unity JsonUtility
                    string rawJson = webRequest.downloadHandler.text;
                    string wrappedJson = "{\"items\":" + rawJson + "}";
                    try
                    {
                        CharacterSummaryListWrapper wrapper = JsonUtility.FromJson<CharacterSummaryListWrapper>(wrappedJson);
                        onSuccess?.Invoke(wrapper.items);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"JSON parsing error: {ex.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"Failed to load characters: {webRequest.error}");
                }
            }
        }

        // Fetches detail for a specific character
        public IEnumerator GetCharacterDetails(string baseUrl, int id, Action<CharacterDetails> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/characters/{id}";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        CharacterDetails details = JsonUtility.FromJson<CharacterDetails>(webRequest.downloadHandler.text);
                        onSuccess?.Invoke(details);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"JSON parsing error: {ex.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"Failed to load character details: {webRequest.error}");
                }
            }
        }

        // Downloads and caches sprites
        public IEnumerator LoadSprite(string url, Action<Sprite> callback, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                onError?.Invoke("URL is empty");
                yield break;
            }

            if (spriteCache.TryGetValue(url, out Sprite cachedSprite))
            {
                callback?.Invoke(cachedSprite);
                yield break;
            }

            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    
                    // Optimizations for pixel art or clean illustrations
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;

                    // Calculate dynamic Pixels Per Unit to set physical height to exactly 0.35m (35 cm) in the AR world
                    float targetHeightMeters = 0.35f;
                    float ppu = texture.height / targetHeightMeters;

                    Sprite sprite = Sprite.Create(
                        texture, 
                        new Rect(0, 0, texture.width, texture.height), 
                        new Vector2(0.5f, 0.5f), // Pivot center
                        ppu
                    );

                    spriteCache[url] = sprite;
                    callback?.Invoke(sprite);
                }
                else
                {
                    onError?.Invoke($"Failed to download texture: {webRequest.error}");
                }
            }
        }
    }
}
