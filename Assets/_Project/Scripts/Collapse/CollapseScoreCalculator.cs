using System;
using System.Collections.Generic;

namespace MemoryTower
{
    public enum FunctionCardRank
    {
        Rank1 = 1,
        Rank2 = 2,
        Rank3 = 3
    }

    [Serializable]
    public sealed class CollapseFormulaModifiers
    {
        public int fChips;
        public int additiveMultiplier;
        public int fixedScore;
        public readonly List<int> independentMultipliers = new List<int>();

        public static CollapseFormulaModifiers Empty()
        {
            return new CollapseFormulaModifiers();
        }
    }

    public sealed class CollapseScoreBreakdown
    {
        public readonly PokerHandType handType;
        public readonly int baseChips;
        public readonly int cardChips;
        public readonly int fChips;
        public readonly int baseMultiplier;
        public readonly int additiveMultiplier;
        public readonly int independentMultiplierProduct;
        public readonly int fixedScore;
        public readonly int totalScore;

        public int ChipSubtotal
        {
            get { return baseChips + cardChips + fChips; }
        }

        public int MultiplierSubtotal
        {
            get { return baseMultiplier + additiveMultiplier; }
        }

        public CollapseScoreBreakdown(
            PokerHandType handType,
            int baseChips,
            int cardChips,
            int fChips,
            int baseMultiplier,
            int additiveMultiplier,
            int independentMultiplierProduct,
            int fixedScore,
            int totalScore)
        {
            this.handType = handType;
            this.baseChips = baseChips;
            this.cardChips = cardChips;
            this.fChips = fChips;
            this.baseMultiplier = baseMultiplier;
            this.additiveMultiplier = additiveMultiplier;
            this.independentMultiplierProduct = independentMultiplierProduct;
            this.fixedScore = fixedScore;
            this.totalScore = totalScore;
        }
    }

    public sealed class CollapseScoreAccumulator
    {
        public int Target { get; private set; }
        public int STotal { get; private set; }
        public int ExtractTotal { get; private set; }

        public int CollapseTotal
        {
            get { return STotal + ExtractTotal; }
        }

        public bool IsComplete
        {
            get { return CollapseTotal >= Target; }
        }

        public CollapseScoreAccumulator(int target)
        {
            if (target <= 0)
            {
                throw new ArgumentOutOfRangeException("target", "Target must be greater than zero.");
            }

            Target = target;
        }

        public int AddBasePlay(CollapseScoreBreakdown breakdown)
        {
            if (breakdown == null)
            {
                throw new ArgumentNullException("breakdown");
            }

            return AddBasePlayScore(breakdown.totalScore);
        }

        public int AddBasePlayScore(int score)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException("score", "Base play score cannot be negative.");
            }

            STotal += score;
            return STotal;
        }

        public int AddExtract(FunctionCardRank rank)
        {
            int score = CollapseScoreCalculator.CalculateExtractScore(Target, rank);
            ExtractTotal += score;
            return ExtractTotal;
        }
    }

    public static class CollapseScoreCalculator
    {
        public static CollapseScoreBreakdown CalculateBasePlay(IList<PlayingCard> cards)
        {
            return CalculateBasePlay(PokerHandEvaluator.Evaluate(cards), CollapseFormulaModifiers.Empty());
        }

        public static CollapseScoreBreakdown CalculateBasePlay(IList<PlayingCard> cards, CollapseFormulaModifiers modifiers)
        {
            return CalculateBasePlay(PokerHandEvaluator.Evaluate(cards), modifiers);
        }

        public static CollapseScoreBreakdown CalculateBasePlay(PokerHandEvaluation hand, CollapseFormulaModifiers modifiers)
        {
            if (hand == null)
            {
                throw new ArgumentNullException("hand");
            }

            if (modifiers == null)
            {
                modifiers = CollapseFormulaModifiers.Empty();
            }

            int independentMultiplierProduct = CalculateIndependentMultiplierProduct(modifiers.independentMultipliers);
            int chipSubtotal = hand.baseChips + hand.cardChips + modifiers.fChips;
            int multiplierSubtotal = hand.baseMultiplier + modifiers.additiveMultiplier;
            int totalScore = (chipSubtotal * multiplierSubtotal * independentMultiplierProduct) + modifiers.fixedScore;

            if (totalScore < 0)
            {
                throw new ArgumentOutOfRangeException("modifiers", "Collapse score cannot be negative.");
            }

            return new CollapseScoreBreakdown(
                hand.handType,
                hand.baseChips,
                hand.cardChips,
                modifiers.fChips,
                hand.baseMultiplier,
                modifiers.additiveMultiplier,
                independentMultiplierProduct,
                modifiers.fixedScore,
                totalScore);
        }

        public static int CalculateExtractScore(int target, FunctionCardRank rank)
        {
            if (target <= 0)
            {
                throw new ArgumentOutOfRangeException("target", "Target must be greater than zero.");
            }

            switch (rank)
            {
                case FunctionCardRank.Rank1:
                    return target;
                case FunctionCardRank.Rank2:
                    return CeilPercent(target, 10);
                case FunctionCardRank.Rank3:
                    return CeilPercent(target, 5);
                default:
                    throw new ArgumentOutOfRangeException("rank", "Unsupported function card rank.");
            }
        }

        private static int CalculateIndependentMultiplierProduct(IList<int> independentMultipliers)
        {
            if (independentMultipliers == null || independentMultipliers.Count == 0)
            {
                return 1;
            }

            int product = 1;
            for (int i = 0; i < independentMultipliers.Count; i++)
            {
                int multiplier = independentMultipliers[i];
                if (multiplier <= 0)
                {
                    throw new ArgumentOutOfRangeException("independentMultipliers", "Independent multipliers must be greater than zero.");
                }

                product *= multiplier;
            }

            return product;
        }

        private static int CeilPercent(int target, int percent)
        {
            long raw = (long)target * percent;
            return (int)((raw + 99L) / 100L);
        }
    }
}
