using System;
using UnityEditor;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public static class CollapseScoreCalculatorSelfTest
    {
        [MenuItem("Memory Tower/Self-Test Collapse Score Calculator")]
        public static void Run()
        {
            AssertUnmodifiedBasePlay();
            AssertModifiedBasePlay();
            AssertExtractScores();
            AssertAccumulator();
            AssertInvalidIndependentMultiplier();

            Debug.Log("Collapse score calculator self-test passed.");
        }

        private static PlayingCard Card(CardSuit suit, CardRank rank)
        {
            return new PlayingCard(suit, rank);
        }

        private static void AssertUnmodifiedBasePlay()
        {
            CollapseScoreBreakdown pair = CollapseScoreCalculator.CalculateBasePlay(new[]
            {
                Card(CardSuit.Clubs, CardRank.Ace),
                Card(CardSuit.Spades, CardRank.Ace)
            });

            AssertEqual("pair type", PokerHandType.Pair, pair.handType);
            AssertEqual("pair score", 64, pair.totalScore);
            AssertEqual("pair chip subtotal", 32, pair.ChipSubtotal);
            AssertEqual("pair multiplier subtotal", 2, pair.MultiplierSubtotal);

            CollapseScoreBreakdown straight = CollapseScoreCalculator.CalculateBasePlay(new[]
            {
                Card(CardSuit.Clubs, CardRank.Ten),
                Card(CardSuit.Diamonds, CardRank.Jack),
                Card(CardSuit.Hearts, CardRank.Queen),
                Card(CardSuit.Spades, CardRank.King),
                Card(CardSuit.Clubs, CardRank.Ace)
            });

            AssertEqual("straight type", PokerHandType.Straight, straight.handType);
            AssertEqual("straight score", 324, straight.totalScore);
        }

        private static void AssertModifiedBasePlay()
        {
            CollapseFormulaModifiers modifiers = new CollapseFormulaModifiers();
            modifiers.fChips = 6;
            modifiers.additiveMultiplier = 3;
            modifiers.fixedScore = 70;
            modifiers.independentMultipliers.Add(2);

            CollapseScoreBreakdown score = CollapseScoreCalculator.CalculateBasePlay(new[]
            {
                Card(CardSuit.Clubs, CardRank.Ten),
                Card(CardSuit.Diamonds, CardRank.Jack),
                Card(CardSuit.Hearts, CardRank.Queen),
                Card(CardSuit.Spades, CardRank.King),
                Card(CardSuit.Clubs, CardRank.Ace)
            }, modifiers);

            AssertEqual("modified chip subtotal", 87, score.ChipSubtotal);
            AssertEqual("modified multiplier subtotal", 7, score.MultiplierSubtotal);
            AssertEqual("modified independent product", 2, score.independentMultiplierProduct);
            AssertEqual("modified fixed score", 70, score.fixedScore);
            AssertEqual("modified total", 1288, score.totalScore);
        }

        private static void AssertExtractScores()
        {
            AssertEqual("rank1 exact target", 900, CollapseScoreCalculator.CalculateExtractScore(900, FunctionCardRank.Rank1));
            AssertEqual("rank2 exact percent", 90, CollapseScoreCalculator.CalculateExtractScore(900, FunctionCardRank.Rank2));
            AssertEqual("rank3 exact percent", 45, CollapseScoreCalculator.CalculateExtractScore(900, FunctionCardRank.Rank3));

            AssertEqual("rank2 ceil percent", 103, CollapseScoreCalculator.CalculateExtractScore(1025, FunctionCardRank.Rank2));
            AssertEqual("rank3 ceil percent", 52, CollapseScoreCalculator.CalculateExtractScore(1025, FunctionCardRank.Rank3));
        }

        private static void AssertAccumulator()
        {
            CollapseScoreAccumulator accumulator = new CollapseScoreAccumulator(500);
            accumulator.AddBasePlayScore(324);
            AssertEqual("s total", 324, accumulator.STotal);
            AssertEqual("extract total initial", 0, accumulator.ExtractTotal);
            AssertEqual("collapse total initial", 324, accumulator.CollapseTotal);
            AssertEqual("not complete", false, accumulator.IsComplete);

            accumulator.AddExtract(FunctionCardRank.Rank2);
            AssertEqual("extract total rank2", 50, accumulator.ExtractTotal);
            AssertEqual("collapse total rank2", 374, accumulator.CollapseTotal);
            AssertEqual("still not complete", false, accumulator.IsComplete);

            accumulator.AddExtract(FunctionCardRank.Rank1);
            AssertEqual("extract total after rank1", 550, accumulator.ExtractTotal);
            AssertEqual("collapse total after rank1", 874, accumulator.CollapseTotal);
            AssertEqual("complete after rank1", true, accumulator.IsComplete);
        }

        private static void AssertInvalidIndependentMultiplier()
        {
            bool threw = false;
            try
            {
                CollapseFormulaModifiers modifiers = new CollapseFormulaModifiers();
                modifiers.independentMultipliers.Add(0);
                CollapseScoreCalculator.CalculateBasePlay(new[]
                {
                    Card(CardSuit.Clubs, CardRank.Two)
                }, modifiers);
            }
            catch (ArgumentOutOfRangeException)
            {
                threw = true;
            }

            AssertEqual("invalid independent multiplier throws", true, threw);
        }

        private static void AssertEqual<T>(string label, T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception("Collapse score test failed: " + label + " expected " + expected + " but got " + actual);
            }
        }
    }
}
