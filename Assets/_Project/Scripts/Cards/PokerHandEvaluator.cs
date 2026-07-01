using System;
using System.Collections.Generic;

namespace MemoryTower
{
    public enum CardSuit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum CardRank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum PokerHandType
    {
        HighCard,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }

    [Serializable]
    public struct PlayingCard
    {
        public CardSuit suit;
        public CardRank rank;

        public PlayingCard(CardSuit suit, CardRank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }
    }

    public sealed class PokerHandEvaluation
    {
        public PokerHandType handType;
        public int baseChips;
        public int baseMultiplier;
        public int cardChips;
        public int cardCount;

        public int BaseScoreInput
        {
            get { return baseChips + cardChips; }
        }

        public PokerHandEvaluation(PokerHandType handType, int baseChips, int baseMultiplier, int cardChips, int cardCount)
        {
            this.handType = handType;
            this.baseChips = baseChips;
            this.baseMultiplier = baseMultiplier;
            this.cardChips = cardChips;
            this.cardCount = cardCount;
        }
    }

    public static class PokerHandEvaluator
    {
        private const int MinPlayedCards = 1;
        private const int MaxPlayedCards = 5;

        public static PokerHandEvaluation Evaluate(IList<PlayingCard> cards)
        {
            ValidateCards(cards);

            Dictionary<CardRank, int> rankCounts = CountRanks(cards);
            bool requiresFiveCards = cards.Count == MaxPlayedCards;
            bool isFlush = requiresFiveCards && IsFlush(cards);
            bool isStraight = requiresFiveCards && IsStraight(rankCounts);

            PokerHandType handType = ResolveHandType(rankCounts, isFlush, isStraight);
            int baseChips;
            int baseMultiplier;
            GetBaseValues(handType, out baseChips, out baseMultiplier);

            return new PokerHandEvaluation(
                handType,
                baseChips,
                baseMultiplier,
                SumCardChips(cards),
                cards.Count);
        }

        public static int GetRankChips(CardRank rank)
        {
            if (rank == CardRank.Ace)
            {
                return 11;
            }

            if (rank >= CardRank.Jack)
            {
                return 10;
            }

            return (int)rank;
        }

        public static List<PlayingCard> CreateStandardDeck()
        {
            List<PlayingCard> deck = new List<PlayingCard>(52);
            CardSuit[] suits = (CardSuit[])Enum.GetValues(typeof(CardSuit));
            CardRank[] ranks = (CardRank[])Enum.GetValues(typeof(CardRank));

            foreach (CardSuit suit in suits)
            {
                foreach (CardRank rank in ranks)
                {
                    deck.Add(new PlayingCard(suit, rank));
                }
            }

            return deck;
        }

        private static void ValidateCards(IList<PlayingCard> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException("cards");
            }

            if (cards.Count < MinPlayedCards || cards.Count > MaxPlayedCards)
            {
                throw new ArgumentException("Poker hands must contain 1 to 5 cards.", "cards");
            }

            HashSet<int> seenCards = new HashSet<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                PlayingCard card = cards[i];
                int key = ((int)card.suit * 16) + (int)card.rank;
                if (!seenCards.Add(key))
                {
                    throw new ArgumentException("Poker hands cannot contain duplicate cards.", "cards");
                }
            }
        }

        private static Dictionary<CardRank, int> CountRanks(IList<PlayingCard> cards)
        {
            Dictionary<CardRank, int> counts = new Dictionary<CardRank, int>();
            for (int i = 0; i < cards.Count; i++)
            {
                CardRank rank = cards[i].rank;
                int count;
                counts.TryGetValue(rank, out count);
                counts[rank] = count + 1;
            }

            return counts;
        }

        private static PokerHandType ResolveHandType(Dictionary<CardRank, int> rankCounts, bool isFlush, bool isStraight)
        {
            if (isFlush && isStraight)
            {
                return PokerHandType.StraightFlush;
            }

            int pairCount = 0;
            bool hasThree = false;
            bool hasFour = false;

            foreach (int count in rankCounts.Values)
            {
                if (count == 4)
                {
                    hasFour = true;
                }
                else if (count == 3)
                {
                    hasThree = true;
                }
                else if (count == 2)
                {
                    pairCount++;
                }
            }

            if (hasFour)
            {
                return PokerHandType.FourOfAKind;
            }

            if (hasThree && pairCount == 1)
            {
                return PokerHandType.FullHouse;
            }

            if (isFlush)
            {
                return PokerHandType.Flush;
            }

            if (isStraight)
            {
                return PokerHandType.Straight;
            }

            if (hasThree)
            {
                return PokerHandType.ThreeOfAKind;
            }

            if (pairCount == 2)
            {
                return PokerHandType.TwoPair;
            }

            if (pairCount == 1)
            {
                return PokerHandType.Pair;
            }

            return PokerHandType.HighCard;
        }

        private static bool IsFlush(IList<PlayingCard> cards)
        {
            CardSuit suit = cards[0].suit;
            for (int i = 1; i < cards.Count; i++)
            {
                if (cards[i].suit != suit)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsStraight(Dictionary<CardRank, int> rankCounts)
        {
            if (rankCounts.Count != MaxPlayedCards)
            {
                return false;
            }

            List<int> ranks = new List<int>(MaxPlayedCards);
            foreach (CardRank rank in rankCounts.Keys)
            {
                ranks.Add((int)rank);
            }

            ranks.Sort();

            bool regularStraight = true;
            for (int i = 1; i < ranks.Count; i++)
            {
                if (ranks[i] != ranks[i - 1] + 1)
                {
                    regularStraight = false;
                    break;
                }
            }

            if (regularStraight)
            {
                return true;
            }

            return ranks[0] == (int)CardRank.Two
                && ranks[1] == (int)CardRank.Three
                && ranks[2] == (int)CardRank.Four
                && ranks[3] == (int)CardRank.Five
                && ranks[4] == (int)CardRank.Ace;
        }

        private static int SumCardChips(IList<PlayingCard> cards)
        {
            int total = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                total += GetRankChips(cards[i].rank);
            }

            return total;
        }

        private static void GetBaseValues(PokerHandType handType, out int baseChips, out int baseMultiplier)
        {
            switch (handType)
            {
                case PokerHandType.Pair:
                    baseChips = 10;
                    baseMultiplier = 2;
                    return;
                case PokerHandType.TwoPair:
                    baseChips = 20;
                    baseMultiplier = 2;
                    return;
                case PokerHandType.ThreeOfAKind:
                    baseChips = 30;
                    baseMultiplier = 3;
                    return;
                case PokerHandType.Straight:
                    baseChips = 30;
                    baseMultiplier = 4;
                    return;
                case PokerHandType.Flush:
                    baseChips = 35;
                    baseMultiplier = 4;
                    return;
                case PokerHandType.FullHouse:
                    baseChips = 40;
                    baseMultiplier = 4;
                    return;
                case PokerHandType.FourOfAKind:
                    baseChips = 60;
                    baseMultiplier = 7;
                    return;
                case PokerHandType.StraightFlush:
                    baseChips = 100;
                    baseMultiplier = 8;
                    return;
                default:
                    baseChips = 5;
                    baseMultiplier = 1;
                    return;
            }
        }
    }
}
