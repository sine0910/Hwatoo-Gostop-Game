using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGameUI : CSingletonMonobehaviour<AIGameUI>
{
    AISendManager sendManager;
    Queue<List<string>> waiting_packets;

    Sprite[] hwatoo_sprites;
    Sprite back_image;

    [SerializeField]
    Transform deck_slot;
    [SerializeField]
    Transform floor_slot_root;
    [SerializeField]
    List<Vector3> floor_slot_position;

    List<PlayerCardPosition> player_card_positions;

    List<CardPicture> total_card_pictures;

    CardCollision card_collision_manager;

    CardManager card_manager;

    byte player_me_index;

    byte ui_slot_index;

    //바닥의 카드를 관리하는 슬롯 12개를 보관하는 리스트
    List<VisualFloorSlot> floor_ui_slots;
    //덱의 카드를 뽑아쓰기 위한 스택
    Stack<CardPicture> deck_cards;
    //플레이어들이 들고있는 카드를 관리 및 보관
    List<HandCardManager> hand_card_managers;
    //플레이어들이 먹은 카드를 관리 및 보관
    List<PlayerCardManager> player_card_manager;
    //플레이어의 정보를 보여주기위한 슬롯
    List<PlayerInfoSlot> player_info_slots;

    Queue<Card> floor_cards = new Queue<Card>();

    List<CardPicture> begin_cards_picture = new List<CardPicture>();

    public bool test_mode = false;

    readonly Vector3 SCALE_TO_FLOOR = new Vector3(1.1f, 1.1f, 1f);
    readonly Vector3 SCALE_TO_OTHER_HAND = new Vector3(0.9f, 0.9f, 1f);
    readonly Vector3 SCALE_TO_MY_HAND = new Vector3(1.75f, 1.75f, 1f);

    readonly Vector3 SCALE_TO_OTHER_FLOOR = new Vector3(0.55f, 0.55f, 1);
    readonly Vector3 SCALE_TO_MY_FLOOR = new Vector3(0.55f, 0.55f, 1);

    GameObject get_other_pee_count;
    public GameObject get_other_pee_panel;

    public GameObject my_turn;
    public GameObject other_turn;

    public IEnumerator auto_select;

    private void Awake()
    {
        this.sendManager = GameObject.Find("AIGameManager").GetComponent<AISendManager>();
        this.waiting_packets = new Queue<List<string>>();
        this.card_collision_manager = GameObject.Find("AIGameManager").GetComponent<CardCollision>();
        this.card_collision_manager.callback_on_touch = this.on_card_touch;

        this.player_me_index = 0;
        this.ui_slot_index = 0;

        this.deck_cards = new Stack<CardPicture>();
        this.card_manager = new CardManager();

        this.floor_ui_slots = new List<VisualFloorSlot>();
        for (byte i = 0; i < 12; i++)
        {
            this.floor_ui_slots.Add(new VisualFloorSlot(i, byte.MaxValue));
        }

        this.total_card_pictures = new List<CardPicture>();

        this.hand_card_managers = new List<HandCardManager>();
        this.hand_card_managers.Add(new HandCardManager());
        this.hand_card_managers.Add(new HandCardManager());

        this.player_card_manager = new List<PlayerCardManager>();
        this.player_card_manager.Add(new PlayerCardManager());
        this.player_card_manager.Add(new PlayerCardManager());

        this.player_card_positions = new List<PlayerCardPosition>();
        this.player_info_slots = new List<PlayerInfoSlot>();

        this.hwatoo_sprites = Resources.LoadAll<Sprite>("Image/allcard");
        this.back_image = Resources.Load<Sprite>("Image/back");

        this.floor_slot_position = new List<Vector3>();
        make_slot_positions(this.floor_slot_root, this.floor_slot_position);

        get_other_pee_count = Resources.Load("Prefab/GetOtherPeeText") as GameObject;

        load_hint_arrows();
    }

    void make_slot_positions(Transform root, List<Vector3> targets)
    {
        Transform[] slots = root.GetComponentsInChildren<Transform>();
        for (int i = 0; i < slots.Length; ++i)
        {
            if (slots[i] == root)
            {
                continue;
            }
            targets.Add(slots[i].position);
        }
    }

    void Start()
    {
        StartCoroutine(PaketHandler());

        sendManager.on_start(this);
    }

    public void on_recive(List<string> msg)
    {
        List<string> clone = msg.ToList();

        waiting_packets.Enqueue(clone);
    }

    IEnumerator PaketHandler()
    {
        while (true)
        {
            if (waiting_packets.Count <= 0)
            {
                yield return 0;
                continue;
            }

            List<string> msg_list = waiting_packets.Dequeue();
            int protocol = Int32.Parse(PopAt(msg_list));

            switch (protocol)
            {
                case 2:
                    {
                        Debug.Log("LOCAL_SERVER_START");

                        this.player_me_index = (byte)Int32.Parse(PopAt(msg_list));

                        setting();

                        List<string> msg = new List<string>();
                        msg.Add((short)PROTOCOL.READY_TO_START + "");
                        AISendManager.send_from_player(msg);
                    }
                    break;

                case 11:
                    {
                        Debug.Log("BEGIN_CARD_INFO");

                        reset();

                        byte floor_count = (byte)Int32.Parse(PopAt(msg_list));

                        for (byte i = 0; i < floor_count; ++i)
                        {
                            byte number = (byte)Int32.Parse(PopAt(msg_list));
                            PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                            byte position = (byte)Int32.Parse(PopAt(msg_list));

                            Card card = this.card_manager.find_card(number, pae_type, position);
                            Debug.Log("floor card : " + card + "number: " + number + "pae_type: " + pae_type + "position: " + position);
                            floor_cards.Enqueue(card);
                        }

                        Dictionary<byte, Queue<Card>> player_cards = new Dictionary<byte, Queue<Card>>();
                        byte player_count = (byte)Int32.Parse(PopAt(msg_list));
                        for (byte player = 0; player < player_count; ++player)
                        {
                            Queue<Card> cards = new Queue<Card>();
                            byte player_index = (byte)Int32.Parse(PopAt(msg_list));
                            byte card_count = (byte)Int32.Parse(PopAt(msg_list));

                            for (byte i = 0; i < card_count; ++i)
                            {
                                byte number = (byte)Int32.Parse(PopAt(msg_list));
                                if (number != byte.MaxValue)
                                {
                                    PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                                    byte position = (byte)Int32.Parse(PopAt(msg_list));
                                    Card card = this.card_manager.find_card(number, pae_type, position);
                                    cards.Enqueue(card);
                                }
                            }
                            player_cards.Add(player_index, cards);
                        }
                        yield return StartCoroutine(distribute_cards(player_cards));
                    }
                    break;

                case 33:
                    {
                        Debug.Log("START_BONUSPEE");

                        byte bonus_card_count = (byte)Int32.Parse(PopAt(msg_list));
                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

                        distribute_floor_bounus_card(player_index);

                        for (byte i = 0; i < bonus_card_count; ++i)
                        {
                            byte card_number_msg = (byte)Int32.Parse(PopAt(msg_list));
                            PAE_TYPE card_pae_type = Converter.PaeType(PopAt(msg_list));
                            byte card_position = (byte)Int32.Parse(PopAt(msg_list));

                            yield return StartCoroutine(flip_deck_card(player_index, card_number_msg, card_pae_type, card_position));
                        }

                        sort_floor_cards_when_finished_turn();

                        List<string> msg = new List<string>();
                        msg.Add((short)PROTOCOL.BONUS_START + "");
                        msg.Add(0 + "");
                        AISendManager.send_from_player(msg);
                    }
                    break;

                case 98:
                    {
                        Debug.Log("START_TURN");

                        my_turn.SetActive(true);
                        other_turn.SetActive(false);

                        byte remain_bomb_card_count = 0;
                        if (msg_list.Count > 0)
                        {
                            remain_bomb_card_count = (byte)Int32.Parse(PopAt(msg_list));
                        }

                        if (!test_mode)
                        {
                            //내 차례가 되었을 때 카드 선택 기능을 활성화 시켜준다
                            this.card_collision_manager.enabled = true;
                            this.hand_card_managers[player_me_index].enable_all_colliders(true);

                            //이전에 폭탄낸게 남아있다면 가운데 카드를 뒤집을 수 있도록 충돌박스를 켜준다
                            if (remain_bomb_card_count > 0)
                            {
                                CardPicture top_card = deck_cards.Peek();
                                top_card.enable_collider(true);

                                Debug.Log("top_card: " + top_card.ToString() + "pos: " + top_card.transform.position);
                                show_hint_mark(top_card.transform.position);
                            }
                            //내가 낼 수 있는 카드를 알려주는 힌트마크를 표시한다
                            refresh_hint_mark();
                        }
                        auto_select = auto_card_select(remain_bomb_card_count);
                        StartCoroutine(auto_select);
                    }
                    break;

                case 14:
                    {
                        Debug.Log("SELECT_CARD_ACK");

                        yield return StartCoroutine(on_select_card_ack(msg_list));
                    }
                    break;

                case 44:
                    {
                        Debug.Log("SELECT_BONUS_CARD_ACK");

                        byte current_turn_player_index = (byte)Int32.Parse(PopAt(msg_list));

                        byte slot_index = (byte)Int32.Parse(PopAt(msg_list));
                        byte number = (byte)Int32.Parse(PopAt(msg_list));
                        PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                        byte position = (byte)Int32.Parse(PopAt(msg_list));

                        on_select_bonus_card_ack(current_turn_player_index, slot_index, number, pae_type, position);
                    }
                    break;

                case 19:
                    {
                        Debug.Log("FLIP_DECK_CARD_ACK");

                        yield return StartCoroutine(on_flip_deck_card_ack(msg_list));
                    }
                    break;

                case 46:
                    {
                        Debug.Log("FLIP_DECK_BONUS_CARD_ACK");

                        Dictionary<byte, Card> player_cards = new Dictionary<byte, Card>();

                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

                        byte deck_card_number = (byte)Int32.Parse(PopAt(msg_list));
                        PAE_TYPE deck_card_pae_type = Converter.PaeType(PopAt(msg_list));
                        byte deck_card_position = (byte)Int32.Parse(PopAt(msg_list));

                        Card card = this.card_manager.find_card(deck_card_number, deck_card_pae_type, deck_card_position);

                        player_cards.Add(player_index, card);

                        yield return StartCoroutine(on_flip_deck_bonus_card_ack(player_cards));
                    }
                    break;

                case 55:
                    {
                        Debug.Log("FLIP_PLUS_BONUS_CARD_ACK");

                        yield return StartCoroutine(on_flip_plus_bonus_card_ack(msg_list));
                    }
                    break;

                case 57:
                    {
                        Debug.Log("FLIP_BOMB_BONUS_CARD_ACK");

                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

                        byte deck_card_number = (byte)Int32.Parse(PopAt(msg_list));
                        PAE_TYPE deck_card_pae_type = Converter.PaeType(PopAt(msg_list));
                        byte deck_card_position = (byte)Int32.Parse(PopAt(msg_list));
                        byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg_list));

                        yield return StartCoroutine(move_bomb_flip_bonus_card(player_index, deck_card_number, deck_card_pae_type, deck_card_position));
                    }
                    break;

                case 20:
                    {
                        Debug.Log("TURN_RESULT");

                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));
                        yield return StartCoroutine(on_turn_result(player_index, msg_list));
                    }
                    break;

                case 21:
                    {
                        Debug.Log("ASK_GO_OR_STOP");

                        if (test_mode)
                        {
                            //테스터 모드일 경우 원활한 테스트를 위해 점수가 나면 바로 스톱
                            List<string> msg = new List<string>();
                            msg.Add((short)PROTOCOL.ANSWER_GO_OR_STOP + "");
                            msg.Add(0 + "");
                            AISendManager.send_from_player(msg);
                        }
                        else
                        {
                            PopupManager.instance.show(UI_PAGE.POPUP_GO_STOP);
                            PopupGostop popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GO_STOP).GetComponent<PopupGostop>();
                            popup.on_popup(0, player_me_index);
                        }
                    }
                    break;

                case 23:
                    {
                        Debug.Log("UPDATE_PLAYER_STATISTICS");

                        update_player_statistics(msg_list);
                    }
                    break;

                case 24:
                    {
                        Debug.Log("ASK_KOOKJIN_TO_PEE");

                        if (test_mode)
                        {
                            List<string> msg = new List<string>();
                            msg.Add((short)PROTOCOL.ANSWER_KOOKJIN_TO_PEE + "");
                            msg.Add(0 + "");
                            AISendManager.send_from_player(msg);
                        }
                        else
                        {
                            PopupManager.instance.show(UI_PAGE.POPUP_ASK_KOOKJIN);
                            PopupKookjin popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_ASK_KOOKJIN).GetComponent<PopupKookjin>();
                            popup.on_popup(0, player_me_index);
                        }
                    }
                    break;

                case 26:
                    {
                        Debug.Log("MOVE_KOOKJIN_TO_PEE");

                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));
                        yield return StartCoroutine(move_kookjin_to_pee(player_index));
                    }
                    break;

                case 27:
                    {
                        Debug.Log("GAME_RESULT");

                        byte winner = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_score = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_go = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_ppuck= (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_shaking = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_kwang = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_pee = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_yeol = (byte)Int32.Parse(PopAt(msg_list));
                        byte winner_tee = (byte)Int32.Parse(PopAt(msg_list));

                        byte losser_go = (byte)Int32.Parse(PopAt(msg_list));
                        byte losser_kwang = (byte)Int32.Parse(PopAt(msg_list));
                        byte losser_pee = (byte)Int32.Parse(PopAt(msg_list));
                        byte losser_yeol = (byte)Int32.Parse(PopAt(msg_list));
                        byte losser_tee = (byte)Int32.Parse(PopAt(msg_list));

                        byte nagari = (byte)Int32.Parse(PopAt(msg_list));

                        byte winner_start_ppuck = (byte)Int32.Parse(PopAt(msg_list));
                        byte losser_start_ppuck = (byte)Int32.Parse(PopAt(msg_list));

                        game_over(winner, winner_score, winner_go, winner_ppuck, winner_shaking, winner_kwang, winner_pee, winner_yeol, winner_tee,
                            losser_go, losser_kwang, losser_pee, losser_yeol, losser_tee, winner_start_ppuck, losser_start_ppuck, nagari);
                    }
                    break;

                case 101:
                    {
                        Debug.Log("GO");

                        byte player_index = (byte)Int32.Parse(PopAt(msg_list));
                        byte count = (byte)Int32.Parse(PopAt(msg_list));

                        if (count != 101)
                        {
                            PlayEffectManager.instance.GoEffect(count);
                        }
                        else
                        {
                            PlayEffectManager.instance.StopEffect();
                        }
                    }
                    break;
            }
        }
    }

    void reset()
    {
        RecordManager.instance.record_start();

        this.card_manager.MakeAllCards();//카드를 전부 재생성

        for (int i = 0; i < total_card_pictures.Count; i++)
        {
            Destroy(total_card_pictures[i].gameObject);//생성된 카드를 전부 파괴
        }

        total_card_pictures.Clear();

        GameObject original = Resources.Load("Prefab/hwatoo") as GameObject;
        Vector3 pos = deck_slot.position;
        for (int i = 0; i < this.card_manager.cards.Count; ++i)
        {
            GameObject obj = Instantiate(original);
            obj.transform.parent = transform.Find("card_deck");

            obj.AddComponent<MovingObject>();
            CardPicture card_pic = obj.AddComponent<CardPicture>();
            total_card_pictures.Add(card_pic);
        }

        make_deck_cards();

        ui_slot_index = 0;
        floor_cards.Clear();
        begin_cards_picture.Clear();

        for (int i = 0; i < floor_ui_slots.Count; ++i)
        {
            floor_ui_slots[i].reset();
        }

        for (int i = 0; i < hand_card_managers.Count; ++i)
        {
            hand_card_managers[i].reset();
        }

        for (int i = 0; i < player_card_manager.Count; ++i)
        {
            player_card_manager[i].reset();
        }

        clear_ui();
    }

    void setting()
    {
        if (this.player_me_index == 0)
        {
            this.player_card_positions.Add(transform.Find("player_slot/player_01").gameObject.GetComponent<PlayerCardPosition>());
            this.player_card_positions.Add(transform.Find("player_slot/player_02").gameObject.GetComponent<PlayerCardPosition>());

            this.player_info_slots.Add(transform.Find("player_info/player_01").GetComponent<PlayerInfoSlot>());
            this.player_info_slots.Add(transform.Find("player_info/player_02").GetComponent<PlayerInfoSlot>());
        }
        else
        {
            this.player_card_positions.Add(transform.Find("player_slot/player_02").gameObject.GetComponent<PlayerCardPosition>());
            this.player_card_positions.Add(transform.Find("player_slot/player_01").gameObject.GetComponent<PlayerCardPosition>());

            this.player_info_slots.Add(transform.Find("player_info/player_02").GetComponent<PlayerInfoSlot>());
            this.player_info_slots.Add(transform.Find("player_info/player_01").GetComponent<PlayerInfoSlot>());
        }
    }

    void make_deck_cards()
    {
        SpriteLayerManager.Instance.reset();
        Vector3 pos = deck_slot.position;

        deck_cards.Clear();

        for (int i = 0; i < total_card_pictures.Count; ++i)
        {
            Animator ani = total_card_pictures[i].GetComponentInChildren<Animator>();
            ani.Play("card_idle");

            total_card_pictures[i].update_backcard(back_image);
            total_card_pictures[i].enable_collider(false);
            deck_cards.Push(total_card_pictures[i]);

            total_card_pictures[i].transform.localPosition = pos;
            pos.x -= 0.5f;
            pos.y += 0.5f;

            total_card_pictures[i].transform.localScale = SCALE_TO_FLOOR;
            total_card_pictures[i].transform.rotation = Quaternion.identity;

            total_card_pictures[i].sprite_renderer.sortingOrder = SpriteLayerManager.Instance.Order;
        }
    }

    void clear_ui()
    {
        my_turn.SetActive(false);
        other_turn.SetActive(false);

        for (int i = 0; i < player_info_slots.Count; ++i)
        {
            player_info_slots[i].update_score(0);
            player_info_slots[i].update_go(0);
            player_info_slots[i].update_ppuk(0);
            player_info_slots[i].update_peecount(0);
        }
    }

    void move_card(CardPicture card_picture, Vector3 begin, Vector3 to, float duration = 0.1f)
    {
        if (card_picture.card != null)
        {
            int sprite_index = card_picture.card.number * 4 + card_picture.card.position;
            card_picture.update_image(hwatoo_sprites[sprite_index]);
        }
        else
        {
            card_picture.update_image(back_image);
        }

        MovingObject mover = card_picture.GetComponent<MovingObject>();
        mover.begin = begin;
        mover.to = to;
        mover.duration = duration;
        mover.run();
    }

    //화투 이미지를 카드에 맞게 이미지를 부여해준다.
    Sprite get_hwatoo_sprite(Card card)
    {
        int sprite_index = card.number * 4 + card.position;
        Debug.Log("hwatoo_sprites " + hwatoo_sprites + " index " + sprite_index);
        return hwatoo_sprites[sprite_index];
    }

    IEnumerator distribute_cards(Dictionary<byte, Queue<Card>> player_cards)
    {
        PlaySoundManager.instance.on_dis_cards();
        // [바닥 -> 1P -> 2P 나눠주기] 를 두번 반복한다.
        for (int looping = 0; looping < 2; ++looping)
        {
            // 바닥에는 4장씩 분배한다.
            for (int i = 0; i < 4; ++i)
            {
                Card card = floor_cards.Dequeue();
                CardPicture card_picture = deck_cards.Pop();
                card_picture.update_card(card, get_hwatoo_sprite(card));
                begin_cards_picture.Add(card_picture);

                card_picture.transform.localScale = SCALE_TO_FLOOR;
                move_card(card_picture, card_picture.transform.position, floor_slot_position[i + looping * 4]);

                yield return new WaitForSeconds(0.02f);
            }

            // 플레어이의 카드를 분배한다.
            foreach (KeyValuePair<byte, Queue<Card>> kvp in player_cards)
            {
                byte player_index = kvp.Key;
                Queue<Card> cards = kvp.Value;

                ui_slot_index = (byte)(looping * 5);
                // 플레이어에게는 한번에 5장씩 분배한다.
                for (int card_index = 0; card_index < 5; ++card_index)
                {
                    CardPicture card_picture = deck_cards.Pop();
                    card_picture.set_slot_index(ui_slot_index);
                    hand_card_managers[player_index].add(card_picture);

                    // 본인 카드는 해당 이미지를 보여주고,
                    // 상대방 카드(is_nullcard)는 back_image로 처리한다.
                    if (player_index == player_me_index)
                    {
                        Card card = cards.Dequeue();
                        card_picture.update_card(card, get_hwatoo_sprite(card));
                        card_picture.transform.localScale = SCALE_TO_MY_HAND;
                        move_card(card_picture, card_picture.transform.position,
                        player_card_positions[player_index].get_hand_position(ui_slot_index));
                    }
                    else
                    {
                        card_picture.update_backcard(back_image);
                        card_picture.transform.localScale = SCALE_TO_OTHER_HAND;
                        move_card(card_picture, card_picture.transform.position,
                        player_card_positions[player_index].get_hand_position(ui_slot_index));
                    }

                    ++ui_slot_index;

                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        sort_floor_cards_after_distributed(begin_cards_picture);
        sort_player_hand_slots(player_me_index);

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.DISTRIBUTED_ALL_CARDS + "");
        AISendManager.send_from_player(msg);
    }

#region sort_floor_cards
    //카드를 나눈 뒤 플로어의 카드를 정리하는 함수
    void sort_floor_cards_after_distributed(List<CardPicture> begin_cards_picture)
    {
        Dictionary<byte, byte> slots = new Dictionary<byte, byte>();

        for (byte i = 0; i < begin_cards_picture.Count; ++i)
        {
            byte number = begin_cards_picture[i].card.number;
            VisualFloorSlot slot = floor_ui_slots.Find(obj => obj.is_same_card(number));

            Vector3 to = Vector3.zero;
            if (slot == null)
            {
                to = floor_slot_position[i];

                slot = floor_ui_slots[i];
                slot.add_card(begin_cards_picture[i]);
            }
            else
            {
                to = get_ui_slot_position(slot);

                slot.add_card(begin_cards_picture[i]);
            }

            Vector3 begin = floor_slot_position[i];
            move_card(begin_cards_picture[i], begin, to);
        }
    }

    //턴을 마칠때 플로어의 카드를 정리하는 함수
    void sort_floor_cards_when_finished_turn()
    {
        for (int i = 0; i < floor_ui_slots.Count; ++i)
        {
            VisualFloorSlot slot = floor_ui_slots[i];
            if (slot.get_card_count() != 1)
            {
                continue;
            }

            CardPicture card_pic = slot.get_first_card();
            move_card(card_pic, card_pic.transform.position, floor_slot_position[slot.ui_slot_position]);
        }
    }
#endregion

#region start_event_card
    //플로어에 나와있는 이벤트 카드를 해당 플레이어에게 준다.
    void distribute_floor_bounus_card(byte player_index)
    {
        for (byte k = 0; k < this.begin_cards_picture.Count; k++)
        {
            VisualFloorSlot slot = floor_ui_slots.Find(obj => obj.is_has_bonus_card());
            if (slot != null)
            {
                List<CardPicture> card_pic_list = slot.get_bonus_cards();

                for (int i = 0; i < card_pic_list.Count; i++)
                {
                    CardPicture card_pic = card_pic_list[i];

                    if (player_index == player_me_index)
                    {
                        card_pic.transform.localScale = SCALE_TO_MY_FLOOR;
                    }
                    else
                    {
                        card_pic.transform.localScale = SCALE_TO_OTHER_FLOOR;
                    }
                    move_card(card_pic, card_pic.transform.position, get_player_card_position(player_index, card_pic.card.pae_type));
                    player_card_manager[player_index].add(card_pic);
                    slot.remove_card(card_pic);

                    Animator ani = card_pic.GetComponentInChildren<Animator>();
                    ani.enabled = true;
                    ani.Play("card_idle");
                }
            }
            else
            {
                Debug.Log("distribute_floor_bounus_card slot is null");
            }
        }
    }

    IEnumerator flip_deck_card(byte player_index, byte card_number, PAE_TYPE card_pae_type, byte card_position)
    {
        byte card_number_msg = card_number;
        PAE_TYPE card_pae_type_msg = card_pae_type;
        byte card_position_msg = card_position;

        if (card_number_msg == 12)
        {
            //덱에서 뒤집은 카드가 보너스카드일 경우 해당하는 플레이어에게 보너스카드를 준다.
            yield return StartCoroutine(move_flip_start_event_card_on_player(player_index, card_number_msg, card_pae_type_msg, card_position_msg));
        }
        else
        {
            yield return StartCoroutine(move_flip_start_event_card(card_number_msg, card_pae_type_msg, card_position_msg));
        }
    }

    IEnumerator move_flip_start_event_card_on_player(byte player_index, byte number, PAE_TYPE pae_type, byte position)
    {
        CardPicture deck_card_picture = deck_cards.Pop();
        Card flipped_card = this.card_manager.find_card(number, pae_type, position);
        deck_card_picture.update_card(flipped_card, get_hwatoo_sprite(flipped_card));

        yield return StartCoroutine(flip_deck_card_ani(deck_card_picture));

        yield return new WaitForSeconds(0.25f);

        move_card(deck_card_picture, deck_card_picture.transform.position, get_player_card_position(player_index, deck_card_picture.card.pae_type));
        deck_card_picture.transform.localScale = SCALE_TO_MY_FLOOR;
        player_card_manager[player_index].add(deck_card_picture);
    }

    IEnumerator move_flip_start_event_card(byte number, PAE_TYPE pae_type, byte position)
    {
        CardPicture deck_card_picture = deck_cards.Pop();
        Card flipped_card = this.card_manager.find_card(number, pae_type, position);
        deck_card_picture.update_card(flipped_card, get_hwatoo_sprite(flipped_card));
        begin_cards_picture.Add(deck_card_picture);

        yield return StartCoroutine(flip_deck_card_ani(deck_card_picture));

        yield return new WaitForSeconds(0.25f);

        deck_card_picture.transform.localScale = SCALE_TO_FLOOR;
        move_card_to_floor(deck_card_picture);
    }
#endregion

    //승자는 1,0 무승부일 경우 MaxValue값을 받아온다.
    void game_over(byte winner, byte winner_score, byte winner_go, byte winner_ppuck, byte winner_shaking, byte winner_kwang, byte winner_pee, byte winner_yeol, byte winner_tee,
        byte losser_go, byte losser_kwang, byte losser_pee, byte losser_yeol, byte losser_tee, byte winner_start_ppuck, byte losser_start_ppuck, byte nagari)
    {
        Debug.Log("winner info winner: " + winner + " winner_score:" + winner_score + " winner_go" + winner_go + " winner_ppuck" + winner_ppuck + " winner_shaking" + winner_shaking +
            " winner_kwang" + winner_kwang + " winner_pee" + winner_pee + " winner_yeol" + winner_yeol + " winner_tee" + winner_tee);
        if (winner_ppuck > 2)
        {
            Debug.LogError("winner_three_ppuck!");

            int score = winner_score;

            int double_val = 0;

            for (int i = 0; i < nagari; i++)
            {
                double_val++;
            }

            int final_score = score;

            if (double_val >= 0)
            {
                final_score *= (int)Mathf.Pow(2, double_val);
            }

            if (winner_start_ppuck == 1)
            {
                final_score += 10;
            }
            if (losser_start_ppuck == 1)
            {
                final_score -= 10;
            }

            long money_value = final_score * PlayerSetData.instance.per_point_money;

            bool over_money = false;
            if (money_value > GameDataManger.instance.max_money)
            {
                over_money = true;
                money_value = GameDataManger.instance.max_money;
            }

            string money = MoneyManager.instance.convert_money_to_string(money_value);

            if (winner == player_me_index)
            {
                byte win = 1;

                AIPlayManager.instance.win_count++;

                DataManager.instance.ai_win(final_score, money_value);

                RecordManager.instance.save_game_play_record(win, final_score, double_val, money);

                ProfileManager.instance.end_game(0, 1, money_value);

                PopupManager.instance.show(UI_PAGE.POPUP_GAME_RESULT);
                PopupGameResult popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GAME_RESULT).GetComponent<PopupGameResult>();
                popup.on_result(win, winner_score, 0, 0, false, false, false, false, false, winner_start_ppuck, losser_start_ppuck, nagari, double_val, final_score, money_value, over_money);
            }
            else
            {
                byte win = 0;

                AIPlayManager.instance.lose_count++;

                DataManager.instance.ai_lose(money_value);

                RecordManager.instance.save_game_play_record(win, final_score, double_val, money);

                ProfileManager.instance.end_game(1, 0, money_value);

                PopupManager.instance.show(UI_PAGE.POPUP_GAME_RESULT);
                PopupGameResult popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GAME_RESULT).GetComponent<PopupGameResult>();
                popup.on_result(win, winner_score, 0, 0, false, false, false, false, false, winner_start_ppuck, losser_start_ppuck, nagari, double_val, final_score, money_value, over_money);
            }
        }
        else
        {
            int score = winner_score;

            int double_val = 0;

            bool kwang = false;//광박
            bool yeol = false;//멍박
            bool tee = false;//띠박
            bool pee = false;//피박
            bool go_back = false;//고박

            //흘듦 카운트만큼 2배수를 올려준다.
            for (int i = 0; i < winner_shaking; i++)
            {
                double_val++;
            }

            //상대가 광박일 경우 
            if (winner_kwang >= 3 && losser_kwang == 0)
            {
                kwang = true;
                double_val++;
            }
            //상대가 멍박일 경우 
            if (winner_yeol >= 5 && losser_yeol == 0)
            {
                yeol = true;
                double_val++;
            }
            //상대가 띠박일 경우 
            if (winner_tee >= 5 && losser_tee == 0)
            {
                yeol = true;
                double_val++;
            }
            //상대가 피박일 경우 
            if (winner_pee >= 10 && losser_pee <= 7 && losser_pee > 0)
            {
                pee = true;
                double_val++;
            }
            //상대가 고박일 경우 
            if (losser_go > 0)
            {
                go_back = true;
                double_val++;
            }
            //고 카운트 만큼 점수를 올려주고 3고 이상일 경우 2배수로 올려준다.
            for (int i = 0; i < winner_go; i++)
            {
                score++;
                if (i >= 2)
                {
                    double_val++;
                }
            }

            for (int i = 0; i < nagari; i++)
            {
                double_val++;
            }

            int final_score = score;

            if (double_val >= 0)
            {
                final_score *= (int)Mathf.Pow(2, double_val);
            }

            //첫 뻑은 계산되는 점수에 추가되지 않고 추가점으로 받게된다.
            if (winner_start_ppuck == 1)
            {
                final_score += 10;
            }
            if (losser_start_ppuck == 1)
            {
                final_score -= 10;
            }

            long money_value = final_score * PlayerSetData.instance.per_point_money;

            bool over_money = false;
            if (money_value > GameDataManger.instance.max_money)
            {
                over_money = true;
                money_value = GameDataManger.instance.max_money;
            }

            string money = MoneyManager.instance.convert_money_to_string(money_value);

            if (winner == player_me_index)
            {
                byte win = 1;

                AIPlayManager.instance.win_count++;

                DataManager.instance.ai_win(final_score, money_value);

                RecordManager.instance.save_game_play_record(win, final_score, double_val, money);

                ProfileManager.instance.end_game(0, 1, money_value);

                PopupManager.instance.show(UI_PAGE.POPUP_GAME_RESULT);
                PopupGameResult popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GAME_RESULT).GetComponent<PopupGameResult>();
                popup.on_result(win, winner_score, winner_go, winner_shaking, kwang, yeol, tee, pee, go_back, winner_start_ppuck, losser_start_ppuck, nagari, double_val, final_score, money_value, over_money);
            }
            else if (winner != byte.MaxValue)
            {
                byte win = 0;

                AIPlayManager.instance.lose_count++;

                DataManager.instance.ai_lose(money_value);

                RecordManager.instance.save_game_play_record(win, final_score, double_val, money);

                ProfileManager.instance.end_game(1, 0, money_value);

                PopupManager.instance.show(UI_PAGE.POPUP_GAME_RESULT);
                PopupGameResult popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GAME_RESULT).GetComponent<PopupGameResult>();
                popup.on_result(win, winner_score, winner_go, winner_shaking, kwang, yeol, tee, pee, go_back, winner_start_ppuck, losser_start_ppuck, nagari, double_val, final_score, money_value, over_money);
            }
            else//무승부일 경우 다음판의 판돈을 2배로 올려 이어서 치게 된다.
            {
                byte win = byte.MaxValue;

                AIPlayManager.instance.nagari_count++;

                DataManager.instance.ai_nagari();

                RecordManager.instance.save_game_play_record(win, 0, 0, "0 원");

                PopupManager.instance.show(UI_PAGE.POPUP_GAME_RESULT);
                PopupGameResult popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_GAME_RESULT).GetComponent<PopupGameResult>();
                popup.on_result(win, 0, 0, 0, false, false, false, false, false, 0, 0, 0, 0, 0, 0, false);
            }
        }
        TierManager.instance.update_tier_check();
        TimeStamp.check_next_day();
    }

    byte get_loser_index(byte winner)
    {
        if (winner == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

#region move_to_floor
    void move_card_to_floor(CardPicture card_picture)
    {
        byte slot_index = 0;
        Vector3 begin = card_picture.transform.position;
        Vector3 to = Vector3.zero;

        VisualFloorSlot slot = floor_ui_slots.Find(obj => obj.is_same_card(card_picture.card.number));

        if (slot == null)
        {
            byte empty_slot = find_empty_floorslot();
            to = floor_slot_position[empty_slot];
            slot_index = empty_slot;
        }
        else
        {
            to = get_ui_slot_position(slot);

            List<CardPicture> floor_card_pictures = slot.get_cards();

            for (int i = 0; i < floor_card_pictures.Count; ++i)
            {
                Animator ani = floor_card_pictures[i].GetComponentInChildren<Animator>();
                ani.enabled = true;
                ani.Play("card_hit_under");
            }

            slot_index = slot.ui_slot_position;

            Animator card_ani = card_picture.GetComponentInChildren<Animator>();
            card_ani.enabled = true;
            card_ani.Play("card_hit");
        }
        // 바닥 카드로 등록.
        floor_ui_slots[slot_index].add_card(card_picture);
        move_card(card_picture, begin, to, 0.025f);
    }

    IEnumerator move_kookjin_to_pee(byte player_index)
    {
        CardPicture card_picture = player_card_manager[player_index].get_card(8, PAE_TYPE.YEOL, 0);

        // 카드 자리 움직이기.
        move_card(card_picture, card_picture.transform.position, get_player_card_position(player_index, PAE_TYPE.PEE));

        // 열끗에서 지우고 피로 넣는다.
        player_card_manager[player_index].remove(card_picture);

        card_picture.card.change_pae_type(PAE_TYPE.PEE);
        card_picture.card.set_card_status(CARD_STATUS.TWO_PEE);

        player_card_manager[player_index].add(card_picture);

        yield return new WaitForSeconds(0.1f);

        // 바닥의 패를 정렬한다
        refresh_player_floor_slots(PAE_TYPE.YEOL, player_index);
        refresh_player_floor_slots(PAE_TYPE.PEE, player_index);
    }
#endregion

    IEnumerator on_turn_result(byte player_index, List<string> msg_list)
    {
        List<Card> cards_to_give = parse_cards_to_get(msg_list);
        List<CardPicture> take_cards_from_others = parse_cards_to_take_from_others(msg_list);

        yield return StartCoroutine(move_after_flip_card(player_index, take_cards_from_others, cards_to_give));
    }

#region select_nomal_card_ack
    //일반 카드를 냈을 경우 호출
    IEnumerator on_select_card_ack(List<string> msg_list)
    {
        PlaySoundManager.instance.on_card_to_card();

        ui_slot_index--;

        // 데이터 파싱 시작 ----------------------------------------
        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

        // 카드 내는 연출을 위해 필요한 변수들.
        List<Card> bomb_cards_info = new List<Card>();
        List<Card> shaking_cards_info = new List<Card>();

        // 플레이어가 낸 카드 정보.
        byte player_card_number = (byte)Int32.Parse(PopAt(msg_list));
        PAE_TYPE player_card_pae_type = Converter.PaeType(PopAt(msg_list));
        byte player_card_position = (byte)Int32.Parse(PopAt(msg_list));

        byte slot_index = (byte)Int32.Parse(PopAt(msg_list));

        CARD_EVENT_TYPE card_event = Converter.EventType(PopAt(msg_list));

        switch (card_event)
        {
            case CARD_EVENT_TYPE.BOMB:
                {
                    byte bomb_card_count = (byte)Int32.Parse(PopAt(msg_list));

                    for (byte i = 0; i < bomb_card_count; ++i)
                    {
                        byte number = (byte)Int32.Parse(PopAt(msg_list));
                        PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                        byte position = (byte)Int32.Parse(PopAt(msg_list));
                        Card card = this.card_manager.find_card(number, pae_type, position);
                        bomb_cards_info.Add(card);
                    }
                }
                break;

            case CARD_EVENT_TYPE.SHAKING:
                {
                    byte shaking_card_count = (byte)Int32.Parse(PopAt(msg_list));
                    for (byte i = 0; i < shaking_card_count; ++i)
                    {
                        byte number = (byte)Int32.Parse(PopAt(msg_list));
                        PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                        byte position = (byte)Int32.Parse(PopAt(msg_list));
                        Card card = this.card_manager.find_card(number, pae_type, position);
                        shaking_cards_info.Add(card);
                    }
                }
                break;
        }

        List<Sprite> target_to_choice = new List<Sprite>();
        PLAYER_SELECT_CARD_RESULT select_result = Converter.Card_Result(PopAt(msg_list));
        Debug.Log("get_card_ack PLAYER_SELECT_CARD_RESULT: " + card_event);
        if (select_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
        {
            byte count = (byte)Int32.Parse(PopAt(msg_list));
            for (byte i = 0; i < count; ++i)
            {
                byte number = (byte)Int32.Parse(PopAt(msg_list));
                PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                byte position = (byte)Int32.Parse(PopAt(msg_list));

                Card card = this.card_manager.find_card(number, pae_type, position);
                target_to_choice.Add(get_hwatoo_sprite(card));
            }
        }

        refresh_player_floor_slots(PAE_TYPE.PEE, player_index);

        // 화면 연출 진행.
        // 흔들었을 경우 흔든 카드의 정보를 출력해 준다.
        if (card_event == CARD_EVENT_TYPE.SHAKING)
        {
            PopupManager.instance.show(UI_PAGE.POPUP_SHAKING_CARDS);
            PopupShakingCards popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_SHAKING_CARDS).GetComponent<PopupShakingCards>();
            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < shaking_cards_info.Count; ++i)
            {
                sprites.Add(get_hwatoo_sprite(shaking_cards_info[i]));
            }
            popup.popup_on(sprites);

            yield return new WaitForSeconds(0.5f);
            PopupManager.instance.hide(UI_PAGE.POPUP_SHAKING_CARDS);
        }

        // 플레이어가 낸 카드 움직이기.

        yield return StartCoroutine(move_player_cards_to_floor(player_index, card_event, bomb_cards_info, slot_index, player_card_number, player_card_pae_type, player_card_position));

        yield return new WaitForSeconds(0.1f);

        if (card_event != CARD_EVENT_TYPE.NONE)
        {
            // 흔들기는 위에서 팝업으로 보여줬기 때문에 별도의 이펙트는 필요 없다.
            if (card_event != CARD_EVENT_TYPE.SHAKING)
            {
                PlayEffectManager.instance.play(card_event);
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (player_index == this.player_me_index)
        {
            // 바닥에 깔린 카드가 두장일 때 둘중 하나를 선택하는 팝업을 출력한다.
            if (select_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
            {
                if (test_mode)
                {
                    List<string> msg = new List<string>();
                    msg.Add((short)PROTOCOL.CHOOSE_CARD + "");
                    msg.Add((byte)select_result + "");
                    msg.Add((byte)0 + "");
                    AISendManager.send_from_player(msg);
                }
                else
                {
                    PopupManager.instance.show(UI_PAGE.POPUP_CHOICE_CARD);
                    PopupChoiceCard popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_CHOICE_CARD).GetComponent<PopupChoiceCard>();
                    popup.on_popup(0, player_me_index, select_result, target_to_choice[0], target_to_choice[1]);
                }
            }
            else
            {
                // 가운데 카드 뒤집기 요청.
                List<string> msg = new List<string>();
                msg.Add((short)PROTOCOL.FLIP_DECK_CARD_REQ + "");
                AISendManager.send_from_player(msg);
            }
        }
    }

    //플레이어의 핸드슬롯에 있는 카드를 플로어로 이동시켜준다.
    IEnumerator move_player_cards_to_floor(byte player_index, CARD_EVENT_TYPE event_type, List<Card> bomb_cards_info, byte slot_index, byte player_card_number, PAE_TYPE player_card_pae_type, byte player_card_position)
    {
        float card_moving_delay = 0.1f;

        List<CardPicture> targets = new List<CardPicture>();
        if (event_type == CARD_EVENT_TYPE.BOMB)
        {
            // 폭탄인 경우에는 폭탄 카드 수 만큼 낸다.
            if (this.player_me_index == player_index)
            {
                for (int i = 0; i < bomb_cards_info.Count; ++i)
                {
                    CardPicture card_picture = hand_card_managers[player_index].find_card(bomb_cards_info[i].number, bomb_cards_info[i].pae_type, bomb_cards_info[i].position);
                    targets.Add(card_picture);
                }
            }
            else
            {
                for (int i = 0; i < bomb_cards_info.Count; ++i)
                {
                    CardPicture card_picture = hand_card_managers[player_index].get_card(i);
                    Card card = this.card_manager.find_card(bomb_cards_info[i].number, bomb_cards_info[i].pae_type, bomb_cards_info[i].position);
                    card_picture.update_card(card, get_hwatoo_sprite(card));
                    targets.Add(card_picture);
                }
            }
        }
        else
        {
            // 폭탄이 아닌 경우에는 한장의 카드만 낸다.
            CardPicture card_picture = hand_card_managers[player_index].get_card(slot_index);
            targets.Add(card_picture);

            if (this.player_me_index != player_index)
            {
                Card card = this.card_manager.find_card(player_card_number, player_card_pae_type, player_card_position);
                card_picture.update_card(card, get_hwatoo_sprite(card));
            }
        }

        if (event_type == CARD_EVENT_TYPE.BOMB)
        {
            VisualFloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(player_card_number));
            Vector3 to = get_ui_slot_position(slot);
        }

        // 카드 움직이기.
        for (int i = 0; i < targets.Count; ++i)
        {
            // 손에 들고 있는 패에서 제거한다.
            CardPicture player_card = targets[i];
            hand_card_managers[player_index].remove(player_card);

            // 스케일 장면.
            yield return StartCoroutine(scale_to(player_card, 1.0f, 0.025f));

            yield return new WaitForSeconds(card_moving_delay);

            // 이동 장면.
            player_card.transform.localScale = SCALE_TO_FLOOR;
            move_card_to_floor(player_card);
        }
    }
#endregion

#region select_bonus_card_ack
    //보너스 카드를 냈을 경우 호출
    public void on_select_bonus_card_ack(byte player_index_msg, byte slot_index, byte card_number, PAE_TYPE card_pae_type, byte card_position)
    {
        ui_slot_index--;

        byte player_index = player_index_msg;

        Card card = this.card_manager.find_card(card_number, card_pae_type, card_position);

        List<Sprite> target_to_choice = new List<Sprite>();
        target_to_choice.Add(get_hwatoo_sprite(card));

        refresh_player_floor_slots(PAE_TYPE.PEE, player_index);

        StartCoroutine(move_player_bonus_cards_to_player_floor(player_index, slot_index, card_number, card_pae_type, card_position));

        if (player_index == this.player_me_index)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_DECK_BONUS_CARD_REQ + "");
            AISendManager.send_from_player(msg);
        }
    }

    //플레이어가 낸 보너스 카드를 해당 플레이어에게 준다.
    IEnumerator move_player_bonus_cards_to_player_floor(byte player_index, byte slot_index, byte player_card_number, PAE_TYPE player_card_pae_type, byte player_card_position)
    {
        float card_moving_delay = 0.1f;

        List<CardPicture> targets = new List<CardPicture>();

        CardPicture card_picture = this.hand_card_managers[player_index].get_card(slot_index);
        targets.Add(card_picture);

        if (this.player_me_index != player_index)
        {
            Card card = this.card_manager.find_card(player_card_number, player_card_pae_type, player_card_position);
            card_picture.update_card(card, get_hwatoo_sprite(card));
        }

        // 카드 움직이기.
        for (int i = 0; i < targets.Count; ++i)
        {
            // 손에 들고 있는 패에서 제거한다.
            CardPicture player_card = targets[i];
            hand_card_managers[player_index].remove(player_card);

            // 스케일 장면.
            yield return StartCoroutine(scale_to(player_card, 1.0f, 0.025f));

            yield return new WaitForSeconds(card_moving_delay);

            // 이동 장면.
            move_card(player_card, player_card.transform.position, get_player_card_position(player_index, player_card.card.pae_type));
            player_card.transform.localScale = SCALE_TO_MY_FLOOR;
            this.player_card_manager[player_index].add(player_card);
        }
    }
