using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    class HandCardManager
    {
        List<Card> cards;

        public HandCardManager()
        {
            this.cards = new List<Card>();
        }

        public void reset()
        {
            this.cards.Clear();
        }

        public void add(Card card_picture)
        {
            this.cards.Add(card_picture);
        }

        public void remove(Card card_picture)
        {
            try
            {
                this.cards.Remove(card_picture);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot remove the hand card! " + e + "\n" + card_picture.number + " " + card_picture.pae_type + " " + card_picture.position);
            }
        }

        public int get_card_count()
        {
            return this.cards.Count;
        }

        public Card get_card(int index)
        {
            return this.cards[index];
        }

        public int get_index(Card card)
        {
            return this.cards.FindIndex(x => x.is_same_card(card.number, card.pae_type, card.position));
        }

        public Card find_card(byte number, PAE_TYPE pae_type, byte position)
        {
            return this.cards.Find(obj => obj.is_same_card(number, pae_type, position));
        }

        public List<Card> get_same_number_count(byte number)
        {
            List<Card> same_cards = this.cards.FindAll(obj => obj.is_same_number(number));
            return same_cards;
        }

        public List<Card> get_hand_card()
        {
            List<Card> _cards = this.cards.ToList();
            return _cards;
        }

        public void sort_by_number()
        {
            if (this.cards != null)
            {
                this.cards.Sort((Card lhs, Card rhs) =>
                {
                    if (lhs.number < rhs.number)
                    {
                        return -1;
                    }
                    else if (lhs.number > rhs.number)
                    {
                        return 1;
                    }

                    return 0;
                });
            }
        }
    }
}
