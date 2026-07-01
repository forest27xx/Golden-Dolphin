using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public static class PokerHandEvaluatorSelfTest
    {
        [MenuItem("Memory Tower/Self-Test Poker Hand Evaluator")]
        public static void Run()
        {
            AssertStandardDeck();

            AssertHand(
                "single high card",
                PokerHandType.HighCard,
                5,
                1,
                11,
                Card(CardSuit.Spades, CardRank.Ace));

            AssertHand(
                "pair",
                PokerHandType.Pair,
                10,
                2,
                4,
                Card(CardSuit.Clubs, CardRank.Two),
                Card(CardSuit.Spades, CardRank.Two));

            AssertHand(
                "two pair",
                PokerHandType.TwoPair,
                20,
                2,
                17,
                Card(CardSuit.Clubs, CardRank.Two),
                Card(CardSuit.Spades, CardRank.Two),
                Card(CardSuit.Diamonds, CardRank.Three),
                Card(CardSuit.Hearts, CardRank.Three),
                Card(CardSuit.Clubs, CardRank.Seven));

            AssertHand(
                "three of a kind",
                PokerHandType.ThreeOfAKind,
                30,
                3,
                34,
                Card(CardSuit.Clubs, CardRank.Nine),
                Card(CardSuit.Diamonds, CardRank.Nine),
                Card(CardSuit.Spades, CardRank.Nine),
                Card(CardSuit.Hearts, CardRank.Two),
                Card(CardSuit.Clubs, CardRank.Five));

            AssertHand(
                "straight",
                PokerHandType.Straight,
                30,
                4,
                51,
                Card(CardSuit.Clubs, CardRank.Ten),
                Card(CardSuit.Diamonds, CardRank.Jack),
                Card(CardSuit.Hearts, CardRank.Queen),
                Card(CardSuit.Spades, CardRank.King),
                Card(CardSuit.Clubs, CardRank.Ace));

            AssertHand(
                "wheel straight",
                PokerHandType.Straight,
                30,
                4,
                25,
                Card(CardSuit.Clubs, CardRank.Ace),
                Card(CardSuit.Diamonds, CardRank.Two),
                Card(CardSuit.Hearts, CardRank.Three),
                Card(CardSuit.Spades, CardRank.Four),
                Card(CardSuit.Clubs, CardRank.Five));

            AssertHand(
                "flush",
                PokerHandType.Flush,
                35,
                4,
                31,
                Card(CardSuit.Hearts, CardRank.Two),
                Card(CardSuit.Hearts, CardRank.Four),
                Card(CardSuit.Hearts, CardRank.Six),
                Card(CardSuit.Hearts, CardRank.Nine),
                Card(CardSuit.Hearts, CardRank.Jack));

            AssertHand(
                "full house",
                PokerHandType.FullHouse,
                40,
                4,
                31,
                Card(CardSuit.Clubs, CardRank.Seven),
                Card(CardSuit.Diamonds, CardRank.Seven),
                Card(CardSuit.Spades, CardRank.Seven),
                Card(CardSuit.Hearts, CardRank.Five),
                Card(CardSuit.Clubs, CardRank.Five));

            AssertHand(
                "four of a kind",
                PokerHandType.FourOfAKind,
                60,
                7,
                42,
                Card(CardSuit.Clubs, CardRank.Eight),
                Card(CardSuit.Diamonds, CardRank.Eight),
                Card(CardSuit.Hearts, CardRank.Eight),
                Card(CardSuit.Spades, CardRank.Eight),
                Card(CardSuit.Clubs, CardRank.Ten));

            AssertHand(
                "straight flush",
                PokerHandType.StraightFlush,
                100,
                8,
                25,
                Card(CardSuit.Spades, CardRank.Three),
                Card(CardSuit.Spades, CardRank.Four),
                Card(CardSuit.Spades, CardRank.Five),
                Card(CardSuit.Spades, CardRank.Six),
                Card(CardSuit.Spades, CardRank.Seven));

            AssertHand(
                "four card straight is not straight",
                PokerHandType.HighCard,
                5,
                1,
                14,
                Card(CardSuit.Clubs, CardRank.Two),
                Card(CardSuit.Diamonds, CardRank.Three),
                Card(CardSuit.Hearts, CardRank.Four),
                Card(CardSuit.Spades, CardRank.Five));

            AssertHand(
                "four card flush is not flush",
                PokerHandType.HighCard,
                5,
                1,
                22,
                Card(CardSuit.Hearts, CardRank.Two),
                Card(CardSuit.Hearts, CardRank.Four),
                Card(CardSuit.Hearts, CardRank.Six),
                Card(CardSuit.Hearts, CardRank.Ten));

            AssertThrowsDuplicate();

            Debug.Log("Poker hand evaluator self-test passed.");
        }

        private static PlayingCard Card(CardSuit suit, CardRank rank)
        {
            return new PlayingCard(suit, rank);
        }

        private static void AssertHand(string label, PokerHandType expectedType, int expectedBaseChips, int expectedBaseMultiplier, int expectedCardChips, params PlayingCard[] cards)
        {
            PokerHandEvaluation evaluation = PokerHandEvaluator.Evaluate(cards);
            if (evaluation.handType != expectedType
                || evaluation.baseChips != expectedBaseChips
                || evaluation.baseMultiplier != expectedBaseMultiplier
                || evaluation.cardChips != expectedCardChips
                || evaluation.cardCount != cards.Length)
            {
                throw new Exception(
                    "Poker hand test failed: " + label
                    + " expected " + expectedType + " " + expectedBaseChips + "/" + expectedBaseMultiplier + " chips=" + expectedCardChips
                    + " but got " + evaluation.handType + " " + evaluation.baseChips + "/" + evaluation.baseMultiplier + " chips=" + evaluation.cardChips);
            }
        }

        private static void AssertStandardDeck()
        {
            List<PlayingCard> deck = PokerHandEvaluator.CreateStandardDeck();
            if (deck.Count != 52)
            {
                throw new Exception("Poker hand test failed: standard deck should contain 52 cards.");
            }

            HashSet<int> seenCards = new HashSet<int>();
            for (int i = 0; i < deck.Count; i++)
            {
                int key = ((int)deck[i].suit * 16) + (int)deck[i].rank;
                if (!seenCards.Add(key))
                {
                    throw new Exception("Poker hand test failed: standard deck contains duplicate cards.");
                }
            }
        }

        private static void AssertThrowsDuplicate()
        {
            bool threw = false;
            try
            {
                PokerHandEvaluator.Evaluate(new[]
                {
                    Card(CardSuit.Clubs, CardRank.Ace),
                    Card(CardSuit.Clubs, CardRank.Ace)
                });
            }
            catch (ArgumentException)
            {
                threw = true;
            }

            if (!threw)
            {
                throw new Exception("Poker hand test failed: duplicate cards should be rejected.");
            }
        }
    }
}
