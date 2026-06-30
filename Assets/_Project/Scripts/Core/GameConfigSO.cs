using System.Collections.Generic;
using UnityEngine;

namespace MemoryTower
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MemoryTower/Configs/Game Config")]
    public sealed class GameConfigSO : ScriptableObject
    {
        [SerializeField] private List<CardConfigSO> cards = new List<CardConfigSO>();
        [SerializeField] private List<LevelConfigSO> levels = new List<LevelConfigSO>();

        public List<CardConfig> GetCards()
        {
            List<CardConfig> result = new List<CardConfig>();
            if (cards == null)
            {
                return result;
            }

            foreach (CardConfigSO cardAsset in cards)
            {
                if (cardAsset == null)
                {
                    continue;
                }

                result.Add(cardAsset.ToConfig());
            }

            return result;
        }

        public Dictionary<string, CardConfig> GetCardLookup()
        {
            Dictionary<string, CardConfig> lookup = new Dictionary<string, CardConfig>();
            if (cards == null)
            {
                return lookup;
            }

            foreach (CardConfigSO cardAsset in cards)
            {
                if (cardAsset == null)
                {
                    continue;
                }

                CardConfig card = cardAsset.ToConfig();
                if (!string.IsNullOrEmpty(card.id))
                {
                    lookup[card.id] = card;
                }
            }

            return lookup;
        }

        public List<LevelConfig> GetLevels()
        {
            List<LevelConfig> result = new List<LevelConfig>();
            if (levels == null)
            {
                return result;
            }

            foreach (LevelConfigSO levelAsset in levels)
            {
                if (levelAsset == null)
                {
                    continue;
                }

                result.Add(levelAsset.ToConfig());
            }

            return result;
        }

        public CardConfig GetCardById(string id)
        {
            if (string.IsNullOrEmpty(id) || cards == null)
            {
                return null;
            }

            CardConfig match = null;
            foreach (CardConfigSO cardAsset in cards)
            {
                if (cardAsset == null)
                {
                    continue;
                }

                CardConfig card = cardAsset.ToConfig();
                if (card.id == id)
                {
                    match = card;
                }
            }

            return match;
        }

        public LevelConfig GetLevelByIndex(int index)
        {
            if (index < 0 || levels == null)
            {
                return null;
            }

            int validIndex = 0;
            foreach (LevelConfigSO levelAsset in levels)
            {
                if (levelAsset == null)
                {
                    continue;
                }

                if (validIndex == index)
                {
                    return levelAsset.ToConfig();
                }

                validIndex++;
            }

            return null;
        }

        public int GetLevelCount()
        {
            if (levels == null)
            {
                return 0;
            }

            int count = 0;
            foreach (LevelConfigSO levelAsset in levels)
            {
                if (levelAsset != null)
                {
                    count++;
                }
            }

            return count;
        }

        public void Initialize(List<CardConfigSO> cardConfigs, List<LevelConfigSO> levelConfigs)
        {
            this.cards = cardConfigs == null ? new List<CardConfigSO>() : new List<CardConfigSO>(cardConfigs);
            this.levels = levelConfigs == null ? new List<LevelConfigSO>() : new List<LevelConfigSO>(levelConfigs);
        }
    }
}
