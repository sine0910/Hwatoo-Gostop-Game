using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCardManager
{
    // 처음 바닥에 놓을 카드를 보관할 컨테이너.
    public List<Card> begin_cards;

    // 같은 번호의 카드를 하나로 묶어서 보관하는 컨테이너. 바닥 카드 정렬 이후에는 이 컨테이너를 사용한다.
    List<FloorSlot> slots;

    public FloorCardManager()
    {
        // 바닥 초기화.
        this.slots = new List<FloorSlot>();
        for (byte position = 0; position < 12; ++position)
        {
            this.slots.Add(new FloorSlot(position, 3));
        }

        this.begin_cards = new List<Card>();
    }

    public void reset()
    {
        this.begin_cards.Clear();
        for (byte position = 0; position < 12; ++position)
        {
            this.slots[position].reset();
        }
    }

    public void put_to_begin_card(Card card)
    {
        this.begin_cards.Add(card);
    }

    public void pop_to_begin_card(Card card)
    {
        this.begin_cards.Remove(card);
    }

    FloorSlot find_empty_slot()
    {
        FloorSlot slot = this.slots.Find(obj => obj.is_empty());
        return slot;
    }

    public FloorSlot find_slot(byte card_number)
    {
        FloorSlot slot = this.slots.Find(obj => obj.is_same(card_number));
        return slot;
    }

    public void puton_card(Card card, byte player)
    {
        FloorSlot slot = find_slot(card.number);
        if (slot == null)
        {
            slot = find_empty_slot();
            slot.add_card(card, player);
            return;
        }
        this.slots[slot.slot_position].add_card(card, player);
    }

    public void puton_bonus_card(Card card, byte number, byte player)
    {
        FloorSlot slot = find_slot(number);
        if (slot == null)
        {
            slot = find_empty_slot();
            slot.add_card(card, player);
            return;
        }
        this.slots[slot.slot_position].add_bonus_card(card, player);
    }

    public bool check_same_card()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (this.slots[i].cards.Count == 4)
            {
                return true;
            }
        }
        return false;
    }

    public List<Card> get_cards(byte number)
    {
        FloorSlot slot = find_slot(number);
        if (slot == null)
        {
            return null;
        }
        return slot.cards;
    }

    public void remove_card(Card card)
    {
        FloorSlot slot = find_slot(card.number);
        if (slot != null)
        {
            slot.remove_card(card);
        }
    }

    public byte get_player_Index(byte number)
    {
        FloorSlot slot = find_slot(number);
        if (slot == null)
        {
            return byte.MaxValue;
        }
        return slot.player_Index;
    }

    public List<Card> get_bonus_cards()
    {
        List<Card> bonus_cards = new List<Card>();
        for (int i = 0; i < begin_cards.Count; ++i)
        {
            if (begin_cards[i].number == 12)
            {
                bonus_cards.Add(begin_cards[i]);
            }
        }
        return bonus_cards;
    }

    public void refresh_floor_cards()
    {
        for (int i = 0; i < this.begin_cards.Count; ++i)
        {
            puton_card(this.begin_cards[i], 3);
        }
        this.begin_cards.Clear();
    }

    public bool is_empty()
    {
        for (int i = 0; i < this.slots.Count; ++i)
        {
            if (!this.slots[i].is_empty())
            {
                return false;
            }
        }
        return true;
    }
}

public class FloorSlot
{
    public byte slot_position { get; private set; }
    public List<Card> cards { get; private set; }
    public List<Card> bonus_cards { get; private set; }
    public byte player_Index { get; private set; }

    public FloorSlot(byte position, byte player)
    {
        this.cards = new List<Card>();
        this.bonus_cards = new List<Card>();
        this.slot_position = position;
        this.player_Index = player;

        reset();
    }

    public void reset()
    {
        this.cards.Clear();
    }

    public bool is_same(byte number)
    {
        if (this.cards.Count <= 0)
        {
            return false;
        }

        return this.cards[0].number == number;
    }

    public void add_card(Card card, byte player)
    {
        this.cards.Add(card);
        this.player_Index = player;
    }

    public void add_bonus_card(Card card, byte player)
    {
        this.bonus_cards.Add(card);
        this.player_Index = player;
    }

    public void remove_card(Card card)
    {
        this.cards.Remove(card);
    }

    public void remove_bonus_card(Card card)
    {
        this.bonus_cards.Remove(card);
    }

    public List<Card> get_bonus_card()
    {
        return this.bonus_cards;
    }

    public bool is_empty()
    {
        return this.cards.Count + this.bonus_cards.Count <= 0;
    }
}
