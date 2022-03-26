using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCardManager
{
    List<CardPicture> cards;

    public HandCardManager()
    {
        this.cards = new List<CardPicture>();
    }

    public void reset()
    {
        this.cards.Clear();
    }

    public void add(CardPicture card_picture)
    {
        this.cards.Add(card_picture);
    }

    public void remove(CardPicture card_picture)
    {
        bool result = this.cards.Remove(card_picture);
        if (!result)
        {
            Debug.LogError("Cannot remove the hand card!");
        }
    }

    public int get_card_count()
    {
        return this.cards.Count;
    }

    public CardPicture get_card(int index)
    {
        //Debug.Log("hand_card_manager get_card index: " + index + " /slot count: " + this.cards.Count);
        return this.cards[index];
    }

    public int get_index(Card card)
    {
        return this.cards.FindIndex(x => x.is_same(card.number, card.pae_type, card.position));
    }

    public CardPicture find_card(byte number, PAE_TYPE pae_type, byte position)
    {
        return this.cards.Find(obj => obj.card.is_same_card(number, pae_type, position));
    }

    public List<CardPicture> get_same_number_count(byte number)
    {
        List<CardPicture> same_cards = this.cards.FindAll(obj => obj.card.is_same_number(number));
        return same_cards;
    }

    public void sort_by_number()
    {
        if (this.cards != null)
        {
            this.cards.Sort((CardPicture lhs, CardPicture rhs) =>
            {
                if (lhs.card.number < rhs.card.number)
                {
                    return -1;
                }
                else if (lhs.card.number > rhs.card.number)
                {
                    return 1;
                }

                return 0;
            });
        }
    }


    public void enable_all_colliders(bool flag)
    {
        for (int i = 0; i < this.cards.Count; ++i)
        {
            this.cards[i].enable_collider(flag);
        }
    }
}
