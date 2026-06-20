using System;
using System.Collections.Generic;

namespace MemoryTower
{
    public enum CardType
    {
        Basic,
        Function,
        OneShot
    }

    public enum CardTargetType
    {
        None,
        SingleBlock,
        SameRowNeighbors,
        CoreBlock
    }

    public enum BlockState
    {
        Intact,
        Damaged,
        Unstable,
        Collapsed
    }

    public enum LevelOutcome
    {
        Playing,
        Victory,
        Defeat
    }

    [Serializable]
    public sealed class CardConfig
    {
        public string id;
        public string displayName;
        public CardType cardType;
        public string description;
        public int damage;
        public int collapseDelta;
        public CardTargetType targetType;
        public bool isOneShot;
        public int unlockLevel;

        public bool RequiresTarget
        {
            get { return targetType != CardTargetType.None; }
        }

        public CardConfig(string id, string displayName, CardType cardType, string description, int damage, int collapseDelta, CardTargetType targetType, bool isOneShot, int unlockLevel)
        {
            this.id = id;
            this.displayName = displayName;
            this.cardType = cardType;
            this.description = description;
            this.damage = damage;
            this.collapseDelta = collapseDelta;
            this.targetType = targetType;
            this.isOneShot = isOneShot;
            this.unlockLevel = unlockLevel;
        }
    }

    [Serializable]
    public sealed class LevelConfig
    {
        public string id;
        public string displayName;
        public int rows;
        public int columns;
        public int collapseThreshold;
        public int actionCount;
        public int requiredFragments;
        public List<string> initialDeckCardIds = new List<string>();
        public List<string> rewardCardIds = new List<string>();
        public bool hasFinalCore;

        public LevelConfig(string id, string displayName, int rows, int columns, int collapseThreshold, int actionCount, int requiredFragments, IEnumerable<string> initialDeckCardIds, IEnumerable<string> rewardCardIds, bool hasFinalCore)
        {
            this.id = id;
            this.displayName = displayName;
            this.rows = rows;
            this.columns = columns;
            this.collapseThreshold = collapseThreshold;
            this.actionCount = actionCount;
            this.requiredFragments = requiredFragments;
            this.initialDeckCardIds.AddRange(initialDeckCardIds);
            this.rewardCardIds.AddRange(rewardCardIds);
            this.hasFinalCore = hasFinalCore;
        }
    }

    [Serializable]
    public sealed class BlockModel
    {
        public string id;
        public int row;
        public int column;
        public int hp;
        public int maxHp;
        public bool isSupportBlock;
        public bool isKeyBlock;
        public bool isMemoryBlock;
        public bool isCoreBlock;
        public BlockState state;
        public int unstableTicks;

        public bool IsCollapsed
        {
            get { return state == BlockState.Collapsed; }
        }

        public bool IsSelectable
        {
            get { return state != BlockState.Collapsed; }
        }

        public BlockModel(string id, int row, int column, int hp, bool isSupportBlock, bool isKeyBlock, bool isMemoryBlock, bool isCoreBlock)
        {
            this.id = id;
            this.row = row;
            this.column = column;
            this.hp = hp;
            maxHp = hp;
            this.isSupportBlock = isSupportBlock;
            this.isKeyBlock = isKeyBlock;
            this.isMemoryBlock = isMemoryBlock;
            this.isCoreBlock = isCoreBlock;
            state = BlockState.Intact;
            unstableTicks = 0;
        }
    }

    public sealed class DamageResult
    {
        public bool hpChanged;
        public readonly List<BlockModel> newlyCollapsed = new List<BlockModel>();
    }

    public sealed class CardResolution
    {
        public bool hpChanged;
        public bool forceSupportCheck;
        public readonly List<BlockModel> newlyCollapsed = new List<BlockModel>();
    }
}
