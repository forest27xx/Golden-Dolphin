using System.Collections.Generic;

namespace MemoryTower
{
    public sealed class CardEffectResolver
    {
        private int nextDamageBonus;

        public void Reset()
        {
            nextDamageBonus = 0;
        }

        public CardResolution Resolve(CardConfig card, BlockModel target, BuildingModel building, CollapseSystem collapse, RewardSystem rewards)
        {
            CardResolution resolution = new CardResolution();
            if (card == null)
            {
                return resolution;
            }

            collapse.Add(card.collapseDelta);

            if (card.id == "stabilize")
            {
                return resolution;
            }

            if (card.id == "inspect_crack")
            {
                nextDamageBonus = 1;
                return resolution;
            }

            if (card.id == "sealed_echo")
            {
                rewards.QueueNextCollapseBonus();
                return resolution;
            }

            if (card.targetType == CardTargetType.SameRowNeighbors)
            {
                List<BlockModel> affected = building.GetSameRowNeighbors(target);
                foreach (BlockModel block in affected)
                {
                    ApplyDamageToBlock(card, block, building, resolution);
                }

                nextDamageBonus = 0;
                return resolution;
            }

            ApplyDamageToBlock(card, target, building, resolution);
            nextDamageBonus = 0;

            if (card.id == "cut_support" && target != null && target.isSupportBlock)
            {
                resolution.forceSupportCheck = true;
            }

            return resolution;
        }

        private void ApplyDamageToBlock(CardConfig card, BlockModel block, BuildingModel building, CardResolution resolution)
        {
            if (block == null)
            {
                return;
            }

            int damage = card.damage + (card.damage > 0 ? nextDamageBonus : 0);
            DamageResult damageResult = building.ApplyDamage(block, damage);
            resolution.hpChanged = resolution.hpChanged || damageResult.hpChanged;
            resolution.newlyCollapsed.AddRange(damageResult.newlyCollapsed);

            foreach (BlockModel collapsedBlock in damageResult.newlyCollapsed)
            {
                if (collapsedBlock.isSupportBlock)
                {
                    resolution.forceSupportCheck = true;
                }
            }
        }
    }
}
