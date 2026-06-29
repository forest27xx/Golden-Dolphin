using System.Collections.Generic;

namespace MemoryTower
{
    public sealed class RewardSystem
    {
        public int FragmentsThisLevel { get; private set; }

        private bool nextCollapseBonus;

        public void Initialize()
        {
            FragmentsThisLevel = 0;
            nextCollapseBonus = false;
        }

        public void QueueNextCollapseBonus()
        {
            nextCollapseBonus = true;
        }

        public int CollectFragments(IEnumerable<BlockModel> collapsedBlocks)
        {
            int gained = 0;
            foreach (BlockModel block in collapsedBlocks)
            {
                if (block.isMemoryBlock || block.isCoreBlock)
                {
                    gained++;
                }
            }

            if (gained > 0 && nextCollapseBonus)
            {
                gained++;
                nextCollapseBonus = false;
            }

            FragmentsThisLevel += gained;
            if (gained > 0)
            {
                AudioEvents.RequestSfx(SfxType.FragmentGained);
            }

            return gained;
        }
    }
}
