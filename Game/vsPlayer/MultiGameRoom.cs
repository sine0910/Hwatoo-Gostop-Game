using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGameRoom
{
    Dictionary<byte, PROTOCOL> received_protocol;

    public GameEngine engine;
    public List<MultiPlayer> players;

    public MultiGameRoom(MultiSendManager MultiSendManager)
    {
        engine = new GameEngine();
        players = new List<MultiPlayer>();

        for (int i = 0; i < 2; ++i)
        {
            switch (i)
            {
                case 0:
                    this.players.Add(new MultiPlayer((byte)i, MULTI_PLAYER_TYPE.HOST, MultiSendManager.send_to_host, this));
                    break;
                case 1:
                    this.players.Add(new MultiPlayer((byte)i, MULTI_PLAYER_TYPE.GUEST, MultiSendManager.send_to_guest, this));
                    break;
            }
        }
        this.received_protocol = new Dictionary<byte, PROTOCOL>();
    }

    public void on_ready_start()
    {
        for (int i = 0; i < this.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add((byte)PROTOCOL.LOCAL_SERVER_STARTED + "");
            msg.Add(i.ToString()); 
            this.players[i].send(msg);
        }
    }

    bool is_received(byte player_index, PROTOCOL protocol)
    {
        if (!this.received_protocol.ContainsKey(player_index))
        {
            return false;
        }
        return this.received_protocol[player_index] == protocol;
    }

    void checked_protocol(byte player_index, PROTOCOL protocol)
    {
        if (this.received_protocol.ContainsKey(player_index))
        {
            if (!this.received_protocol.ContainsValue(protocol))
            {
                return;
            }
            else
            {
                this.received_protocol.Remove(player_index);
            }
        }
        this.received_protocol.Add(player_index, protocol);
    }

    bool all_received(PROTOCOL protocol)
    {
        if (this.received_protocol.Count < this.players.Count)
        {
            Debug.Log("all_received this.received_protocol.Count < this.players.Count");
            return false;
        }

        foreach (KeyValuePair<byte, PROTOCOL> kvp in this.received_protocol)
        {
            if (kvp.Value != protocol)
            {
                Debug.Log("kvp.Value != protocol");
                return false;
            }
        }
        clear_received_protocol();
        return true;
    }

    void clear_received_protocol()
    {
        this.received_protocol.Clear();
    }

    public void on_receive(byte player_index, List<string> msg_list)
    {
        PROTOCOL protocol = (PROTOCOL)Convert.ToInt32(PopAt(msg_list));
        Debug.Log("MultiGameRoom on_receive protocol: " + protocol + "\nfrom player: " + player_index);
        if (is_received(player_index, protocol))
        {
            return;
        }

        checked_protocol(player_index, protocol);

        switch (protocol)
        {
            case PROTOCOL.READY_TO_START:
                {
                    Debug.Log("READY_TO_START");
                    on_ready_req();
                }
                break;

            case PROTOCOL.DISTRIBUTED_ALL_CARDS:
                {
                    Debug.Log("DISTRIBUTED_ALL_CARDS");

                    List<Card> cards = this.engine.start_bonus_card();

                    if (cards.Count != 0)
                    {
                        //보너스 카드를 플레이어에게 주고 플로어에 4장이 깔린 경우 재시작
                        if (this.engine.check_floor_card())
                        {
                            on_restart_req();
                        }
                        else
                        {
                            on_start_bonus_card(cards);
                        }
                    }
                    else
                    {
                        if (this.engine.check_floor_card())
                        {
                            on_restart_req();
                        }
                        else
                        {
                            List<string> _msg = new List<string>();
                            _msg.Add((short)PROTOCOL.START_TURN + "");
                            players[this.engine.current_player_index].send(_msg);
                        }
                    }
                }
                break;

            case PROTOCOL.BONUS_START:
                {
                    clear_received_protocol();

                    List<string> _msg = new List<string>();
                    _msg.Add((short)PROTOCOL.START_TURN + "");
                    players[this.engine.current_player_index].send(_msg);
                }
                break;

            case PROTOCOL.SELECT_CARD_REQ://카드를 선택했을 때 UI에서 카드 정보를 받아옴
                {
                    byte number = (byte)Convert.ToInt32(PopAt(msg_list));
                    PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                    byte position = (byte)Convert.ToInt32(PopAt(msg_list));
                    byte slot_index = (byte)Convert.ToInt32(PopAt(msg_list));
                    byte is_shaking = (byte)Convert.ToInt32(PopAt(msg_list));
                    Debug.Log("player select card: " + "number = " + number + "pae_type = " + pae_type + "position = " + position + "slot_index = " + slot_index + "is_shaking = " + is_shaking);
                    on_player_put_card(player_index, number, pae_type, position, slot_index, is_shaking);
                }
                break;

            case PROTOCOL.CHOOSE_CARD:
                {
                    clear_received_protocol();

                    PLAYER_SELECT_CARD_RESULT client_result = Converter.Card_Result(PopAt(msg_list));
                    byte choice_index = (byte)Convert.ToInt32(PopAt(msg_list));
                    PLAYER_SELECT_CARD_RESULT server_result = this.engine.on_choose_card(player_index, client_result, choice_index);
                    if (server_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
                    {
                        PLAYER_SELECT_CARD_RESULT choose_result = this.engine.flip_process(player_index, TURN_RESULT_TYPE.RESULT_OF_NORMAL_CARD);

                        if (choose_result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
                        {
                            send_flip_plus_bonus_result(this.engine.current_player_index);
                        }
                        else
                        {
                            send_flip_result(choose_result, this.engine.current_player_index);
                        }
                    }
                    else
                    {
                        send_turn_result(this.engine.current_player_index);
                    }
                }
                break;

            case PROTOCOL.FLIP_BOMB_CARD_REQ:
                {
                    clear_received_protocol();

                    if (this.engine.current_player_index != player_index)
                    {
                        break;
                    }

                    if (!players[player_index].agent.decrease_bomb_count())
                    {
                        Debug.Log(string.Format("Invalid bomb count player " + player_index));
                        break;
                    }

                    PLAYER_SELECT_CARD_RESULT choose_result = this.engine.flip_process(player_index, TURN_RESULT_TYPE.RESULT_OF_BOMB_CARD);

                    if (choose_result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
                    {
                        this.engine.give_bomb_bonus_cards_to_player(this.engine.current_player_index, this.engine.card_from_deck);
                        send_flip_bomb_plus_bonus_result(this.engine.current_player_index);
                    }
                    else
                    {
                        send_flip_result(choose_result, this.engine.current_player_index);
                    }
                }
                break;

            case PROTOCOL.FLIP_BOMB_BONUS_CARD_REQ:
                {
                    clear_received_protocol();

                    if (this.engine.current_player_index != player_index)
                    {
                        break;
                    }

                    PLAYER_SELECT_CARD_RESULT choose_result = this.engine.flip_process(player_index, TURN_RESULT_TYPE.RESULT_OF_BOMB_CARD);

                    if (choose_result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
                    {
                        this.engine.give_bomb_bonus_cards_to_player(this.engine.current_player_index, this.engine.card_from_deck);
                        send_flip_bomb_plus_bonus_result(this.engine.current_player_index);
                    }
                    else
                    {
                        send_flip_result(choose_result, this.engine.current_player_index);
                    }
                }
                break;

            case PROTOCOL.FLIP_DECK_CARD_REQ:
                {
                    clear_received_protocol();

                    PLAYER_SELECT_CARD_RESULT result = this.engine.flip_process(this.engine.current_player_index, TURN_RESULT_TYPE.RESULT_OF_NORMAL_CARD);

                    if (result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
                    {
                        send_flip_plus_bonus_result(this.engine.current_player_index);
                    }
                    else
                    {
                        send_flip_result(result, this.engine.current_player_index);
                    }
                }
                break;

            case PROTOCOL.FLIP_DECK_BONUS_CARD_REQ:
                {
                    clear_received_protocol();

                    this.engine.bonus_flip_process(this.engine.current_player_index);

                    send_flip_bonus_result(this.engine.current_player_index);
                }
                break;

            case PROTOCOL.TURN_END:
                {
                    Debug.Log("TURN_END / " + player_index);
                    clear_received_protocol();

                    PlayerAgent this_player = this.players[this.engine.current_player_index].agent;
                    if (this_player.three_ppuk())
                    {
                        broadcast_game_result(this.engine.current_player_index);
                        engine.reset_nagari();
                        break;
                    }

                    if (this.engine.has_kookjin(this.engine.current_player_index))
                    {
                        List<string> _msg = new List<string>();
                        _msg.Add(((short)PROTOCOL.ASK_KOOKJIN_TO_PEE).ToString());
                        this.players[this.engine.current_player_index].send(_msg);
                    }
                    else
                    {
                        check_game_finish();
                    }
                }
                break;

            case PROTOCOL.BONUS_TURN:
                {
                    clear_received_protocol();

                    bonus_turn();
                }
                break;

            case PROTOCOL.ANSWER_KOOKJIN_TO_PEE:
                {
                    clear_received_protocol();

                    players[player_index].agent.kookjin_selected();

                    byte answer = (byte)Convert.ToInt32(PopAt(msg_list));
                    if (answer == 1)
                    {
                        // 국진을 쌍피로 이동.
                        players[player_index].agent.move_kookjin_to_pee();
                        send_player_statistics();
                        broadcast_move_kookjin(player_index);
                    }
                    check_game_finish();
                }
                break;

            case PROTOCOL.ANSWER_GO_OR_STOP:
                {
                    Debug.Log("ANSWER_GO_OR_STOP / " + player_index);

                    clear_received_protocol();
                    // answer가 1이면 GO, 0이면 STOP.
                    byte answer = (byte)Convert.ToInt32(PopAt(msg_list));

                    List<string> prt = new List<string>();
                    prt.Add("101");
                    if (answer == 1)//고했을 때 고카운트 증가 및 다음 턴
                    {
                        players[player_index].agent.plus_go_count();

                        prt.Add(player_index.ToString());
                        prt.Add(players[player_index].agent.go_count.ToString());

                        for (int i = 0; i < players.Count; i++)
                        {
                            players[i].send(prt);
                        }

                        next_turn();
                    }
                    else//스탑했을 때 승리
                    {
                        prt.Add(player_index.ToString());
                        prt.Add(101 + "");

                        for (int i = 0; i < players.Count; i++)
                        {
                            players[i].send(prt);
                        }
                        broadcast_game_result(this.engine.current_player_index);
                        engine.reset_nagari();
                    }
                }
                break;

            case PROTOCOL.SET_START_USER:
                {
                    Debug.Log("SET_START_USER");

                    //2021-04-21 16:50 나눠줄 카드를 겹치지 않게 정해 섞어 나누어줌//2021-04-23 09:58 선/후 카드를 선택하는 팝업창을 띄우는 프로토콜을 보냄
                    Queue<Card> cards = new Queue<Card>();
                    engine.set_first_user(cards);

                    List<string> prt = new List<string>();
                    prt.Add((short)PROTOCOL.ON_SET_START_USER + "");
                    for (int i = 0; i < 3; i++)
                    {
                        Card card = cards.Dequeue();
                        prt.Add(card.number + "");
                        prt.Add(card.pae_type + "");
                        prt.Add(card.position + "");
                    }

                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].send(prt);
                    }
                }
                break;

            case PROTOCOL.READY_TO_USER_SELET_START:
                {
                    Debug.Log("READY_TO_USER_SELET_START");
                    if (all_received(PROTOCOL.READY_TO_USER_SELET_START))
                    {
                        first_set_user_turn_start(0);
                    }
                }
                break;

            case PROTOCOL.SET_START_USER_CARD_REQ:
                {
                    Debug.Log("SET_START_USER_CARD_REQ");
                    clear_received_protocol();

                    //2021-04-23 10:01 선택한 결과를 받아 처리 후 호스트와 게스트에게 MultiGameUI로 넘김
                    byte select_card_slot = (byte)Convert.ToInt32(PopAt(msg_list));//2021-04-23 16:14 호스트가 선택한 슬롯 기록

                    byte select_card_number = (byte)Convert.ToInt32(PopAt(msg_list));
                    PAE_TYPE select_card_type = Converter.PaeType(PopAt(msg_list));
                    byte select_card_position = (byte)Convert.ToInt32(PopAt(msg_list));

                    Card select_card = new Card(select_card_number, select_card_type, select_card_position);//2021-04-23 16:14 호스트가 선택한 카드 기록
                    engine.save_player_select_card(player_index, select_card_slot, select_card);

                    List<string> prt = new List<string>();
                    prt.Add((short)PROTOCOL.SET_START_USER_CARD_ACK + "");
                    prt.Add(player_index + "");
                    prt.Add(select_card_slot + "");

                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].send(prt);
                    }
                }
                break;

            case PROTOCOL.SET_START_USER_TURN_END:
                {
                    Debug.Log("SET_START_USER_TURN_END");
                    clear_received_protocol();

                    if (!engine.end_set_first_player())
                    {
                        first_set_user_turn_start(this.engine.find_next_player_index_of(player_index));
                    }
                    else
                    {
                        List<PlayerPickCard> cards = this.engine.get_first_player();

                        byte first_user = cards[0].player;

                        Debug.Log("SET_START_USER_TURN_END RESULT: " + first_user);

                        List<string> prt = new List<string>();
                        prt.Add((short)PROTOCOL.END_SET_START_USER + "");

                        prt.Add(first_user + "");

                        for (int i = 0; i < players.Count; i++)
                        {
                            players[i].send(prt);
                        }
                    }
                }
                break;
        }
    }

    public void first_set_user_turn_start(byte next_player)
    {
        List<string> prt = new List<string>();
        prt.Add((short)PROTOCOL.START_USER_SELET_START + "");
        prt.Add(next_player + "");
        for (int i = 0; i < this.players.Count; i++)
        {
            this.players[i].send(prt);
        }
    }

    public void on_ready_req()
    {
        Debug.Log("MultiGameRoom on_ready_req");

        if (!all_received(PROTOCOL.READY_TO_START))
        {
            Debug.Log("MultiGameRoom on_ready_req not all received");
            return;
        }

        Debug.Log("MultiGameRoom on_ready_req all received");

        //추후 돈이 없을 경우 게임을 시작하지 못하게 막아야 한다
        reset();

        this.engine.on_multi_start(this.players);

        for (int i = 0; i < this.players.Count; ++i)
        {
            send_cardinfo_to_player(this.players[i]);
        }
    }

    void reset()
    {
        this.engine.reset();
    }

    void on_start_bonus_card(List<Card> cards)
    {
        Debug.Log("AIGameRoom on_start_bonus_card");

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.START_BONUSPEE + "");
        msg.Add(cards.Count.ToString());
        msg.Add(this.engine.first_player_index.ToString());

        add_flip_event_result_to(msg, cards);//보너스카드 카운트와 덱에서 뽑는 카드 정보 리스트를 메세지로 보냄

        for (int i = 0; i < this.players.Count; i++)
        {
            this.players[i].send(msg);
        }

        send_player_statistics();
    }

    void add_flip_event_result_to(List<string> msg, List<Card> bonus_cards)
    {
        Debug.Log("AIGameRoom add_flip_event_result_to");
        //보너스 카드의 갯수 만큼 덱에서 뒤집은 카드 정보를 리스트에 담아 차례대로 메세지에 넣어줌
        for (int b = 0; b < bonus_cards.Count; b++)
        {
            // 덱에서 뒤집은 카드 정보.
            msg.Add(bonus_cards[b].number.ToString());
            msg.Add(bonus_cards[b].pae_type.ToString());
            msg.Add(bonus_cards[b].position.ToString());
        }
    }

    //바닥에 4장이 깔려 재시작하는 함수
    public void on_restart_req()
    {
        Debug.Log("MultiGameRoom on_restart_req");

        reset();

        this.engine.on_multi_start(this.players); // 고스톱엔진에게 시작하게 해서 카드 셔플 및 분배

        for (int i = 0; i < this.players.Count; ++i)
        {
            send_cardinfo_to_player(this.players[i]);
        }
    }

    void send_cardinfo_to_player(MultiPlayer player)
    {
        Debug.Log("MultiGameRoom send_cardinfo_to_player");

        byte count = (byte)this.engine.distributed_floor_cards.Count;

        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.BEGIN_CARD_INFO).ToString());

        msg.Add(count.ToString());
        for (int i = 0; i < count; ++i)
        {
            msg.Add(this.engine.distributed_floor_cards[i].number.ToString());
            msg.Add(this.engine.distributed_floor_cards[i].pae_type.ToString());
            msg.Add(this.engine.distributed_floor_cards[i].position.ToString());
        }

        msg.Add(this.players.Count.ToString());

        for (int i = 0; i < this.players.Count; ++i)
        {
            byte player_index = this.players[i].player_index;
            byte players_card_count = (byte)this.engine.distributed_players_cards[player_index].Count;
            msg.Add(player_index.ToString());
            msg.Add(players_card_count.ToString());

            // 플레이어 본인의 카드정보만 실제 카드로 보내주고,
            // 다른 플레이어의 카드는 null카드로 보내줘서 플레이어가 알지 못하게 한다.
            if (player.player_index == player_index)
            {
                for (int card_index = 0; card_index < players_card_count; ++card_index)
                {
                    msg.Add(this.engine.distributed_players_cards[player_index][card_index].number.ToString());
                    msg.Add(this.engine.distributed_players_cards[player_index][card_index].pae_type.ToString());
                    msg.Add(this.engine.distributed_players_cards[player_index][card_index].position.ToString());
                }
            }
            else
            {
                for (int card_index = 0; card_index < players_card_count; ++card_index)
                {
                    // 다른 플레이어의 카드는 null카드로 보내준다.
                    msg.Add(byte.MaxValue.ToString());
                }
            }
        }
        player.send(msg);
    }

    public void on_player_put_card(byte player_index, byte card_number, PAE_TYPE pae_type, byte position, byte slot_index, byte is_shaking)
    {
        PLAYER_SELECT_CARD_RESULT result = this.engine.player_put_card(player_index, card_number, pae_type, position, slot_index, is_shaking);

        if (result == PLAYER_SELECT_CARD_RESULT.ERROR_INVALID_CARD)
        {
            Debug.Log("PLAYER_SELECT_CARD_RESULT.ERROR_INVALID_CARD");
            return;
        }
        if (result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
        {
            //보너스 카드일 때 해당 플레이어에게 이벤트 피를 주고 덱에서 카드를 뽑아 플레이어가 낼 카드에 추가한 뒤 자신의 턴으로 진행
            clear_received_protocol();
            send_select_bonus_card_ack(player_index, slot_index);
        }
        else
        {
            clear_received_protocol();
            send_select_card_ack(result, player_index, slot_index);
        }
    }

    void send_select_card_ack(PLAYER_SELECT_CARD_RESULT result, byte current_turn_player_index, byte slot_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.SELECT_CARD_ACK).ToString());
            // 플레이어 정보.
            msg.Add(current_turn_player_index.ToString());

            // 낸 카드 정보.
            add_player_select_result_to(msg, slot_index);

            // 둘중 하나를 선택하는 경우 대상이 되는 카드 정보를 담는다.
            Debug.Log("select_card_ack PLAYER_SELECT_CARD_RESULT: " + result);
            msg.Add(result + "");
            if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
            {
                add_choice_card_info_to(msg);
            }

            this.players[i].send(msg);
        }
    }

    void send_select_bonus_card_ack(byte current_turn_player_index, byte slot_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.SELECT_BONUS_CARD_ACK).ToString());

            msg.Add(current_turn_player_index.ToString());

            msg.Add(slot_index.ToString());

            msg.Add(this.engine.card_from_player.number.ToString());
            msg.Add(this.engine.card_from_player.pae_type.ToString());
            msg.Add(this.engine.card_from_player.position.ToString());

            this.players[i].send(msg);
        }
    }

    void send_flip_result(PLAYER_SELECT_CARD_RESULT result, byte current_turn_player_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.FLIP_DECK_CARD_ACK).ToString());

            // 플레이어 정보.
            msg.Add(current_turn_player_index.ToString());

            // 덱에서 뒤집은 카드 정보
            msg.Add(this.engine.card_from_deck.number.ToString());
            msg.Add(this.engine.card_from_deck.pae_type.ToString());
            msg.Add(this.engine.card_from_deck.position.ToString());

            msg.Add(result.ToString());
            if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK)
            {
                add_choice_card_info_to(msg);
            }
            else
            {
                add_player_get_cards_info_to(msg);
                add_others_card_result_to(msg);
                add_turn_result_to(msg);
            }

            this.players[i].send(msg);
        }
    }

    void add_player_get_cards_info_to(List<string> msg)
    {
        // 플레이어가 가져갈 카드 정보.
        byte count_to_player = (byte)this.engine.cards_to_give_player.Count;
        msg.Add(count_to_player.ToString());
        for (byte card_index = 0; card_index < count_to_player; ++card_index)
        {
            Card card = this.engine.cards_to_give_player[card_index];

            msg.Add(card.number.ToString());
            msg.Add(card.pae_type.ToString());
            msg.Add(card.position.ToString());
        }
    }

    void add_player_select_result_to(List<string> msg, byte slot_index)
    {
        // 플레이어가 낸 카드 정보.
        msg.Add(this.engine.card_from_player.number.ToString());
        msg.Add(this.engine.card_from_player.pae_type.ToString());
        msg.Add(this.engine.card_from_player.position.ToString());

        msg.Add(slot_index.ToString());

        // 카드 이벤트.
        msg.Add(this.engine.card_event_type.ToString());

        // 폭탄 카드 정보.
        switch (this.engine.card_event_type)
        {
            case CARD_EVENT_TYPE.BOMB:
                {
                    byte bomb_cards_count = (byte)this.engine.bomb_cards_from_player.Count;
                    msg.Add(bomb_cards_count.ToString());
                    for (byte card_index = 0; card_index < bomb_cards_count; ++card_index)
                    {
                        msg.Add(this.engine.bomb_cards_from_player[card_index].number.ToString());
                        msg.Add(this.engine.bomb_cards_from_player[card_index].pae_type.ToString());
                        msg.Add(this.engine.bomb_cards_from_player[card_index].position.ToString());
                    }
                }
                break;

            case CARD_EVENT_TYPE.SHAKING:
                {
                    if (this.engine.card_from_player.number != 12)
                    {
                        byte shaking_cards_count = (byte)this.engine.shaking_cards.Count;
                        msg.Add(shaking_cards_count.ToString());
                        for (byte card_index = 0; card_index < shaking_cards_count; ++card_index)
                        {
                            msg.Add(this.engine.shaking_cards[card_index].number.ToString());
                            msg.Add(this.engine.shaking_cards[card_index].pae_type.ToString());
                            msg.Add(this.engine.shaking_cards[card_index].position.ToString());
                        }
                    }
                }
                break;
        }
    }

    void add_others_card_result_to(List<string> msg)
    {
        msg.Add(this.engine.other_cards_to_player.Count.ToString());

        foreach (KeyValuePair<byte, List<Card>> kvp in this.engine.other_cards_to_player)
        {
            msg.Add(kvp.Key.ToString());
            byte count = (byte)this.engine.other_cards_to_player[kvp.Key].Count;
            msg.Add(count.ToString());
            for (byte card_index = 0; card_index < count; ++card_index)
            {
                Card card = this.engine.other_cards_to_player[kvp.Key][card_index];
                msg.Add(card.number.ToString());
                msg.Add(card.pae_type.ToString());
                msg.Add(card.position.ToString());
            }
        }
    }


    void send_flip_plus_bonus_result(byte current_turn_player_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.FLIP_PLUS_BONUS_CARD_ACK).ToString());
            // 플레이어 정보.
            msg.Add(current_turn_player_index.ToString());

            msg.Add(this.engine.card_from_player.number.ToString());
            msg.Add(this.engine.card_from_player.pae_type.ToString());
            msg.Add(this.engine.card_from_player.position.ToString());
            msg.Add(this.engine.same_card_count_with_deck.ToString());

            msg.Add(this.engine.card_from_deck.number.ToString());
            msg.Add(this.engine.card_from_deck.pae_type.ToString());
            msg.Add(this.engine.card_from_deck.position.ToString());
            msg.Add(this.engine.same_card_count_with_deck.ToString());

            this.players[i].send(msg);
        }
    }

    void send_flip_bomb_plus_bonus_result(byte current_turn_player_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.FLIP_BOMB_BONUS_CARD_ACK).ToString());
            msg.Add(current_turn_player_index.ToString());
            msg.Add(this.engine.card_from_deck.number.ToString());
            msg.Add(this.engine.card_from_deck.pae_type.ToString());
            msg.Add(this.engine.card_from_deck.position.ToString());
            msg.Add(this.engine.same_card_count_with_deck.ToString());
            this.players[i].send(msg);
        }
    }

    void send_flip_bonus_result(byte current_turn_player_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.FLIP_DECK_BONUS_CARD_ACK).ToString());
            msg.Add(current_turn_player_index.ToString());
            msg.Add(this.engine.card_from_deck.number.ToString());
            msg.Add(this.engine.card_from_deck.pae_type.ToString());
            msg.Add(this.engine.card_from_deck.position.ToString());

            this.players[i].send(msg);
        }
    }

    void add_turn_result_to(List<string> msg)
    {
        byte count = (byte)this.engine.flipped_card_event_type.Count;
        msg.Add(count.ToString());
        for (byte i = 0; i < count; ++i)
        {
            msg.Add(this.engine.flipped_card_event_type[i].ToString());
        }
    }

    void add_choice_card_info_to(List<string> msg)
    {
        List<Card> target_cards = this.engine.target_cards_to_choice;
        byte count = (byte)target_cards.Count;
        msg.Add(count.ToString());

        for (int i = 0; i < count; ++i)
        {
            Card card = target_cards[i];
            msg.Add(card.number.ToString());
            msg.Add(card.pae_type.ToString());
            msg.Add(card.position.ToString());
        }
    }

    void send_turn_result(byte current_turn_player_index)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.TURN_RESULT).ToString());
            msg.Add(current_turn_player_index.ToString());

            add_player_get_cards_info_to(msg);
            add_others_card_result_to(msg);
            add_turn_result_to(msg);

            this.players[i].send(msg);
        }
    }

    void next_turn()
    {
        Debug.Log("next_turn");

        send_player_statistics();

        this.engine.clear_turn_data();
        this.engine.move_to_next_player();

        List<string> turn_msg = new List<string>();
        turn_msg.Add(((short)PROTOCOL.START_TURN).ToString());
        turn_msg.Add(this.players[this.engine.current_player_index].agent.remain_bomb_count.ToString());

        this.players[this.engine.current_player_index].send(turn_msg);
    }

    void bonus_turn()
    {
        send_player_statistics();

        this.engine.clear_turn_data();

        List<string> turn_msg = new List<string>();
        turn_msg.Add(((short)PROTOCOL.START_TURN).ToString());
        turn_msg.Add(this.players[this.engine.current_player_index].agent.remain_bomb_count.ToString());

        this.players[this.engine.current_player_index].send(turn_msg);
    }

    void send_go_or_stop()
    {
        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.ASK_GO_OR_STOP).ToString());
        this.players[this.engine.current_player_index].send(msg);
    }

    void check_game_finish()
    {
        if (this.engine.is_time_to_ask_gostop())//2021-01-24 10:20 점수가 났을 경우
        {
            if (this.players[this.engine.current_player_index].agent.hand_pae.Count > 0 || this.players[this.engine.current_player_index].agent.remain_bomb_count > 0)//2021-01-24 10:15 손에 들고있는 카드가 1장 이상일 경우
            {
                send_player_statistics();
                send_go_or_stop();
            }
            else
            {
                broadcast_game_result(this.engine.current_player_index);
                engine.reset_nagari();
            }
        }
        else if (this.players[0].agent.hand_pae.Count + this.players[0].agent.remain_bomb_count == 0 && this.players[1].agent.hand_pae.Count + this.players[1].agent.remain_bomb_count == 0)//2021-01-24 10:18 플레이어가 점수가 나지않고 양쪽 다 낼 수 있는 카드가 없을 경우
        {
            engine.add_nagari();
            broadcast_game_result(byte.MaxValue);
        }
        else
        {
            next_turn();
        }
    }

    void broadcast_move_kookjin(byte who)
    {
        for (int i = 0; i < this.players.Count; ++i)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.MOVE_KOOKJIN_TO_PEE).ToString());
            msg.Add(who.ToString());
            this.players[i].send(msg);
        }
    }

    public void broadcast_game_result(byte winner)//게임이 끝났을 때 네크워크에서 정보를 받아 승패를 나눔
    {
        Debug.Log("broadcast_game_result");

        PlayerAgent player = this.players[0].agent;
        PlayerAgent other_player = this.players[1].agent;

        if (winner != byte.MaxValue)
        {
            this.engine.first_player_index = winner;
        }

        for (int i = 0; i < players.Count; i++)
        {
            List<string> result_msg = new List<string>();
            result_msg.Add(((short)PROTOCOL.GAME_RESULT).ToString());
            result_msg.Add(winner.ToString());

            switch (winner)
            {
                case 0:
                    result_msg.Add(player.score.ToString());
                    result_msg.Add(player.go_count.ToString());
                    result_msg.Add(player.ppuk_count.ToString());
                    result_msg.Add(player.shaking_count.ToString());
                    result_msg.Add(player.get_kwang_count().ToString());
                    result_msg.Add(player.get_pee_count().ToString());
                    result_msg.Add(player.get_yeol_count().ToString());
                    result_msg.Add(player.get_tee_count().ToString());

                    result_msg.Add(other_player.go_count.ToString());
                    result_msg.Add(other_player.get_kwang_count().ToString());
                    result_msg.Add(other_player.get_pee_count().ToString());
                    result_msg.Add(other_player.get_yeol_count().ToString());
                    result_msg.Add(other_player.get_tee_count().ToString());

                    result_msg.Add(engine.get_nagari().ToString());

                    result_msg.Add(player.start_ppuk.ToString());
                    result_msg.Add(other_player.start_ppuk.ToString());
                    break;

                case 1:
                    result_msg.Add(other_player.score.ToString());
                    result_msg.Add(other_player.go_count.ToString());
                    result_msg.Add(other_player.ppuk_count.ToString());
                    result_msg.Add(other_player.shaking_count.ToString());
                    result_msg.Add(other_player.get_kwang_count().ToString());
                    result_msg.Add(other_player.get_pee_count().ToString());
                    result_msg.Add(other_player.get_yeol_count().ToString());
                    result_msg.Add(other_player.get_tee_count().ToString());

                    result_msg.Add(player.go_count.ToString());
                    result_msg.Add(player.get_kwang_count().ToString());
                    result_msg.Add(player.get_pee_count().ToString());
                    result_msg.Add(player.get_yeol_count().ToString());
                    result_msg.Add(player.get_tee_count().ToString());

                    result_msg.Add(engine.get_nagari().ToString());

                    result_msg.Add(other_player.start_ppuk.ToString());
                    result_msg.Add(player.start_ppuk.ToString());
                    break;

                case byte.MaxValue:
                    {
                        result_msg.Add(player.score.ToString());
                        result_msg.Add(player.go_count.ToString());
                        result_msg.Add(player.ppuk_count.ToString());
                        result_msg.Add(player.shaking_count.ToString());
                        result_msg.Add(player.get_kwang_count().ToString());
                        result_msg.Add(player.get_pee_count().ToString());
                        result_msg.Add(player.get_yeol_count().ToString());
                        result_msg.Add(player.get_tee_count().ToString());

                        result_msg.Add(other_player.go_count.ToString());
                        result_msg.Add(other_player.get_kwang_count().ToString());
                        result_msg.Add(other_player.get_pee_count().ToString());
                        result_msg.Add(other_player.get_yeol_count().ToString());
                        result_msg.Add(other_player.get_tee_count().ToString());

                        result_msg.Add(engine.get_nagari().ToString());

                        result_msg.Add(player.start_ppuk.ToString());
                        result_msg.Add(other_player.start_ppuk.ToString());
                    }
                    break;
            }
            this.players[i].send(result_msg);
        }
    }

    public void send_player_statistics()
    {
        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.UPDATE_PLAYER_STATISTICS).ToString());

        for (int i = 0; i < this.players.Count; i++)
        {
            msg.Add(this.players[i].agent.score.ToString());
            msg.Add(this.players[i].agent.go_count.ToString());
            msg.Add(this.players[i].agent.ppuk_count.ToString());
            msg.Add(this.players[i].agent.get_pee_count().ToString());
        }

        this.players[0].send(msg);
        this.players[1].send(msg);
    }

    public string PopAt(List<string> list)
    {
        string r = list[0];
        list.RemoveAt(0);
        return r;
    }
}
