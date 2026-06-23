using System.Collections.Generic;

namespace MemoryTower
{
    public static class BuiltInConfigs
    {
        public static readonly List<CardConfig> Cards = new List<CardConfig>
        {
            new CardConfig("tap", "轻敲", CardType.Basic, "对单个方块造成 1 点伤害。", 1, 1, CardTargetType.SingleBlock, false, 0),
            new CardConfig("strike", "重击", CardType.Basic, "对单个方块造成 2 点伤害。", 2, 2, CardTargetType.SingleBlock, false, 0),
            new CardConfig("stabilize", "稳住", CardType.Basic, "坍塌值 -3，最低到 0；补牌后通常净降低 2。", 0, -3, CardTargetType.None, false, 0),
            new CardConfig("inspect_crack", "观察裂缝", CardType.Basic, "下一张破坏牌伤害 +1。", 0, 0, CardTargetType.None, false, 0),
            new CardConfig("cut_support", "支点切除", CardType.Function, "造成 2 点伤害；若目标是承重块，立即检查支撑。", 2, 3, CardTargetType.SingleBlock, false, 1),
            new CardConfig("chain_shock", "连锁震动", CardType.Function, "目标和同层左右相邻方块各受 1 点伤害。", 1, 3, CardTargetType.SameRowNeighbors, false, 2),
            new CardConfig("sealed_echo", "尘封回声", CardType.Function, "下一次记忆或核心坍塌额外 +1 碎片。", 0, 1, CardTargetType.None, false, 2),
            new CardConfig("core_fracture", "核心裂解", CardType.OneShot, "最终关一次性牌，对核心块造成 4 点伤害。", 4, 2, CardTargetType.CoreBlock, true, 4)
        };

        public static readonly List<LevelConfig> Levels = new List<LevelConfig>
        {
            new LevelConfig(
                "tutorial",
                "教学关：危楼入口",
                2,
                3,
                8,
                5,
                1,
                new[] { "tap", "tap", "tap", "strike", "strike", "stabilize", "tap", "strike" },
                new[] { "cut_support" },
                false),
            new LevelConfig(
                "level_01",
                "普通关 1：基础拆解",
                3,
                4,
                10,
                6,
                2,
                new[] { "tap", "tap", "tap", "strike", "strike", "strike", "stabilize", "cut_support", "tap", "strike" },
                new[] { "chain_shock" },
                false),
            new LevelConfig(
                "level_02",
                "普通关 2：坍塌管理",
                4,
                4,
                12,
                8,
                3,
                new[] { "tap", "tap", "strike", "strike", "stabilize", "cut_support", "chain_shock", "inspect_crack", "tap", "strike", "sealed_echo" },
                new[] { "sealed_echo" },
                false),
            new LevelConfig(
                "level_03",
                "普通关 3：结构规划",
                5,
                5,
                14,
                9,
                4,
                new[] { "tap", "tap", "strike", "strike", "stabilize", "cut_support", "chain_shock", "inspect_crack", "sealed_echo", "tap", "strike", "cut_support" },
                new string[0],
                false),
            new LevelConfig(
                "final",
                "最终关：记忆核心",
                5,
                5,
                18,
                8,
                1,
                new[] { "tap", "tap", "strike", "strike", "stabilize", "cut_support", "chain_shock", "inspect_crack", "sealed_echo", "tap", "strike" },
                new string[0],
                true)
        };

        public static Dictionary<string, CardConfig> CreateCardLookup()
        {
            Dictionary<string, CardConfig> lookup = new Dictionary<string, CardConfig>();
            foreach (CardConfig card in Cards)
            {
                lookup[card.id] = card;
            }

            return lookup;
        }
    }
}