#endregion

#region flip_type_nomal_card
    IEnumerator on_flip_deck_card_ack(List<string> msg_list)
    {
        PlaySoundManager.instance.on_card_to_card();

        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

        // 덱에서 뒤집은 카드 정보.
        byte deck_card_number = (byte)Int32.Parse(PopAt(msg_list));
        PAE_TYPE deck_card_pae_type = Converter.PaeType(PopAt(msg_list));
        byte deck_card_position = (byte)Int32.Parse(PopAt(msg_list));

        List<Sprite> target_to_choice = new List<Sprite>();
        PLAYER_SELECT_CARD_RESULT result = Converter.Card_Result(PopAt(msg_list));
        if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK)
        {
            byte count = (byte)Int32.Parse(PopAt(msg_list));
            for (byte i = 0; i < count; ++i)
            {
                byte number = (byte)Int32.Parse(PopAt(msg_list));
                PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                byte position = (byte)Int32.Parse(PopAt(msg_list));

                Card card = this.card_manager.find_card(number, pae_type, position);
                target_to_choice.Add(get_hwatoo_sprite(card));
            }

            yield return StartCoroutine(move_flip_card(deck_card_number, deck_card_pae_type, deck_card_position));

            if (player_index == this.player_me_index)
            {
                if (test_mode)
                {
                    List<string> msg = new List<string>();
                    msg.Add((short)PROTOCOL.CHOOSE_CARD + "");
                    msg.Add((byte)result + "");
                    msg.Add(0 + "");
                    AISendManager.send_from_player(msg);
                }
                else
                {
                    PopupManager.instance.show(UI_PAGE.POPUP_CHOICE_CARD);
                    PopupChoiceCard popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_CHOICE_CARD).GetComponent<PopupChoiceCard>();
                    popup.on_popup(0, player_me_index, result, target_to_choice[0], target_to_choice[1]);
                }
            }
        }
        else
        {
            List<Card> cards_to_give = parse_cards_to_get(msg_list);
            List<CardPicture> take_cards_from_others = parse_cards_to_take_from_others(msg_list);
            List<CARD_EVENT_TYPE> events = parse_flip_card_events(msg_list);

            refresh_player_floor_slots(PAE_TYPE.PEE, player_index);

            // 화면 연출 진행.
            yield return StartCoroutine(move_flip_card(deck_card_number, deck_card_pae_type, deck_card_position));

            if (events.Count > 0)
            {
                for (int i = 0; i < events.Count; ++i)
                {
                    PlayEffectManager.instance.play(events[i]);
                    yield return new WaitForSeconds(0.5f);
                }
            }
            yield return StartCoroutine(move_after_flip_card(player_index, take_cards_from_others, cards_to_give));
        }
    }

    IEnumerator move_flip_card(byte number, PAE_TYPE pae_type, byte position)
    {
        // 뒤집은 카드 움직이기.
        CardPicture deck_card_picture = this.deck_cards.Pop();
        Card flipped_card = this.card_manager.find_card(number, pae_type, position);
        deck_card_picture.update_card(flipped_card, get_hwatoo_sprite(flipped_card));
        yield return StartCoroutine(flip_deck_card_ani(deck_card_picture));

        yield return new WaitForSeconds(0.1f);

        deck_card_picture.transform.localScale = SCALE_TO_FLOOR;
        move_card_to_floor(deck_card_picture);
    }
