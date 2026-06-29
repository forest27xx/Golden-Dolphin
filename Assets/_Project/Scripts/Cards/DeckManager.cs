using System;
using System.Collections.Generic;

namespace MemoryTower
{
    public sealed class DeckManager
    {
        private readonly List<CardConfig> drawPile = new List<CardConfig>();
        private readonly List<CardConfig> discardPile = new List<CardConfig>();
        private readonly List<CardConfig> exhaustedPile = new List<CardConfig>();
        private readonly Random random = new Random();

        public readonly List<CardConfig> Hand = new List<CardConfig>();

        public int DrawPileCount
        {
            get { return drawPile.Count; }
        }

        public int DiscardPileCount
        {
            get { return discardPile.Count; }
        }

        public int ExhaustedPileCount
        {
            get { return exhaustedPile.Count; }
        }

        public void Initialize(IEnumerable<string> cardIds, Dictionary<string, CardConfig> cardLookup)
        {
            drawPile.Clear();
            discardPile.Clear();
            exhaustedPile.Clear();
            Hand.Clear();

            foreach (string cardId in cardIds)
            {
                CardConfig card;
                if (cardLookup.TryGetValue(cardId, out card))
                {
                    drawPile.Add(card);
                }
            }

            Shuffle(drawPile);
        }

        public int Draw(int count, int handLimit)
        {
            int drawn = 0;
            for (int i = 0; i < count; i++)
            {
                if (Hand.Count >= handLimit)
                {
                    break;
                }

                if (drawPile.Count == 0)
                {
                    RecycleDiscardIntoDrawPile();
                }

                if (drawPile.Count == 0)
                {
                    break;
                }

                CardConfig card = drawPile[0];
                drawPile.RemoveAt(0);
                Hand.Add(card);
                drawn++;
            }

            if (drawn > 0)
            {
                AudioEvents.RequestSfx(SfxType.CardDrawn);
            }

            return drawn;
        }

        public CardConfig GetHandCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= Hand.Count)
            {
                return null;
            }

            return Hand[handIndex];
        }

        public CardConfig RemoveFromHand(int handIndex, bool exhausted)
        {
            CardConfig card = GetHandCard(handIndex);
            if (card == null)
            {
                return null;
            }

            Hand.RemoveAt(handIndex);
            if (exhausted)
            {
                exhaustedPile.Add(card);
            }
            else
            {
                discardPile.Add(card);
            }

            return card;
        }

        public int RedrawHand(int handLimit)
        {
            int count = Hand.Count;
            discardPile.AddRange(Hand);
            Hand.Clear();
            return Draw(count, handLimit);
        }

        public bool AddCardToHand(CardConfig card, int handLimit)
        {
            if (card == null || Hand.Count >= handLimit)
            {
                return false;
            }

            Hand.Add(card);
            return true;
        }

        private void RecycleDiscardIntoDrawPile()
        {
            if (discardPile.Count == 0)
            {
                return;
            }

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
        }

        private void Shuffle(List<CardConfig> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                CardConfig temp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = temp;
            }
        }
    }
}
