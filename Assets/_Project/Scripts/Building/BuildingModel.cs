using System.Collections.Generic;
using UnityEngine;

namespace MemoryTower
{
    public sealed class BuildingModel
    {
        private readonly List<BlockModel> blocks = new List<BlockModel>();
        private BlockModel[,] grid;

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int TotalBlocks { get; private set; }

        public IReadOnlyList<BlockModel> Blocks
        {
            get { return blocks; }
        }

        public void Generate(LevelConfig level)
        {
            Rows = level.rows;
            Columns = level.columns;
            TotalBlocks = Rows * Columns;
            blocks.Clear();
            grid = new BlockModel[Rows, Columns];

            HashSet<Vector2Int> memoryPositions = CreateMemoryPositions(level);
            Vector2Int corePosition = new Vector2Int(Rows - 1, Columns / 2);

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    bool bottom = row == 0;
                    bool support = bottom && (column == Columns / 2 || column == (Columns - 1) / 2);
                    bool core = level.hasFinalCore && row == corePosition.x && column == corePosition.y;
                    bool memory = core || memoryPositions.Contains(new Vector2Int(row, column));
                    bool key = core || memory;
                    int hp = core ? 5 : support ? 3 : memory ? 2 : 2;

                    BlockModel block = new BlockModel("block_" + row + "_" + column, row, column, hp, support, key, memory, core);
                    blocks.Add(block);
                    grid[row, column] = block;
                }
            }
        }

        public BlockModel GetBlock(int row, int column)
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
            {
                return null;
            }

            return grid[row, column];
        }

        public List<BlockModel> GetSameRowNeighbors(BlockModel center)
        {
            List<BlockModel> result = new List<BlockModel>();
            if (center == null)
            {
                return result;
            }

            AddIfActive(result, GetBlock(center.row, center.column - 1));
            AddIfActive(result, center);
            AddIfActive(result, GetBlock(center.row, center.column + 1));
            return result;
        }

        public DamageResult ApplyDamage(BlockModel block, int damage)
        {
            DamageResult result = new DamageResult();
            if (block == null || block.IsCollapsed || damage <= 0)
            {
                return result;
            }

            int previousHp = block.hp;
            block.hp = Mathf.Max(0, block.hp - damage);
            result.hpChanged = previousHp != block.hp;

            if (block.hp <= 0)
            {
                CollapseBlock(block, result.newlyCollapsed);
            }
            else if (block.hp < block.maxHp)
            {
                block.state = BlockState.Damaged;
            }

            return result;
        }

        public List<BlockModel> RunSupportCheck()
        {
            List<BlockModel> collapsed = new List<BlockModel>();

            for (int row = 1; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    BlockModel block = grid[row, column];
                    if (block == null || block.IsCollapsed)
                    {
                        continue;
                    }

                    if (HasSupport(block))
                    {
                        block.unstableTicks = 0;
                        if (block.state == BlockState.Unstable)
                        {
                            block.state = block.hp < block.maxHp ? BlockState.Damaged : BlockState.Intact;
                        }

                        continue;
                    }

                    if (block.unstableTicks > 0 || block.state == BlockState.Unstable)
                    {
                        CollapseBlock(block, collapsed);
                    }
                    else
                    {
                        block.unstableTicks = 1;
                        block.state = BlockState.Unstable;
                    }
                }
            }

            return collapsed;
        }

        public bool HasSupport(BlockModel block)
        {
            if (block == null || block.IsCollapsed)
            {
                return false;
            }

            if (block.row == 0)
            {
                return true;
            }

            return IsSupporting(block.row - 1, block.column)
                || IsSupporting(block.row - 1, block.column - 1)
                || IsSupporting(block.row - 1, block.column + 1);
        }

        public bool IsBuildingCollapsed()
        {
            int remaining = 0;
            foreach (BlockModel block in blocks)
            {
                if (!block.IsCollapsed)
                {
                    remaining++;
                }
            }

            float remainingRatio = TotalBlocks == 0 ? 0f : (float)remaining / TotalBlocks;
            return remainingRatio <= 0.35f || AllKeyBlocksCollapsed();
        }

        public bool AllKeyBlocksCollapsed()
        {
            bool hasKey = false;
            foreach (BlockModel block in blocks)
            {
                if (!block.isKeyBlock)
                {
                    continue;
                }

                hasKey = true;
                if (!block.IsCollapsed)
                {
                    return false;
                }
            }

            return hasKey;
        }

        public bool IsCoreCollapsed()
        {
            foreach (BlockModel block in blocks)
            {
                if (block.isCoreBlock)
                {
                    return block.IsCollapsed;
                }
            }

            return false;
        }

        private HashSet<Vector2Int> CreateMemoryPositions(LevelConfig level)
        {
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            int needed = Mathf.Max(1, level.requiredFragments);
            int topRow = level.rows - 1;
            int center = level.columns / 2;

            Vector2Int[] candidates =
            {
                new Vector2Int(topRow, center),
                new Vector2Int(Mathf.Max(1, topRow - 1), Mathf.Max(0, center - 1)),
                new Vector2Int(Mathf.Max(1, topRow - 1), Mathf.Min(level.columns - 1, center + 1)),
                new Vector2Int(topRow, 0),
                new Vector2Int(topRow, level.columns - 1)
            };

            for (int i = 0; i < candidates.Length && positions.Count < needed; i++)
            {
                positions.Add(candidates[i]);
            }

            return positions;
        }

        private bool IsSupporting(int row, int column)
        {
            BlockModel block = GetBlock(row, column);
            return block != null && !block.IsCollapsed;
        }

        private void AddIfActive(List<BlockModel> target, BlockModel block)
        {
            if (block != null && !block.IsCollapsed)
            {
                target.Add(block);
            }
        }

        private void CollapseBlock(BlockModel block, List<BlockModel> newlyCollapsed)
        {
            if (block == null || block.IsCollapsed)
            {
                return;
            }

            block.hp = 0;
            block.state = BlockState.Collapsed;
            block.unstableTicks = 0;
            newlyCollapsed.Add(block);
        }
    }
}
