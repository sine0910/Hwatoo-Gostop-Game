using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain
{
    delegate void PacketFn(List<string> msg);
    Dictionary<PROTOCOL, PacketFn> packet_handler;

    byte player_index;
    List<Card> floor_cards;
    List<Card> hand_cards;

    AIGameRoom room;

    public AIBrain(AIGameRoom gameRoom)
    {
        this.room = gameRoom;

        this.hand_cards = new List<Card>();
        this.floor_cards = new List<Card>();

        this.packet_handler = new Dictionary<PROTOCOL, PacketFn>();
        this.packet_handler.Add(PROTOCOL.LOCAL_SERVER_STARTED, ON_LOCAL_SERVER_STARTED);
        this.packet_handler.Add(PROTOCOL.BEGIN_CARD_INFO, ON_BEGIN_CARD_INFO);
        this.packet_handler.Add(PROTOCOL.START_BONUSPEE, ON_START_BONUS);
        this.packet_handler.Add(PROTOCOL.START_TURN, ON_START_TURN);
        this.packet_handler.Add(PROTOCOL.SELECT_CARD_ACK, ON_SELECT_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.SELECT_BONUS_CARD_ACK, ON_SELECT_BONUS_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.FLIP_DECK_CARD_ACK, ON_FLIP_DECK_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.FLIP_DECK_BONUS_CARD_ACK, ON_FLIP_DECK_BONUS_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.FLIP_PLUS_BONUS_CARD_ACK, FLIP_PLUS_BONUS_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.FLIP_BOMB_BONUS_CARD_ACK, ON_FLIP_BOMB_BONUS_CARD_ACK);
        this.packet_handler.Add(PROTOCOL.TURN_RESULT, ON_TURN_RESULT);
        this.packet_handler.Add(PROTOCOL.ASK_GO_OR_STOP, ON_ASK_GO_OR_STOP);
        this.packet_handler.Add(PROTOCOL.ASK_KOOKJIN_TO_PEE, ON_ASK_KOOKJIN_TO_PEE);
        this.packet_handler.Add(PROTOCOL.GAME_RESULT, ON_GAME_RESULT);
    }

    public void send(List<string> msg)
    {
        List<string> msg_list = msg.ToList();

        on_receive(msg_list);
    }

    void on_receive(List<string> msg)
    {
        PROTOCOL protocol = (PROTOCOL)Convert.ToInt32(PopAt(msg));

        if (!this.packet_handler.ContainsKey(protocol))
        {
            return;
        }
        this.packet_handler[protocol](msg);
    }

    public void reset()
    {
        this.hand_cards.Clear();
        this.floor_cards.Clear();
    }

    void ON_LOCAL_SERVER_STARTED(List<string> msg)
    {
        Debug.Log("AI ON_LOCAL_SERVER_STARTED");

        this.player_index = (byte)Int32.Parse(PopAt(msg));

        send_ready_to_start();
    }

    void ON_BEGIN_CARD_INFO(List<string> msg_list)
    {
        Debug.Log("AI ON_BEGIN_CARD_INFO");

        reset();

        Queue<Card> floor_card = new Queue<Card>();

        // floor cards.
        byte floor_count = (byte)Int32.Parse(PopAt(msg_list));
        for (byte i = 0; i < floor_count; ++i)
        {
            byte number = (byte)Int32.Parse(PopAt(msg_list));
            PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
            byte position = (byte)Int32.Parse(PopAt(msg_list));

            Card card = new Card(number, pae_type, position);
            floor_card.Enqueue(card);
            this.floor_cards.Add(card);
        }

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
                    // AI플레이어 본인 것만 담는다.
                    PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                    byte position = (byte)Int32.Parse(PopAt(msg_list));

                    Debug.Log("AI Hand Card number: " + number + " pae_type: " + pae_type + " position: " + position);

                    Card card = new Card(number, pae_type, position);
                    this.hand_cards.Add(card);
                }
            }
        }

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.DISTRIBUTED_ALL_CARDS + "");
        AISendManager.send_from_ai(msg);
    }

    void ON_START_BONUS(List<string> msg_list)
    {
        Debug.Log("AI ON_START_BONUS");

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.BONUS_START + "");
        AISendManager.send_from_ai(msg);
    }

    void ON_START_TURN(List<string> msg_list)
    {
        Debug.Log("AI ON_START_TURN");

        byte remain_bomb_card_count = 0;
        if (msg_list.Count > 0)
        {
            remain_bomb_card_count = (byte)Int32.Parse(PopAt(msg_list));
        }

        if (remain_bomb_card_count > 0)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_BOMB_CARD_REQ + "");
            AISendManager.send_from_ai(msg);
        }
        else
        {
            byte slot_index = byte.MaxValue;

            //손에 들고 있는 카드와 바닥 패를 비교하여 번호가 같은 카드가 있다면 그 카드를 내고, 없으면 그냥 첫번째 카드를 낸다. 
            //보너스 카드의 경우 낼 카드가 없을 때 가장 우선시한다.
            Debug.Log("AI this.hand_cards.Count: " + this.hand_cards.Count + " this.floor_cards.Count: " + this.floor_cards.Count);
            for (int i = 0; i < this.hand_cards.Count; i++)
            {
                for (int f = 0; f < this.floor_cards.Count; f++)
                {
                    if (this.hand_cards[i].number == this.floor_cards[f].number)
                    {
                        slot_index = (byte)i;
                        break;
                    }
                }
            }

            if (slot_index == byte.MaxValue)
            {
                for (int i = 0; i < this.hand_cards.Count; i++)
                {
                    //낼 카드가 없고 보너스 카드를 손에 들고 있을 때 보너스카드를 낸다.
                    if (this.hand_cards[i].number == 12)
                    {
                        slot_index = (byte)i;
                        break;
                    }
                }

                if (slot_index == byte.MaxValue)
                {
                    slot_index = 0;
                }
            }

            Card card = this.hand_cards[slot_index];
            Debug.Log("AI select this card num " + card.number + " type " + card.pae_type + " pos " + card.position + " slot " + slot_index);

            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.SELECT_CARD_REQ + "");
            msg.Add(card.number + "");
            msg.Add(card.pae_type + "");
            msg.Add(card.position + "");
            msg.Add(slot_index + "");
            msg.Add(1 + "");
            AISendManager.send_from_ai(msg);
        }
    }

    void ON_CHOICE_ONE_CARD(List<string> msg_list)
    {
        Debug.Log("AI ON_CHOICE_ONE_CARD");

        PLAYER_SELECT_CARD_RESULT result = Converter.Card_Result(PopAt(msg_list));

        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.CHOOSE_CARD + "");
        msg.Add((byte)result + "");
        msg.Add(0 + "");
        AISendManager.send_from_ai(msg);
    }

    void ON_SELECT_CARD_ACK(List<string> msg_list)
    {
        Debug.Log("AI ON_SELECT_CARD_ACK");

        byte current_player_index = (byte)Int32.Parse(PopAt(msg_list));

        if (current_player_index == 1)
        {
            // 카드 내는 연출을 위해 필요한 변수들.
            CARD_EVENT_TYPE card_event = CARD_EVENT_TYPE.NONE;
            byte slot_index = byte.MaxValue;
            byte player_card_number = byte.MaxValue;
            PAE_TYPE player_card_pae_type = PAE_TYPE.PEE;
            byte player_card_position = byte.MaxValue;

            // 플레이어가 낸 카드 정보.
            player_card_number = (byte)Int32.Parse(PopAt(msg_list));
            player_card_pae_type = Converter.PaeType(PopAt(msg_list));
            player_card_position = (byte)Int32.Parse(PopAt(msg_list));

            slot_index = (byte)Int32.Parse(PopAt(msg_list));

            card_event = Converter.EventType(PopAt(msg_list));

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

                            Card card = this.hand_cards.Find(obj => obj.is_same_card(number, pae_type, position));
                            this.hand_cards.Remove(card);
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
                        }

                        this.hand_cards.RemoveAt(slot_index);
                    }
                    break;

                default:
                    this.hand_cards.RemoveAt(slot_index);
                    break;
            }


            PLAYER_SELECT_CARD_RESULT select_result = Converter.Card_Result(PopAt(msg_list));
            if (select_result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER)
            {
                byte count = (byte)Int32.Parse(PopAt(msg_list));
                for (byte i = 0; i < count; ++i)
                {
                    byte number = (byte)Int32.Parse(PopAt(msg_list));
                    PAE_TYPE pae_type = Converter.PaeType(PopAt(msg_list));
                    byte position = (byte)Int32.Parse(PopAt(msg_list));
                }
                List<string> c_msg = new List<string>();
                c_msg.Add((short)PROTOCOL.CHOOSE_CARD + "");
                c_msg.Add(0 + "");
                c_msg.Add((byte)select_result + "");
                AISendManager.send_from_ai(c_msg);
                return;
            }
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_DECK_CARD_REQ + "");
            AISendManager.send_from_ai(msg);
        }
    }

    void ON_SELECT_BONUS_CARD_ACK(List<string> msg_list)
    {
        Debug.Log("AI ON_SELECT_BONUS_CARD_ACK");

        byte current_player_index = (byte)Int32.Parse(PopAt(msg_list));

        if (current_player_index == 1)
        {
            byte slot_index = (byte)Int32.Parse(PopAt(msg_list));
            this.hand_cards.RemoveAt(slot_index);

            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_DECK_BONUS_CARD_REQ + "");
            AISendManager.send_from_ai(msg);
        }
    }

    void ON_FLIP_DECK_CARD_ACK(List<string> msg_list)
    {
        Debug.Log("AI ON_FLIP_DECK_CARD_ACK");

        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

        // 덱에서 뒤집은 카드 정보.
        byte deck_card_number = (byte)Int32.Parse(PopAt(msg_list));
        PAE_TYPE deck_card_pae_type = Converter.PaeType(PopAt(msg_list));
        byte deck_card_position = (byte)Int32.Parse(PopAt(msg_list));

        PLAYER_SELECT_CARD_RESULT result = Converter.Card_Result(PopAt(msg_list));

        if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK)
        {
            if (player_index == 1)
            {
                List<string> msg = new List<string>();
                msg.Add((short)PROTOCOL.CHOOSE_CARD + "");
                msg.Add((byte)result + "");
                msg.Add(0 + "");

                AISendManager.send_from_ai(msg);
            }
        }
        else
        {
            ON_TURN_RESULT(msg_list);
        }
    }

    void ON_FLIP_DECK_BONUS_CARD_ACK(List<string> msg_list)
    {
        Debug.Log("AI ON_FLIP_DECK_BONUS_CARD_ACK");

        byte current_turn_player_index = (byte)Int32.Parse(PopAt(msg_list));

        if (current_turn_player_index == 1)
        {
            byte deck_card_number = (byte)Int32.Parse(PopAt(msg_list));
            PAE_TYPE deck_card_pae_type = Converter.PaeType(PopAt(msg_list));
            byte deck_card_position = (byte)Int32.Parse(PopAt(msg_list));

            Card card = new Card(deck_card_number, deck_card_pae_type, deck_card_position);

            this.hand_cards.Add(card);

            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.BONUS_TURN + "");
            AISendManager.send_from_ai(msg);
        }
    }

    void FLIP_PLUS_BONUS_CARD_ACK(List<string> msg_list)
    {
        byte player_index = (byte)Int32.Parse(PopAt(msg_list));

        if (player_index == 1)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_DECK_CARD_REQ + "");
            AISendManager.send_from_player(msg);
        }
    }

    void ON_FLIP_BOMB_BONUS_CARD_ACK(List<string> msg_list)
    {
        Debug.Log("AI ON_FLIP_BOMB_BONUS_CARD_ACK");
        byte player_index = (byte)Int32.Parse(PopAt(msg_list));
        if (player_index == 1)
        {
            List<string> msg = new List<string>();
            msg.Add((short)PROTOCOL.FLIP_BOMB_BONUS_CARD_REQ + "");
            AISendManager.send_from_ai(msg);
        }
    }

    void ON_TURN_RESULT(List<string> msg_list)
    {
        Debug.Log("AI ON_TURN_RESULT");
        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.TURN_END + "");
        AISendManager.send_from_ai(msg);
    }

    void ON_ASK_GO_OR_STOP(List<string> msg_list)
    {
        Debug.Log("AI ON_ASK_GO_OR_STOP");
        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.ANSWER_GO_OR_STOP + "");

        // 0:스톱, 1:고.
        if (room.players[0].agent.score >= 5)
        {
            msg.Add(0 + "");
        }
        else
        {
            msg.Add(1 + "");
        }

        AISendManager.send_from_ai(msg);
    }

    void ON_ASK_KOOKJIN_TO_PEE(List<string> msg_list)
    {
        Debug.Log("AI ON_ASK_KOOKJIN_TO_PEE");
        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.ANSWER_KOOKJIN_TO_PEE + "");
        msg.Add((byte)1 + "");

        AISendManager.send_from_ai(msg);
    }

    void ON_GAME_RESULT(List<string> msg_list)
    {
        Debug.Log("AI ON_GAME_RESULT");
        send_ready_to_start();
    }

    public void send_ready_to_start()
    {
        List<string> msg = new List<string>();
        msg.Add((short)PROTOCOL.READY_TO_START + "");
        AISendManager.send_from_ai(msg);
    }

    public string PopAt(List<string> list)
    {
        string r = list[0];
        list.RemoveAt(0);
        return r;
    }
}