#endregion

#region flip_type_bonus_card
    //보너스 카드를 내어 덱에서 꺼낸 카드를 해당 플레이어의 패에 추가한다.
    IEnumerator on_flip_deck_bonus_card_ack(Dictionary<byte, Card> player_card)
    {
        foreach (KeyValuePair<byte, Card> kvp in player_card)
        {
            byte player_index = kvp.Key;
            Card card = kvp.Value;

            CardPicture card_picture = deck_cards.Pop();
            card_picture.set_slot_index(ui_slot_index);
            hand_card_managers[player_index].add(card_picture);

            // 본인 카드는 해당 이미지를 보여주고,
            // 상대방 카드(is_nullcard)는 back_image로 처리한다.
            if (player_index == player_me_index)
            {
                card_picture.update_card(card, get_hwatoo_sprite(card));
                card_picture.transform.localScale = SCALE_TO_MY_HAND;
                move_card(card_picture, card_picture.transform.position, player_card_positions[player_index].get_hand_position(ui_slot_index));
            }
            else
            {
                card_picture.update_backcard(back_image);
                card_picture.transform.localScale = SCALE_TO_OTHER_HAND;
                move_card(card_picture, card_picture.transform.position, player_card_positions[player_index].get_hand_position(ui_slot_index));
            }
            ui_slot_index++;

            yield return new WaitForSeconds(0.25f);

            if (player_index == this.player_me_index)
            {
                sort_player_hand_slots(player_index);

                List<string> msg = new List<string>();
                msg.Add((short)PROTOCOL.BONUS_TURN + "");
                AISendManager.send_from_player(msg);
            }
        }
    }

    IEnumerator on_flip_plus_bonus_card_ack(List<string> msg_list)
    {
        PlaySoundManager.instance.on_card_to_card();

        Debug.Log("on_flip_plus_bonus_card_ack");
        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

        //플레이어가 낸 카드 정보
        byte pick_card_number = (byte)Int32.Parse(PopAt(msg_list));
        PAE_TYPE pick_card_pae_type = Converter.PaeType(PopAt(msg_list));
        byte pick_card_position = (byte)Int32.Parse(PopAt(msg_list));
        byte pick_card_same_count_with_deck = (byte)Int32.Parse(PopAt(msg_list));
        // 덱에서 뒤집은 카드 정보. 
        byte flip_number = (byte)Int32.Parse(PopAt(msg_list));
        PAE_TYPE flip_pae_type = Converter.PaeType(PopAt(msg_list));
        byte flip_position = (byte)Int32.Parse(PopAt(msg_list));
        byte same_count_with_deck = (byte)Int32.Parse(PopAt(msg_list));

        List<Sprite> target_to_choice = new List<Sprite>();

        // 화면 연출 진행.
        yield return StartCoroutine(move_plus_flip_bonus_card(pick_card_number, pick_card_pae_type, pick_card_position, flip_number, flip_pae_type, flip_position));

        yield return new WaitForSeconds(0.1f);

        if (player_index == player_me_index)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_DECK_CARD_REQ + "");
            AISendManager.send_from_player(msg);
        }
    }

    //덱의 카드에서 보너스 카드가 나왔을 경우
    IEnumerator move_plus_flip_bonus_card(byte pick_number, PAE_TYPE pick_pae_type, byte pick_position, byte number, PAE_TYPE pae_type, byte position)
    {
        Debug.Log("move_plus_flip_bonus_card");
        Card pick_card = this.card_manager.find_card(pick_number, pick_pae_type, pick_position);

        CardPicture deck_card_picture = this.deck_cards.Pop();
        Card flipped_card = this.card_manager.find_card(number, pae_type, position);
        deck_card_picture.update_card(flipped_card, get_hwatoo_sprite(flipped_card));
        yield return StartCoroutine(flip_deck_card_ani(deck_card_picture));

        yield return new WaitForSeconds(0.1f);

        deck_card_picture.transform.localScale = SCALE_TO_FLOOR;
        move_plus_bonus_card_to_floor(pick_card, deck_card_picture);
    }

    //덱의 카드에서 보너스 카드가 나왔을 경우에 호출 임시적으로 이전에 플레이어가 낸 카드에 놓아진다
    void move_plus_bonus_card_to_floor(Card player_card_picture, CardPicture card_picture)
    {
        byte slot_index = 0;
        Vector3 begin = card_picture.transform.position;
        Vector3 to = Vector3.zero;

        VisualFloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(player_card_picture.number));

        to = get_ui_slot_position(slot);

        List<CardPicture> floor_card_pictures = slot.get_cards();

        for (int i = 0; i < floor_card_pictures.Count; ++i)
        {
            Animator ani = floor_card_pictures[i].GetComponentInChildren<Animator>();
            ani.enabled = true;
            ani.Play("card_hit_under");
        }

        slot_index = slot.ui_slot_position;

        Animator card_ani = card_picture.GetComponentInChildren<Animator>();
        card_ani.enabled = true;
        card_ani.Play("card_hit");

        slot.add_card(card_picture);

        move_card(card_picture, begin, to, 0.025f);
    }

    //폭탄으로 덱의 카드가 보너스 카드로 나왔을 경우
    IEnumerator move_bomb_flip_bonus_card(byte player_index, byte number, PAE_TYPE pae_type, byte position)
    {
        CardPicture deck_card_picture = this.deck_cards.Pop();
        Card flipped_card = this.card_manager.find_card(number, pae_type, position);
        deck_card_picture.update_card(flipped_card, get_hwatoo_sprite(flipped_card));

        yield return StartCoroutine(flip_deck_card_ani(deck_card_picture));

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(on_flip_bomb_bonus_card_move(player_index, deck_card_picture));
    }

    IEnumerator on_flip_bomb_bonus_card_move(byte player_index, CardPicture card_picture)//폭탄으로 덱을 뒤집었을 떄 보너스 카드가 나오면 실행
    {
        Vector3 begin = card_picture.transform.position;
        Vector3 to = get_player_card_position(player_index, card_picture.card.pae_type);

        if (this.player_me_index == player_index)
        {
            card_picture.transform.localScale = SCALE_TO_MY_FLOOR;
        }
        else
        {
            card_picture.transform.localScale = SCALE_TO_OTHER_FLOOR;
        }

        move_card(card_picture, begin, to);
        Animator ani = card_picture.GetComponentInChildren<Animator>();
        ani.enabled = true;
        ani.Play("card_idle");

        this.player_card_manager[player_index].add(card_picture);

        yield return new WaitForSeconds(0.1f);

        if (this.player_me_index == player_index)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_BOMB_BONUS_CARD_REQ + "");
            AISendManager.send_from_player(msg);
        }
    }
