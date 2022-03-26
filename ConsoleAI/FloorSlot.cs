using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    class FloorSlot
    {
        public byte ui_slot_position { get; private set; }
        byte card_number;
        byte bonus_card_number;

        List<byte> bonus_card_position;

        List<Card> card_pictures;
        List<Card> bonus_card_pictures;

        public FloorSlot(byte ui_slot_position, byte card_number, byte bonus_card_number)
        {
            this.ui_slot_position = ui_slot_position;
            this.card_number = card_number;
            this.bonus_card_number = bonus_card_number;
            this.bonus_card_position = new List<byte>();
            this.card_pictures = new List<Card>();
            this.bonus_card_pictures = new List<Card>();
        }

        public void reset()
        {
            this.card_number = byte.MaxValue;
            this.card_pictures.Clear();
            this.bonus_card_position.Clear();
            this.bonus_card_pictures.Clear();
        }


        public void add_card(Card card_pic)
        {
            this.card_number = card_pic.number;
            this.card_pictures.Add(card_pic);
        }

        public void add_bonus_card(Card card_pic)
        {
            this.bonus_card_number = card_pic.number;
            this.bonus_card_position.Add(card_pic.position);
            this.bonus_card_pictures.Add(card_pic);
        }

        public void start_remove_card(Card card_pic)
        {
            this.card_pictures.Remove(card_pic);

            if (this.card_pictures.Count <= 0)
            {
                this.card_number = byte.MaxValue;
            }
        }

        public void remove_card(Card card_pic)
        {
            if (card_pic.number == 12)
            {
                this.bonus_card_pictures.Remove(card_pic);
                this.bonus_card_position.Remove(card_pic.position);

                if (this.bonus_card_pictures.Count <= 0)
                {
                    this.bonus_card_number = 0;
                }
            }
            else
            {
                this.card_pictures.Remove(card_pic);

                if (this.card_pictures.Count <= 0)
                {
                    this.card_number = byte.MaxValue;
                }
            }
        }

        public int get_card_count()
        {
            return this.card_pictures.Count + this.bonus_card_pictures.Count;
        }

        public int begin_card_count()
        {
            return this.card_pictures.Count;
        }

        public bool is_same_card(byte number)
        {
            return this.card_number == number;
        }

        public bool is_start_bonus_card(byte number)
        {
            return this.card_number == 12;
        }

        public bool is_bonus_card(byte number)
        {
            return this.bonus_card_number == number;
        }

        public Card find_card(Card card)
        {
            return this.card_pictures.Find(obj =>
                obj.is_same_card(card.number, card.pae_type, card.position));
        }

        public Card find_bonus_card(Card card)
        {
            return this.bonus_card_pictures.Find(obj =>
                obj.is_same_card(card.number, card.pae_type, card.position));
        }

        public Card get_first_card()
        {
            if (get_card_count() <= 0)
            {
                return null;
            }

            return this.card_pictures[0];
        }


        public List<Card> get_cards()
        {
            return this.card_pictures;
        }
    }
}
