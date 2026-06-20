using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;

namespace MemoryTower.EditorTools
{
    public static class MemoryTowerProjectBuilder
    {
        private const string MenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

        [MenuItem("Memory Tower/Build Demo Scenes")]
        public static void BuildAll()
        {
            EnsureDirectories();
            EditorSettings.serializationMode = SerializationMode.ForceText;
            PlayerSettings.productName = "记忆危楼 Demo";
            ConfigureVisualAssets();

            CreateMainMenuScene();
            CreateGameScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Memory Tower demo scenes generated.");
        }

        private static void EnsureDirectories()
        {
            string[] directories =
            {
                "Assets/_Project",
                "Assets/_Project/Art",
                "Assets/_Project/Audio",
                "Assets/_Project/Data/Cards",
                "Assets/_Project/Data/Levels",
                "Assets/_Project/Data/Text",
                "Assets/_Project/Materials",
                "Assets/_Project/Prefabs/Cards",
                "Assets/_Project/Prefabs/Building",
                "Assets/_Project/Prefabs/UI",
                "Assets/_Project/Resources/Art/UI",
                "Assets/_Project/Resources/Art/Building",
                "Assets/_Project/Resources/Art/Cards",
                "Assets/_Project/Resources/Art/VFX",
                "Assets/_Project/Scenes",
                "Assets/_Project/Scripts/Core",
                "Assets/_Project/Scripts/Cards",
                "Assets/_Project/Scripts/Building",
                "Assets/_Project/Scripts/Collapse",
                "Assets/_Project/Scripts/Rewards",
                "Assets/_Project/Scripts/UI",
                "Assets/_Project/Scripts/Save",
                "Assets/_Project/Scripts/Editor"
            };

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        [MenuItem("Memory Tower/Configure Visual Assets")]
        public static void ConfigureVisualAssets()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Resources/Art" });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = path.Contains("background") ? 2048 : 1024;
                importer.spritePixelsPerUnit = 100f;

                if (path.Contains("button_"))
                {
                    importer.spriteBorder = new Vector4(28f, 28f, 28f, 28f);
                }
                else if (path.Contains("panel_") || path.Contains("hud_strip") || path.Contains("hand_tray"))
                {
                    importer.spriteBorder = new Vector4(42f, 42f, 42f, 42f);
                }

                importer.SaveAndReimport();
            }
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.07f, 0.08f, 0.09f, 1f));
            CreateEventSystem();

            GameObject root = new GameObject("MainMenuRoot");
            root.AddComponent<MainMenuController>();

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void CreateGameScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.08f, 0.09f, 0.1f, 1f));
            CreateEventSystem();

            GameObject root = new GameObject("GameRoot");
            UIManager uiManager = root.AddComponent<UIManager>();
            LevelManager levelManager = root.AddComponent<LevelManager>();

            SerializedObject serializedLevel = new SerializedObject(levelManager);
            serializedLevel.FindProperty("uiManager").objectReferenceValue = uiManager;
            serializedLevel.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        private static void CreateCamera(Color backgroundColor)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
