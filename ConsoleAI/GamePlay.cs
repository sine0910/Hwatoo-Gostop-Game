using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace AIProject
{
    public class GamePlay
    {
        bool host;
        bool other_out = false;

        FirestoreDb db;
        FirestoreChangeListener listener;

        string game_room;

        AutoBrain autoBrain;

        List<Card> begin_cards = new List<Card>();

        public byte player_me_index;

        List<FloorSlot> floor_ui_slots;
        BonusSlot bonus_ui_slot;
        // 가운데 쌓여있는 카드 객체.
        Stack<Card> deck_cards;
        List<HandCardManager> player_hand_card_manager;
        // 플레이어가 먹은 카드 객체.
        List<PlayerCardManager> player_card_manager;

        CardManager card_manager;

        Queue<Card> floor_cards = new Queue<Card>();
        List<Card> begin_cards_picture = new List<Card>();

        FirstSet firstSet;

        bool reGame;

        byte Bomb_card = 0;

        bool d = true;

        public GamePlay(FirestoreDb db, string game_room)
        {
            Console.WriteLine("GamePlayStart GameRoom ID: " + game_room);

            this.host = false;
            this.db = db;
            this.game_room = game_room;

            autoBrain = new AutoBrain();
            firstSet = new FirstSet();

            this.player_me_index = 0;

            this.deck_cards = new Stack<Card>();
            this.card_manager = new CardManager();

            this.floor_ui_slots = new List<FloorSlot>();
            for (byte i = 0; i < 12; i++)
            {
                this.floor_ui_slots.Add(new FloorSlot(i, byte.MaxValue, 0));
            }

            this.bonus_ui_slot = new BonusSlot();

            this.player_hand_card_manager = new List<HandCardManager>();
            this.player_hand_card_manager.Add(new HandCardManager());
            this.player_hand_card_manager.Add(new HandCardManager());

            this.player_card_manager = new List<PlayerCardManager>();
            this.player_card_manager.Add(new PlayerCardManager());
            this.player_card_manager.Add(new PlayerCardManager());

            PlayListen();

            Start();//사용자 에게 플레이 시작 프로토콜 전송
        }

        public async void Start()
        {
            DocumentReference docRef = db.Collection("MultiRoom").Document(game_room).Collection("host").Document("protocol");
            Dictionary<string, object> user = new Dictionary<string, object>
            {
                    { "value",  "900bSET_FIRST_USER" }
            };
            await docRef.SetAsync(user);
        }

        public async void Protocol(string msg)
        {
            if (d)
            {
                TextLog.Log("Send Protocol " + msg);
            }
            Console.WriteLine("Send Protocol " + msg);

            DocumentReference docRef = db.Collection("MultiRoom").Document(game_room).Collection("host").Document("protocol");
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                   { "value",  msg }
            };
            await docRef.SetAsync(data);
        }

        public void PlayListen()
        {
            DocumentReference _ref = db.Collection("MultiRoom").Document(game_room).Collection("guest").Document("protocol");
            this.listener = _ref.Listen(snapshot =>
            {
                if (snapshot.Exists)
                {
                    Dictionary<string, object> city = snapshot.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in city)
                    {
                        if (pair.Value != null)
                        {
                            string value = pair.Value.ToString();
                            string[] lines = value.Split(new string[] { "b" }, StringSplitOptions.None);

                            List<string> msg = new List<string>();

                            foreach (string line in lines)
                            {
                                msg.Add(line);
                            }

                            int protocol = Int32.Parse(PopAt(msg));

                            switch (protocol)
                            {
                                case 1:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("LOCAL_SERVER_STARETED");
                                        }
                                        Console.WriteLine("LOCAL_SERVER_STARETED");

                                        System.String prt = "";

                                        prt += ((short)PROTOCOL.READY_TO_START).ToString();

                                        Protocol(prt);
                                    }
                                    break;

                                case 11:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("BEGIN_CARD_INFO");
                                        }
                                        Console.WriteLine("BEGIN_CARD_INFO");

                                        reset();

                                        // floor cards.
                                        this.player_me_index = (byte)Int32.Parse(PopAt(msg));
                                        byte floor_count = (byte)Int32.Parse(PopAt(msg));

                                        ////Console.WriteLine("플로어 카운트: " + floor_cards.Count);
                                        for (byte i = 0; i < floor_count; ++i)
                                        {
                                            byte number = (byte)Int32.Parse(PopAt(msg));
                                            PAE_TYPE pae_type = PaeType(PopAt(msg));
                                            byte position = (byte)Int32.Parse(PopAt(msg));

                                            Card card = this.card_manager.find_card(number, pae_type, position);
                                            if (card == null)
                                            {
                                                if (d)
                                                {
                                                    TextLog.Log(string.Format("Cannot find the card. {0}, {1}, {2}", number, pae_type, position));
                                                }
                                                Console.WriteLine(string.Format("Cannot find the card. {0}, {1}, {2}", number, pae_type, position));
                                            }
                                            floor_cards.Enqueue(card);
                                        }

                                        Dictionary<byte, Queue<Card>> player_cards = new Dictionary<byte, Queue<Card>>();
                                        byte player_count = (byte)Int32.Parse(PopAt(msg));
                                        for (byte player = 0; player < player_count; ++player)
                                        {
                                            Queue<Card> cards = new Queue<Card>();
                                            byte player_index = (byte)Int32.Parse(PopAt(msg));
                                            byte card_count = (byte)Int32.Parse(PopAt(msg));

                                            for (byte i = 0; i < card_count; ++i)
                                            {
                                                byte number = (byte)Int32.Parse(PopAt(msg));
                                                if (number != byte.MaxValue)
                                                {
                                                    PAE_TYPE pae_type = PaeType(PopAt(msg));
                                                    byte position = (byte)Int32.Parse(PopAt(msg));
                                                    Card card = this.card_manager.find_card(number, pae_type, position);
                                                    cards.Enqueue(card);
                                                }
                                            }
                                            player_cards.Add(player_index, cards);
                                        }
                                        distribute_multi_cards(player_cards);
                                    }
                                    break;

                                case 33://보너스 피의 갯수 만큼 덱에서 뽑은 카드 이동을 반복
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("STARTBONUSPEE");
                                        }
                                        Console.WriteLine("STARTBONUSPEE");

                                        byte f_count = (byte)Int32.Parse(PopAt(msg));
                                        byte player_index = (byte)Int32.Parse(PopAt(msg));

                                        byte bonusCount = (byte)Int32.Parse(PopAt(msg));

                                        MoveStartBonusCard(f_count, player_index);

                                        for (byte bc = 0; bc < bonusCount; ++bc)
                                        {
                                            byte card_number_msg = (byte)Int32.Parse(PopAt(msg));
                                            PAE_TYPE card_pae_type = PaeType(PopAt(msg));
                                            byte card_position = (byte)Int32.Parse(PopAt(msg));
                                            byte card_slot_index = (byte)Int32.Parse(PopAt(msg));

                                            FlipDeckCard(player_index, bonusCount, card_number_msg, card_pae_type, card_position, card_slot_index);
                                        }

                                        update_player_statistics(msg);

                                        Floor(player_index);
                                    }
                                    break;

                                case 98:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("START_TURN");
                                        }
                                        Console.WriteLine("START_TURN");

                                        byte remain_bomb_card_count = 0;
                                        if (msg.Count > 0)
                                        {
                                            remain_bomb_card_count = (byte)Int32.Parse(PopAt(msg));
                                            Bomb_card = remain_bomb_card_count;
                                        }
                                        AI_Auto_CardPick();
                                    }
                                    break;

                                case 44:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("SELECT_BONUS_CARD_ACK");
                                        }
                                        Console.WriteLine("SELECT_BONUS_CARD_ACK");

                                        byte current_turn_player_index = (byte)Int32.Parse(PopAt(msg));

                                        byte slot_index = (byte)Int32.Parse(PopAt(msg));

                                        byte number = (byte)Int32.Parse(PopAt(msg));
                                        PAE_TYPE pae_type = PaeType(PopAt(msg));
                                        byte position = (byte)Int32.Parse(PopAt(msg));

                                        on_select_bonus_card_ack(current_turn_player_index, slot_index, number, pae_type, position);
                                    }
                                    break;

                                case 14:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("SELECT_CARD_ACK");
                                        }
                                        Console.WriteLine("SELECT_CARD_ACK");

                                        on_select_card_ack(msg);
                                    }
                                    break;

                                case 19:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("FLIP_DECK_CARD_ACK");
                                        }
                                        Console.WriteLine("FLIP_DECK_CARD_ACK");

                                        on_flip_deck_card_ack(msg);
                                    }
                                    break;

                                case 46:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("FLIP_DECK_BONUS_CARD_ACK");
                                        }
                                        Console.WriteLine("FLIP_DECK_BONUS_CARD_ACK");

                                        Dictionary<byte, Queue<Card>> player_cards = new Dictionary<byte, Queue<Card>>();

                                        byte player_index = (byte)Int32.Parse(PopAt(msg));
                                        Console.WriteLine("deckcard player_index" + player_index);

                                        // 덱에서 뒤집은 카드 정보.
                                        byte deck_card_number = (byte)Int32.Parse(PopAt(msg));
                                        PAE_TYPE deck_card_pae_type = PaeType(PopAt(msg));
                                        byte deck_card_position = (byte)Int32.Parse(PopAt(msg));
                                        byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg));

                                        Queue<Card> cards = new Queue<Card>();
                                        Card card = this.card_manager.find_card(deck_card_number, deck_card_pae_type, deck_card_position);
                                        cards.Enqueue(card);

                                        player_cards.Add(player_index, cards);

                                        on_flip_deck_bonus_card_ack(player_cards);
                                    }
                                    break;

                                case 55:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("FLIP_PLUS_BONUS_CARD_ACK");
                                        }
                                        Console.WriteLine("FLIP_PLUS_BONUS_CARD_ACK");

                                        on_flip_plus_bonus_card_ack(msg);
                                    }
                                    break;

                                case 57:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("FLIP_BOMB_BONUS_CARD_ACK");
                                        }
                                        Console.WriteLine("FLIP_BOMB_BONUS_CARD_ACK");

                                        byte player_index = (byte)Int32.Parse(PopAt(msg));

                                        // 덱에서 뒤집은 카드 정보.
                                        byte deck_card_number = (byte)Int32.Parse(PopAt(msg));
                                        PAE_TYPE deck_card_pae_type = PaeType(PopAt(msg));
                                        byte deck_card_position = (byte)Int32.Parse(PopAt(msg));
                                        byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg));

                                        move_bomb_flip_bonus_card(player_index, deck_card_number, deck_card_pae_type, deck_card_position);
                                    }
                                    break;

                                case 20:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("TURN_RESULT");
                                        }
                                        Console.WriteLine("TURN_RESULT");
                                        // 데이터 파싱 시작 ----------------------------------------
                                        byte player_index = (byte)Int32.Parse(PopAt(msg));
                                        on_turn_result(player_index, msg);
                                    }
                                    break;

                                case 21:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("ASK_GO_OR_STOP");
                                        }
                                        Console.WriteLine("ASK_GO_OR_STOP");

                                        int val = this.autoBrain.SelectGoAndStop();

                                        System.String prt = "";

                                        prt += ((short)PROTOCOL.ANSWER_GO_OR_STOP).ToString();

                                        prt += "b" + player_me_index;
                                        prt += "b" + val;

                                        Protocol(prt);
                                    }
                                    break;

                                case 24:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("ASK_KOOKJIN_TO_PEE");
                                        }
                                        Console.WriteLine("ASK_KOOKJIN_TO_PEE");

                                        System.String prt = "";

                                        prt += ((short)PROTOCOL.ANSWER_KOOKJIN_TO_PEE).ToString();

                                        prt += "b" + player_me_index;
                                        prt += "b" + 1;

                                        Protocol(prt);
                                    }
                                    break;

                                case 26:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("MOVE_KOOKJIN_TO_PEE");
                                        }
                                        Console.WriteLine("MOVE_KOOKJIN_TO_PEE");

                                        byte player_index = (byte)Int32.Parse(PopAt(msg));
                                        move_kookjin_to_pee(player_index);
                                    }
                                    break;

                                case 27:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("GAME_RESULT");
                                        }
                                        Console.WriteLine("GAME_RESULT");
                                        //2021-02-05 11:28 첫뻑 잔 데이타 처리
                                        //byte winner_ppuck = (byte)Int32.Parse(PopAt(msg));

                                        byte winner = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_score = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_go = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_shaking = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_kwang = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_pee = (byte)Int32.Parse(PopAt(msg));

                                        byte losser_go = (byte)Int32.Parse(PopAt(msg));
                                        byte losser_kwang = (byte)Int32.Parse(PopAt(msg));
                                        byte losser_pee = (byte)Int32.Parse(PopAt(msg));

                                        if (msg.Count == 4)//2021-01-22 14:38 멍박이 추가되지 않은 이전 버전으로 인한 충돌 에러 방지
                                        {
                                            byte winner_yeol = (byte)Int32.Parse(PopAt(msg));
                                            byte losser_yeol = (byte)Int32.Parse(PopAt(msg));

                                            byte h_start_ppuck = (byte)Int32.Parse(PopAt(msg));
                                            byte g_start_ppuck = (byte)Int32.Parse(PopAt(msg));

                                            OnGameOver(winner, winner_score, winner_go, winner_shaking, winner_kwang, winner_pee, winner_yeol, losser_go, losser_kwang, losser_pee, losser_yeol, h_start_ppuck, g_start_ppuck);
                                        }
                                        else
                                        {
                                            OnGameOver(winner, winner_score, winner_go, winner_shaking, winner_kwang, winner_pee, 0, losser_go, losser_kwang, losser_pee, 0, 0, 0);
                                        }
                                    }
                                    break;

                                case 29:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("PUCK_GAME_RESULT");
                                        }
                                        Console.WriteLine("PUCK_GAME_RESULT");

                                        byte winner = (byte)Int32.Parse(PopAt(msg));
                                        byte winner_score = (byte)Int32.Parse(PopAt(msg));

                                        PuckGameOver(winner, winner_score);
                                    }
                                    break;

                                case 30:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("NAGARI_GAME");
                                        }
                                        Console.WriteLine("NAGARI_GAME");

                                        Nagari();
                                    }
                                    break;

                                case 101:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("GO");
                                        }
                                        Console.WriteLine("GO");
                                    }
                                    break;

                                case 82:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("ON_SET_START_USER");
                                        }
                                        Console.WriteLine("ON_SET_START_USER");
                                        //2021-04-23 09:44 카드를 선택하는 팝업창을 띄움                                     
                                        Queue<Card> cards = new Queue<Card>();
                                        for (int i = 0; i < 3; i++)
                                        {
                                            byte number = (byte)Convert.ToInt32(PopAt(msg));
                                            PAE_TYPE type = PaeType(PopAt(msg));
                                            byte position = (byte)Convert.ToInt32(PopAt(msg));

                                            Card card = new Card(number, type, position);
                                            firstSet.IndexList.Add(card);
                                        }
                                    }
                                    break;

                                case 84:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("HOST_SET_START_USER_ACK");
                                        }
                                        Console.WriteLine("HOST_SET_START_USER_ACK");
                                        //2021-04-23 10:00 선택한 결과를 보여줌
                                        int pick_slot = Convert.ToInt32(PopAt(msg));

                                        firstSet.host_select_slot_index = pick_slot;

                                        int slot = 0;
                                        if (firstSet.host_select_slot_index != 1)
                                        {
                                            slot = 1;
                                        }

                                        System.String prt = "";

                                        prt += ((short)PROTOCOL.GUEST_SET_START_USER_REQ).ToString();

                                        prt += "b" + slot;
                                        prt += "b" + firstSet.IndexList[slot].number;
                                        prt += "b" + firstSet.IndexList[slot].pae_type;
                                        prt += "b" + firstSet.IndexList[slot].position;
                                        Protocol(prt);
                                    }
                                    break;

                                case 86:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("GUEST_SET_START_USER_ACK");
                                        }
                                        Console.WriteLine("GUEST_SET_START_USER_ACK");
                                        //2021-04-23 10:00 선택한 결과를 보여줌
                                        Protocol("87");
                                    }
                                    break;

                                case 88:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("END_SET_START_USER_ACK");
                                        }
                                        Console.WriteLine("END_SET_START_USER_ACK");
                                        Delay(2000);
                                        Protocol("900");
                                    }
                                    break;

                                case 909:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("OTHER_POCUS_OUT");
                                        }
                                        Console.WriteLine("OTHER_POCUS_OUT");
                                        out_time_check();
                                        if (d)
                                        {
                                            TextLog.Log("OTHER_POCUS_OUT out_time_check end");
                                        }
                                        Console.WriteLine("OTHER_POCUS_OUT out_time_check end");
                                    }
                                    break;

                                case 979:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("OTHER_POCUS_IN");
                                        }
                                        Console.WriteLine("OTHER_POCUS_IN");
                                        other_out = false;
                                    }
                                    break;

                                case 999:
                                    {
                                        if (d)
                                        {
                                            TextLog.Log("OTHER_GAME_QUIT");
                                        }
                                        Console.WriteLine("OTHER_GAME_QUIT");
                                        Stop();
                                    }
                                    break;
                            }
                        }
                    }
                }
            });
        }

        void out_time_check()
        {
            Console.WriteLine("out_time_check");
            other_out = true;
            PocusManager pocus = new PocusManager();
            Task task = pocus.delay_out_time(check_delay_out_time);
            Console.WriteLine("out_time_check end");
        }

        void check_delay_out_time()
        {
            Console.WriteLine("check_delay_out_time");
            if (other_out)
            {
                if (d)
                {
                    TextLog.Log("check_delay_out_time other_out");
                }
                Console.WriteLine("check_delay_out_time other_out");
                Protocol("999");
                Stop();
            }
        }


        private static DateTime Delay(int MS)
        {
            // Thread 와 Timer보다 효율 적으로 사용할 수 있음.
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        public async void Stop()
        {
            if (d)
            {
                TextLog.Log("MultiGame End");
            }
            Console.WriteLine("MultiGame End!");

            await this.listener.StopAsync();

            AILogin.Instance.CheckChangeTime();
        }

        void reset()
        {
            this.card_manager.make_Multi_all_cards();//카드를 전부 재생성
            this.card_manager.multi_set_allcard_status();

            this.autoBrain.DataReset();

            floor_cards.Clear();
            begin_cards.Clear();
            begin_cards_picture.Clear();

            for (int i = 0; i < this.floor_ui_slots.Count; ++i)
            {
                this.floor_ui_slots[i].reset();
            }

            this.bonus_ui_slot.reset();

            make_deck_cards();

            for (int i = 0; i < this.player_hand_card_manager.Count; ++i)
            {
                this.player_hand_card_manager[i].reset();
            }

            for (int i = 0; i < this.player_card_manager.Count; ++i)
            {
                this.player_card_manager[i].reset();
            }
        }

        void make_deck_cards()
        {
            this.deck_cards.Clear();

            for (int i = 0; i < this.card_manager.cards.Count; ++i)
            {
                this.deck_cards.Push(this.card_manager.cards[i]);
            }
        }

        void distribute_multi_cards(Dictionary<byte, Queue<Card>> player_cards)
        {
            // [바닥 -> 1P -> 2P 나눠주기] 를 두번 반복한다.
            for (int looping = 0; looping < 2; ++looping)
            {
                // 바닥에는 4장씩 분배한다.
                for (int i = 0; i < 4; ++i)
                {
                    Card card = floor_cards.Dequeue();
                    Card card_picture = this.deck_cards.Pop();
                    card_picture = card;
                    begin_cards_picture.Add(card_picture);
                    begin_cards.Add(card);
                }

                // 플레어이의 카드를 분배한다.
                foreach (KeyValuePair<byte, Queue<Card>> kvp in player_cards)
                {
                    byte player_index = kvp.Key;
                    Queue<Card> cards = kvp.Value;

                    // 플레이어에게는 한번에 5장씩 분배한다.
                    for (int card_index = 0; card_index < 5; ++card_index)
                    {
                        Card card_picture = this.deck_cards.Pop();

                        // 본인 카드는 해당 이미지를 보여주고,
                        // 상대방 카드(is_nullcard)는 back_image로 처리한다.
                        if (player_index == this.player_me_index)
                        {
                            Card card = cards.Dequeue();
                            card_picture = card;
                            autoBrain.slotManager.SetMyCardToHand(card);

                            this.player_hand_card_manager[player_index].add(card_picture);
                        }
                        else
                        {
                            this.player_hand_card_manager[player_index].add(card_picture);
                        }
                    }
                }
            }
            sort_floor_cards_after_distributed(begin_cards_picture);

            if (!host)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.DISTRIBUTED_ALL_CARDS).ToString();

                Protocol(prt);
            }
        }

        void MoveStartBonusCard(byte f_count, byte player_index)
        {
            for (byte klo = 0; klo < f_count; ++klo)
            {
                ////Debug.Log("k: " + klo);
                ////Debug.Log("begin_cards_nuber:  " + begin_cards[klo].number);
                FloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_start_bonus_card((byte)begin_cards[klo].number));
                if (slot != null)
                {
                    Card card_pic = slot.find_card(begin_cards[klo]);

                    if (card_pic != null)//보너스 카드
                    {
                        this.player_card_manager[player_index].add(card_pic);
                        begin_cards_picture.Remove(card_pic);
                        slot.start_remove_card(card_pic);
                        klo--;

                        FloorSlotList floorSlot = this.autoBrain.slotManager.floorSlotLists.Find(obj => obj.card_number == 12);
                        if (floorSlot != null)
                        {
                            floorSlot.remove_card(card_pic);
                        }
                        if (player_index == player_me_index)
                        {
                            this.autoBrain.slotManager.SetMyCardToFloor(card_pic);
                        }
                        else
                        {
                            this.autoBrain.slotManager.SetOtherCardToFloor(card_pic);
                        }
                    }
                    else
                    {
                        if (d)
                        {
                            TextLog.Log("MoveStartBonusCard card_pic null");
                        }
                        Console.WriteLine("MoveStartBonusCard card_pic null");
                    }
                }
            }
        }

        void FlipDeckCard(byte player_index, byte bonusCount, byte card_number, PAE_TYPE card_pae_type, byte card_position, byte card_slot_index)
        {
            byte card_number_msg = card_number;
            PAE_TYPE card_pae_type_msg = card_pae_type;
            byte card_position_msg = card_position;
            byte card_slot_index_msg = card_slot_index;

            if (card_number_msg == 12)
            {
                move_flip_start_event_card_on_player(player_index, card_number_msg, card_pae_type_msg, card_position_msg);
            }
            else
            {
                move_flip_start_event_card(card_number_msg, card_pae_type_msg, card_position_msg);
            }
        }

        void Floor(byte player_index)
        {
            if (host)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.BONUS_START).ToString();

                Protocol(prt);
            }
        }

        void sort_floor_cards_after_distributed(List<Card> begin_cards_picture)
        {
            Dictionary<byte, byte> slots = new Dictionary<byte, byte>();

            for (byte i = 0; i < begin_cards_picture.Count; ++i)
            {
                byte number = begin_cards_picture[i].number;
                FloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(number));
                if (slot == null)
                {
                    slot = this.floor_ui_slots[i];
                    slot.add_card(begin_cards_picture[i]);
                }
                else
                {
                    slot.add_card(begin_cards_picture[i]);

                    if (slot.get_card_count() == 4 && number != 12)
                    {
                        reGame = true;
                    }
                }

                FloorSlotList slotList = this.autoBrain.slotManager.floorSlotLists.Find(obj => obj.is_same_card(number));
                if (slotList == null)
                {
                    this.autoBrain.slotManager.floorSlotLists[i].add_card(begin_cards_picture[i]);
                }
                else
                {
                    slotList.add_card(begin_cards_picture[i]);
                }
            }
        }

        void OnGameOver(byte winner, byte winner_score, byte winner_go, byte winner_shaking, byte winner_kwang, byte winner_pee, byte winner_yeol, byte losser_go, byte losser_kwang, byte losser_pee, byte losser_yeol, byte winner_start_ppuck, byte losser_start_ppuck)//게임 데이터 받아와서 점수 계산
        {
            bool kwang = false;
            bool pee = false;
            bool yeol = false;
            bool go_back = false;

            if (winner == player_me_index)//이긴 사람이 나일 떄 : 이긴 경우
            {
                byte is_win = 1;

                int score = winner_score;

                int double_value = 0;

                for (int i = 0; i < winner_go; i++)
                {
                    score++;
                    if (i >= 2)//3고 부터는 2배곱 적용
                    {
                        double_value++;
                    }
                }
                if (winner_kwang >= 3 && losser_kwang == 0)
                {
                    kwang = true;
                    double_value++;
                }
                if (winner_pee >= 10 && losser_pee < 7 && losser_pee > 0)
                {
                    pee = true;
                    double_value++;
                }
                if (winner_yeol >= 5 && losser_yeol == 0)//2021-01-22 14:49 승자의 열끗이 5장 이상이고 상대방의 열끗이 0장일 때 멍박
                {
                    yeol = true;
                    double_value++;
                }
                for (int i = 0; i < winner_shaking; i++)
                {
                    double_value++;
                }
                if (losser_go >= 1)
                {
                    go_back = true;
                    double_value++;
                }

                int final_score = score;

                if (double_value >= 0)
                {
                    final_score = score * (int)Math.Pow(2, double_value);
                }

                if (winner_start_ppuck == 1)//2021-01-23 16:17 승자가 첫 뻑인 경우 추가되는 점수에서 30점 추가/ 패배자가 첫 뻑인 경우 추가되는 점수에서 30점 다운
                {
                    final_score += 30;
                }

                if (losser_start_ppuck == 1)
                {
                    final_score -= 30;
                }

            }
            else
            {
                byte is_win = 0;

                int score = winner_score;

                int double_value = 0;

                for (int i = 0; i < winner_go; i++)
                {
                    score++;
                    if (i >= 2)
                    {
                        double_value++;
                    }
                }
                if (winner_kwang >= 3 && losser_kwang == 0)
                {
                    kwang = true;
                    double_value++;
                }
                if (winner_pee >= 10 && losser_pee < 7 && losser_pee > 0)
                {
                    pee = true;
                    double_value++;
                }
                if (winner_yeol >= 5 && losser_yeol == 0)//2021-01-22 14:53 승자의 열끗이 5장 이상이고 상대방의 열끗이 0장일 때 멍박
                {
                    yeol = true;
                    double_value++;
                }
                for (int i = 0; i < winner_shaking; i++)
                {
                    double_value++;
                }
                if (losser_go >= 1)
                {
                    go_back = true;
                    double_value++;
                }

                int final_score = score;

                if (double_value >= 0)
                {
                    final_score = score * (int)Math.Pow(2, double_value);
                }

                if (winner_start_ppuck == 1)//2021-01-23 16:16 승자가 첫 뻑인 경우 깍이는 점수에서 30점 추가/ 패배자가 첫 뻑인 경우 깍이는 점수에서 30점 다운
                {
                    final_score += 30;
                }

                if (losser_start_ppuck == 1)
                {
                    final_score -= 30;
                }
            }
           
            System.String prt = "";

            prt += 200;

            Protocol(prt);
        }

        void PuckGameOver(byte winner, byte winner_score)//3퍽으로 게임이 끝나면 따로 전용 함수를 호출
        {
            byte is_win = 0;
            int final_score = 0;

            if (winner == player_me_index)
            {
                is_win = 0;
            }
            else
            {
                is_win = 1;
            }

            System.String prt = "";

            prt += 200;

            Protocol(prt);
        }

        void Nagari()
        {
            System.String prt = "";

            prt += 200;

            Protocol(prt);
        }

        void move_kookjin_to_pee(byte player_index)
        {
            Card card_picture = this.player_card_manager[player_index].get_card(8, PAE_TYPE.YEOL, 0);

            if (player_index == player_me_index)
            {
                this.autoBrain.slotManager.RemoveMyCardToFloor(card_picture);
            }
            else
            {
                this.autoBrain.slotManager.RemoveOtherCardToFloor(card_picture);
            }

            // 열끗에서 지우고 피로 넣는다.
            this.player_card_manager[player_index].remove(card_picture);

            card_picture.change_pae_type(PAE_TYPE.PEE);
            card_picture.set_card_status(CARD_STATUS.TWO_PEE);

            if (player_index == player_me_index)
            {
                this.autoBrain.slotManager.SetMyCardToFloor(card_picture);
            }
            else
            {
                this.autoBrain.slotManager.SetOtherCardToFloor(card_picture);
            }

            this.player_card_manager[player_index].add(card_picture);
        }

        void update_player_statistics(List<string> msg)
        {
            if (msg.Count != 10)
            {
                Console.WriteLine("비정상 플레이어 스탯 " + msg.Count);
                if (msg.Count >= 10)
                {
                    msg = msg.GetRange(msg.Count - 10, 10);
                }
                else
                {
                    return;
                }
            }

            byte host_score = (byte)Int32.Parse(PopAt(msg));
            byte host_go_count = (byte)Int32.Parse(PopAt(msg));
            byte host_shaking_count = (byte)Int32.Parse(PopAt(msg));
            byte host_ppuk_count = (byte)Int32.Parse(PopAt(msg));
            byte host_pee_count = (byte)Int32.Parse(PopAt(msg));

            byte guest_score = (byte)Int32.Parse(PopAt(msg));
            byte guest_go_count = (byte)Int32.Parse(PopAt(msg));
            byte guest_shaking_count = (byte)Int32.Parse(PopAt(msg));
            byte guest_ppuk_count = (byte)Int32.Parse(PopAt(msg));
            byte guest_pee_count = (byte)Int32.Parse(PopAt(msg));
        }

        List<Card> parse_cards_to_get(List<string> msg)
        {
            List<Card> cards_to_give = new List<Card>();
            byte count_to_give = (byte)Int32.Parse(PopAt(msg));
            Console.WriteLine("UI count_to_give " + count_to_give);
            //Console.WriteLine(string.Format("================== count to give. {0}", count_to_give));
            for (int i = 0; i < count_to_give; ++i)
            {
                byte card_number = (byte)Int32.Parse(PopAt(msg));
                PAE_TYPE pae_type = PaeType(PopAt(msg));
                byte position = (byte)Int32.Parse(PopAt(msg));
                Card card = this.card_manager.find_card(card_number, pae_type, position);
                cards_to_give.Add(card);
            }

            return cards_to_give;
        }


        List<Card> parse_cards_to_take_from_others(byte player_index, List<string> msg)
        {
            // 뺏어올 카드.
            List<Card> take_cards_from_others = new List<Card>();
            byte victim_count = (byte)Int32.Parse(PopAt(msg));
            for (byte victim = 0; victim < victim_count; ++victim)
            {
                byte victim_index = (byte)Int32.Parse(PopAt(msg));
                byte count_to_take = (byte)Int32.Parse(PopAt(msg));
                for (byte i = 0; i < count_to_take; ++i)
                {
                    byte card_number = (byte)Int32.Parse(PopAt(msg));
                    PAE_TYPE pae_type = PaeType(PopAt(msg));
                    byte position = (byte)Int32.Parse(PopAt(msg));

                    Card card_pic = this.player_card_manager[victim_index].get_card(
                        card_number, pae_type, position);
                    take_cards_from_others.Add(card_pic);
                    this.player_card_manager[victim_index].remove(card_pic);
                }
            }

            short score = (short)Int32.Parse(PopAt(msg));
            byte remain_bomb_card_count = (byte)Int32.Parse(PopAt(msg));

            return take_cards_from_others;
        }


        void on_turn_result(byte player_index, List<string> msg)
        {
            Console.WriteLine("on_turn_result");

            List<Card> cards_to_give = parse_cards_to_get(msg);
            List<Card> take_cards_from_others = parse_cards_to_take_from_others(player_index, msg);

            move_after_flip_card(player_index, take_cards_from_others, cards_to_give);
        }

        void on_select_card_ack(List<string> msg)
        {
            // 데이터 파싱 시작 ----------------------------------------
            byte player_index = (byte)Int32.Parse(PopAt(msg));

            // 카드 내는 연출을 위해 필요한 변수들.
            List<Card> bomb_cards_info = new List<Card>();
            List<Card> shaking_cards_info = new List<Card>();

            // 플레이어가 낸 카드 정보.
            byte player_card_number = (byte)Int32.Parse(PopAt(msg));
            PAE_TYPE player_card_pae_type = PaeType(PopAt(msg));
            byte player_card_position = (byte)Int32.Parse(PopAt(msg));
            byte same_count_with_player = (byte)Int32.Parse(PopAt(msg));
            byte slot_index = (byte)Int32.Parse(PopAt(msg));
            //Console.WriteLine("on select card ack. " + slot_index);

            CARD_EVENT_TYPE card_event = EventType(PopAt(msg));
            //////Console.WriteLine("-------------------- event " + card_event);
            switch (card_event)
            {
                case CARD_EVENT_TYPE.BOMB:
                    {
                        byte bomb_card_count = (byte)Int32.Parse(PopAt(msg));
                        Console.WriteLine("bomb_card_count " + bomb_card_count);
                        for (byte i = 0; i < bomb_card_count; ++i)
                        {
                            byte number = (byte)Int32.Parse(PopAt(msg));
                            PAE_TYPE pae_type = PaeType(PopAt(msg));
                            byte position = (byte)Int32.Parse(PopAt(msg));
                            Card card = this.card_manager.find_card(number, pae_type, position);
                            bomb_cards_info.Add(card);
                        }
                    }
                    break;

                case CARD_EVENT_TYPE.SHAKING:
                    {
                        byte shaking_card_count = (byte)Int32.Parse(PopAt(msg));
                        for (byte i = 0; i < shaking_card_count; ++i)
                        {
                            byte number = (byte)Int32.Parse(PopAt(msg));
                            PAE_TYPE pae_type = PaeType(PopAt(msg));
                            byte position = (byte)Int32.Parse(PopAt(msg));
                            Card card = this.card_manager.find_card(number, pae_type, position);
                            shaking_cards_info.Add(card);
                        }
                    }
                    break;
            }

            List<Card> autoBrain_choice_card = new List<Card>();
            PLAYER_SELECT_CARD_RESULT select_result = Card_Result(PopAt(msg));
            if (select_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
            {
                byte count = (byte)Int32.Parse(PopAt(msg));
                for (byte i = 0; i < count; ++i)
                {
                    byte number = (byte)Int32.Parse(PopAt(msg));
                    PAE_TYPE pae_type = PaeType(PopAt(msg));
                    byte position = (byte)Int32.Parse(PopAt(msg));

                    Card card = this.card_manager.find_card(number, pae_type, position);
                    autoBrain_choice_card.Add(card);
                }
            }

            move_player_cards_to_floor(player_index, card_event, bomb_cards_info, slot_index, player_card_number, player_card_pae_type, player_card_position);

            if (player_index == this.player_me_index)
            {
                // 바닥에 깔린 카드가 두장일 때 둘중 하나를 선택하는 팝업을 출력한다.
                if (select_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
                {
                    int val = this.autoBrain.ChoiceCard(autoBrain_choice_card[0], autoBrain_choice_card[1]);

                    System.String prt = "";

                    prt += ((short)PROTOCOL.CHOOSE_CARD).ToString();

                    prt += "b" + player_me_index;
                    prt += "b" + select_result;
                    prt += "b" + val;

                    Protocol(prt);
                }
                else
                {
                    // 가운데 카드 뒤집기 요청.
                    System.String prt = "";

                    prt += ((short)PROTOCOL.FLIP_DECK_CARD_REQ).ToString();

                    prt += "b" + this.player_me_index;

                    Protocol(prt);
                }
            }
        }

        public void on_select_bonus_card_ack(byte player_index_msg, byte slot_index, byte card_number, PAE_TYPE card_pae_type, byte card_position)
        {
            byte player_index = player_index_msg;

            Card card = this.card_manager.find_card(card_number, card_pae_type, card_position);

            move_player_bonus_cards_to_player_floor(player_index, slot_index, card_number, card_pae_type, card_position);

            if (player_index == this.player_me_index)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.FLIP_DECK_BONUS_CARD_REQ).ToString();

                Protocol(prt);
            }
        }

        void move_flip_card(byte number, PAE_TYPE pae_type, byte position)
        {
            // 뒤집은 카드 움직이기.
            Card deck_card_picture = this.deck_cards.Pop();
            Card flipped_card = this.card_manager.find_card(number, pae_type, position);
            deck_card_picture = flipped_card;

            move_card_to_floor(deck_card_picture);
        }

        void move_plus_flip_bonus_card(byte pick_number, PAE_TYPE pick_pae_type, byte pick_position, byte number, PAE_TYPE pae_type, byte position)
        {
            Card pick_card = this.card_manager.find_card(pick_number, pick_pae_type, pick_position);

            Card deck_card_picture = this.deck_cards.Pop();
            Card flipped_card = this.card_manager.find_card(number, pae_type, position);
            deck_card_picture = flipped_card;

            move_plus_bonus_card_to_floor(pick_card, deck_card_picture);
        }
        //폭탄카드로 보너스 카드가 나왔을 경우 프로토콜을 받아와 보너스 카드를 움직이는 함수 실행
        void move_bomb_flip_bonus_card(byte player_index, byte number, PAE_TYPE pae_type, byte position)
        {
            Card deck_card_picture = this.deck_cards.Pop();
            Card flipped_card = this.card_manager.find_card(number, pae_type, position);
            deck_card_picture = flipped_card;

            on_flip_bomb_bonus_card_ack_move(player_index, deck_card_picture);
        }

        void move_flip_start_event_card_on_player(byte player_index, byte number, PAE_TYPE pae_type, byte position)
        {
            Card deck_card_picture = this.deck_cards.Pop();
            Card flipped_card = this.card_manager.find_card(number, pae_type, position);
            deck_card_picture = flipped_card;

            if (deck_card_picture == null)
            {
                Console.WriteLine("move_flip_start_event_card_on_player deck_card_picture null");
            }
            this.player_card_manager[player_index].add(deck_card_picture);

            if (player_index == player_me_index)
            {
                this.autoBrain.slotManager.SetMyCardToFloor(deck_card_picture);
            }
            else
            {
                this.autoBrain.slotManager.SetOtherCardToFloor(deck_card_picture);
            }
        }

        void move_flip_start_event_card(byte number, PAE_TYPE pae_type, byte position)
        {
            // 뒤집은 카드 움직이기.
            Card deck_card_picture = this.deck_cards.Pop();
            Card flipped_card = this.card_manager.find_card(number, pae_type, position);
            deck_card_picture = flipped_card;
            begin_cards_picture.Add(deck_card_picture);

            bonus_move_card_to_floor(deck_card_picture);
        }

        void on_flip_deck_card_ack(List<string> msg)
        {
            byte player_index = (byte)Int32.Parse(PopAt(msg));

            // 덱에서 뒤집은 카드 정보.
            byte deck_card_number = (byte)Int32.Parse(PopAt(msg));
            PAE_TYPE deck_card_pae_type = PaeType(PopAt(msg));
            byte deck_card_position = (byte)Int32.Parse(PopAt(msg));
            byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg));

            List<Card> autoBrain_choice_target = new List<Card>();
            PLAYER_SELECT_CARD_RESULT result = Card_Result(PopAt(msg));
            if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK)
            {
                byte count = (byte)Int32.Parse(PopAt(msg));
                for (byte i = 0; i < count; ++i)
                {
                    byte number = (byte)Int32.Parse(PopAt(msg));
                    PAE_TYPE pae_type = PaeType(PopAt(msg));
                    byte position = (byte)Int32.Parse(PopAt(msg));

                    Card card = this.card_manager.find_card(number, pae_type, position);
                    autoBrain_choice_target.Add(card);
                }

                move_flip_card(deck_card_number, deck_card_pae_type, deck_card_position);

                if (player_index == this.player_me_index)
                {
                    int val = this.autoBrain.ChoiceCard(autoBrain_choice_target[0], autoBrain_choice_target[1]);

                    System.String prt = "";

                    prt += ((short)PROTOCOL.CHOOSE_CARD).ToString();

                    prt += "b" + player_me_index;
                    prt += "b" + result;
                    prt += "b" + val;

                    Protocol(prt);
                }
            }
            else
            {
                List<Card> cards_to_give = parse_cards_to_get(msg);
                List<Card> take_cards_from_others = parse_cards_to_take_from_others(player_index, msg);
                List<CARD_EVENT_TYPE> events = parse_flip_card_events(msg);

                // 화면 연출 진행.
                move_flip_card(deck_card_number, deck_card_pae_type, deck_card_position);

                move_after_flip_card(player_index, take_cards_from_others, cards_to_give);
            }
        }

        void on_flip_plus_bonus_card_ack(List<string> msg)
        {
            byte player_index = (byte)Int32.Parse(PopAt(msg));

            //플레이어가 낸 카드 정보
            byte pick_card_number = (byte)Int32.Parse(PopAt(msg));
            PAE_TYPE pick_card_pae_type = PaeType(PopAt(msg));
            byte pick_card_position = (byte)Int32.Parse(PopAt(msg));
            byte pick_card_same_count_with_deck = (byte)Int32.Parse(PopAt(msg));
            // 덱에서 뒤집은 카드 정보. 
            byte flip_number = (byte)Int32.Parse(PopAt(msg));
            PAE_TYPE flip_pae_type = PaeType(PopAt(msg));
            byte flip_position = (byte)Int32.Parse(PopAt(msg));
            byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg));

            // 화면 연출 진행.
            move_plus_flip_bonus_card(pick_card_number, pick_card_pae_type, pick_card_position, flip_number, flip_pae_type, flip_position);

            if (player_index == this.player_me_index)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.FLIP_DECK_CARD_REQ).ToString();

                prt += "b" + this.player_me_index;

                Protocol(prt);
            }
        }

        void on_flip_bomb_bonus_card_ack_move(byte player_index, Card card_picture)//폭탄으로 덱을 뒤집었을 떄 보너스 카드가 나오면 실행
        {
            if (card_picture == null)
            {
                Console.WriteLine("on_flip_bomb_bonus_card_ack_move card_picture null");
            }

            this.player_card_manager[player_index].add(card_picture);

            if (player_index == player_me_index)
            {
                this.autoBrain.slotManager.SetMyCardToFloor(card_picture);
            }
            else
            {
                this.autoBrain.slotManager.SetOtherCardToFloor(card_picture);
            }

            if (player_index == this.player_me_index)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.FLIP_BOMB_BONUS_CARD_REQ).ToString();

                prt += "b" + this.player_me_index;

                Protocol(prt);
            }
        }

        void on_flip_deck_bonus_card_ack(Dictionary<byte, Queue<Card>> player_cards)
        {
            foreach (KeyValuePair<byte, Queue<Card>> kvp in player_cards)
            {
                byte player_index = kvp.Key;
                Queue<Card> cards = kvp.Value;

                Card card_picture = this.deck_cards.Pop();

                // 본인 카드는 해당 이미지를 보여주고,
                // 상대방 카드(is_nullcard)는 back_image로 처리한다.
                if (player_index == this.player_me_index)
                {
                    Card card = cards.Dequeue();
                    card_picture = card;
                    this.player_hand_card_manager[player_index].add(card_picture);
                    this.autoBrain.slotManager.SetMyCardToHand(card);
                }
                else
                {
                    this.player_hand_card_manager[player_index].add(card_picture);
                }
            }

            sort_player_hand_slots(this.player_me_index);

            if (host)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.BONUS_TURN).ToString();

                Protocol(prt);
            }
        }

        List<CARD_EVENT_TYPE> parse_flip_card_events(List<string> msg)
        {
            List<CARD_EVENT_TYPE> events = new List<CARD_EVENT_TYPE>();
            byte count = (byte)Int32.Parse(PopAt(msg));
            for (byte i = 0; i < count; ++i)
            {
                CARD_EVENT_TYPE type = EventType(PopAt(msg));
                events.Add(type);
            }

            return events;
        }

        //턴이 끝날 때 최종적으로 플레이어가 먹을 카드만 이동
        async void move_after_flip_card(byte player_index,
            List<Card> take_cards_from_others,
            List<Card> cards_to_give)
        {
            Console.WriteLine("move_after_flip_card");

            // 상대방에게 뺏어올 카드 움직이기.
            for (int i = 0; i < take_cards_from_others.Count; ++i)
            {
                if (take_cards_from_others[i] == null)
                {
                    Console.WriteLine("move_after_flip_card take_cards_from_others null");
                }
                this.player_card_manager[player_index].add(take_cards_from_others[i]);

                if (player_index == this.player_me_index)
                {
                    autoBrain.slotManager.SetMyCardToFloor(take_cards_from_others[i]);
                    autoBrain.slotManager.RemoveOtherCardToFloor(take_cards_from_others[i]);
                }
                else
                {
                    autoBrain.slotManager.SetOtherCardToFloor(take_cards_from_others[i]);
                    autoBrain.slotManager.RemoveMyCardToFloor(take_cards_from_others[i]);
                }
            }
            //플레이어가 플로어에서 먹을 카드 움직이기
            for (int i = 0; i < cards_to_give.Count; ++i)
            {
                FloorSlot slot;
                Card card_pic;

                ////Console.WriteLine("find start slot");
                if (cards_to_give[i].number == 12)
                {
                    card_pic = this.bonus_ui_slot.find_bonus_card(cards_to_give[i]);

                    if (card_pic == null)
                    {
                        Console.WriteLine(string.Format("Cannot find the card. {0}, {1}, {2}", cards_to_give[i].number, cards_to_give[i].pae_type, cards_to_give[i].position));
                    }

                    this.bonus_ui_slot.remove_card(card_pic);
                }
                else
                {
                    slot = this.floor_ui_slots.Find(obj => obj.is_same_card(cards_to_give[i].number));
                    //////Console.WriteLine(slot + " " + cards_to_give[i].number + cards_to_give[i].pae_type + cards_to_give[i].position);
                    if (slot == null)
                    {
                        Console.WriteLine(string.Format("cannot find the card. {0}, {1}, {2}", cards_to_give[i].number, cards_to_give[i].pae_type, cards_to_give[i].position));
                    }

                    card_pic = slot.find_card(cards_to_give[i]);

                    if (card_pic == null)
                    {
                        Console.WriteLine(string.Format("Cannot find the card. {0}, {1}, {2}", cards_to_give[i].number, cards_to_give[i].pae_type, cards_to_give[i].position));
                    }

                    slot.remove_card(card_pic);

                    FloorSlotList floorSlot = this.autoBrain.slotManager.floorSlotLists.Find(obj => obj.is_same_card(cards_to_give[i].number));
                    if (floorSlot != null)
                    {
                        floorSlot.remove_card(card_pic);
                    }
                }

                if (this.player_me_index == player_index)
                {
                    autoBrain.slotManager.SetMyCardToFloor(card_pic);
                }
                else
                {
                    autoBrain.slotManager.SetOtherCardToFloor(card_pic);
                }

                if (card_pic == null)
                {
                    Console.WriteLine("move_after_flip_card card_pic null");
                }

                this.player_card_manager[player_index].add(card_pic);
            }

            await Task.Delay(300);

            if (player_index == player_me_index)
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.TURN_END).ToString();

                Protocol(prt);
            }
        }

        /// <summary>
        /// 플레이어가 선택한 카드를 바닥에 내는 장면 구현.
        /// 폭탄 이벤트가 존재할 경우 같은 번호의 카드 세장을 한꺼번에 내도록 구현한다.
        /// </summary>
        /// <param name="player_index"></param>
        /// <param name="event_type"></param>
        /// <param name="slot_index"></param>
        /// <param name="player_card_number"></param>
        /// <param name="player_card_pae_type"></param>
        /// <param name="player_card_position"></param>
        /// <returns></returns>
        void move_player_cards_to_floor(
            byte player_index,
            CARD_EVENT_TYPE event_type,
            List<Card> bomb_cards_info,
            byte slot_index,
            byte player_card_number,
            PAE_TYPE player_card_pae_type,
            byte player_card_position)
        {
            float card_moving_delay = 0.2f;

            List<Card> targets = new List<Card>();
            if (event_type == CARD_EVENT_TYPE.BOMB)
            {
                card_moving_delay = 0.1f;

                // 폭탄인 경우에는 폭탄 카드 수 만큼 낸다.
                if (this.player_me_index == player_index)
                {
                    for (int i = 0; i < bomb_cards_info.Count; ++i)
                    {
                        Card card_picture = this.player_hand_card_manager[player_index].find_card(
                            bomb_cards_info[i].number, bomb_cards_info[i].pae_type, bomb_cards_info[i].position);
                        targets.Add(card_picture);
                    }
                }
                else
                {
                    Console.WriteLine("bomb_cards_info " + bomb_cards_info.Count);
                    for (int i = 0; i < bomb_cards_info.Count; ++i)
                    {
                        Card card_picture = this.player_hand_card_manager[player_index].get_card(i);
                        Card card = this.card_manager.find_card(bomb_cards_info[i].number,
                            bomb_cards_info[i].pae_type, bomb_cards_info[i].position);
                        card_picture = card;
                        targets.Add(card_picture);
                    }
                }
            }
            else
            {
                // 폭탄이 아닌 경우에는 한장의 카드만 낸다.
                Card card_picture = this.player_hand_card_manager[player_index].get_card(slot_index);

                if (this.player_me_index != player_index)
                {
                    Card card = this.card_manager.find_card(player_card_number, player_card_pae_type, player_card_position);
                    card_picture = card;
                }

                targets.Add(card_picture);
            }

            // 카드 움직이기.
            for (int i = 0; i < targets.Count; ++i)
            {
                // 손에 들고 있는 패에서 제거한다.
                Card player_card = targets[i];
                this.player_hand_card_manager[player_index].remove(player_card);

                if (player_index == player_me_index)
                {
                    this.autoBrain.slotManager.RemoveMyCardToHand(player_card);
                }

                move_card_to_floor(player_card);
            }
        }

        void move_player_bonus_cards_to_player_floor(byte player_index, byte slot_index, byte player_card_number, PAE_TYPE player_card_pae_type, byte player_card_position)
        {
            float card_moving_delay = 0.2f;

            List<Card> targets = new List<Card>();

            Card card_picture = this.player_hand_card_manager[player_index].get_card(slot_index);

            if (this.player_me_index != player_index)
            {
                Card card = this.card_manager.find_card(player_card_number,
                    player_card_pae_type, player_card_position);
                card_picture = card;
            }

            targets.Add(card_picture);

            // 카드 움직이기.
            for (int i = 0; i < targets.Count; ++i)
            {
                // 손에 들고 있는 패에서 제거한다.
                Card player_card = targets[i];
                this.player_hand_card_manager[player_index].remove(player_card);
                if (player_me_index == player_index)
                {
                    this.autoBrain.slotManager.RemoveMyCardToHand(player_card);
                }

                // 이동 장면.
                if (player_card == null)
                {
                    Console.WriteLine("move_after_flip_card player_card null");
                }

                this.player_card_manager[player_index].add(player_card);

                if (player_index == player_me_index)
                {
                    this.autoBrain.slotManager.SetMyCardToFloor(player_card);
                }
                else
                {
                    this.autoBrain.slotManager.SetOtherCardToFloor(player_card);
                }
            }
            //sort_player_hand_slots(this.player_me_index);
        }


        void move_card_to_floor(Card card_picture)
        {
            byte slot_index = 0;

            FloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(card_picture.number));

            if (slot == null)
            {
                byte empty_slot = find_empty_floorslot();
                slot_index = empty_slot;
            }
            else
            {
                slot_index = slot.ui_slot_position;
            }

            // 바닥 카드로 등록.
            this.floor_ui_slots[slot_index].add_card(card_picture);
            this.autoBrain.slotManager.floorSlotLists[slot_index].add_card(card_picture);
        }

        void bonus_move_card_to_floor(Card card_picture)
        {
            byte slot_index = 0;

            FloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(card_picture.number));

            if (slot == null)
            {
                byte empty_slot = find_empty_floorslot();
                slot_index = empty_slot;
                slot = this.floor_ui_slots[slot_index];
                slot.add_card(card_picture);
            }
            else
            {
                slot.add_card(card_picture);

                if (slot.get_card_count() == 4)
                {
                    reGame = true;
                }
            }

            this.autoBrain.slotManager.floorSlotLists[slot_index].add_card(card_picture);
        }

        //뒤집어진 카드가 보너스 카드일 때 플레이어가 낸 카드에 곂쳐지게 이동
        void move_plus_bonus_card_to_floor(Card player_card_picture, Card card_picture)
        {
            byte slot_index = 0;

            FloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(player_card_picture.number));

            slot_index = slot.ui_slot_position;

            this.bonus_ui_slot.add_bonus_card(card_picture);
        }

        byte find_empty_floorslot()
        {
            FloorSlot slot = this.floor_ui_slots.Find(obj => obj.get_card_count() == 0);
            if (slot == null)
            {
                return byte.MaxValue;
            }

            return slot.ui_slot_position;
        }

        /// <summary>
        /// 플레이어의 패를 번호 순서에 따라 오름차순 정렬 한다.
        /// </summary>
        /// <param name="player_index"></param>
        void sort_player_hand_slots(byte player_index)
        {
            this.player_hand_card_manager[player_index].sort_by_number();
        }

        void AI_Auto_CardPick()
        {
            this.autoBrain.CheckGame();//2021-05-31 11:13 로직에 필요한 변수의 값들을 미리 지정
            this.autoBrain.SaveGameScore();

            Console.WriteLine("UI_player_hand_card " + this.player_hand_card_manager[this.player_me_index].get_card_count());
            Card card = autoBrain.SelectHandCard(Bomb_card);
            if (card != null)
            {
                int slot_index = this.player_hand_card_manager[this.player_me_index].get_index(card);

                int same_on_hand = this.player_hand_card_manager[this.player_me_index].get_same_number_count(card.number).Count;
                int same_on_floor = get_same_number_count_on_floor(card.number);

                if (card.number == 12)
                {
                    send_select_card(card, (byte)slot_index, 0);
                }
                else
                {
                    if (same_on_hand == 3 && same_on_floor == 0)
                    {
                        send_select_card(card, (byte)slot_index, 1);
                    }
                    else
                    {
                        send_select_card(card, (byte)slot_index, 0);
                    }
                }

                System.String prt = "";

                prt += ((short)PROTOCOL.SELECT_CARD_REQ).ToString();

                prt += "b" + player_me_index;

                prt += "b" + card.number;
                prt += "b" + card.pae_type;
                prt += "b" + card.position;
                prt += "b" + slot_index;
                prt += "b" + 0;

                Protocol(prt);
            }
            else
            {
                System.String prt = "";

                prt += ((short)PROTOCOL.FLIP_BOMB_CARD_REQ).ToString();

                prt += "b" + this.player_me_index;

                Protocol(prt);
            }
        }

        public void send_select_card(Card card, byte slot, byte is_shaking)
        {
            System.String prt = "";

            prt += ((short)PROTOCOL.SELECT_CARD_REQ).ToString();

            prt += "b" + player_me_index;

            prt += "b" + card.number;
            prt += "b" + card.pae_type;
            prt += "b" + card.position;
            prt += "b" + slot;
            prt += "b" + is_shaking;

            Protocol(prt);
        }

        int get_same_number_count_on_floor(byte number)
        {
            List<FloorSlot> slots =
                this.floor_ui_slots.FindAll(obj => obj.is_same_card(number));
            return slots.Count;
        }

        public PAE_TYPE PaeType(string pea_type)
        {
            switch (pea_type)
            {
                case "KWANG":
                    {
                        return (PAE_TYPE)Enum.Parse(typeof(PAE_TYPE), "KWANG");
                    }

                case "TEE":
                    {
                        return (PAE_TYPE)Enum.Parse(typeof(PAE_TYPE), "TEE");
                    }

                case "YEOL":
                    {
                        return (PAE_TYPE)Enum.Parse(typeof(PAE_TYPE), "YEOL");
                    }

                case "PEE":
                    {
                        return (PAE_TYPE)Enum.Parse(typeof(PAE_TYPE), "PEE");
                    }
            }
            return 0;
        }

        public CARD_EVENT_TYPE EventType(string event_type)
        {
            switch (event_type)
            {
                case "NONE":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "NONE");
                    }

                case "KISS":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "KISS");
                    }

                case "PPUK":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "PPUK");
                    }

                case "DDADAK":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "DDADAK");
                    }

                case "BOMB":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "BOMB");
                    }

                case "CLEAN":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "CLEAN");
                    }

                case "EAT_PPUK":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "EAT_PPUK");
                    }

                case "SELF_EAT_PPUK":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "SELF_EAT_PPUK");
                    }

                case "START_PPUK":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "START_PPUK");
                    }

                case "SHAKING":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "SHAKING");
                    }

                case "BONUSCARD":
                    {
                        return (CARD_EVENT_TYPE)Enum.Parse(typeof(CARD_EVENT_TYPE), "BONUSCARD");
                    }
            }
            return 0;
        }

        public PLAYER_SELECT_CARD_RESULT Card_Result(string card_result)
        {
            switch (card_result)
            {
                case "COMPLETED":
                    {
                        return (PLAYER_SELECT_CARD_RESULT)Enum.Parse(typeof(PLAYER_SELECT_CARD_RESULT), "COMPLETED");
                    }

                case "CHOICE_ONE_CARD_FROM_PLAYER":
                    {
                        return (PLAYER_SELECT_CARD_RESULT)Enum.Parse(typeof(PLAYER_SELECT_CARD_RESULT), "CHOICE_ONE_CARD_FROM_PLAYER");
                    }

                case "CHOICE_ONE_CARD_FROM_DECK":
                    {
                        return (PLAYER_SELECT_CARD_RESULT)Enum.Parse(typeof(PLAYER_SELECT_CARD_RESULT), "CHOICE_ONE_CARD_FROM_DECK");
                    }

                case "BONUSCARD":
                    {
                        return (PLAYER_SELECT_CARD_RESULT)Enum.Parse(typeof(PLAYER_SELECT_CARD_RESULT), "BONUSCARD");
                    }

                case "ERROR_INVALID_CARD":
                    {
                        return (PLAYER_SELECT_CARD_RESULT)Enum.Parse(typeof(PLAYER_SELECT_CARD_RESULT), "ERROR_INVALID_CARD");
                    }
            }
            return 0;
        }

        public string PopAt(List<string> list)
        {
            string r = list[0];
            list.RemoveAt(0);
            return r;
        }
    }
}
