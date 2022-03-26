using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public class PlayerCardManager
    {
        Dictionary<PAE_TYPE, List<Card>> floor_slots;

        public PlayerCardManager()
        {
            this.floor_slots = new Dictionary<PAE_TYPE, List<Card>>();
            this.floor_slots.Add(PAE_TYPE.KWANG, new List<Card>());
            this.floor_slots.Add(PAE_TYPE.TEE, new List<Card>());
            this.floor_slots.Add(PAE_TYPE.YEOL, new List<Card>());
            this.floor_slots.Add(PAE_TYPE.PEE, new List<Card>());
        }

        public void reset()
        {
            foreach (KeyValuePair<PAE_TYPE, List<Card>> kvp in this.floor_slots)
            {
                kvp.Value.Clear();
            }
        }

        public void add(Card card)
        {
            try
            {
                PAE_TYPE pae_type = card.pae_type;
                this.floor_slots[pae_type].Add(card);
            }
            catch (Exception e)
            {
                Console.WriteLine("PlayerCardManager add card error: " + e + "\n" + e.StackTrace);
            }
        }

        public void remove(Card card)
        {
            try
            {
                PAE_TYPE pae_type = card.pae_type;
                this.floor_slots[pae_type].Remove(card);
            }
            catch (Exception e)
            {
                Console.WriteLine("PlayerCardManager remove card error: " + e + "\n" + e.StackTrace);
            }
        }

        public int get_card_count(PAE_TYPE pae_type)
        {
            return this.floor_slots[pae_type].Count;
        }

        public Card get_card(byte number, PAE_TYPE pae_type, byte position)
        {
            Card card_pic = this.floor_slots[pae_type].Find(obj => obj.is_same_card(number, pae_type, position));

            return card_pic;
        }

        public Card get_card_at(PAE_TYPE pae_type, int index)
        {
            return this.floor_slots[pae_type][index];
        }

        public List<Card> get_eat_cards()
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < this.floor_slots[PAE_TYPE.KWANG].Count; i++)
            {
                cards.Add(this.floor_slots[PAE_TYPE.KWANG][i]);
            }
            for (int i = 0; i < this.floor_slots[PAE_TYPE.YEOL].Count; i++)
            {
                cards.Add(this.floor_slots[PAE_TYPE.YEOL][i]);
            }
            for (int i = 0; i < this.floor_slots[PAE_TYPE.TEE].Count; i++)
            {
                cards.Add(this.floor_slots[PAE_TYPE.TEE][i]);
            }
            for (int i = 0; i < this.floor_slots[PAE_TYPE.PEE].Count; i++)
            {
                cards.Add(this.floor_slots[PAE_TYPE.PEE][i]);
            }
            return cards;
        }
    }
}
