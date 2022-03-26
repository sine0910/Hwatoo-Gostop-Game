using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardManager
{
    Dictionary<PAE_TYPE, List<CardPicture>> floor_slots;

    public PlayerCardManager()
    {
        this.floor_slots = new Dictionary<PAE_TYPE, List<CardPicture>>();
        this.floor_slots.Add(PAE_TYPE.KWANG, new List<CardPicture>());
        this.floor_slots.Add(PAE_TYPE.TEE, new List<CardPicture>());
        this.floor_slots.Add(PAE_TYPE.YEOL, new List<CardPicture>());
        this.floor_slots.Add(PAE_TYPE.PEE, new List<CardPicture>());
    }

    public void reset()
    {
        foreach (KeyValuePair<PAE_TYPE, List<CardPicture>> kvp in this.floor_slots)
        {
            kvp.Value.Clear();
        }
    }

    public void add(CardPicture card_pic)
    {
        //Debug.Log("PlayerCardManager add cardpic info: " + card_pic.card.number + "/" + card_pic.card.pae_type + "/" + card_pic.card.position);
        PAE_TYPE pae_type = card_pic.card.pae_type;
        this.floor_slots[pae_type].Add(card_pic);
    }

    public void remove(CardPicture card_pic)
    {
        Debug.Log("PlayerCardManager remove cardpic info: " + card_pic.card.number + "/"  + card_pic.card.pae_type + "/" + card_pic.card.position);
        PAE_TYPE pae_type = card_pic.card.pae_type;
        this.floor_slots[pae_type].Remove(card_pic);
    }

    public int get_card_count(PAE_TYPE pae_type)
    {
        return this.floor_slots[pae_type].Count;
    }

    public CardPicture get_card(byte number, PAE_TYPE pae_type, byte position)
    {
        Debug.Log("PlayerCardManager get_card number: " + number + " pae_type: " + pae_type + " position: " + position);
        CardPicture card_pic = this.floor_slots[pae_type].Find(obj => obj.is_same(number, pae_type, position));
        return card_pic;
    }

    public CardPicture get_card_at(PAE_TYPE pae_type, int index)
    {
        return this.floor_slots[pae_type][index];
    }
}
