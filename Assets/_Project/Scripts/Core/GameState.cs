using System.Collections.Generic;

namespace MemoryTower
{
    public sealed class GameState
    {
        private static GameState instance;

        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();
                    instance.ResetProgress();
                }

                return instance;
            }
        }

        public int requestedLevelIndex;
        public int totalFragments;
        public readonly HashSet<string> completedLevelIds = new HashSet<string>();
        public readonly HashSet<string> unlockedCardIds = new HashSet<string>();

        public void ResetProgress()
        {
            requestedLevelIndex = 0;
            totalFragments = 0;
            completedLevelIds.Clear();
            unlockedCardIds.Clear();
            unlockedCardIds.Add("tap");
            unlockedCardIds.Add("strike");
            unlockedCardIds.Add("stabilize");
            unlockedCardIds.Add("inspect_crack");
        }

        public void MarkLevelComplete(LevelConfig level, int levelIndex, int fragmentsEarned)
        {
            completedLevelIds.Add(level.id);
            totalFragments += fragmentsEarned;

            foreach (string rewardCardId in level.rewardCardIds)
            {
                if (!string.IsNullOrEmpty(rewardCardId))
                {
                    unlockedCardIds.Add(rewardCardId);
                }
            }

            requestedLevelIndex = levelIndex + 1;
            if (requestedLevelIndex >= BuiltInConfigs.Levels.Count)
            {
                requestedLevelIndex = BuiltInConfigs.Levels.Count - 1;
            }
        }
    }
}
