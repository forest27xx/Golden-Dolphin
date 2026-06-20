using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryTower
{
    public static class SaveManager
    {
        private const string SaveKey = "MemoryTower.Save";

        public static bool HasSave()
        {
            return PlayerPrefs.HasKey(SaveKey);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        public static void Save(GameState state)
        {
            SaveData data = new SaveData();
            data.saveVersion = "0.1-demo";
            data.requestedLevelIndex = state.requestedLevelIndex;
            data.totalFragments = state.totalFragments;
            data.completedLevelIds.AddRange(state.completedLevelIds);
            data.unlockedCardIds.AddRange(state.unlockedCardIds);

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static bool Load(GameState state)
        {
            if (!HasSave())
            {
                return false;
            }

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
                if (data == null)
                {
                    return false;
                }

                state.ResetProgress();
                state.requestedLevelIndex = Mathf.Clamp(data.requestedLevelIndex, 0, BuiltInConfigs.Levels.Count - 1);
                state.totalFragments = Mathf.Max(0, data.totalFragments);

                state.completedLevelIds.Clear();
                foreach (string id in data.completedLevelIds)
                {
                    state.completedLevelIds.Add(id);
                }

                state.unlockedCardIds.Clear();
                foreach (string id in data.unlockedCardIds)
                {
                    state.unlockedCardIds.Add(id);
                }

                if (state.unlockedCardIds.Count == 0)
                {
                    state.unlockedCardIds.Add("tap");
                    state.unlockedCardIds.Add("strike");
                    state.unlockedCardIds.Add("stabilize");
                    state.unlockedCardIds.Add("inspect_crack");
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Save load failed: " + exception.Message);
                return false;
            }
        }

        [Serializable]
        private sealed class SaveData
        {
            public string saveVersion;
            public int requestedLevelIndex;
            public int totalFragments;
            public List<string> completedLevelIds = new List<string>();
            public List<string> unlockedCardIds = new List<string>();
        }
    }
}