#endregion

    //턴이 끝날 때 최종적으로 플레이어가 먹을 카드만 이동
    IEnumerator move_after_flip_card(byte player_index, List<CardPicture> take_cards_from_others, List<Card> cards_to_give)
    {
        Debug.Log("move_after_flip_card");
        yield return new WaitForSeconds(0.1f);

        // 상대방에게 뺏어올 카드 움직이기.
        for (int i = 0; i < take_cards_from_others.Count; ++i)
        {
            Vector3 pos = get_player_card_position(player_index, PAE_TYPE.PEE);
            move_card(take_cards_from_others[i], take_cards_from_others[i].transform.position, pos);
            this.player_card_manager[player_index].add(take_cards_from_others[i]);

            if (player_index == this.player_me_index)
            {
                var get_pee_count = Instantiate(get_other_pee_count);
                get_pee_count.transform.SetParent(get_other_pee_panel.transform);
                get_pee_count.transform.localPosition = new Vector3(0, 0, 0);
                get_pee_count.transform.localScale = new Vector3(1, 1, 1);

                if (take_cards_from_others[i].card.status == CARD_STATUS.NONE)
                {
                    get_pee_count.GetComponent<GetOtherPee>().plus_pee(1);
                }
                else
                {
                    get_pee_count.GetComponent<GetOtherPee>().plus_pee(2);
                }
            }
            else
            {
                var get_pee_count = Instantiate(get_other_pee_count);
                get_pee_count.transform.SetParent(get_other_pee_panel.transform);
                get_pee_count.transform.localPosition = new Vector3(0, 0, 0);
                get_pee_count.transform.localScale = new Vector3(1, 1, 1);

                if (take_cards_from_others[i].card.status == CARD_STATUS.NONE)
                {
                    get_pee_count.GetComponent<GetOtherPee>().minus_pee(1);
                }
                else
                {
                    get_pee_count.GetComponent<GetOtherPee>().minus_pee(2);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        //플레이어가 플로어에서 먹을 카드 움직이기
        for (int i = 0; i < cards_to_give.Count; ++i)
        {
            VisualFloorSlot slot;
            CardPicture card_pic;

            if (cards_to_give[i].number == 12)
            {
                slot = this.floor_ui_slots.Find(obj => obj.is_same_bonus_card(cards_to_give[i].number, cards_to_give[i].pae_type, cards_to_give[i].position));
                if (slot == null)
                {
                    Debug.Log("move_after_flip_card) Can not find slot type: bonus_card");
                    continue;
                }

                card_pic = slot.find_bonus_card(cards_to_give[i]);
                if (card_pic == null)
                {
                    Debug.Log("move_after_flip_card) Can not find card_pic type: bonus_card");
                    continue;
                }
            }
            else
            {
                slot = this.floor_ui_slots.Find(obj => obj.is_same_card(cards_to_give[i].number));
                if (slot == null)
                {
                    Debug.Log("move_after_flip_card) Can not find slot type: nomal_card");
                    continue;
                }

                card_pic = slot.find_card(cards_to_give[i]);
                if (card_pic == null)
                {
                    Debug.Log("move_after_flip_card) Can not find card_pic type: nomal_card");
                    continue;
                }

                slot.remove_card(card_pic);
            }

            Vector3 begin = card_pic.transform.position;
            Vector3 to = get_player_card_position(player_index, card_pic.card.pae_type);

            if (this.player_me_index == player_index)
            {
                card_pic.transform.localScale = SCALE_TO_MY_FLOOR;
            }
            else
            {
                card_pic.transform.localScale = SCALE_TO_OTHER_FLOOR;
            }

            move_card(card_pic, begin, to);
            Animator ani = card_pic.GetComponentInChildren<Animator>();
            ani.enabled = true;
            ani.Play("card_idle");

            this.player_card_manager[player_index].add(card_pic);

            yield return new WaitForSeconds(0.1f);
        }
        sort_floor_cards_when_finished_turn();
        refresh_player_hand_slots(player_index);

        yield return new WaitForSeconds(0.1f);

        if (my_turn.activeSelf == true)
        {
            my_turn.SetActive(false);
            other_turn.SetActive(true);
        }
        else
        {
            my_turn.SetActive(true);
            other_turn.SetActive(false);
        }

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.TURN_END + "");
        AISendManager.send_from_player(msg);
    }

    List<CARD_EVENT_TYPE> parse_flip_card_events(List<string> msg)
    {
        List<CARD_EVENT_TYPE> events = new List<CARD_EVENT_TYPE>();
        byte count = (byte)Int32.Parse(PopAt(msg));
        for (byte i = 0; i < count; ++i)
        {
            CARD_EVENT_TYPE type = Converter.EventType(PopAt(msg));
            events.Add(type);
        }
        return events;
    }

#region parse_other_card
    List<Card> parse_cards_to_get(List<string> msg)
    {
        List<Card> cards_to_give = new List<Card>();
        byte count_to_give = (byte)Int32.Parse(PopAt(msg));

        for (int i = 0; i < count_to_give; ++i)
        {
            byte card_number = (byte)Int32.Parse(PopAt(msg));
            PAE_TYPE pae_type = Converter.PaeType(PopAt(msg));
            byte position = (byte)Int32.Parse(PopAt(msg));
            Card card = this.card_manager.find_card(card_number, pae_type, position);
            cards_to_give.Add(card);
        }
        return cards_to_give;
    }

    List<CardPicture> parse_cards_to_take_from_others(List<string> msg)
    {
        // 뺏어올 카드.
        List<CardPicture> take_cards_from_others = new List<CardPicture>();
        byte victim_count = (byte)Int32.Parse(PopAt(msg));
        for (byte victim = 0; victim < victim_count; ++victim)
        {
            byte victim_index = (byte)Int32.Parse(PopAt(msg));
            byte count_to_take = (byte)Int32.Parse(PopAt(msg));
            for (byte i = 0; i < count_to_take; ++i)
            {
                byte card_number = (byte)Int32.Parse(PopAt(msg));
                PAE_TYPE pae_type = Converter.PaeType(PopAt(msg));
                byte position = (byte)Int32.Parse(PopAt(msg));

                Debug.Log("PlayerCardManager get_card number: " + card_number + " pae_type: " + pae_type + " position: " + position);
                CardPicture card_pic = this.player_card_manager[victim_index].get_card(card_number, pae_type, position);
                take_cards_from_others.Add(card_pic);
                this.player_card_manager[victim_index].remove(card_pic);
            }
        }
        // UI적용은 추후 적용
        //short score = (short)Int32.Parse(PopAt(msg));
        //this.player_info_slots[player_index].update_score(score);
        return take_cards_from_others;
    }
#endregion

#region select_hand_card
    void on_card_touch(CardPicture card_picture)
    {
        on_select = true;
        if (auto_select != null)
        {
            StopCoroutine(auto_select);
        }
        if (auto_count != null)
        {
            StopCoroutine(auto_count);
        }

        // 카드 연속 터치등을 막기 위한 처리.
        card_collision_manager.enabled = false;

        for (int i = 0; i < hand_card_managers.Count; ++i)
        {
            this.hand_card_managers[i].enable_all_colliders(false);
        }

        hide_hint_mark();

        // 일반 카드, 폭탄 카드에 따라 다르게 처리한다.
        if (card_picture.is_back_card())
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_BOMB_CARD_REQ + "");
            AISendManager.send_from_player(msg);
        }
        else
        {
            //보너스 카드가 아닐 경우, 손에 같은 카드 3장이 있고 바닥에 같은카드가 없을 때 흔들기 팝업을 출력한다.
            if (card_picture.card.number == 12)
            {
                send_select_card(card_picture.card, card_picture.slot, 0);
            }
            else
            {
                List<CardPicture> same_on_hand = hand_card_managers[this.player_me_index].get_same_number_count(card_picture.card.number);
                List<VisualFloorSlot> slots = this.floor_ui_slots.FindAll(obj => obj.is_same_card(card_picture.card.number));
                int same_on_floor = slots.Count;

                if (same_on_hand.Count == 3 && same_on_floor == 0)
                {
                    List<Sprite> sprites = new List<Sprite>();
                    for (int i = 0; i < same_on_hand.Count; i++)
                    {
                        sprites.Add(get_hwatoo_sprite(same_on_hand[i].card));
                    }

                    PopupManager.instance.show(UI_PAGE.POPUP_ASK_SHAKING);
                    PopupShaking popup = PopupManager.instance.get_uipage(UI_PAGE.POPUP_ASK_SHAKING).GetComponent<PopupShaking>();
                    popup.popup_on(0, player_me_index, card_picture.card, card_picture.slot, sprites);
                }
                else
                {
                    send_select_card(card_picture.card, card_picture.slot, 0);
                }
            }
        }
    }

    public static void send_select_card(Card card, byte slot, byte is_shaking)
    {
        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.SELECT_CARD_REQ + "");

        msg.Add(card.number + "");
        msg.Add(card.pae_type + "");
        msg.Add(card.position + "");
        msg.Add(slot + "");
        msg.Add(is_shaking + "");

        AISendManager.send_from_player(msg);
    }

    bool on_select = false;
    IEnumerator auto_count;
    public IEnumerator auto_card_select(byte bomb_card)
    {
        on_select = false;

        if (!test_mode)
        {
            auto_count = auto_select_count();
            yield return StartCoroutine(auto_count);
        }

        if (!on_select)
        {
            this.card_collision_manager.enabled = false;

            CardPicture top_card = deck_cards.Peek();
            top_card.enable_collider(false);

            hide_hint_mark(); 

            if (bomb_card != 0)
            {
                List<string> msg = new List<string>();
                msg.Add((short)PROTOCOL.FLIP_BOMB_CARD_REQ + "");
                AISendManager.send_from_player(msg);
            }
            else
            {
                byte slot_index = byte.MaxValue;
                bool have_bonus = false;

                for (int i = 0; i < hand_card_managers[this.player_me_index].get_card_count(); ++i)//플레이어가 갖고있는 카드만큼 for문 돌림
                {
                    CardPicture card_pic = hand_card_managers[this.player_me_index].get_card(i);
                    if (card_pic.card.number == 12)//보너스 카드는 바로 사용
                    {
                        have_bonus = true;

                        slot_index = (byte)i;
                        send_select_card(card_pic.card, slot_index, 0);
                        break;
                    }
                }

                if (!have_bonus)
                {
                    for (int i = 0; i < hand_card_managers[this.player_me_index].get_card_count(); ++i)//플레이어가 갖고있는 카드만큼 for문 돌림
                    {
                        CardPicture card_pic = hand_card_managers[this.player_me_index].get_card(i);
                        VisualFloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(card_pic.card.number));
                        if (slot != null)
                        {
                            slot_index = (byte)i;
                            send_select_card(card_pic.card, slot_index, 0);
                            break;
                        }
                    }
                }

                if (slot_index == byte.MaxValue)
                {
                    slot_index = 0;
                    CardPicture card_pic = hand_card_managers[this.player_me_index].get_card(slot_index);
                    send_select_card(card_pic.card, slot_index, 0);
                }
            }
        }
    }

    IEnumerator auto_select_count()
    {
        yield return new WaitForSeconds(7f);
        //MultiPlayManager.instance.on_system_message("3초 후 자동으로 카드를 선택합니다");
        yield return new WaitForSeconds(3f);
        //MultiPlayManager.instance.on_system_message("자동으로 카드를 선택합니다.");
    }
