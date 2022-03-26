using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualFloorSlot
{
    public byte ui_slot_position { get; private set; }
    byte card_number;

    List<CardPicture> card_pictures;
    List<CardPicture> bonus_card_pictures;

    public VisualFloorSlot(byte ui_slot_position, byte card_number)
    {
        this.ui_slot_position = ui_slot_position;
        this.card_number = card_number;

        this.card_pictures = new List<CardPicture>();
        this.bonus_card_pictures = new List<CardPicture>();
    }

    public void reset()
    {
        this.card_number = byte.MaxValue;
        this.card_pictures.Clear();
        this.bonus_card_pictures.Clear();
    }

    public void add_card(CardPicture card_pic)
    {
        if (card_pic.card.number != 12)
        {
            this.card_number = card_pic.card.number;
            this.card_pictures.Add(card_pic);
        }
        else
        {
            this.bonus_card_pictures.Add(card_pic);
        }
    }

    public void remove_card(CardPicture card_pic)
    {
        if (card_pic.card.number == 12)
        {
            this.bonus_card_pictures.Remove(card_pic);
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
        return this.card_pictures.Count;
    }

    public bool is_same_card(byte number)
    {
        return this.card_number == number;
    }

    public bool is_same_bonus_card(byte number, PAE_TYPE type, byte pos)
    {
        return this.bonus_card_pictures.Find(x => x.card.is_same_card(number, type, pos));
    }

    public bool is_has_bonus_card()
    {
        if (this.bonus_card_pictures.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public CardPicture find_card(Card card)
    {
        return this.card_pictures.Find(obj => obj.card.is_same_card(card.number, card.pae_type, card.position));
    }

    public CardPicture find_bonus_card(Card card)
    {
        return this.bonus_card_pictures.Find(obj => obj.card.is_same_card(card.number, card.pae_type, card.position));
    }

    public List<CardPicture> get_cards()
    {
        return this.card_pictures;
    }

    public List<CardPicture> get_bonus_cards()
    {
        return this.bonus_card_pictures;
    }

    public CardPicture get_first_card()
    {
        if (this.get_card_count() == 0)
        {
            return null;
        }
        return this.card_pictures[0];
    }
}
