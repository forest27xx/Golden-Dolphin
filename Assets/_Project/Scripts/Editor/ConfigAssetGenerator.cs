using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public sealed class ConfigAssetGenerator : EditorWindow
    {
        private const string DataFolder = "Assets/_Project/Data";
        private const string ConfigsFolder = DataFolder + "/Configs";
        private const string CardsFolder = ConfigsFolder + "/Cards";
        private const string LevelsFolder = ConfigsFolder + "/Levels";
        private const string GameConfigPath = ConfigsFolder + "/GameConfig.asset";

        [MenuItem("Tools/Generate Config Assets")]
        public static void Open()
        {
            ConfigAssetGenerator window = GetWindow<ConfigAssetGenerator>("Config Assets");
            window.minSize = new Vector2(360f, 140f);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Generate Config Assets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates card, level, and master GameConfig assets from BuiltInConfigs.",
                MessageType.Info);

            if (GUILayout.Button("Generate Config Assets", GUILayout.Height(30f)))
            {
                Generate();
            }
        }

        private static void Generate()
        {
            EnsureFolder("Assets/_Project", "Data");
            EnsureFolder(DataFolder, "Configs");
            EnsureFolder(ConfigsFolder, "Cards");
            EnsureFolder(ConfigsFolder, "Levels");

            List<CardConfigSO> cardAssets = GenerateCardAssets();
            List<LevelConfigSO> levelAssets = GenerateLevelAssets();
            GameConfigSO gameConfig = GenerateGameConfigAsset(cardAssets, levelAssets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = gameConfig;

            EditorUtility.DisplayDialog(
                "Config Assets Generated",
                "Generated config assets under " + ConfigsFolder + ".",
                "OK");
        }

        private static List<CardConfigSO> GenerateCardAssets()
        {
            List<CardConfigSO> cardAssets = new List<CardConfigSO>();
            foreach (CardConfig card in BuiltInConfigs.Cards)
            {
                string path = CardsFolder + "/" + ToAssetFileName(card.id, "card") + ".asset";
                CardConfigSO asset = AssetDatabase.LoadAssetAtPath<CardConfigSO>(path);
                if (asset == null)
                {
                    asset = CreateInstance<CardConfigSO>();
                    asset.Initialize(card);
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    asset.Initialize(card);
                    EditorUtility.SetDirty(asset);
                }

                cardAssets.Add(asset);
            }

            return cardAssets;
        }

        private static List<LevelConfigSO> GenerateLevelAssets()
        {
            List<LevelConfigSO> levelAssets = new List<LevelConfigSO>();
            foreach (LevelConfig level in BuiltInConfigs.Levels)
            {
                string path = LevelsFolder + "/" + ToAssetFileName(level.id, "level") + ".asset";
                LevelConfigSO asset = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(path);
                if (asset == null)
                {
                    asset = CreateInstance<LevelConfigSO>();
                    asset.Initialize(level);
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    asset.Initialize(level);
                    EditorUtility.SetDirty(asset);
                }

                levelAssets.Add(asset);
            }

            return levelAssets;
        }

        private static GameConfigSO GenerateGameConfigAsset(List<CardConfigSO> cardAssets, List<LevelConfigSO> levelAssets)
        {
            GameConfigSO asset = AssetDatabase.LoadAssetAtPath<GameConfigSO>(GameConfigPath);
            if (asset == null)
            {
                asset = CreateInstance<GameConfigSO>();
                asset.Initialize(cardAssets, levelAssets);
                AssetDatabase.CreateAsset(asset, GameConfigPath);
            }
            else
            {
                asset.Initialize(cardAssets, levelAssets);
                EditorUtility.SetDirty(asset);
            }

            return asset;
        }

        private static void EnsureFolder(string parentFolder, string folderName)
        {
            string path = parentFolder + "/" + folderName;
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        private static string ToAssetFileName(string id, string fallback)
        {
            if (string.IsNullOrEmpty(id))
            {
                return fallback;
            }

            char[] characters = id.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                char c = characters[i];
                bool valid = char.IsLetterOrDigit(c) || c == '_' || c == '-';
                if (!valid)
                {
                    characters[i] = '_';
                }
            }

            return new string(characters);
        }
    }
}
