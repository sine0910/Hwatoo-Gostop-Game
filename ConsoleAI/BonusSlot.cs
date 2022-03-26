using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProject
{
    class BonusSlot
    {
        List<Card> bonus_card_pictures;

        public BonusSlot()
        {
            this.bonus_card_pictures = new List<Card>();
        }

        public void reset()
        {
            this.bonus_card_pictures.Clear();
        }

        public void add_bonus_card(Card card_pic)
        {
            this.bonus_card_pictures.Add(card_pic);
        }

        public Card find_bonus_card(Card card)
        {
            return this.bonus_card_pictures.Find(obj =>
                obj.is_same_card(card.number, card.pae_type, card.position));
        }

        public void remove_card(Card card_pic)
        {
            this.bonus_card_pictures.Remove(card_pic);
        }
    }
}