#endregion

#region get_position
    Vector3 get_player_card_position(byte player_index, PAE_TYPE pae_type)
    {
        int count = player_card_manager[player_index].get_card_count(pae_type);
        return player_card_positions[player_index].get_floor_position(count, pae_type);
    }

    Vector3 get_ui_slot_position(VisualFloorSlot slot)
    {
        Vector3 position = floor_slot_position[slot.ui_slot_position];
        int stacked_count = slot.get_card_count();
        position.x += (stacked_count * 13f);
        position.y -= (stacked_count * 10f);
        return position;
    }
#endregion

#region sort_card_slots
    void sort_player_hand_slots(byte player_index)
    {
        hand_card_managers[player_index].sort_by_number();
        refresh_player_hand_slots(player_index);
    }

    void refresh_player_hand_slots(byte player_index)
    {
        HandCardManager hand_card_manager = hand_card_managers[player_index];
        byte count = (byte)hand_card_manager.get_card_count();
        for (byte card_index = 0; card_index < count; ++card_index)
        {
            CardPicture card = hand_card_manager.get_card(card_index);
            // 슬롯 인덱스를 재설정 한다.
            card.set_slot_index(card_index);
            // 화면 위치를 재설정 한다.
            card.transform.position = player_card_positions[player_index].get_hand_position(card_index);
        }
    }

    void refresh_player_floor_slots(PAE_TYPE pae_type, byte player_index)
    {
        int count = player_card_manager[player_index].get_card_count(pae_type);
        for (int i = 0; i < count; ++i)
        {
            Vector3 pos = player_card_positions[player_index].get_floor_position(i, pae_type);
            CardPicture card_pic = player_card_manager[player_index].get_card_at(pae_type, i);
            pos.z = card_pic.transform.position.z;
            card_pic.transform.position = pos;
        }
    }
