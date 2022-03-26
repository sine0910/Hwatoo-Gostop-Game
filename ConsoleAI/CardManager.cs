using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    class CardManager
    {
        public List<Card> cards { get; private set; }

        public CardManager()
        {
            this.cards = new List<Card>();
        }

        public void MakePickCard(Queue<Card> target)
        {
            //2021-04-21 17:05 동일한 숫자를 제외한 카드를 뽑아 넣어줌
            Queue<PAE_TYPE> total_pae_type = new Queue<PAE_TYPE>();
            // 1
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            // 2
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 3
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            // 4
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 5
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 6
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 7
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 8
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            // 9
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 10
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            // 11
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            // 12
            total_pae_type.Enqueue(PAE_TYPE.KWANG);

            List<Card> first_user_select_pick_cards = new List<Card>();

            for (byte number = 0; number < 12; ++number)
            {
                first_user_select_pick_cards.Add(new Card(number, total_pae_type.Dequeue(), 0));
            }

            Shuffle(first_user_select_pick_cards);

            first_user_select_pick_cards.ForEach(obj => target.Enqueue(obj));
        }


        public void make_Multi_all_cards()
        {
            // Generate cards.
            Queue<PAE_TYPE> total_pae_type = new Queue<PAE_TYPE>();
            // 1
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 2
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 3
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 4
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 5
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 6
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 7
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 8
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 9
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 10
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 11
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            // 12
            total_pae_type.Enqueue(PAE_TYPE.KWANG);
            total_pae_type.Enqueue(PAE_TYPE.YEOL);
            total_pae_type.Enqueue(PAE_TYPE.TEE);
            total_pae_type.Enqueue(PAE_TYPE.PEE);

            //보너스 카드
            for (byte pos = 0; pos < 3; ++pos)
            {
                total_pae_type.Enqueue(PAE_TYPE.PEE);
            }

            ////Debug.Log("total_pae_type: " + total_pae_type.Count);

            this.cards.Clear();

            for (byte number = 0; number < 13; ++number)
            {
                if (number == 12)
                {
                    for (byte pos = 0; pos < 3; ++pos)
                    {
                        this.cards.Add(new Card(number, total_pae_type.Dequeue(), pos));
                        ////Debug.Log("event pos: " + pos);
                    }
                }
                else
                {
                    for (byte pos = 0; pos < 4; ++pos)
                    {
                        this.cards.Add(new Card(number, total_pae_type.Dequeue(), pos));
                        ////Debug.Log("pos: " + pos);
                    }
                }
            }
            ////Debug.Log("카드 생성 갯수" + this.cards.Count);
        }

        public void multi_set_allcard_status()
        {
            // 카드 속성 설정.
            // 고도리.
            apply_card_status(1, PAE_TYPE.YEOL, 0, CARD_STATUS.GODORI);
            apply_card_status(3, PAE_TYPE.YEOL, 0, CARD_STATUS.GODORI);
            apply_card_status(7, PAE_TYPE.YEOL, 1, CARD_STATUS.GODORI);

            // 청단, 홍단, 초단
            apply_card_status(5, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);
            apply_card_status(8, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);
            apply_card_status(9, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);

            apply_card_status(0, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);
            apply_card_status(1, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);
            apply_card_status(2, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);

            apply_card_status(3, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);
            apply_card_status(4, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);
            apply_card_status(6, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);

            // 쌍피.
            apply_card_status(10, PAE_TYPE.PEE, 1, CARD_STATUS.TWO_PEE);
            apply_card_status(11, PAE_TYPE.PEE, 3, CARD_STATUS.TWO_PEE);

            // 국진.
            apply_card_status(8, PAE_TYPE.YEOL, 0, CARD_STATUS.KOOKJIN);

            //보너스 피
            for (byte pos = 0; pos < 3; ++pos)
            {
                apply_card_status(12, PAE_TYPE.PEE, pos, CARD_STATUS.BONUS_PEE);
            }
        }

        void set_allcard_status()
        {
            // 카드 속성 설정.

            // 고도리.
            apply_card_status(1, PAE_TYPE.YEOL, 0, CARD_STATUS.GODORI);
            apply_card_status(3, PAE_TYPE.YEOL, 0, CARD_STATUS.GODORI);
            apply_card_status(7, PAE_TYPE.YEOL, 1, CARD_STATUS.GODORI);

            // 청단, 홍단, 초단
            apply_card_status(5, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);
            apply_card_status(8, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);
            apply_card_status(9, PAE_TYPE.TEE, 1, CARD_STATUS.CHEONG_DAN);

            apply_card_status(0, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);
            apply_card_status(1, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);
            apply_card_status(2, PAE_TYPE.TEE, 1, CARD_STATUS.HONG_DAN);

            apply_card_status(3, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);
            apply_card_status(4, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);
            apply_card_status(6, PAE_TYPE.TEE, 1, CARD_STATUS.CHO_DAN);

            // 쌍피.
            apply_card_status(10, PAE_TYPE.PEE, 1, CARD_STATUS.TWO_PEE);
            apply_card_status(11, PAE_TYPE.PEE, 3, CARD_STATUS.TWO_PEE);

            // 국진.
            apply_card_status(8, PAE_TYPE.YEOL, 0, CARD_STATUS.KOOKJIN);

            //보너스 피
            for (byte pos = 0; pos < GameData.Instance.bonus_card; pos++)
            {
                apply_card_status(12, PAE_TYPE.PEE, pos, CARD_STATUS.BONUS_PEE);
            }
        }

        void apply_card_status(byte number, PAE_TYPE pae_type, byte position, CARD_STATUS status)
        {
            Card card = find_card(number, pae_type, position);
            card.set_card_status(status);
        }

        public Card find_card(byte number, PAE_TYPE pae_type, byte position)
        {
            return this.cards.Find(obj => obj.is_same_card(number, pae_type, position));
        }

        public void on_suffle()
        {
            set_allcard_status();
            Shuffle<Card>(this.cards);
        }

        //카드를 섞기 위한 함수
        public static void Shuffle<T>(List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void fill_to(Queue<Card> target)
        {
            this.cards.ForEach(obj => target.Enqueue(obj));
        }
    }
}
