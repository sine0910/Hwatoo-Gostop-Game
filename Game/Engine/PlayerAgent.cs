using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgent
{
    byte player_index;

    Dictionary<PAE_TYPE, List<Card>> floor_pae;
    public List<Card> hand_pae;

    public short score { get; private set; }
    public short prev_score { get; private set; }

    public byte go_count { get; private set; }
    public byte shaking_count { get; private set; }
    public byte ppuk_count { get; private set; }
    public byte start_ppuk { get; private set; }

    public byte remain_bomb_count { get; private set; }
    public bool is_used_kookjin { get; private set; }

    public byte turn { get; private set; }

    public PlayerAgent(byte player_index)
    {
        this.player_index = player_index;
        this.hand_pae = new List<Card>();
        this.floor_pae = new Dictionary<PAE_TYPE, List<Card>>();
        this.score = 0;
        this.prev_score = 0;
        this.remain_bomb_count = 0;
        this.start_ppuk = 0;

        this.turn = 0;
    }

    public void reset()
    {
        this.score = 0;
        this.prev_score = 0;
        this.go_count = 0;
        this.shaking_count = 0;
        this.ppuk_count = 0;
        this.start_ppuk = 0;
        this.hand_pae.Clear();
        this.floor_pae.Clear();
        this.remain_bomb_count = 0;
        this.is_used_kookjin = false;

        this.turn = 0;
    }

    public void add_card_to_hand(Card card)
    {
        this.hand_pae.Add(card);
    }

    public Card pop_card_from_hand(byte card_number, PAE_TYPE pae_type, byte position)
    {
        Card card = this.hand_pae.Find(obj =>
        {
            return obj.number == card_number && obj.pae_type == pae_type && obj.position == position;
        });

        if (card == null)
        {
            return null;
        }

        this.hand_pae.Remove(card);
        return card;
    }

    public List<Card> pop_all_cards_from_hand(byte card_number)
    {
        List<Card> cards = this.hand_pae.FindAll(obj => obj.is_same_number(card_number));
        List<Card> result = new List<Card>();
        for (int i = 0; i < cards.Count; ++i)
        {
            result.Add(cards[i]);
            this.hand_pae.Remove(cards[i]);
        }
        return result;
    }


    public void add_card_to_floor(Card card)
    {
        if (!this.floor_pae.ContainsKey(card.pae_type))
        {
            this.floor_pae.Add(card.pae_type, new List<Card>());
        }
        this.floor_pae[card.pae_type].Add(card);
    }


    public List<Card> pop_card_from_floor(byte pee_count_to_want)
    {
        // �ǰ� ���嵵 ���ٸ� �� �� �ִ°� ����.
        if (!this.floor_pae.ContainsKey(PAE_TYPE.PEE))
        {
            return null;
        }

        List<Card> player_pees = this.floor_pae[PAE_TYPE.PEE];
        if (player_pees.Count <= 0)
        {
            return null;
        }

        List<Card> result = new List<Card>();
        if (player_pees.Count == 1)
        {
            // ���� �ִ� �ǰ� ����ۿ� ���� ��쿡�� �װ͹ۿ� �ٰ� ����.
            result.Add(player_pees[0]);
        }
        else
        {
            if (pee_count_to_want == 1)
            {
                Card onepee_card = player_pees.Find(obj => obj.status != CARD_STATUS.TWO_PEE && obj.status != CARD_STATUS.BONUS_PEE);
                if (onepee_card != null)
                {
                    result.Add(onepee_card);
                }
                else
                {
                    result.Add(player_pees[0]);
                }
            }
            else if (pee_count_to_want == 2)
            {
                // ����¥�� ������ �ִٸ� ���Ǹ� �����ش�.
                Card twopee_card = player_pees.Find(obj => obj.status == CARD_STATUS.TWO_PEE);
                Card bonus_card = player_pees.Find(obj => obj.status == CARD_STATUS.BONUS_PEE);
                if (twopee_card != null)
                {
                    result.Add(twopee_card);
                }
                else if (bonus_card != null)
                {
                    result.Add(bonus_card);
                }
                else
                {
                    int p = 0;
                    for (int i = 0; i < pee_count_to_want + p; ++i)
                    {
                        if (player_pees[i].status == CARD_STATUS.TWO_PEE)
                        {
                            p++;
                        }
                        else if (player_pees[i].status == CARD_STATUS.BONUS_PEE)
                        {
                            p++;
                        }
                        else
                        {
                            result.Add(player_pees[i]);
                        }
                    }
                }
            }
        }
        // �÷��̾��� �ٴ��п��� ����.
        for (int i = 0; i < result.Count; ++i)
        {
            player_pees.Remove(result[i]);
        }
        if (player_pees.Count <= 0)
        {
            this.floor_pae.Remove(PAE_TYPE.PEE);
        }
        return result;
    }

    Card pop_specific_card_from_floor(PAE_TYPE pae_type, CARD_STATUS status)
    {
        if (!this.floor_pae.ContainsKey(pae_type))
        {
            return null;
        }

        Card card = this.floor_pae[pae_type].Find(obj => obj.status == status);
        this.floor_pae[pae_type].Remove(card);
        return card;
    }

    List<Card> find_cards(PAE_TYPE pae_type)
    {
        if (this.floor_pae.ContainsKey(pae_type))
        {
            return this.floor_pae[pae_type];
        }

        return null;
    }

    public byte get_card_count(PAE_TYPE pae_type, CARD_STATUS status)
    {
        if (!this.floor_pae.ContainsKey(pae_type))
        {
            return 0;
        }

        List<Card> targets = this.floor_pae[pae_type].FindAll(obj => obj.is_same_status(status));
        if (targets == null)
        {
            return 0;
        }

        return (byte)targets.Count;
    }

    public byte get_same_card_count_from_hand(byte number)
    {
        List<Card> same_cards = find_same_cards_from_hand(number);
        if (same_cards == null)
        {
            return 0;
        }

        return (byte)same_cards.Count;
    }

    public List<Card> find_same_cards_from_hand(byte number)
    {
        return this.hand_pae.FindAll(obj => obj.is_same_number(number));
    }

    public byte get_pee_count()
    {
        List<Card> cards = find_cards(PAE_TYPE.PEE);
        if (cards == null)
        {
            return 0;
        }

        byte twopee_count = get_card_count(PAE_TYPE.PEE, CARD_STATUS.TWO_PEE);
        byte bonuspee_count = get_card_count(PAE_TYPE.PEE, CARD_STATUS.BONUS_PEE);

        byte total_pee_count = (byte)(cards.Count + twopee_count + bonuspee_count);

        return total_pee_count;
    }

    public byte get_kwang_count()
    {
        List<Card> cards = find_cards(PAE_TYPE.KWANG);
        if (cards == null)
        {
            return 0;
        }

        byte total_kwang_count = (byte)(cards.Count);

        return total_kwang_count;
    }

    public byte get_yeol_count()//2021-01-22 14:33 �۹��� üũ�ϱ� ���� yeol�� ������ �޾ƿ�
    {
        List<Card> cards = find_cards(PAE_TYPE.YEOL);
        if (cards == null)
        {
            return 0;
        }

        byte total_yeol_count = (byte)(cards.Count);

        return total_yeol_count;
    }

    public byte get_tee_count()//2021-01-22 14:33 �۹��� üũ�ϱ� ���� yeol�� ������ �޾ƿ�
    {
        List<Card> cards = find_cards(PAE_TYPE.TEE);
        if (cards == null)
        {
            return 0;
        }

        byte total_yeol_count = (byte)(cards.Count);

        return total_yeol_count;
    }

    short get_score_by_type(PAE_TYPE pae_type)
    {
        short pae_score = 0;

        List<Card> cards = find_cards(pae_type);
        if (cards == null)
        {
            return 0;
        }

        switch (pae_type)
        {
            case PAE_TYPE.PEE:
                {
                    byte twopee_count = get_card_count(PAE_TYPE.PEE, CARD_STATUS.TWO_PEE);
                    byte bonuspee_count = get_card_count(PAE_TYPE.PEE, CARD_STATUS.BONUS_PEE);
                    byte total_pee_count = (byte)(cards.Count + twopee_count + bonuspee_count);

                    if (total_pee_count >= 10)
                    {
                        pae_score = (short)(total_pee_count - 9);
                    }
                }
                break;

            case PAE_TYPE.TEE:
                if (cards.Count >= 5)
                {
                    pae_score = (short)(cards.Count - 4);
                }
                break;

            case PAE_TYPE.YEOL:
                if (cards.Count >= 5)
                {
                    pae_score = (short)(cards.Count - 4);
                }
                break;

            case PAE_TYPE.KWANG:
                if (cards.Count == 5)
                {
                    pae_score = 15;
                }
                else if (cards.Count == 4)
                {
                    pae_score = 4;
                }
                else if (cards.Count == 3)
                {
                    // ���� ���ԵǾ� ������ 2��. �ƴϸ� 3��.
                    bool is_exist_beekwang = cards.Exists(obj => obj.is_same_number(11));
                    if (is_exist_beekwang)
                    {
                        pae_score = 2;
                    }
                    else
                    {
                        pae_score = 3;
                    }
                }
                break;
        }
        return pae_score;
    }

    public void calculate_score()
    {
        this.score = 0;
        this.score += get_score_by_type(PAE_TYPE.PEE);
        this.score += get_score_by_type(PAE_TYPE.TEE);
        this.score += get_score_by_type(PAE_TYPE.YEOL);
        this.score += get_score_by_type(PAE_TYPE.KWANG);

        // ����
        byte godori_count = get_card_count(PAE_TYPE.YEOL, CARD_STATUS.GODORI);
        if (godori_count == 3)
        {
            this.score += 5;
        }

        // ȫ��, �ʴ�, û��
        byte cheongdan_count = get_card_count(PAE_TYPE.TEE, CARD_STATUS.CHEONG_DAN);
        byte hongdan_count = get_card_count(PAE_TYPE.TEE, CARD_STATUS.HONG_DAN);
        byte chodan_count = get_card_count(PAE_TYPE.TEE, CARD_STATUS.CHO_DAN);
        if (cheongdan_count == 3)
        {
            this.score += 3;
        }

        if (hongdan_count == 3)
        {
            this.score += 3;
        }

        if (chodan_count == 3)
        {
            this.score += 3;
        }
    }

    /// <summary>
    /// �÷��̾��� �и� ��ȣ ������ ���� �������� ���� �Ѵ�.
    /// </summary>
    /// <param name="player_index"></param>
    public void sort_player_hand_slots()
    {
        this.hand_pae.Sort((Card lhs, Card rhs) =>
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
        string debug = string.Format("player [{0}] ", this.player_index);
        for (int i = 0; i < this.hand_pae.Count; ++i)
        {
            debug += string.Format("{0}, ", this.hand_pae[i].number);
        }
        Debug.Log("sort_player_hand_slots " + debug);
    }

    public void add_bomb_count(byte count)
    {
        this.remain_bomb_count += count;
    }

    public bool decrease_bomb_count()
    {
        if (this.remain_bomb_count > 0)
        {
            --this.remain_bomb_count;
            return true;
        }
        return false;
    }

    public bool can_finish()
    {
        if (this.score < 7)
        {
            return false;
        }

        if (this.prev_score >= this.score)
        {
            return false;
        }

        this.prev_score = this.score;
        return true;
    }

    public void plus_go_count()
    {
        ++this.go_count;
    }

    public void plus_shaking_count()
    {
        ++this.shaking_count;
    }

    public void plus_ppuk_count()
    {
        ++this.ppuk_count;
    }

    public void on_start_ppuk()
    {
        this.start_ppuk = 1;
        ++this.ppuk_count;
    }

    public bool three_ppuk()
    {
        if (this.ppuk_count > 2)
        {
            //3���� ��� ������ 30���� �ްԵǰ� ���ʽ� ������ ù ���� ������ ������ ������ �����ϴ� �������� �ʱ�ȭ���ش�.
            this.score = 30;
            this.go_count = 0;
            this.shaking_count = 0;
            return true;
        }
        return false;
    }

    public void kookjin_selected()
    {
        this.is_used_kookjin = true;
    }

    public void move_kookjin_to_pee()
    {
        Card card = pop_specific_card_from_floor(PAE_TYPE.YEOL, CARD_STATUS.KOOKJIN);
        if (card == null)
        {
            return;
        }

        card.change_pae_type(PAE_TYPE.PEE);
        card.set_card_status(CARD_STATUS.TWO_PEE);

        add_card_to_floor(card);
        calculate_score();
    }

    public void add_turn()
    {
        this.turn++;
    }

    public int get_now_player_turn()
    {
        return this.turn;
    }
}