#endregion

#region hint_mark
    Queue<GameObject> hint_arrows = new Queue<GameObject>();
    List<GameObject> enable_hint_arrows = new List<GameObject>();
    void load_hint_arrows()
    {
        GameObject arrow = Resources.Load("Prefab/hint") as GameObject;
        for (int i = 0; i < 10; i++)
        {
            GameObject clone = Instantiate(arrow) as GameObject;
            clone.SetActive(false);
            hint_arrows.Enqueue(clone);
        }
    }

    void hide_hint_mark()
    {
        for (int i = 0; i < enable_hint_arrows.Count; ++i)
        {
            enable_hint_arrows[i].SetActive(false);
            hint_arrows.Enqueue(enable_hint_arrows[i]);
        }
        enable_hint_arrows.Clear();
    }

    void refresh_hint_mark()
    {
        for (int i = 0; i < this.hand_card_managers[this.player_me_index].get_card_count(); ++i)
        {
            CardPicture card_picture = this.hand_card_managers[this.player_me_index].get_card(i);
            VisualFloorSlot slot = this.floor_ui_slots.Find(obj => obj.is_same_card(card_picture.card.number));
            if (slot != null)
            {
                show_hint_mark(card_picture.transform.position);
            }
        }
    }

    void show_hint_mark(Vector3 position)
    {
        GameObject hint = hint_arrows.Dequeue();
        hint.SetActive(true);
        hint.transform.position = position;

        this.enable_hint_arrows.Add(hint);
    }
