using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using CozyAR.AR;
using CozyAR.Core;
using CozyAR.UI;
using System.IO;
using UnityEditor.Build.Reporting;

using CharacterController = CozyAR.AR.CharacterController;

namespace CozyAR.Editor
{
    public class ProjectSetup : EditorWindow
    {
        [MenuItem("Tools/Cozy AR/Setup, Build & Run")]
        public static void RunSetupBuildAndRun()
        {
            // 1. Run the setup first to regenerate everything cleanly
            RunSetup();

            Debug.Log("Starting Build & Run sequence for Android...");

            // 2. Configure build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/Home.unity", "Assets/Scenes/AR.unity" };
            
            // Define output path for the APK
            string buildFolder = "Builds";
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }
            buildPlayerOptions.locationPathName = Path.Combine(buildFolder, "CozyAR.apk");
            buildPlayerOptions.target = BuildTarget.Android;
            
            // AutoRunPlayer compiles, deploys, and opens the app on the connected device via ADB
            buildPlayerOptions.options = BuildOptions.AutoRunPlayer;

            // 3. Trigger the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded! APK size: {summary.totalSize / (1024 * 1024):F2} MB. Deployed and launched on phone.");
                EditorUtility.DisplayDialog("Cozy AR Builder", 
                    "Setup, Build & Run completed successfully! Look at your phone screen!", 
                    "Cozy!");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"Build failed with {summary.totalErrors} errors.");
                EditorUtility.DisplayDialog("Cozy AR Builder", 
                    "Build failed. Check the Unity Console logs for details.", 
                    "Understood");
            }
        }

        [MenuItem("Tools/Cozy AR/Setup Scenes & Prefabs")]
        public static void RunSetup()
        {
            Debug.Log("Starting Cozy AR Project Scaffolding...");

            // 0. Configure Android Build Settings (Required for ARCore & modern 64-bit devices)
            PlayerSettings.productName = "Luma";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

            // Configure Graphics API (ARCore requires OpenGLES3, Vulkan is not supported)
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            // Allow cleartext HTTP connections (required for local server testing in Editor & Devices)
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;

            // 1. Create Folder Structure
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Prefabs");
            EnsureFolder("Assets/Materials");

            // 2. Create Default Material for Character Sprite Renderer
            Material spriteMaterial = CreateDefaultSpriteMaterial();

            // 3. Create the Character Prefab
            GameObject prefabAsset = CreateCharacterPrefab(spriteMaterial);

            // 3.5 Create the AR Plane Prefab
            GameObject planePrefabAsset = CreateARPlanePrefab();

            // 4. Create the Home Scene
            CreateHomeScene(prefabAsset);

            // 5. Create the AR Scene
            CreateARScene(prefabAsset, planePrefabAsset);

            // 6. Update Build Settings
            UpdateBuildSettings();

            Debug.Log("Cozy AR Project Scaffolding completed successfully!");
            EditorUtility.DisplayDialog("Cozy AR Setup", "Scenes, Prefabs, and Materials have been generated successfully!", "Cozy!");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path);
                string name = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Material CreateDefaultSpriteMaterial()
        {
            string matPath = "Assets/Materials/CozySpriteMat.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader spriteShader = Shader.Find("Sprites/Default");
                mat = new Material(spriteShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            return mat;
        }

        private static GameObject CreateCharacterPrefab(Material mat)
        {
            string prefabPath = "Assets/Resources/Prefabs/CharacterPrefab.prefab";
            
            // Delete old one if exists
            AssetDatabase.DeleteAsset(prefabPath);

            // Create GameObject structure
            GameObject root = new GameObject("CharacterPrefab");
            
            // Add components
            var controller = root.AddComponent<CharacterController>();
            root.AddComponent<CharacterBillboard>();
            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1, 1, 0.1f);

            // Create child visual representation (Quad)
            GameObject spriteChild = GameObject.CreatePrimitive(PrimitiveType.Quad);
            spriteChild.name = "SpriteVisual";
            spriteChild.transform.SetParent(root.transform);
            spriteChild.transform.localPosition = Vector3.zero; // Pivot at bottom, aligned with parent pivot
            
            // Remove the mesh collider on the visual child
            DestroyImmediate(spriteChild.GetComponent<Collider>());

            // Replace MeshRenderer with SpriteRenderer
            DestroyImmediate(spriteChild.GetComponent<MeshRenderer>());
            DestroyImmediate(spriteChild.GetComponent<MeshFilter>());
            var spriteRenderer = spriteChild.AddComponent<SpriteRenderer>();
            spriteRenderer.material = mat;

            // Link visual sprite renderer to controller using serialized properties
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            serializedController.ApplyModifiedProperties();

            // Save prefab
            GameObject prefabObj = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"Scaffolded Character Prefab saved at: {prefabPath}");
            return prefabObj;
        }

        private static GameObject CreateARPlanePrefab()
        {
            string prefabPath = "Assets/Resources/Prefabs/ARPlane.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            GameObject root = new GameObject("ARPlanePrefab");
            
            // Add components required for plane visualization and detection
            root.AddComponent<ARPlane>();
            root.AddComponent<ARPlaneMeshVisualizer>();
            root.AddComponent<MeshFilter>();
            var renderer = root.AddComponent<MeshRenderer>();
            root.AddComponent<MeshCollider>();

            // Create a nice cozy, semi-transparent material for the plane
            Material planeMat = new Material(Shader.Find("Standard"));
            planeMat.SetFloat("_Mode", 3); // 3 corresponds to Transparent mode
            planeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMat.SetInt("_ZWrite", 0);
            planeMat.DisableKeyword("_ALPHATEST_ON");
            planeMat.EnableKeyword("_ALPHABLEND_ON");
            planeMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            planeMat.renderQueue = 3000;
            planeMat.color = new Color(0.4f, 0.8f, 0.6f, 0.3f); // soft cozy green transparent

            AssetDatabase.CreateAsset(planeMat, "Assets/Materials/ARPlaneMaterial.mat");
            renderer.material = planeMat;

            // Save prefab
            GameObject prefabObj = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            Debug.Log($"Scaffolded AR Plane Prefab saved at: {prefabPath}");
            return prefabObj;
        }

        private static void CreateHomeScene(GameObject prefabAsset)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create UI Canvas
            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Create EventSystem (Essential for UI input and button clicks in Unity!)
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 0. Create Luma brand vertical gradient texture
            string bgDir = "Assets/Materials";
            if (!Directory.Exists(bgDir)) Directory.CreateDirectory(bgDir);
            string bgPath = Path.Combine(bgDir, "LumaBackgroundGradient.png");
            if (!File.Exists(bgPath))
            {
                Texture2D gradTex = new Texture2D(1, 128);
                for (int y = 0; y < 128; y++)
                {
                    float t = (float)y / 127f;
                    Color col = Color.Lerp(
                        new Color(44f/255f, 32f/255f, 69f/255f),  // Bottom: Rich Midnight Violet
                        new Color(20f/255f, 16f/255f, 32f/255f),  // Top: Deep Purple-Navy
                        t
                    );
                    gradTex.SetPixel(0, y, col);
                }
                gradTex.Apply();
                byte[] bytes = gradTex.EncodeToPNG();
                File.WriteAllBytes(bgPath, bytes);
                AssetDatabase.Refresh();
            }

            // Configure Texture Importer as Sprite if needed
            TextureImporter importer = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);

            // Create Background Image
            GameObject bgGo = new GameObject("Background", typeof(RectTransform));
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Create Title Header
            GameObject titleGo = new GameObject("LumaTitle", typeof(RectTransform));
            titleGo.transform.SetParent(canvasGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "L U M A";
            titleText.fontStyle = FontStyles.Bold;
            titleText.fontSize = 42;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -180);
            titleRect.sizeDelta = new Vector2(600, 60);

            // Create Subtitle Header
            GameObject subGo = new GameObject("LumaSubtitle", typeof(RectTransform));
            subGo.transform.SetParent(canvasGo.transform, false);
            var subText = subGo.AddComponent<TextMeshProUGUI>();
            subText.text = "AR COMPANION";
            subText.fontStyle = FontStyles.Normal;
            subText.fontSize = 14;
            subText.color = new Color(180f/255f, 180f/255f, 210f/255f, 1f); // Muted lavender
            subText.alignment = TextAlignmentOptions.Center;
            var subRect = subGo.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 1);
            subRect.anchorMax = new Vector2(0.5f, 1);
            subRect.anchoredPosition = new Vector2(0, -235);
            subRect.sizeDelta = new Vector2(600, 30);

            // Create Status Text
            GameObject statusGo = new GameObject("StatusText", typeof(RectTransform));
            statusGo.transform.SetParent(canvasGo.transform, false);
            var statusText = statusGo.AddComponent<TextMeshProUGUI>();
            statusText.text = "Select a companion to begin!";
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.fontSize = 18;
            statusText.color = new Color(180f/255f, 180f/255f, 210f/255f, 1f);
            var statusRect = statusGo.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, 240);
            statusRect.sizeDelta = new Vector2(600, 60);

            // Create IP Input Field (Frosted glassmorphism)
            GameObject ipGo = new GameObject("IPInputField", typeof(RectTransform));
            ipGo.transform.SetParent(canvasGo.transform, false);
            var ipImage = ipGo.AddComponent<Image>();
            ipImage.color = new Color(255f/255f, 255f/255f, 255f/255f, 0.08f);
            var ipField = ipGo.AddComponent<TMP_InputField>();
            var ipRect = ipGo.GetComponent<RectTransform>();
            ipRect.anchoredPosition = new Vector2(-105, 140);
            ipRect.sizeDelta = new Vector2(430, 60);

            // Input field text child
            GameObject ipTextGo = new GameObject("Text", typeof(RectTransform));
            ipTextGo.transform.SetParent(ipGo.transform, false);
            var ipText = ipTextGo.AddComponent<TextMeshProUGUI>();
            ipText.fontSize = 18;
            ipText.color = Color.white;
            var ipTextRect = ipTextGo.GetComponent<RectTransform>();
            ipTextRect.sizeDelta = new Vector2(410, 48);
            ipField.textComponent = ipText;

            // Create Connect Button (Glowing Lavender)
            GameObject btnGo = new GameObject("ConnectButton", typeof(RectTransform));
            btnGo.transform.SetParent(canvasGo.transform, false);
            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(126f/255f, 85f/255f, 224f/255f, 1f);
            var btn = btnGo.AddComponent<Button>();
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(215, 140);
            btnRect.sizeDelta = new Vector2(170, 60);

            GameObject btnTextGo = new GameObject("BtnText", typeof(RectTransform));
            btnTextGo.transform.SetParent(btnGo.transform, false);
            var btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnText.text = "CONNECT";
            btnText.fontSize = 18;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnTextGo.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 50);

            // Create Scroll View for Characters
            GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform));
            scrollGo.transform.SetParent(canvasGo.transform, false);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchoredPosition = new Vector2(0, -240);
            scrollRect.sizeDelta = new Vector2(660, 950);
            var scrollComp = scrollGo.AddComponent<ScrollRect>();

            GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.sizeDelta = new Vector2(660, 950);
            // Use RectMask2D instead of Mask for transparent viewport (Mask + Alpha 0 hides children!)
            viewportGo.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(660, 950);
            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.padding = new RectOffset(10, 10, 10, 10);
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollComp.viewport = viewportRect;
            scrollComp.content = contentRect;

            // Create Slot Template Prefab inside Resources
            string slotTemplatePath = "Assets/Resources/Prefabs/SlotTemplate.prefab";
            AssetDatabase.DeleteAsset(slotTemplatePath);

            GameObject slotGo = new GameObject("SlotTemplate", typeof(RectTransform));
            var slotBtn = slotGo.AddComponent<Button>();
            var slotImg = slotGo.AddComponent<Image>();
            slotImg.color = new Color(255f/255f, 255f/255f, 255f/255f, 0.05f); // Frosted translucent glass card
            var slotRect = slotGo.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(640, 130);

            // Icon Child (Rounded Avatar)
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(slotGo.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(65, 0);
            iconRect.sizeDelta = new Vector2(90, 90);

            // Text Child (Character Name)
            GameObject textGo = new GameObject("NameText", typeof(RectTransform));
            textGo.transform.SetParent(slotGo.transform, false);
            var slotText = textGo.AddComponent<TextMeshProUGUI>();
            slotText.text = "Character Name";
            slotText.color = Color.white;
            slotText.fontSize = 24;
            slotText.fontStyle = FontStyles.Bold;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(140, 0);
            textRect.offsetMax = new Vector2(-150, 0);

            // Invite Tag capsule on the right
            GameObject inviteGo = new GameObject("InviteTag", typeof(RectTransform));
            inviteGo.transform.SetParent(slotGo.transform, false);
            var inviteImg = inviteGo.AddComponent<Image>();
            inviteImg.color = new Color(242f/255f, 153f/255f, 141f/255f, 0.15f); // Coral transparent
            var inviteRect = inviteGo.GetComponent<RectTransform>();
            inviteRect.anchorMin = new Vector2(1, 0.5f);
            inviteRect.anchorMax = new Vector2(1, 0.5f);
            inviteRect.anchoredPosition = new Vector2(-75, 0);
            inviteRect.sizeDelta = new Vector2(100, 45);

            GameObject inviteTextGo = new GameObject("Text", typeof(RectTransform));
            inviteTextGo.transform.SetParent(inviteGo.transform, false);
            var inviteTxt = inviteTextGo.AddComponent<TextMeshProUGUI>();
            inviteTxt.text = "INVITE";
            inviteTxt.fontSize = 14;
            inviteTxt.fontStyle = FontStyles.Bold;
            inviteTxt.color = new Color(242f/255f, 153f/255f, 141f/255f, 1f); // Coral solid
            inviteTxt.alignment = TextAlignmentOptions.Center;
            inviteTextGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);

            GameObject slotTemplateAsset = PrefabUtility.SaveAsPrefabAsset(slotGo, slotTemplatePath);
            DestroyImmediate(slotGo);

            // Create Manager Object
            GameObject mgrGo = new GameObject("HomeController");
            var homeController = mgrGo.AddComponent<HomeController>();

            // Setup linkages
            var serializedHome = new SerializedObject(homeController);
            serializedHome.FindProperty("ipInputField").objectReferenceValue = ipField;
            serializedHome.FindProperty("connectButton").objectReferenceValue = btn;
            serializedHome.FindProperty("statusText").objectReferenceValue = statusText;
            serializedHome.FindProperty("slotsParent").objectReferenceValue = contentRect;
            serializedHome.FindProperty("slotTemplatePrefab").objectReferenceValue = slotTemplateAsset;
            serializedHome.ApplyModifiedProperties();

            // Create GameManager on a Manager object
            GameObject gameMgrGo = new GameObject("GameManager");
            gameMgrGo.AddComponent<GameManager>();

            // Save Scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Home.unity");
            Debug.Log("Scaffolded Home Scene saved.");
        }

        private static void CreateARScene(GameObject prefabAsset, GameObject planePrefabAsset)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Create Directional Light
            GameObject lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 2. Create AR Session
            GameObject sessionGo = new GameObject("AR Session");
            sessionGo.AddComponent<ARSession>();
            sessionGo.AddComponent<ARInputManager>();

            // 3. Create XR Origin (replaces deprecated ARSessionOrigin)
            GameObject originGo = new GameObject("XR Origin");
            var origin = originGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            originGo.AddComponent<ARRaycastManager>();
            var planeManager = originGo.AddComponent<ARPlaneManager>();

            // Set references on ARPlaneManager
            if (planePrefabAsset != null)
            {
                var serializedPlaneManager = new SerializedObject(planeManager);
                serializedPlaneManager.FindProperty("m_PlanePrefab").objectReferenceValue = planePrefabAsset;
                serializedPlaneManager.ApplyModifiedProperties();
            }

            GameObject cameraOffsetGo = new GameObject("Camera Offset");
            cameraOffsetGo.transform.SetParent(originGo.transform, false);

            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(cameraOffsetGo.transform, false);
            camGo.tag = "MainCamera";
            
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            
            camGo.AddComponent<ARCameraManager>();
            camGo.AddComponent<ARCameraBackground>();
            camGo.AddComponent<ARPoseDriver>();

            // Assign camera references directly
            origin.Camera = camera;
            origin.CameraFloorOffsetObject = cameraOffsetGo;

            // 4. Create UI HUD Canvas
            GameObject canvasGo = new GameObject("AR HUD Canvas", typeof(RectTransform));
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Create EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Photo Button (Solid white capsule)
            GameObject photoGo = new GameObject("PhotoBtn", typeof(RectTransform));
            photoGo.transform.SetParent(canvasGo.transform, false);
            var photoImg = photoGo.AddComponent<Image>();
            photoImg.color = Color.white;
            var photoBtn = photoGo.AddComponent<Button>();
            var photoRect = photoGo.GetComponent<RectTransform>();
            photoRect.anchorMin = new Vector2(0.5f, 0);
            photoRect.anchorMax = new Vector2(0.5f, 0);
            photoRect.anchoredPosition = new Vector2(-95, 60);
            photoRect.sizeDelta = new Vector2(160, 70);
            
            GameObject photoTxtGo = new GameObject("Text", typeof(RectTransform));
            photoTxtGo.transform.SetParent(photoGo.transform, false);
            var photoTxt = photoTxtGo.AddComponent<TextMeshProUGUI>();
            photoTxt.text = "PHOTO";
            photoTxt.fontSize = 18;
            photoTxt.fontStyle = FontStyles.Bold;
            photoTxt.color = new Color(44f/255f, 32f/255f, 69f/255f, 1f); // Deep brand purple text
            photoTxt.alignment = TextAlignmentOptions.Center;
            photoTxtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 70);

            // Video Button (Coral-red capsule)
            GameObject videoGo = new GameObject("VideoBtn", typeof(RectTransform));
            videoGo.transform.SetParent(canvasGo.transform, false);
            var videoImg = videoGo.AddComponent<Image>();
            videoImg.color = new Color(242f/255f, 100f/255f, 100f/255f, 1f);
            var videoBtn = videoGo.AddComponent<Button>();
            var videoRect = videoGo.GetComponent<RectTransform>();
            videoRect.anchorMin = new Vector2(0.5f, 0);
            videoRect.anchorMax = new Vector2(0.5f, 0);
            videoRect.anchoredPosition = new Vector2(95, 60);
            videoRect.sizeDelta = new Vector2(160, 70);

            GameObject videoTxtGo = new GameObject("Text", typeof(RectTransform));
            videoTxtGo.transform.SetParent(videoGo.transform, false);
            var videoTxt = videoTxtGo.AddComponent<TextMeshProUGUI>();
            videoTxt.text = "VIDEO";
            videoTxt.fontSize = 18;
            videoTxt.fontStyle = FontStyles.Bold;
            videoTxt.color = Color.white;
            videoTxt.alignment = TextAlignmentOptions.Center;
            videoTxtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 70);

            // Toast / Instructions Text (Floating dark glass capsule panel)
            GameObject toastGo = new GameObject("ToastTextPanel", typeof(RectTransform));
            toastGo.transform.SetParent(canvasGo.transform, false);
            var toastImg = toastGo.AddComponent<Image>();
            toastImg.color = new Color(24f/255f, 20f/255f, 38f/255f, 0.8f);
            
            var toastRect = toastGo.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 1);
            toastRect.anchorMax = new Vector2(0.5f, 1);
            toastRect.anchoredPosition = new Vector2(0, -120);
            toastRect.sizeDelta = new Vector2(700, 65);

            // Child Text GameObject
            GameObject toastTextGo = new GameObject("Text", typeof(RectTransform));
            toastTextGo.transform.SetParent(toastGo.transform, false);
            var toastTxt = toastTextGo.AddComponent<TextMeshProUGUI>();
            toastTxt.text = "Scan floor and tap to invite companion";
            toastTxt.alignment = TextAlignmentOptions.Center;
            toastTxt.fontSize = 18;
            toastTxt.color = Color.white;
            
            var toastTextRect = toastTextGo.GetComponent<RectTransform>();
            toastTextRect.anchorMin = Vector2.zero;
            toastTextRect.anchorMax = Vector2.one;
            toastTextRect.offsetMin = Vector2.zero;
            toastTextRect.offsetMax = Vector2.zero;

            // Recording Indicator Overlay
            GameObject recordIndGo = new GameObject("RecordingIndicator", typeof(RectTransform));
            recordIndGo.transform.SetParent(canvasGo.transform, false);
            var recordIndImg = recordIndGo.AddComponent<Image>();
            recordIndImg.color = new Color(1, 0, 0, 0.2f); // Soft screen tint
            var recordIndRect = recordIndGo.GetComponent<RectTransform>();
            recordIndRect.anchorMin = new Vector2(0, 0);
            recordIndRect.anchorMax = new Vector2(1, 1);
            recordIndRect.sizeDelta = Vector2.zero; // Full screen overlay

            GameObject recTextGo = new GameObject("RecText", typeof(RectTransform));
            recTextGo.transform.SetParent(recordIndGo.transform, false);
            var recTxt = recTextGo.AddComponent<TextMeshProUGUI>();
            recTxt.text = "● RECORDING...";
            recTxt.fontSize = 24;
            recTxt.fontStyle = FontStyles.Bold;
            recTxt.color = Color.red;
            recTxt.alignment = TextAlignmentOptions.Center;

            // Back Button (Translucent dark glass)
            GameObject backGo = new GameObject("BackBtn", typeof(RectTransform));
            backGo.transform.SetParent(canvasGo.transform, false);
            backGo.AddComponent<Image>().color = new Color(24f/255f, 20f/255f, 38f/255f, 0.75f);
            var backBtn = backGo.AddComponent<Button>();
            var backRect = backGo.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 1);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.anchoredPosition = new Vector2(90, -110);
            backRect.sizeDelta = new Vector2(130, 60);

            GameObject backTextGo = new GameObject("Text", typeof(RectTransform));
            backTextGo.transform.SetParent(backGo.transform, false);
            var backTxt = backTextGo.AddComponent<TextMeshProUGUI>();
            backTxt.text = "← LUMA";
            backTxt.color = Color.white;
            backTxt.fontSize = 18;
            backTxt.fontStyle = FontStyles.Bold;
            backTxt.alignment = TextAlignmentOptions.Center;
            backTextGo.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 60);

            // 5. Controls Panel (Translucent dark dock tray)
            GameObject ctrlPanelGo = new GameObject("ControlsPanel", typeof(RectTransform));
            ctrlPanelGo.transform.SetParent(canvasGo.transform, false);
            ctrlPanelGo.AddComponent<Image>().color = new Color(24f/255f, 20f/255f, 38f/255f, 0.85f);
            var ctrlPanelRect = ctrlPanelGo.GetComponent<RectTransform>();
            ctrlPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            ctrlPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            ctrlPanelRect.anchoredPosition = new Vector2(0, -220);
            ctrlPanelRect.sizeDelta = new Vector2(800, 100);
            var ctrlLayout = ctrlPanelGo.AddComponent<HorizontalLayoutGroup>();
            ctrlLayout.spacing = 8;
            ctrlLayout.padding = new RectOffset(10, 10, 10, 10);
            ctrlLayout.childControlWidth = true;
            ctrlLayout.childControlHeight = true;
            ctrlLayout.childForceExpandWidth = true;
            ctrlLayout.childForceExpandHeight = true;

            // Horizontal buttons layout helpers
            Button happyBtn = CreatePanelButton(ctrlPanelGo.transform, "Happy", "Happy");
            Button sadBtn = CreatePanelButton(ctrlPanelGo.transform, "Sad", "Sad");
            Button waveBtn = CreatePanelButton(ctrlPanelGo.transform, "Wave", "Wave");
            Button idleBtn = CreatePanelButton(ctrlPanelGo.transform, "Idle", "Idle");
            Button resetBtn = CreatePanelButton(ctrlPanelGo.transform, "Reset", "Reset");
            Button delBtn = CreatePanelButton(ctrlPanelGo.transform, "Delete", "Delete");
            Button spawnCopyBtn = CreatePanelButton(ctrlPanelGo.transform, "Spawn", "Spawn");

            // 6. Create Managers
            GameObject arMgrsGo = new GameObject("ARManagers");
            var placementMgr = arMgrsGo.AddComponent<ARPlacementManager>();
            var uiController = arMgrsGo.AddComponent<ARUiController>();

            // Setup linkages
            var serializedUi = new SerializedObject(uiController);
            serializedUi.FindProperty("arHudCanvas").objectReferenceValue = canvas;
            serializedUi.FindProperty("controlPanel").objectReferenceValue = ctrlPanelGo;
            serializedUi.FindProperty("expressionHappyBtn").objectReferenceValue = happyBtn;
            serializedUi.FindProperty("expressionSadBtn").objectReferenceValue = sadBtn;
            serializedUi.FindProperty("expressionWaveBtn").objectReferenceValue = waveBtn;
            serializedUi.FindProperty("expressionIdleBtn").objectReferenceValue = idleBtn;
            serializedUi.FindProperty("resetPosBtn").objectReferenceValue = resetBtn;
            serializedUi.FindProperty("deleteBtn").objectReferenceValue = delBtn;
            serializedUi.FindProperty("spawnAnotherBtn").objectReferenceValue = spawnCopyBtn;
            serializedUi.FindProperty("photoBtn").objectReferenceValue = photoBtn;
            serializedUi.FindProperty("videoBtn").objectReferenceValue = videoBtn;
            serializedUi.FindProperty("backBtn").objectReferenceValue = backBtn;
            serializedUi.FindProperty("toastText").objectReferenceValue = toastTxt;
            serializedUi.FindProperty("recordingIndicator").objectReferenceValue = recordIndGo;
            serializedUi.ApplyModifiedProperties();

            // Link prefab to placement manager
            var serializedPlacement = new SerializedObject(placementMgr);
            serializedPlacement.FindProperty("characterPrefab").objectReferenceValue = prefabAsset;
            serializedPlacement.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/AR.unity");
            Debug.Log("Scaffolded AR Scene saved.");
        }

        private static Button CreatePanelButton(Transform parent, string name, string label)
        {
            GameObject btnGo = new GameObject(name + "Btn", typeof(RectTransform));
            btnGo.transform.SetParent(parent, false);
            btnGo.AddComponent<Image>().color = new Color(255f/255f, 255f/255f, 255f/255f, 0.12f); // Frosted transparent capsule
            var btn = btnGo.AddComponent<Button>();
            
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(btnGo.transform, false);
            var txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.text = label.ToUpper(); // Refined uppercase
            txt.fontSize = 15;
            txt.fontStyle = FontStyles.Bold;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            
            return btn;
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
            
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[2];
            newScenes[0] = new EditorBuildSettingsScene("Assets/Scenes/Home.unity", true);
            newScenes[1] = new EditorBuildSettingsScene("Assets/Scenes/AR.unity", true);

            EditorBuildSettings.scenes = newScenes;
            Debug.Log("Build settings updated with Home and AR scenes.");
        }
    }
}
