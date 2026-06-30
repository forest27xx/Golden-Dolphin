using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MemoryTower
{
    public static class ConfigLoader
    {
        private const string GameConfigResourcePath = "Configs/GameConfig";

#if UNITY_EDITOR
        private const string GeneratedGameConfigPath = "Assets/_Project/Data/Configs/GameConfig.asset";
#endif

        private static bool isLoaded;
        private static GameConfigSO gameConfig;
        private static List<CardConfig> cards;
        private static List<LevelConfig> levels;

        public static List<CardConfig> Cards
        {
            get
            {
                EnsureLoaded();
                return cards;
            }
        }

        public static List<LevelConfig> Levels
        {
            get
            {
                EnsureLoaded();
                return levels;
            }
        }

        public static int LevelCount
        {
            get
            {
                EnsureLoaded();
                return levels.Count;
            }
        }

        public static Dictionary<string, CardConfig> CreateCardLookup()
        {
            EnsureLoaded();

            Dictionary<string, CardConfig> lookup = new Dictionary<string, CardConfig>();
            foreach (CardConfig card in cards)
            {
                if (!string.IsNullOrEmpty(card.id))
                {
                    lookup[card.id] = card;
                }
            }

            return lookup;
        }

        public static LevelConfig GetLevel(int index)
        {
            EnsureLoaded();
            return levels[index];
        }

        private static void EnsureLoaded()
        {
            if (isLoaded)
            {
                return;
            }

            isLoaded = true;
            gameConfig = Resources.Load<GameConfigSO>(GameConfigResourcePath);

#if UNITY_EDITOR
            if (gameConfig == null)
            {
                gameConfig = AssetDatabase.LoadAssetAtPath<GameConfigSO>(GeneratedGameConfigPath);
            }
#endif

            if (gameConfig != null)
            {
                cards = gameConfig.GetCards();
                levels = gameConfig.GetLevels();
                return;
            }

            cards = CloneCards(BuiltInConfigs.Cards);
            levels = CloneLevels(BuiltInConfigs.Levels);
        }

        private static List<CardConfig> CloneCards(List<CardConfig> source)
        {
            List<CardConfig> result = new List<CardConfig>();
            foreach (CardConfig card in source)
            {
                result.Add(CloneCard(card));
            }

            return result;
        }

        private static List<LevelConfig> CloneLevels(List<LevelConfig> source)
        {
            List<LevelConfig> result = new List<LevelConfig>();
            foreach (LevelConfig level in source)
            {
                result.Add(CloneLevel(level));
            }

            return result;
        }

        private static CardConfig CloneCard(CardConfig card)
        {
            return new CardConfig(
                card.id,
                card.displayName,
                card.cardType,
                card.description,
                card.damage,
                card.collapseDelta,
                card.targetType,
                card.isOneShot,
                card.unlockLevel);
        }

        private static LevelConfig CloneLevel(LevelConfig level)
        {
            return new LevelConfig(
                level.id,
                level.displayName,
                level.rows,
                level.columns,
                level.collapseThreshold,
                level.actionCount,
                level.requiredFragments,
                level.initialDeckCardIds,
                level.rewardCardIds,
                level.hasFinalCore);
        }
    }
}