#endregion

    IEnumerator flip_deck_card_ani(CardPicture deck_card_picture)
    {
        Debug.Log("flip_deck_card_ani");
        Animator ani = deck_card_picture.GetComponentInChildren<Animator>();
        ani.enabled = true;
        ani.Play("rotation");

        yield return StartCoroutine(scale_to(deck_card_picture, 1.0f, 0.025f));
    }

    IEnumerator scale_to(CardPicture card_picture, float ratio, float duration)
    {
        card_picture.sprite_renderer.sortingOrder = SpriteLayerManager.Instance.Order;

        Vector3 from = card_picture.transform.localScale;
        float begin = Time.time;
        Vector3 to = from * ratio;
        while (Time.time - begin <= duration)
        {
            float t = (Time.time - begin) / duration;

            Vector3 scale = from;
            scale.x = MovingUtil.linear(from.x, to.x, t);
            scale.y = MovingUtil.linear(from.y, to.y, t);

            card_picture.transform.localScale = scale;

            yield return 0;
        }
        card_picture.transform.localScale = to;
    }

    byte find_empty_floorslot()
    {
        VisualFloorSlot slot = floor_ui_slots.Find(obj => obj.get_card_count() == 0);
        if (slot == null)
        {
            return byte.MaxValue;
        }
        return slot.ui_slot_position;
    }

    void update_player_statistics(List<string> msg)
    {
        if (msg.Count < 8)
        {
            return;
        }

        byte score = (byte)Int32.Parse(PopAt(msg));
        byte go_count = (byte)Int32.Parse(PopAt(msg));
        byte ppuk_count = (byte)Int32.Parse(PopAt(msg));
        byte pee_count = (byte)Int32.Parse(PopAt(msg));

        byte ai_score = (byte)Int32.Parse(PopAt(msg));
        byte ai_go_count = (byte)Int32.Parse(PopAt(msg));
        byte ai_ppuk_count = (byte)Int32.Parse(PopAt(msg));
        byte ai_pee_count = (byte)Int32.Parse(PopAt(msg));

        this.player_info_slots[0].update_score(score);
        this.player_info_slots[0].update_go(go_count);
        this.player_info_slots[0].update_ppuk(ppuk_count);
        this.player_info_slots[0].update_peecount(pee_count);

        this.player_info_slots[1].update_score(ai_score);
        this.player_info_slots[1].update_go(ai_go_count);
        this.player_info_slots[1].update_ppuk(ai_ppuk_count);
        this.player_info_slots[1].update_peecount(ai_pee_count);
    }

    public string PopAt(List<string> list)
    {
        string r = list[0];
        list.RemoveAt(0);
        return r;
    }
}