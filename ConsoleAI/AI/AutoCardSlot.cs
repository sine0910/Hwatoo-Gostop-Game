using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public class AutoCardSlot
    {
        public List<FloorSlotList> floorSlotLists;

        public List<Card> my_hand_cards;

        public List<Card> my_kwang_floor_cards;
        public List<Card> my_yeol_floor_cards;
        public List<Card> my_tee_floor_cards;
        public List<Card> my_pee_floor_cards;

        public List<Card> other_kwang_floor_cards;
        public List<Card> other_yeol_floor_cards;
        public List<Card> other_tee_floor_cards;
        public List<Card> other_pee_floor_cards;

        public AutoCardSlot()
        {
            floorSlotLists = new List<FloorSlotList>();
            for (byte i = 0; i < 12; i++)
            {
                this.floorSlotLists.Add(new FloorSlotList(i));
            }

            my_hand_cards = new List<Card>();

            my_kwang_floor_cards = new List<Card>();
            my_yeol_floor_cards = new List<Card>();
            my_tee_floor_cards = new List<Card>();
            my_pee_floor_cards = new List<Card>();

            other_kwang_floor_cards = new List<Card>();
            other_yeol_floor_cards = new List<Card>();
            other_tee_floor_cards = new List<Card>();
            other_pee_floor_cards = new List<Card>();
        }

        public void RemoveFloor(Card card)
        {
            FloorSlotList slot = this.floorSlotLists.Find(obj => obj.is_same_card(card.number));
            slot.remove_card(card);
        }

        public void SetMyCardToHand(Card card)
        {
            my_hand_cards.Add(card);
        }

        public void RemoveMyCardToHand(Card card)
        {
            my_hand_cards.Remove(card);
        }

        public void SetMyCardToFloor(Card card)
        {
            switch (card.pae_type)
            {
                case PAE_TYPE.KWANG:
                    {
                        my_kwang_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.YEOL:
                    {
                        my_yeol_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.TEE:
                    {
                        my_tee_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.PEE:
                    {
                        my_pee_floor_cards.Add(card);
                    }
                    break;
            }
        }

        public void RemoveMyCardToFloor(Card card)
        {
            switch (card.pae_type)
            {
                case PAE_TYPE.KWANG:
                    {
                        my_kwang_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.YEOL:
                    {
                        my_yeol_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.TEE:
                    {
                        my_tee_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.PEE:
                    {
                        my_pee_floor_cards.Remove(card);
                    }
                    break;
            }
        }

        public void SetOtherCardToFloor(Card card)
        {
            switch (card.pae_type)
            {
                case PAE_TYPE.KWANG:
                    {
                        other_kwang_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.YEOL:
                    {
                        other_yeol_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.TEE:
                    {
                        other_tee_floor_cards.Add(card);
                    }
                    break;
                case PAE_TYPE.PEE:
                    {
                        other_pee_floor_cards.Add(card);
                    }
                    break;
            }
        }

        public void RemoveOtherCardToFloor(Card card)
        {
            switch (card.pae_type)
            {
                case PAE_TYPE.KWANG:
                    {
                        other_kwang_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.YEOL:
                    {
                        other_yeol_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.TEE:
                    {
                        other_tee_floor_cards.Remove(card);
                    }
                    break;
                case PAE_TYPE.PEE:
                    {
                        other_pee_floor_cards.Remove(card);
                    }
                    break;
            }
        }

        public int GetHandCardStatus(CARD_STATUS status)
        {
            List<Card> cards = this.my_hand_cards.FindAll(obj => obj.is_same_status(status));
            return cards.Count;
        }

        public int GetHandCardType(PAE_TYPE type)
        {
            List<Card> cards = this.my_hand_cards.FindAll(obj => obj.pae_type == type);
            return cards.Count;
        }

        public int GetHandCardNumber(byte number)
        {
            List<Card> same_cards = this.my_hand_cards.FindAll(obj => obj.is_same_number(number));
            return same_cards.Count;
        }

        public int GetMyTeeCheongdan()
        {
            List<Card> same_cards = this.my_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.CHEONG_DAN));
            return same_cards.Count;
        }
        public int GetMyTeeChodan()
        {
            List<Card> same_cards = this.my_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.CHO_DAN));
            return same_cards.Count;
        }
        public int GetMyTeeHongdan()
        {
            List<Card> same_cards = this.my_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.HONG_DAN));
            return same_cards.Count;
        }

        public int GetMyYeolGodori()
        {
            List<Card> same_cards = this.my_yeol_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.GODORI));
            return same_cards.Count;
        }

        public int GetMyKwang()
        {
            return my_kwang_floor_cards.Count;
        }

        public int GetOtherTeeCheongdan()
        {
            List<Card> same_cards = this.other_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.CHEONG_DAN));
            return same_cards.Count;
        }
        public int GetOtherTeeChodan()
        {
            List<Card> same_cards = this.other_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.CHO_DAN));
            return same_cards.Count;
        }
        public int GetOtherTeeHongdan()
        {
            List<Card> same_cards = this.other_tee_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.HONG_DAN));
            return same_cards.Count;
        }

        public int GetOtherYeolGodori()
        {
            List<Card> same_cards = this.other_yeol_floor_cards.FindAll(obj => obj.is_same_status(CARD_STATUS.GODORI));
            return same_cards.Count;
        }

        public int GetOtherKwang()
        {
            return other_kwang_floor_cards.Count;
        }

        public bool HaveBeaKwang()
        {
            bool is_exist_beekwang = this.my_kwang_floor_cards.Exists(obj => obj.is_same_number(11));
            return is_exist_beekwang;
        }

        public int IsSameNumberCount(byte number)
        {
            int result = 0;

            List<Card> mk_same_cards = this.my_kwang_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += mk_same_cards.Count;

            List<Card> my_same_cards = this.my_yeol_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += my_same_cards.Count;

            List<Card> mt_same_cards = this.my_tee_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += mt_same_cards.Count;

            List<Card> mp_same_cards = this.my_pee_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += mp_same_cards.Count;

            List<Card> ok_same_cards = this.other_kwang_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += ok_same_cards.Count;

            List<Card> oy_same_cards = this.other_yeol_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += oy_same_cards.Count;

            List<Card> ot_same_cards = this.other_tee_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += ot_same_cards.Count;

            List<Card> op_same_cards = this.other_pee_floor_cards.FindAll(obj => obj.is_same_number(number));
            result += op_same_cards.Count;

            return result;
        }

        public List<Card> FindEatFloorCard(PAE_TYPE pae_type)
        {
            if (pae_type == PAE_TYPE.KWANG)
            {
                List<Card> mk_same_cards = this.other_kwang_floor_cards;
                return mk_same_cards;
            }
            else if (pae_type == PAE_TYPE.YEOL)
            {
                List<Card> mk_same_cards = this.other_yeol_floor_cards;
                return mk_same_cards;
            }
            else if (pae_type == PAE_TYPE.TEE)
            {
                List<Card> mk_same_cards = this.other_tee_floor_cards;
                return mk_same_cards;
            }
            else if (pae_type == PAE_TYPE.PEE)
            {
                List<Card> mk_same_cards = this.other_pee_floor_cards;
                return mk_same_cards;
            }
            return null;
        }

        public byte GetEatFloorCard(PAE_TYPE pae_type, CARD_STATUS status)
        {
            List<Card> targets = null;
            if (pae_type == PAE_TYPE.KWANG)
            {
                targets = this.other_kwang_floor_cards.FindAll(obj => obj.is_same_status(status));
            }
            else if (pae_type == PAE_TYPE.YEOL)
            {
                targets = this.other_yeol_floor_cards.FindAll(obj => obj.is_same_status(status));
            }
            else if (pae_type == PAE_TYPE.TEE)
            {
                targets = this.other_tee_floor_cards.FindAll(obj => obj.is_same_status(status));
            }
            else if (pae_type == PAE_TYPE.PEE)
            {
                targets = this.other_pee_floor_cards.FindAll(obj => obj.is_same_status(status));
            }

            if (targets == null)
            {
                return 0;
            }

            return (byte)targets.Count;
        }

        short get_score_by_type(PAE_TYPE pae_type)
        {
            short pae_score = 0;

            List<Card> cards = FindEatFloorCard(pae_type);
            if (cards == null)
            {
                return 0;
            }

            switch (pae_type)
            {
                case PAE_TYPE.PEE:
                    {
                        byte twopee_count = GetEatFloorCard(PAE_TYPE.PEE, CARD_STATUS.TWO_PEE);
                        byte bonuspee_count = GetEatFloorCard(PAE_TYPE.PEE, CARD_STATUS.BONUS_PEE);
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
                        // 비광이 포함되어 있으면 2점. 아니면 3점.
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

        public int CalculateScore()
        {
            int score = 0;

            score += get_score_by_type(PAE_TYPE.PEE);
            score += get_score_by_type(PAE_TYPE.TEE);
            score += get_score_by_type(PAE_TYPE.YEOL);
            score += get_score_by_type(PAE_TYPE.KWANG);

            byte godori_count = GetEatFloorCard(PAE_TYPE.YEOL, CARD_STATUS.GODORI);
            if (godori_count == 3)
            {
                score += 5;
            }

            byte cheongdan_count = GetEatFloorCard(PAE_TYPE.TEE, CARD_STATUS.CHEONG_DAN);
            byte hongdan_count = GetEatFloorCard(PAE_TYPE.TEE, CARD_STATUS.HONG_DAN);
            byte chodan_count = GetEatFloorCard(PAE_TYPE.TEE, CARD_STATUS.CHO_DAN);
            if (cheongdan_count == 3)
            {
                score += 3;
            }

            if (hongdan_count == 3)
            {
                score += 3;
            }

            if (chodan_count == 3)
            {
                score += 3;
            }
            return score;
        }

        public void ResetSlot()
        {
            for (byte i = 0; i < 12; i++)
            {
                this.floorSlotLists[i].reset();
            }

            my_hand_cards.Clear();

            my_kwang_floor_cards.Clear();
            my_yeol_floor_cards.Clear();
            my_tee_floor_cards.Clear();
            my_pee_floor_cards.Clear();

            other_kwang_floor_cards.Clear();
            other_yeol_floor_cards.Clear();
            other_tee_floor_cards.Clear();
            other_pee_floor_cards.Clear();
        }
    }

    public class FloorSlotList
    {
        public byte slot_index;
        List<Card> floor_cards;
        public byte card_number;

        public FloorSlotList(byte slot)
        {
            this.slot_index = slot;
            this.floor_cards = new List<Card>();
            this.card_number = 0;
        }

        public void add_card(Card card)
        {
            this.card_number = card.number;
            this.floor_cards.Add(card);
        }

        public void remove_card(Card card)
        {
            this.floor_cards.Remove(card);

            if (this.floor_cards.Count <= 0)
            {
                this.card_number = byte.MaxValue;
            }
        }

        public void reset()
        {
            this.floor_cards.Clear();
            this.card_number = 0;
        }

        public void get_card_list(List<Card> cards)
        {
            this.floor_cards = cards;
            if (cards.Count > 0)
            {
                if (cards[0].number != 12)
                {
                    this.card_number = cards[0].number;
                }
            }
        }

        public int get_card_count()
        {
            return this.floor_cards.Count;
        }

        public bool is_same_card(byte number)
        {
            return this.card_number == number;
        }

        public Card find_card(Card card)
        {
            return this.floor_cards.Find(obj => obj.is_same_card(card.number, card.pae_type, card.position));
        }

        public Card get_first_card()
        {
            if (get_card_count() <= 0)
            {
                return null;
            }

            return this.floor_cards[0];
        }

        public List<Card> get_cards()
        {
            return this.floor_cards;
        }
    }
}
