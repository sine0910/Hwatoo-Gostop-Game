using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public class AutoBrain
    {
        public AutoCardSlot slotManager;

        int other_score;
        int before_other_score;

        int other_cheongdan_count;
        int other_chodan_count;
        int other_hongdan_count;
        int other_godori_count;
        int other_kwang_count;

        int my_cheongdan_count;
        int my_chodan_count;
        int my_hongdan_count;
        int my_godori_count;
        int my_kwang_count;

        public AutoBrain()
        {
            slotManager = new AutoCardSlot();
        }

        public Card SelectHandCard(int BombCount)
        {
            Console.Write("AI_slotManager.my_hand_cards: " + slotManager.my_hand_cards.Count);

            Card most_select_card;

            List<SelectHandCards> select_cards = new List<SelectHandCards>();
            bool non_eat_card = true;
            //2021-05-31 11:57 손에 들고있는 카드 중 더 이상 먹을 것이 없거나 보너스 카드일 경우
            Console.Write("slotManager.my_hand_cards.Count: " + slotManager.my_hand_cards.Count);
            for (int k = 0; k < slotManager.my_hand_cards.Count; k++)
            {
                Console.Write("AI_slotManager.my_hand_cards[k].status: " + slotManager.my_hand_cards[k].status);

                int eat_cards = slotManager.IsSameNumberCount(slotManager.my_hand_cards[k].number);
                Console.Write("AI_eat card count: " + eat_cards);
                int hand_cards = slotManager.GetHandCardNumber(slotManager.my_hand_cards[k].number);
                Console.Write("AI_hand card count: " + hand_cards);

                FloorSlotList slot = slotManager.floorSlotLists.Find(obj => obj.is_same_card(slotManager.my_hand_cards[k].number));
                if (slot != null)
                {
                    non_eat_card = false;

                    List<Card> cards = slot.get_cards();
                    if (cards.Count > 0)
                    {
                        for (int a = 0; a < cards.Count; a++)
                        {
                            Card card = cards[a];

                            select_cards.Add(new SelectHandCards(slotManager.my_hand_cards[k], card));
                            if (hand_cards > 3)
                            {
                                select_cards[select_cards.Count - 1].score += 50;
                            }
                            if (hand_cards == 3 || cards.Count == 3)
                            {
                                if (slotManager.other_pee_floor_cards.Count > 1)
                                {
                                    select_cards[select_cards.Count - 1].score += 30;
                                }
                                else
                                {
                                    select_cards[select_cards.Count - 1].score -= 10;
                                }
                            }
                            if (hand_cards == 2)
                            {
                                select_cards[select_cards.Count - 1].score += 5;
                            }
                            //짜르봄바
                            if (Data.Instance.Acount_ID == "bomin")
                            {
                                #region 
                                if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.PEE)
                                {
                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.KOOKJIN ||
                                       select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.TWO_PEE)
                                    {
                                        select_cards[select_cards.Count - 1].score += 7;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (slotManager.my_tee_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHEONG_DAN)
                                    {
                                        if ((my_cheongdan_count >= 0 && other_cheongdan_count == 0) || (other_cheongdan_count >= 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHO_DAN)
                                    {
                                        if ((my_chodan_count >= 0 && other_chodan_count == 0) || (other_chodan_count >= 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count >= 0 && other_hongdan_count == 0) || (other_hongdan_count >= 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.GODORI)
                                    {
                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_godori_count >= 1 && other_godori_count == 0) || (other_godori_count >= 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if (my_godori_count > 1 || other_godori_count > 1)
                                        {
                                            select_cards[select_cards.Count - 1].score += 40;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 50;
                                    }
                                    else if ((my_kwang_count > 2 && other_kwang_count == 0) || (my_kwang_count < 0 && other_kwang_count > 2))
                                    {
                                        select_cards[select_cards.Count - 1].score += 10;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count != 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 2))
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 6;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count == 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 1))
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count != 0)
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                    }
                                    else
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                    }
                                }

                                if (card.pae_type == PAE_TYPE.PEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (card.status == CARD_STATUS.KOOKJIN || card.status == CARD_STATUS.TWO_PEE || card.status == CARD_STATUS.BONUS_PEE)
                                    {
                                        select_cards[select_cards.Count - 1].score += 7;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score -= 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        if (card.status != CARD_STATUS.GODORI)
                                        {
                                            select_cards[select_cards.Count - 1].score -= 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (slotManager.my_tee_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.CHEONG_DAN)
                                    {
                                        if ((my_cheongdan_count >= 0 && other_cheongdan_count == 0) || (other_cheongdan_count >= 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.CHO_DAN)
                                    {
                                        if ((my_chodan_count >= 0 && other_chodan_count == 0) || (other_chodan_count >= 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count >= 0 && other_hongdan_count == 0) || (other_hongdan_count >= 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.GODORI)
                                    {
                                        if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                            select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                        {
                                            if (card.status != CARD_STATUS.GODORI)
                                            {
                                                select_cards[select_cards.Count - 1].score -= 3;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 3;
                                            }
                                        }

                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_godori_count >= 1 && other_godori_count == 0) || (other_godori_count >= 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else if (my_godori_count > 1 || other_godori_count > 1)
                                        {
                                            select_cards[select_cards.Count - 1].score += 40;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 50;
                                    }
                                    else if ((my_kwang_count > 2 && other_kwang_count == 0) || (my_kwang_count < 0 && other_kwang_count > 2))
                                    {
                                        select_cards[select_cards.Count - 1].score += 30;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count != 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 2))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 15;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 25;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count == 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 1))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 10;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count != 0)
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                }
                                #endregion
                            }
                            else if (Data.Instance.Acount_ID == "talTal" || Data.Instance.Acount_ID == "gostopLuBu")
                            {
                                #region 
                                if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.PEE)
                                {
                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.TWO_PEE)
                                    {
                                        select_cards[select_cards.Count - 1].score += 4;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (slotManager.my_tee_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHEONG_DAN)
                                    {
                                        if ((my_cheongdan_count == 0 && other_cheongdan_count == 0) || (other_cheongdan_count == 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 1;
                                        }
                                        else if ((my_cheongdan_count > 0 && other_cheongdan_count == 0) || (other_cheongdan_count > 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHO_DAN)
                                    {
                                        if ((my_chodan_count == 0 && other_chodan_count == 0) || (other_chodan_count == 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 1;
                                        }
                                        else if ((my_chodan_count > 0 && other_chodan_count == 0) || (other_chodan_count > 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count == 0 && other_hongdan_count == 0) || (other_hongdan_count == 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 1;
                                        }
                                        else if ((my_hongdan_count > 0 && other_hongdan_count == 0) || (other_hongdan_count > 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.KOOKJIN)
                                    {
                                        select_cards[select_cards.Count - 1].score += 4;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.GODORI)
                                    {
                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 1;
                                        }
                                        else if ((my_godori_count == 1 && other_godori_count == 0) || (other_godori_count == 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else if (my_godori_count == 2 || other_godori_count == 2)
                                        {
                                            select_cards[select_cards.Count - 1].score += 10;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 25;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count > 2)
                                    {
                                        if (my_kwang_count == 0 || other_kwang_count == 0)
                                        {
                                            select_cards[select_cards.Count - 1].score += 15;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 10;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count > 1)
                                    {
                                        if (my_kwang_count == 0 || other_kwang_count == 0)
                                        {
                                            if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                            {
                                                select_cards[select_cards.Count - 1].score += 7;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 10;
                                            }
                                        }
                                        else
                                        {
                                            if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                            {
                                                select_cards[select_cards.Count - 1].score += 5;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 7;
                                            }
                                        }
                                    }
                                    else if (my_kwang_count > 0 || other_kwang_count > 0)
                                    {
                                        if (my_kwang_count == 0 || other_kwang_count == 0)
                                        {
                                            if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                            {
                                                select_cards[select_cards.Count - 1].score += 5;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 7;
                                            }
                                        }
                                        else
                                        {
                                            if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                            {
                                                select_cards[select_cards.Count - 1].score += 3;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 5;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }

                                if (card.pae_type == PAE_TYPE.PEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (card.status == CARD_STATUS.KOOKJIN || card.status == CARD_STATUS.TWO_PEE || card.status == CARD_STATUS.BONUS_PEE)
                                    {
                                        select_cards[select_cards.Count - 1].score += 7;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score -= 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        if (card.status != CARD_STATUS.GODORI)
                                        {
                                            select_cards[select_cards.Count - 1].score -= 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (slotManager.my_tee_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.CHEONG_DAN)
                                    {
                                        if ((my_cheongdan_count >= 0 && other_cheongdan_count == 0) || (other_cheongdan_count >= 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.CHO_DAN)
                                    {
                                        if ((my_chodan_count >= 0 && other_chodan_count == 0) || (other_chodan_count >= 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count >= 0 && other_hongdan_count == 0) || (other_hongdan_count >= 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.KOOKJIN)
                                    {
                                        select_cards[select_cards.Count - 1].score += 5;
                                    }

                                    if (card.status == CARD_STATUS.GODORI)
                                    {
                                        if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                            select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                        {
                                            if (card.status != CARD_STATUS.GODORI)
                                            {
                                                select_cards[select_cards.Count - 1].score -= 3;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 3;
                                            }
                                        }

                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_godori_count >= 1 && other_godori_count == 0) || (other_godori_count >= 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else if (my_godori_count > 1 || other_godori_count > 1)
                                        {
                                            select_cards[select_cards.Count - 1].score += 40;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 50;
                                    }
                                    else if ((my_kwang_count > 2 && other_kwang_count == 0) || (my_kwang_count < 0 && other_kwang_count > 2))
                                    {
                                        select_cards[select_cards.Count - 1].score += 30;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count != 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 2))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 15;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 25;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count == 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 1))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 10;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count != 0)
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                }
                                #endregion
                            }
                            //윤만석
                            if (Data.Instance.Acount_ID == "soomin3203" || Data.Instance.Acount_ID == "frezia")
                            {
                                #region 
                                if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.PEE)//피 일때
                                {
                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.TWO_PEE)//쌍피 일때
                                    {
                                        select_cards[select_cards.Count - 1].score += 15;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }
                                //띠 일때
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (slotManager.my_tee_floor_cards.Count >= 4 || slotManager.other_yeol_floor_cards.Count >= 4)//나, 상대가 먹은 띠가 4장 이상일 때
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHEONG_DAN)//청단 일때
                                    {
                                        if ((my_cheongdan_count >= 0 && other_cheongdan_count == 0) || (other_cheongdan_count >= 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.CHO_DAN)//초단 일때
                                    {
                                        if ((my_chodan_count >= 0 && other_chodan_count == 0) || (other_chodan_count >= 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count >= 0 && other_hongdan_count == 0) || (other_hongdan_count >= 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.KOOKJIN)
                                    {
                                        select_cards[select_cards.Count - 1].score += 10;
                                    }

                                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.GODORI)
                                    {
                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else if ((my_godori_count >= 1 && other_godori_count == 0) || (other_godori_count >= 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if (my_godori_count > 1 || other_godori_count > 1)
                                        {
                                            select_cards[select_cards.Count - 1].score += 40;
                                        }
                                    }
                                }
                                else if (select_cards[select_cards.Count - 1].my_hend_card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 50;
                                    }
                                    else if ((my_kwang_count > 2 && other_kwang_count == 0) || (my_kwang_count < 0 && other_kwang_count > 2))
                                    {
                                        select_cards[select_cards.Count - 1].score += 10;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count != 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 2))
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 6;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count == 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 1))
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count != 0)
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                    }
                                    else
                                    {
                                        if (select_cards[select_cards.Count - 1].my_hend_card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 2;
                                        }
                                    }
                                }

                                if (card.pae_type == PAE_TYPE.PEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (card.status == CARD_STATUS.KOOKJIN || card.status == CARD_STATUS.TWO_PEE || card.status == CARD_STATUS.BONUS_PEE)
                                    {
                                        select_cards[select_cards.Count - 1].score += 7;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.TEE)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score -= 3;
                                    }

                                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                    {
                                        if (card.status != CARD_STATUS.GODORI)
                                        {
                                            select_cards[select_cards.Count - 1].score -= 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (slotManager.my_tee_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.CHEONG_DAN)
                                    {
                                        if ((my_cheongdan_count >= 0 && other_cheongdan_count == 0) || (other_cheongdan_count >= 0 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_cheongdan_count > 1 && other_cheongdan_count == 0) || (other_cheongdan_count > 1 && my_cheongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.CHO_DAN)
                                    {
                                        if ((my_chodan_count >= 0 && other_chodan_count == 0) || (other_chodan_count >= 0 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_chodan_count > 1 && other_chodan_count == 0) || (other_chodan_count > 1 && my_chodan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else if (card.status == CARD_STATUS.HONG_DAN)
                                    {
                                        if ((my_hongdan_count >= 0 && other_hongdan_count == 0) || (other_hongdan_count >= 0 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 4;
                                        }
                                        else if ((my_hongdan_count > 1 && other_hongdan_count == 0) || (other_hongdan_count > 1 && my_hongdan_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.YEOL)
                                {
                                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                                    {
                                        select_cards[select_cards.Count - 1].score += 3;
                                    }

                                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }
                                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                                    {
                                        select_cards[select_cards.Count - 1].score += 2;
                                    }
                                    else
                                    {
                                        select_cards[select_cards.Count - 1].score -= 1;
                                    }

                                    if (card.status == CARD_STATUS.KOOKJIN)
                                    {
                                        select_cards[select_cards.Count - 1].score += 7;
                                    }

                                    if (slotManager.my_yeol_floor_cards.Count >= 5 || slotManager.other_yeol_floor_cards.Count >= 5)
                                    {
                                        select_cards[select_cards.Count - 1].score += 1;
                                    }

                                    if (card.status == CARD_STATUS.GODORI)
                                    {
                                        if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                                            select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                                        {
                                            if (card.status != CARD_STATUS.GODORI)
                                            {
                                                select_cards[select_cards.Count - 1].score -= 3;
                                            }
                                            else
                                            {
                                                select_cards[select_cards.Count - 1].score += 3;
                                            }
                                        }

                                        if ((my_godori_count == 0 && other_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else if ((my_godori_count >= 1 && other_godori_count == 0) || (other_godori_count >= 1 && my_godori_count == 0))
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else if (my_godori_count > 1 || other_godori_count > 1)
                                        {
                                            select_cards[select_cards.Count - 1].score += 40;
                                        }
                                    }
                                }
                                else if (card.pae_type == PAE_TYPE.KWANG)
                                {
                                    if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 11:15 오광을 노린다!
                                    {
                                        select_cards[select_cards.Count - 1].score += 50;
                                    }
                                    else if ((my_kwang_count > 2 && other_kwang_count == 0) || (my_kwang_count < 0 && other_kwang_count > 2))
                                    {
                                        select_cards[select_cards.Count - 1].score += 30;
                                    }
                                    else if (my_kwang_count > 2 || other_kwang_count != 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 2))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 15;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 25;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count == 0 || (my_kwang_count < other_kwang_count && other_kwang_count > 1))
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 10;
                                        }
                                    }
                                    else if (my_kwang_count > 1 || other_kwang_count != 0)
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 7;
                                        }
                                    }
                                    else
                                    {
                                        if (card.number == Card.BEE_KWANG)
                                        {
                                            select_cards[select_cards.Count - 1].score += 3;
                                        }
                                        else
                                        {
                                            select_cards[select_cards.Count - 1].score += 5;
                                        }
                                    }
                                }
                                #endregion
                            }
                            if (eat_cards == 2 || select_cards[select_cards.Count - 1].my_hend_card.status != CARD_STATUS.BONUS_PEE)
                            {
                                select_cards[select_cards.Count - 1].score -= 3;
                            }
                        }
                    }
                    else
                    {
                        select_cards.Add(new SelectHandCards(slotManager.my_hand_cards[k], null));

                        select_cards[select_cards.Count - 1].score -= 5;

                        if (eat_cards == 0)
                        {
                            select_cards[select_cards.Count - 1].score -= 3;
                            if (hand_cards == 4)
                            {
                                select_cards[select_cards.Count - 1].score -= 5;
                            }
                        }
                        if (eat_cards == 2)//2021-05-31 12:38 카드가 짤린 카드일 경우 낼 카드가 없을 경우에만 우선으로 냄
                        {
                            select_cards[select_cards.Count - 1].score -= 3;
                            if (hand_cards == 2)
                            {
                                select_cards[select_cards.Count - 1].score -= 5;
                            }
                        }
                        if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.BONUS_PEE)
                        {
                            select_cards[select_cards.Count - 1].score += 30;
                        }
                        else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.KOOKJIN ||
                            select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.TWO_PEE)
                        {
                            select_cards[select_cards.Count - 1].score -= 5;
                        }
                    }
                }
                else
                {
                    select_cards.Add(new SelectHandCards(slotManager.my_hand_cards[k], null));

                    select_cards[select_cards.Count - 1].score -= 5;

                    if (eat_cards == 0)
                    {
                        select_cards[select_cards.Count - 1].score -= 3;
                        if (hand_cards == 4)
                        {
                            select_cards[select_cards.Count - 1].score -= 5;
                        }
                    }
                    if (eat_cards == 2)//2021-05-31 12:38 카드가 짤린 카드일 경우 낼 카드가 없을 경우에만 우선으로 냄
                    {
                        select_cards[select_cards.Count - 1].score -= 3;
                        if (hand_cards == 2)
                        {
                            select_cards[select_cards.Count - 1].score -= 5;
                        }
                    }
                    if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.BONUS_PEE)
                    {
                        select_cards[select_cards.Count - 1].score += 30;
                    }
                    else if (select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.KOOKJIN ||
                        select_cards[select_cards.Count - 1].my_hend_card.status == CARD_STATUS.TWO_PEE)
                    {
                        select_cards[select_cards.Count - 1].score -= 5;
                    }
                }
            }

            if (non_eat_card)
            {
                select_cards.Clear();

                for (int k = 0; k < slotManager.my_hand_cards.Count; k++)
                {
                    select_cards.Add(new SelectHandCards(slotManager.my_hand_cards[k], null));

                    int eat_cards = slotManager.IsSameNumberCount(slotManager.my_hand_cards[k].number);
                    Console.Write("AI_eat card count: " + eat_cards);
                    int hand_cards = slotManager.GetHandCardNumber(slotManager.my_hand_cards[k].number);
                    Console.Write("AI_hand card count: " + hand_cards);

                    if (hand_cards == 4)
                    {
                        select_cards[select_cards.Count - 1].score += 15;
                    }
                    else if (hand_cards == 3)
                    {
                        select_cards[select_cards.Count - 1].score -= 3;
                    }
                    else if (hand_cards == 2)//2021-05-31 12:38 카드가 짤린 카드일 경우 우선으로 냄
                    {
                        if (eat_cards == 2)
                        {
                            select_cards[select_cards.Count - 1].score += 2;
                        }
                        select_cards[select_cards.Count - 1].score += 1;
                    }

                    if (((other_kwang_count > 0 || my_kwang_count < 1) || (other_kwang_count < 1 || my_kwang_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 0 ||
                        select_cards[select_cards.Count - 1].my_hend_card.number == 2 || select_cards[select_cards.Count - 1].my_hend_card.number == 7 ||
                        select_cards[select_cards.Count - 1].my_hend_card.number == 10 || select_cards[select_cards.Count - 1].my_hend_card.number == 11))
                    {
                        select_cards[select_cards.Count - 1].score -= 3;
                    }

                    if (((other_godori_count > 0 && my_godori_count < 1) || (other_godori_count < 1 && my_godori_count > 0)) && (select_cards[select_cards.Count - 1].my_hend_card.number == 1 ||
                        select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 7))
                    {
                        select_cards[select_cards.Count - 1].score -= 2;
                    }
                    else
                    {
                        select_cards[select_cards.Count - 1].score += 1;
                    }

                    if (((other_cheongdan_count > 0 && my_cheongdan_count < 1) || (other_cheongdan_count < 1 && my_cheongdan_count > 0)) &&
                        (select_cards[select_cards.Count - 1].my_hend_card.number == 5 || select_cards[select_cards.Count - 1].my_hend_card.number == 8 ||
                        select_cards[select_cards.Count - 1].my_hend_card.number == 9))
                    {
                        select_cards[select_cards.Count - 1].score -= 1;
                    }
                    else
                    {
                        select_cards[select_cards.Count - 1].score += 1;
                    }
                    if (((other_chodan_count > 0 && my_chodan_count < 1) || (other_chodan_count < 1 && my_chodan_count > 0)) &&
                       (select_cards[select_cards.Count - 1].my_hend_card.number == 3 || select_cards[select_cards.Count - 1].my_hend_card.number == 4 ||
                       select_cards[select_cards.Count - 1].my_hend_card.number == 6))
                    {
                        select_cards[select_cards.Count - 1].score -= 1;
                    }
                    else
                    {
                        select_cards[select_cards.Count - 1].score += 1;
                    }
                    if (((other_hongdan_count > 0 && my_hongdan_count < 1) || (other_hongdan_count < 1 && my_hongdan_count > 0)) &&
                       (select_cards[select_cards.Count - 1].my_hend_card.number == 1 || select_cards[select_cards.Count - 1].my_hend_card.number == 2 ||
                       select_cards[select_cards.Count - 1].my_hend_card.number == 3))
                    {
                        select_cards[select_cards.Count - 1].score -= 1;
                    }
                    else
                    {
                        select_cards[select_cards.Count - 1].score += 1;
                    }

                    if (slotManager.my_hand_cards[k].pae_type == PAE_TYPE.KWANG)
                    {
                        select_cards[select_cards.Count - 1].score = 0;
                    }
                    else if (slotManager.my_hand_cards[k].pae_type == PAE_TYPE.YEOL)
                    {
                        if ((slotManager.my_yeol_floor_cards.Count > 4 && slotManager.other_yeol_floor_cards.Count == 0) ||
                            (slotManager.other_yeol_floor_cards.Count > 4 && slotManager.my_yeol_floor_cards.Count == 0))
                        {
                            select_cards[select_cards.Count - 1].score += 0;
                        }
                        else if ((slotManager.my_yeol_floor_cards.Count < 5 && slotManager.other_yeol_floor_cards.Count > 0) ||
                              (slotManager.other_yeol_floor_cards.Count < 5 && slotManager.my_yeol_floor_cards.Count > 0))
                        {
                            select_cards[select_cards.Count - 1].score += 1;
                        }
                        else
                        {
                            select_cards[select_cards.Count - 1].score += 2;
                        }
                        if (slotManager.my_hand_cards[k].status == CARD_STATUS.GODORI)
                        {
                            if (my_godori_count > 0 && other_godori_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 2;
                            }
                            else if (my_godori_count > 1 && other_godori_count > 1)//2021-05-31 12:16 절대 내면 안된다구!
                            {
                                select_cards[select_cards.Count - 1].score += 0;
                            }
                            else
                            {
                                select_cards[select_cards.Count - 1].score += 1;
                            }
                        }
                    }
                    else if (slotManager.my_hand_cards[k].pae_type == PAE_TYPE.TEE)
                    {
                        if ((slotManager.my_tee_floor_cards.Count > 4 && slotManager.other_tee_floor_cards.Count == 0) ||
                           (slotManager.my_tee_floor_cards.Count == 0 && slotManager.other_tee_floor_cards.Count > 4))
                        {
                            select_cards[select_cards.Count - 1].score += 1;
                        }
                        else
                        {
                            select_cards[select_cards.Count - 1].score += 0;
                        }

                        if (slotManager.my_hand_cards[k].status == CARD_STATUS.CHEONG_DAN)
                        {
                            if (my_cheongdan_count > 1 || other_cheongdan_count > 1)
                            {
                                select_cards[select_cards.Count - 1].score += 2;
                            }
                            else if (my_cheongdan_count > 0 || other_cheongdan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 1;
                            }
                            if (my_cheongdan_count > 0 && other_cheongdan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 0;
                            }
                        }
                        else if (slotManager.my_hand_cards[k].status == CARD_STATUS.CHO_DAN)
                        {
                            if (my_chodan_count > 1 || other_chodan_count > 1)
                            {
                                select_cards[select_cards.Count - 1].score += 2;
                            }
                            else if (my_chodan_count > 0 || other_chodan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 1;
                            }
                            if (my_chodan_count > 0 && other_chodan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 0;
                            }
                        }
                        else if (slotManager.my_hand_cards[k].status == CARD_STATUS.HONG_DAN)
                        {
                            if (my_hongdan_count > 1 || other_hongdan_count > 1)
                            {
                                select_cards[select_cards.Count - 1].score += 2;
                            }
                            else if (my_hongdan_count > 0 || other_cheongdan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 1;
                            }
                            if (my_hongdan_count > 0 && other_cheongdan_count > 0)
                            {
                                select_cards[select_cards.Count - 1].score += 0;
                            }
                        }
                    }
                    else if (slotManager.my_hand_cards[k].pae_type == PAE_TYPE.PEE)
                    {
                        if (slotManager.my_hand_cards[k].status == CARD_STATUS.KOOKJIN || slotManager.my_hand_cards[k].status == CARD_STATUS.TWO_PEE)
                        {
                            select_cards[select_cards.Count - 1].score -= 10;
                        }
                        else if (slotManager.my_hand_cards[k].status == CARD_STATUS.BONUS_PEE)
                        {
                            select_cards[select_cards.Count - 1].score += 30;
                        }
                        else
                        {
                            select_cards[select_cards.Count - 1].score -= 1;
                        }
                    }
                }
            }

            if (BombCount > 0)
            {
                if (non_eat_card)
                {
                    select_cards.Add(new SelectHandCards(null, null));
                    select_cards[select_cards.Count - 1].score += 3;
                }
                else
                {
                    select_cards.Add(new SelectHandCards(null, null));
                    select_cards[select_cards.Count - 1].score -= 3;
                }
            }

            select_cards = select_cards.OrderByDescending(x => x.score).ToList();
            if (select_cards != null)
            {
                Console.Write("AI_Final Select Card Score: " + select_cards[0].score);
                most_select_card = select_cards[0].my_hend_card;
                return most_select_card;
            }
            else
            {
                Console.Write("AI_Final slotManager.my_hand_cards[0]: " + slotManager.my_hand_cards[0].ToString());
                return slotManager.my_hand_cards[0];
            }
        }

        public int ChoiceCard(Card card1, Card card2)
        {
            int result_card_index = 0;

            SelectChoiceCard choice_one_card = new SelectChoiceCard(card1);
            SelectChoiceCard choice_two_card = new SelectChoiceCard(card2);

            if (choice_one_card.card.pae_type == PAE_TYPE.KWANG)
            {
                if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 17:56 오광은 나야나
                {
                    return 1;
                }
                else if (my_kwang_count > 1 || other_kwang_count > 1)
                {
                    choice_one_card.score += 5;
                }
                else
                {
                    choice_one_card.score += 3;
                }
            }
            else if (choice_one_card.card.pae_type == PAE_TYPE.YEOL)
            {
                if (choice_one_card.card.status == CARD_STATUS.KOOKJIN)
                {
                    choice_one_card.score += 4;
                }

                if (choice_one_card.card.status == CARD_STATUS.GODORI)
                {
                    if (my_godori_count > 1 || other_godori_count > 1 || slotManager.GetHandCardStatus(CARD_STATUS.GODORI) > 1)
                    {
                        choice_one_card.score += 7;
                    }
                    else if (my_godori_count > 0 && other_godori_count > 0)
                    {
                        choice_one_card.score += 0;
                    }
                    else
                    {
                        choice_one_card.score += 2;
                    }
                }
            }
            else if (choice_one_card.card.pae_type == PAE_TYPE.TEE)
            {
                if (choice_one_card.card.status == CARD_STATUS.CHEONG_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 1 ||
                       (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 && my_cheongdan_count > 0) ||
                       other_cheongdan_count > 1)
                    {
                        choice_one_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_one_card.score += 2;
                    }
                    else
                    {
                        choice_one_card.score += 1;
                    }
                }
                else if (choice_one_card.card.status == CARD_STATUS.CHO_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 1 ||
                       (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 && my_cheongdan_count > 0) ||
                       other_cheongdan_count > 1)
                    {
                        choice_one_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_one_card.score += 2;
                    }
                    else
                    {
                        choice_one_card.score += 1;
                    }
                }
                else if (choice_one_card.card.status == CARD_STATUS.HONG_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 1 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 && my_cheongdan_count > 0) ||
                        other_cheongdan_count > 1)
                    {
                        choice_one_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_one_card.score += 2;
                    }
                    else
                    {
                        choice_one_card.score += 1;
                    }
                }
            }
            else if (choice_one_card.card.pae_type == PAE_TYPE.PEE)
            {
                if (choice_one_card.card.status == CARD_STATUS.TWO_PEE)
                {
                    choice_one_card.score += 7;
                }
                else
                {
                    choice_one_card.score += 2;
                }
            }

            if (choice_two_card.card.pae_type == PAE_TYPE.KWANG)
            {
                if (my_kwang_count > 3 || other_kwang_count > 3)//2021-05-31 17:56 오광은 나야나
                {
                    return 2;
                }
                else if (my_kwang_count > 1 || other_kwang_count > 1)
                {
                    choice_two_card.score += 5;
                }
                else
                {
                    choice_two_card.score += 3;
                }
            }
            else if (choice_two_card.card.pae_type == PAE_TYPE.YEOL)
            {
                if (choice_two_card.card.status == CARD_STATUS.GODORI)
                {
                    if (choice_two_card.card.status == CARD_STATUS.KOOKJIN)
                    {
                        choice_two_card.score += 4;
                    }

                    if (my_godori_count > 1 || other_godori_count > 1 || slotManager.GetHandCardStatus(CARD_STATUS.GODORI) > 1)
                    {
                        choice_two_card.score += 7;
                    }
                    else if (my_godori_count > 0 && other_godori_count > 0)
                    {
                        choice_two_card.score += 0;
                    }
                    else
                    {
                        choice_two_card.score += 2;
                    }
                }
            }
            else if (choice_two_card.card.pae_type == PAE_TYPE.TEE)
            {
                if (choice_two_card.card.status == CARD_STATUS.CHEONG_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 1 ||
                       (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 && my_cheongdan_count > 0) ||
                       other_cheongdan_count > 1)
                    {
                        choice_two_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_two_card.score += 2;
                    }
                    else
                    {
                        choice_two_card.score += 1;
                    }
                }
                else if (choice_two_card.card.status == CARD_STATUS.CHO_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 1 ||
                       (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 && my_cheongdan_count > 0) ||
                       other_cheongdan_count > 1)
                    {
                        choice_two_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_two_card.score += 2;
                    }
                    else
                    {
                        choice_two_card.score += 1;
                    }
                }
                else if (choice_two_card.card.status == CARD_STATUS.HONG_DAN)
                {
                    if (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 1 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 && my_cheongdan_count > 0) ||
                        other_cheongdan_count > 1)
                    {
                        choice_two_card.score += 3;
                    }
                    else if (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 ||
                        (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) > 0 && my_cheongdan_count == 0))
                    {
                        choice_two_card.score += 2;
                    }
                    else
                    {
                        choice_two_card.score += 1;
                    }
                }
            }
            else if (choice_two_card.card.pae_type == PAE_TYPE.PEE)
            {
                if (choice_two_card.card.status == CARD_STATUS.KOOKJIN ||
                    choice_two_card.card.status == CARD_STATUS.TWO_PEE)
                {
                    choice_two_card.score += 5;
                }
                else
                {
                    choice_two_card.score += 1;
                }
            }

            if (choice_one_card.score > choice_two_card.score)
                result_card_index = 0;
            else
                result_card_index = 1;

            return result_card_index;
        }

        public int SelectGoAndStop()
        {
            int go_an_stop = 0;
            int score = 0;
            int floor_card_count = 0;

            for (int k = 0; k < slotManager.floorSlotLists.Count; k++)
            {
                FloorSlotList slot = slotManager.floorSlotLists[k];
                List<Card> cards = slot.get_cards();
                if (cards != null || cards.Count != 0)
                {
                    floor_card_count += cards.Count;
                    //짜르봄바
                    if (Data.Instance.Acount_ID == "bomin")
                    {
                        #region
                        if ((other_score + 4 >= 7 || other_score + 4 >= before_other_score) && cards.Count > 2)
                        {
                            score -= 1;
                        }

                        for (int i = 0; i < cards.Count; i++)
                        {
                            if (((other_kwang_count + 1 == 3 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score)) || (other_kwang_count + 1 == 4 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score))) &&
                                ((other_kwang_count > 1 && my_kwang_count < other_kwang_count) || ((slotManager.GetHandCardType(PAE_TYPE.KWANG) == 0) || cards[i].pae_type == PAE_TYPE.KWANG)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_godori_count > 1 && my_godori_count == 0) && ((slotManager.GetHandCardStatus(CARD_STATUS.GODORI) == 0 || cards[i].status == CARD_STATUS.GODORI)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_cheongdan_count > 1 && my_cheongdan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) == 0 || cards[i].status == CARD_STATUS.CHEONG_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_chodan_count > 1 && my_chodan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) == 0 || cards[i].status == CARD_STATUS.CHO_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_hongdan_count > 1 && my_hongdan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) == 0 || cards[i].status == CARD_STATUS.HONG_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && slotManager.other_pee_floor_cards.Count > 10 &&
                                (slotManager.GetHandCardStatus(CARD_STATUS.TWO_PEE) == 0 || slotManager.GetHandCardStatus(CARD_STATUS.KOOKJIN) == 0 &&
                                 cards[i].status == CARD_STATUS.TWO_PEE || cards[i].status == CARD_STATUS.KOOKJIN))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                        }
                        #endregion
                    }
                    else if (Data.Instance.Acount_ID == "talTal" || Data.Instance.Acount_ID == "frezia")
                    {
                        #region
                        if ((other_score + 4 >= 7 || other_score + 4 >= before_other_score) && cards.Count > 2)
                        {
                            score -= 1;
                        }

                        for (int i = 0; i < cards.Count; i++)
                        {
                            if (((other_kwang_count + 1 == 3 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score)) || (other_kwang_count + 1 == 4 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score))) &&
                                ((other_kwang_count > 1 && my_kwang_count < other_kwang_count) || ((slotManager.GetHandCardType(PAE_TYPE.KWANG) == 0) || cards[i].pae_type == PAE_TYPE.KWANG)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if (((other_score + 5 >= 7 || other_score + 5 >= before_other_score) || (other_godori_count > 1 && my_godori_count == 0)) && ((slotManager.GetHandCardStatus(CARD_STATUS.GODORI) == 0 || cards[i].status == CARD_STATUS.GODORI)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if (((other_score + 3 >= 7 || other_score + 3 >= before_other_score) || ((other_cheongdan_count > 1 && my_cheongdan_count == 0)) || (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) == 0 || cards[i].status == CARD_STATUS.CHEONG_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if (((other_score + 3 >= 7 || other_score + 3 >= before_other_score) || ((other_chodan_count > 1 && my_chodan_count == 0)) || (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) == 0 || cards[i].status == CARD_STATUS.CHO_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if (((other_score + 3 >= 7 || other_score + 3 >= before_other_score) || ((other_hongdan_count > 1 && my_hongdan_count == 0)) || (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) == 0 || cards[i].status == CARD_STATUS.HONG_DAN)))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 4 >= 7 || other_score + 4 >= before_other_score) && slotManager.other_pee_floor_cards.Count > 10 &&
                                (slotManager.GetHandCardStatus(CARD_STATUS.TWO_PEE) == 0 || slotManager.GetHandCardStatus(CARD_STATUS.KOOKJIN) == 0 &&
                                 cards[i].status == CARD_STATUS.TWO_PEE || cards[i].status == CARD_STATUS.KOOKJIN))
                            {
                                score -= 1;
                            }
                            else
                            {
                                score += 1;
                            }
                        }
                        #endregion
                    }
                    //윤만석
                    if (Data.Instance.Acount_ID == "soomin3203" || Data.Instance.Acount_ID == "gostopLuBu")
                    {
                        #region
                        if ((other_score + 4 >= 7 || other_score + 4 >= before_other_score) && cards.Count > 2)
                        {
                            score -= 1;
                        }

                        for (int i = 0; i < cards.Count; i++)
                        {//상대 광2개이며, 상대 점수가 
                            if (((other_kwang_count + 1 == 3 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score)) || (other_kwang_count + 1 == 4 && (other_score + 3 >= 7 || other_score + 3 >= before_other_score))) &&
                                ((other_kwang_count > 1 && my_kwang_count < other_kwang_count) || ((slotManager.GetHandCardType(PAE_TYPE.KWANG) == 0) || cards[i].pae_type == PAE_TYPE.KWANG)))
                            {
                                score -= 2;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_godori_count > 1 && my_godori_count == 0) && ((slotManager.GetHandCardStatus(CARD_STATUS.GODORI) == 0 || cards[i].status == CARD_STATUS.GODORI)))
                            {
                                score -= 2;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_cheongdan_count > 1 && my_cheongdan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.CHEONG_DAN) == 0 || cards[i].status == CARD_STATUS.CHEONG_DAN)))
                            {
                                score -= 2;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_chodan_count > 1 && my_chodan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.CHO_DAN) == 0 || cards[i].status == CARD_STATUS.CHO_DAN)))
                            {
                                score -= 2;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && ((other_hongdan_count > 1 && my_hongdan_count == 0) || (slotManager.GetHandCardStatus(CARD_STATUS.HONG_DAN) == 0 || cards[i].status == CARD_STATUS.HONG_DAN)))
                            {
                                score -= 2;
                            }
                            else
                            {
                                score += 1;
                            }
                            if ((other_score + 3 >= 7 || other_score + 3 >= before_other_score) && slotManager.other_pee_floor_cards.Count > 10 &&
                                (slotManager.GetHandCardStatus(CARD_STATUS.TWO_PEE) == 0 || slotManager.GetHandCardStatus(CARD_STATUS.KOOKJIN) == 0 &&
                                 cards[i].status == CARD_STATUS.TWO_PEE || cards[i].status == CARD_STATUS.KOOKJIN))
                            {
                                score -= 3;
                            }
                            else
                            {
                                score += 1;
                            }
                        }
                        #endregion
                    }
                }
            }

            if (score >= 0)
            {
                go_an_stop = 1;
            }
            else
            {
                go_an_stop = 0;
            }
            Console.Write("AI Go/Stop score: " + score + " result: " + go_an_stop);
            return go_an_stop;
        }

        public void CheckGame()
        {
            other_cheongdan_count = slotManager.GetOtherTeeCheongdan();
            other_chodan_count = slotManager.GetOtherTeeChodan();
            other_hongdan_count = slotManager.GetOtherTeeHongdan();
            other_godori_count = slotManager.GetOtherYeolGodori();
            other_kwang_count = slotManager.GetOtherKwang();

            my_cheongdan_count = slotManager.GetMyTeeCheongdan();
            my_chodan_count = slotManager.GetMyTeeChodan();
            my_hongdan_count = slotManager.GetMyTeeHongdan();
            my_godori_count = slotManager.GetMyYeolGodori();
            my_kwang_count = slotManager.GetMyKwang();
        }

        public void SaveGameScore()
        {
            if (other_score > before_other_score)
            {
                before_other_score = other_score;
            }
            other_score = slotManager.CalculateScore();
            Console.Write("AI other_score: " + other_score + "\nbefore_other_score" + before_other_score);
        }

        public byte FindEmptyFloorSlot()
        {
            FloorSlotList slot = this.slotManager.floorSlotLists.Find(obj => obj.get_card_count() == 0);
            if (slot == null)
            {
                return byte.MaxValue;
            }

            return slot.slot_index;
        }

        public void DataReset()
        {
            before_other_score = 0;
            other_score = 0;

            other_cheongdan_count = 0;
            other_chodan_count = 0;
            other_hongdan_count = 0;
            other_godori_count = 0;
            other_kwang_count = 0;

            my_cheongdan_count = 0;
            my_chodan_count = 0;
            my_hongdan_count = 0;
            my_godori_count = 0;
            my_kwang_count = 0;

            slotManager.ResetSlot();
        }

        public class SelectHandCards
        {
            public int score;
            public Card my_hend_card;
            public Card floor_card;

            public SelectHandCards(Card hend, Card floor)
            {
                this.score = 0;
                this.my_hend_card = hend;
                this.floor_card = floor;
            }
        }

        public class SelectChoiceCard
        {
            public int score;
            public Card card;

            public SelectChoiceCard(Card card)
            {
                this.score = 0;
                this.card = card;
            }
        }
    }
}
