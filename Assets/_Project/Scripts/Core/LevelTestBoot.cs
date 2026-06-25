#if UNITY_EDITOR
using UnityEditor;

namespace MemoryTower
{
    /// <summary>
    /// 编辑器专用：跨 domain reload 把"测试要进入的关卡索引"传给 LevelManager 启动钩子。
    /// 整类被 #if UNITY_EDITOR 包裹，正式构建中完全不存在。
    /// </summary>
    public static class LevelTestBoot
    {
        private const string TargetKey = "MemoryTower.LevelTest.TargetIndex";

        public static void SetTarget(int levelIndex)
        {
            SessionState.SetInt(TargetKey, levelIndex);
        }

        public static bool TryConsumeTarget(out int levelIndex)
        {
            const int missing = int.MinValue;
            int stored = SessionState.GetInt(TargetKey, missing);
            if (stored == missing)
            {
                levelIndex = -1;
                return false;
            }

            SessionState.EraseInt(TargetKey);
            levelIndex = stored;
            return true;
        }

        public static void TryApplyTestTarget()
        {
            int targetIndex;
            if (!TryConsumeTarget(out targetIndex))
            {
                return;
            }

            GameState state = GameState.Instance;
            state.ResetProgress();
            foreach (CardConfig card in BuiltInConfigs.Cards)
            {
                state.unlockedCardIds.Add(card.id);
            }

            state.requestedLevelIndex = targetIndex;
        }
    }
}
#endif
