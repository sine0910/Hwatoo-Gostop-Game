using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine
{
    //전체 카드를 보관하는 컨테이너
    CardManager card_manager;
    Queue<Card> card_queue;

    //플레이어 관련 변수들 
    public byte first_player_index;
    List<PlayerAgent> player_agents;

    FloorCardManager floor_manager;

    //플레이어끼리 선을 정할때 사용되는 변수와 클레스 PlayerPickCard
    Dictionary<int, PlayerPickCard> player_select_cards { get; set; }

    // 게임 진행시 카드 정보들을 저장해놓을 임시 변수들
    // 한턴이 끝나면 모두 초기화 시켜줘야 한다
    public TURN_RESULT_TYPE turn_result_type { get; private set; }
    public Card card_from_player { get; private set; }
    public byte selected_slot_index { get; private set; }
    public Card card_from_deck { get; private set; }
    public List<Card> bomb_cards_from_player { get; private set; }
    public List<Card> target_cards_to_choice { get; private set; }
    public byte same_card_count_with_player { get; private set; }
    public byte same_card_count_with_deck { get; private set; }
    public CARD_EVENT_TYPE card_event_type { get; private set; }
    public List<CARD_EVENT_TYPE> flipped_card_event_type { get; private set; }
    public List<Card> bonus_cards_to_give_player { get; private set; }
    public List<Card> cards_to_give_player { get; private set; }
    public List<Card> cards_to_floor { get; private set; }
    public Dictionary<byte, List<Card>> other_cards_to_player { get; private set; }
    public List<Card> shaking_cards { get; private set; }

    PLAYER_SELECT_CARD_RESULT expected_result_type;

    public byte current_player_index { get; private set; }

    public List<Card> distributed_floor_cards { get; private set; }
    public List<List<Card>> distributed_players_cards { get; private set; }

    public byte nagari_value = 0;

    public GameEngine()
    {
        this.player_select_cards = new Dictionary<int, PlayerPickCard>();
        this.first_player_index = 0;
        this.floor_manager = new FloorCardManager();
        this.card_manager = new CardManager();
        this.player_agents = new List<PlayerAgent>();
        this.distributed_floor_cards = new List<Card>();
        this.distributed_players_cards = new List<List<Card>>();

        this.bonus_cards_to_give_player = new List<Card>();
        this.cards_to_give_player = new List<Card>();
        this.cards_to_floor = new List<Card>();
        this.other_cards_to_player = new Dictionary<byte, List<Card>>();
        this.current_player_index = 0;
        this.flipped_card_event_type = new List<CARD_EVENT_TYPE>();
        this.bomb_cards_from_player = new List<Card>();
        this.target_cards_to_choice = new List<Card>();
        this.shaking_cards = new List<Card>();
        this.card_queue = new Queue<Card>();
    }

    public void reset()
    {
        this.player_agents.ForEach(obj => obj.reset());

        this.current_player_index = this.first_player_index;

        this.player_select_cards.Clear();

        this.card_manager.MakeAllCards();
        this.distributed_floor_cards.Clear();

        this.distributed_players_cards.Clear();
        this.floor_manager.reset();

        clear_turn_data();
    }

    public void clear_turn_data()
    {
        this.turn_result_type = TURN_RESULT_TYPE.RESULT_OF_NORMAL_CARD;
        this.card_from_player = null;
        this.selected_slot_index = byte.MaxValue;
        this.card_from_deck = null;
        this.target_cards_to_choice.Clear();
        this.same_card_count_with_player = 0;
        this.same_card_count_with_deck = 0;
        this.card_event_type = CARD_EVENT_TYPE.NONE;
        this.flipped_card_event_type.Clear();
        this.cards_to_give_player.Clear();
        this.bonus_cards_to_give_player.Clear();
        this.cards_to_floor.Clear();
        this.other_cards_to_player.Clear();
        this.bomb_cards_from_player.Clear();
        this.shaking_cards.Clear();
        this.expected_result_type = PLAYER_SELECT_CARD_RESULT.ERROR_INVALID_CARD;
    }

    public void set_first_user(Queue<Card> cards)
    {
        this.card_manager.MakePickCard(cards);
    }

    public void save_player_select_card(byte player_index, byte slot_index, Card player_pick_card)
    {
        this.player_select_cards.Add(player_index, new PlayerPickCard(player_index, slot_index, player_pick_card));
    }

    public bool end_set_first_player()
    {
        return this.player_select_cards.Count == MAX_PLAYER_COUNT;
    }

    public List<PlayerPickCard> get_first_player()
    {
        List<PlayerPickCard> player_pick_cards = new List<PlayerPickCard>();
        for (int i = 0; i < player_select_cards.Count; i++)
        {
            player_pick_cards.Add(player_select_cards[i]);
        }
        player_pick_cards = player_pick_cards.OrderByDescending(x => x.card.number).ToList();
        this.first_player_index = player_pick_cards[0].player;
        return player_pick_cards;
    }

    public void on_ai_start(List<AIPlayer> players)
    {
        this.player_agents.Clear();

        for (int i = 0; i < players.Count; ++i)
        {
            this.player_agents.Add(players[i].agent);
            this.player_agents[i].reset();
            this.distributed_players_cards.Add(new List<Card>());
        }

        suffle();
        distribute_cards();

        for (int i = 0; i < this.player_agents.Count; ++i)
        {
            this.player_agents[i].sort_player_hand_slots();
        }
    }

    public void on_multi_start(List<MultiPlayer> players)
    {
        this.player_agents.Clear();

        for (int i = 0; i < players.Count; ++i)
        {
            this.player_agents.Add(players[i].agent);
            this.player_agents[i].reset();
            this.distributed_players_cards.Add(new List<Card>());
        }

        suffle();
        distribute_cards();

        for (int i = 0; i < this.player_agents.Count; ++i)
        {
            this.player_agents[i].sort_player_hand_slots();
        }
    }

    void suffle()
    {
        this.card_manager.on_suffle();
        this.card_queue.Clear();
        this.card_manager.fill_to(this.card_queue);
        Debug.Log("card_queue info: " + this.card_queue);
    }

    Card pop_front_card()
    {
        return this.card_queue.Dequeue();
    }

    // 카드 분배.
    void distribute_cards()
    {
        byte player_index = this.current_player_index;
        byte floor_index = 0;
        // 2번 반복하여 바닥에는 8장, 플레이어에게는 10장씩 돌아가도록 한다.
        for (int count = 0; count < 2; ++count)
        {
            // 바닥에 4장.
            for (byte i = 0; i < 4; ++i)
            {
                Card card = pop_front_card();
                this.distributed_floor_cards.Add(card);

                this.floor_manager.put_to_begin_card(card);

                ++floor_index;
            }

            // 1p에게 5장.
            Debug.Log("engine distribute_cards 1p " + player_index);
            for (int i = 0; i < 5; ++i)
            {
                Card card = pop_front_card();
                this.distributed_players_cards[player_index].Add(card);

                this.player_agents[player_index].add_card_to_hand(card);
            }

            player_index = find_next_player_index_of(player_index);

            // 2p에게 5장.
            Debug.Log("engine distribute_cards 2p " + player_index);
            for (int i = 0; i < 5; ++i)
            {
                Card card = pop_front_card();
                this.distributed_players_cards[player_index].Add(card);

                this.player_agents[player_index].add_card_to_hand(card);
            }

            player_index = find_next_player_index_of(player_index);
        }
    }

    public void refresh_floor_cards()
    {
        this.floor_manager.refresh_floor_cards();
    }

    #region start_bonus_card
    public List<Card> start_bonus_card()
    {
        List<Card> cards_info = new List<Card>();
        while (check_bonus_cards())
        {
            List<Card> bonus_cards = this.floor_manager.get_bonus_cards();

            for (int i = 0; i < bonus_cards.Count; i++)
            {
                this.player_agents[this.current_player_index].add_card_to_floor(bonus_cards[i]);
                this.distributed_floor_cards.Remove(bonus_cards[i]);
                this.floor_manager.pop_to_begin_card(bonus_cards[i]);

                this.card_from_deck = pop_front_card();
                this.distributed_floor_cards.Add(this.card_from_deck);
                this.floor_manager.put_to_begin_card(this.card_from_deck);
                cards_info.Add(this.card_from_deck);
            }
        }
        this.refresh_floor_cards();
        return cards_info;
    }

    public bool check_bonus_cards()
    {
        List<Card> bonus_cards = this.floor_manager.get_bonus_cards();

        if (bonus_cards.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    //2021-02-12 12:02 플로어의 카드가 4장 깔렸는지 체크
    public bool check_floor_card()
    {
        if (this.floor_manager.check_same_card())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //플레이어가 카드를 냈을 때 호출
    public PLAYER_SELECT_CARD_RESULT player_put_card(byte player_index, byte card_number, PAE_TYPE pae_type, byte position, byte slot_index, byte is_shaking)
    {
        this.selected_slot_index = slot_index;

        // 클라이언트가 보내온 카드 정보가 실제로 플레이어가 들고 있는 카드인지 확인한다.
        Card card = this.player_agents[player_index].pop_card_from_hand(card_number, pae_type, position);
        if (card == null)
        {
            Card _card = this.player_agents[player_index].hand_pae[slot_index];
            Debug.Log("error player_put_card info: " + card_number + "/" + pae_type + "/" + position + " player: " + player_index);
        }

        this.card_from_player = card;

        if (card.status == CARD_STATUS.BONUS_PEE)
        {
            this.player_agents[player_index].add_card_to_floor(this.card_from_player);
            calculate_players_score();

            return PLAYER_SELECT_CARD_RESULT.BONUSCARD;
        }

        List<Card> same_cards = this.floor_manager.get_cards(card.number);

        if (same_cards != null)
        {
            this.same_card_count_with_player = (byte)same_cards.Count;
        }
        else
        {
            this.same_card_count_with_player = 0;
        }

        switch (this.same_card_count_with_player)
        {
            case 0:
                {
                    if (is_shaking == 1)
                    {
                        byte count_from_hand = this.player_agents[player_index].get_same_card_count_from_hand(this.card_from_player.number);
                        if (count_from_hand == 2)
                        {
                            this.card_event_type = CARD_EVENT_TYPE.SHAKING;
                            this.player_agents[player_index].plus_shaking_count();

                            // 플레이어에게 흔든 카드 정보를 보내줄 때 사용하기 위해서 리스트에 보관해 놓는다.
                            this.shaking_cards = this.player_agents[player_index].find_same_cards_from_hand(this.card_from_player.number);
                            this.shaking_cards.Add(this.card_from_player);
                            Debug.Log(shaking_cards.Count);
                        }
                    }
                }
                break;

            case 1:
                {
                    // 폭탄인 경우와 아닌 경우를 구분해서 처리 해 준다.
                    byte count_from_hand = this.player_agents[player_index].get_same_card_count_from_hand(this.card_from_player.number);
                    if (count_from_hand == 2)
                    {
                        this.card_event_type = CARD_EVENT_TYPE.BOMB;

                        get_current_player().plus_shaking_count();

                        // 플레이어가 선택한 카드와, 바닥 카드, 폭탄 카드를 모두 가져 간다.
                        this.cards_to_give_player.Add(this.card_from_player);
                        this.cards_to_give_player.Add(same_cards[0]);
                        this.bomb_cards_from_player.Add(this.card_from_player);
                        List<Card> bomb_cards = this.player_agents[player_index].pop_all_cards_from_hand(this.card_from_player.number);
                        for (int i = 0; i < bomb_cards.Count; ++i)
                        {
                            this.cards_to_give_player.Add(bomb_cards[i]);
                            this.bomb_cards_from_player.Add(bomb_cards[i]);
                        }

                        take_cards_from_others(2);
                        this.player_agents[player_index].add_bomb_count(2);
                    }
                    else
                    {
                        // 뒤집이서 뻑이 나오면 못가져갈 수 있으므로 일단 임시변수에 넣어 놓는다.
                        this.cards_to_give_player.Add(this.card_from_player);
                        this.cards_to_give_player.Add(same_cards[0]);
                    }
                }
                break;

            case 2:
                {
                    if (same_cards[0].pae_type != same_cards[1].pae_type)
                    {
                        // 카드 종류가 다르다면 플레이어가 한장을 선택할 수 있도록 해준다.
                        this.target_cards_to_choice.Clear();
                        for (int i = 0; i < same_cards.Count; ++i)
                        {
                            this.target_cards_to_choice.Add(same_cards[i]);
                        }

                        this.expected_result_type = PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER;
                        return PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER;
                    }
                    else if (same_cards[0].status != same_cards[1].status)//쌍피와 일반피를 선택해야 할 떄 쌍피 자동 선택
                    {
                        if (same_cards[0].status == CARD_STATUS.TWO_PEE)
                        {
                            this.cards_to_give_player.Add(this.card_from_player);//플레이어가 낸 카드와 자동 선택된 카드를 cards_to_give_player에 추가
                            this.cards_to_give_player.Add(same_cards[0]);
                        }
                        else
                        {
                            this.cards_to_give_player.Add(this.card_from_player);
                            this.cards_to_give_player.Add(same_cards[1]);
                        }
                    }
                    else
                    {
                        // 같은 종류의 카드라면 플레이어가 선택할 필요가 없으므로 첫번째 카드를 선택해 준다.
                        this.cards_to_give_player.Add(this.card_from_player);
                        this.cards_to_give_player.Add(same_cards[0]);
                    }
                }
                break;

            case 3:
                {
                    //todo:자뻑인지 구분하여 처리하기.
                    this.card_event_type = CARD_EVENT_TYPE.EAT_PPUK;

                    // 쌓여있는 카드를 모두 플레이어에게 준다.
                    this.cards_to_give_player.Add(this.card_from_player);
                    for (int i = 0; i < same_cards.Count; ++i)
                    {
                        this.cards_to_give_player.Add(same_cards[i]);
                    }

                    //뻑에 쌓여있던 보너스 카드들을 전부 가져간다.
                    FloorSlot slot = this.floor_manager.find_slot(same_cards[0].number);
                    if (slot != null)
                    {
                        List<Card> bonus_cards = slot.get_bonus_card();

                        for (int i = 0; i < bonus_cards.Count; ++i)
                        {
                            Card bonus_card = bonus_cards[i];
                            this.cards_to_give_player.Add(bonus_card);
                            slot.remove_bonus_card(bonus_card);
                        }
                    }

                    byte this_cards_player_index = this.floor_manager.get_player_Index(this.card_from_player.number);

                    if (this.current_player_index != this_cards_player_index)
                    {
                        take_cards_from_others(1);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("자뻑! 카드를 낸 플레이어 " + this_cards_player_index);
                        take_cards_from_others(2);
                    }
                }
                break;
        }
        return PLAYER_SELECT_CARD_RESULT.COMPLETED;
    }

    public PLAYER_SELECT_CARD_RESULT flip_process(byte player_index, TURN_RESULT_TYPE turn_result_type)
    {
        this.turn_result_type = turn_result_type;

        byte card_number_from_player = byte.MaxValue;

        if (this.turn_result_type == TURN_RESULT_TYPE.RESULT_OF_NORMAL_CARD)
        {
            card_number_from_player = this.card_from_player.number;
        }
        PLAYER_SELECT_CARD_RESULT result = flip_deck_card(card_number_from_player);

        if (result == PLAYER_SELECT_CARD_RESULT.BONUSCARD)
        {
            return PLAYER_SELECT_CARD_RESULT.BONUSCARD;
        }
        else if (result == PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK)
        {
            return PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK;
        }

        after_flipped_card(player_index);
        return PLAYER_SELECT_CARD_RESULT.COMPLETED;
    }

    //플렝이어가 카드를 내고 덱의 카드를 뒤집을 때 호출된다.
    PLAYER_SELECT_CARD_RESULT flip_deck_card(byte card_number_from_player)
    {
        this.card_from_deck = pop_front_card();

        if (this.card_from_deck.number == 12)
        {
            if (card_number_from_player == byte.MaxValue)
            {
                Debug.Log("bomb card pick result bonus card");
            }
            else
            {
                this.cards_to_give_player.Add(this.card_from_deck);
            }
            return PLAYER_SELECT_CARD_RESULT.BONUSCARD;//보너스 카드로 리턴해준다.
        }
        //플로어에서 같은 숫자의 카드를 가져온다
        List<Card> same_cards = this.floor_manager.get_cards(this.card_from_deck.number);
        if (same_cards != null)
        {
            this.same_card_count_with_deck = (byte)same_cards.Count;
        }
        else
        {
            this.same_card_count_with_deck = 0;
        }

        switch (this.same_card_count_with_deck)
        {
            case 0:
                {
                    if (card_number_from_player == this.card_from_deck.number)
                    {
                        //내가 낼거나 뒤집을 수 있는 카드가 있을 경우에만 쪽을 인정한다.
                        if (this.player_agents[this.current_player_index].hand_pae.Count + this.player_agents[this.current_player_index].remain_bomb_count > 0)
                        {
                            this.flipped_card_event_type.Add(CARD_EVENT_TYPE.KISS);
                            take_cards_from_others(1);
                        }

                        this.cards_to_give_player.Add(this.card_from_player);
                        this.cards_to_give_player.Add(this.card_from_deck);
                    }
                }
                break;

            case 1:
                {
                    if (card_number_from_player == this.card_from_deck.number)
                    {
                        //플레이어가 첫뻑을 했을 경우
                        if (this.player_agents[this.current_player_index].get_now_player_turn() == 0)
                        {
                            this.flipped_card_event_type.Add(CARD_EVENT_TYPE.START_PPUK);
                            get_current_player().on_start_ppuk();
                        }
                        else
                        {
                            this.flipped_card_event_type.Add(CARD_EVENT_TYPE.PPUK);
                            get_current_player().plus_ppuk_count();
                        }

                        //보너스 카드가 있을 경우 뻑이 된 카드와 함께 쌓이게된다.
                        for (int i = 0; i < cards_to_give_player.Count; i++)
                        {
                            if (cards_to_give_player[i].number == 12)
                            {
                                FloorSlot floor_slot = this.floor_manager.find_slot(this.card_from_deck.number);
                                floor_slot.add_bonus_card(cards_to_give_player[i], this.current_player_index);
                            }
                        }
                        this.cards_to_give_player.Clear();
                    }
                    else
                    {
                        this.cards_to_give_player.Add(this.card_from_deck);
                        this.cards_to_give_player.Add(same_cards[0]);
                    }
                }
                break;

            case 2:
                {
                    //따닥일 경우 플레이어가 4장 모두 가져간다.
                    if (this.card_from_deck.number == card_number_from_player)
                    {
                        if (this.player_agents[this.current_player_index].hand_pae.Count + this.player_agents[this.current_player_index].remain_bomb_count > 0)
                        {
                            this.flipped_card_event_type.Add(CARD_EVENT_TYPE.DDADAK);

                            take_cards_from_others(2);
                        }

                        cards_to_give_player.Clear();

                        for (int i = 0; i < same_cards.Count; ++i)
                        {
                            this.cards_to_give_player.Add(same_cards[i]);
                        }
                        this.cards_to_give_player.Add(this.card_from_deck);
                        this.cards_to_give_player.Add(this.card_from_player);

                        FloorSlot floor_slot = this.floor_manager.find_slot(this.card_from_deck.number);
                        if (floor_slot != null)
                        {
                            List<Card> bonus_cards = floor_slot.get_bonus_card();

                            for (int i = 0; i < bonus_cards.Count; ++i)
                            {
                                Card bonus_card = bonus_cards[i];
                                this.cards_to_give_player.Add(bonus_card);
                                floor_slot.remove_bonus_card(bonus_card);
                            }
                        }
                    }
                    else//카드를 2개중 하나 선택해야하는 경우
                    {
                        if (same_cards[0].pae_type != same_cards[1].pae_type)//두 카드의 카드 타입이 다를 떄
                        {
                            // 뒤집었는데 타입이 다른 카드 두장과 같다면 한장을 선택하도록 한다.
                            this.target_cards_to_choice.Clear();
                            for (int i = 0; i < same_cards.Count; ++i)
                            {
                                this.target_cards_to_choice.Add(same_cards[i]);
                            }

                            this.expected_result_type = PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK;
                            return PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK;
                        }
                        else if (same_cards[0].status != same_cards[1].status)
                        {
                            if (same_cards[0].status == CARD_STATUS.TWO_PEE)
                            {
                                this.cards_to_give_player.Add(this.card_from_deck);
                                this.cards_to_give_player.Add(same_cards[0]);
                            }
                            else
                            {
                                this.cards_to_give_player.Add(this.card_from_deck);
                                this.cards_to_give_player.Add(same_cards[1]);
                            }
                        }
                        else
                        {
                            this.cards_to_give_player.Add(this.card_from_deck);
                            this.cards_to_give_player.Add(same_cards[0]);
                        }
                        // 카드 타입이 같다면 첫번째 카드를 선택해 준다.
                    }
                }
                break;

            case 3:
                {
                    // 플레이어가 4장 모두 가져간다.
                    byte this_cards_player_index = this.floor_manager.get_player_Index(this.card_from_deck.number);

                    if (this.current_player_index != this_cards_player_index)
                    {
                        take_cards_from_others(1);
                        this.flipped_card_event_type.Add(CARD_EVENT_TYPE.EAT_PPUK);
                    }
                    else
                    {
                        take_cards_from_others(2);
                        this.flipped_card_event_type.Add(CARD_EVENT_TYPE.SELF_EAT_PPUK);
                    }

                    this.cards_to_give_player.Add(this.card_from_deck);

                    for (int i = 0; i < same_cards.Count; ++i)
                    {
                        this.cards_to_give_player.Add(same_cards[i]);
                    }

                    FloorSlot slot = this.floor_manager.find_slot(this.card_from_deck.number);

                    if (slot != null)
                    {
                        List<Card> bonus_cards = slot.get_bonus_card();

                        for (int i = 0; i < bonus_cards.Count; ++i)
                        {
                            Card bonus_card = bonus_cards[i];
                            this.cards_to_give_player.Add(bonus_card);
                            slot.remove_bonus_card(bonus_card);
                        }
                    }
                }
                break;
        }
        return PLAYER_SELECT_CARD_RESULT.COMPLETED;
    }

    public void bonus_flip_process(byte player_index)
    {
        bonus_flip_deck_card(player_index);
        calculate_players_score();
    }

    //플레이어가 보너스 카드를 내어 카드를 뒤집는 경우 호출
    void bonus_flip_deck_card(byte player_index)
    {
        this.card_from_deck = pop_front_card();//덱에서 카드를 뽑음
        this.distributed_players_cards[player_index].Add(this.card_from_deck);
        this.player_agents[player_index].add_card_to_hand(this.card_from_deck);
    }

    //최종적으로 카드가 뒤집어진 결과를 확인하는 함수
    void after_flipped_card(byte player_index)
    {
        // 플레이어가 가져갈 카드와 바닥에 내려놓을 카드를 처리한다.
        give_floor_cards_to_player(player_index);

        // 폭탄으로 뒤집는 경우에는 플레이어가 낸 카드가 없으므로 처리를 건너 뛴다.
        if (this.card_from_player != null)
        {
            // 플레이어가 가져갈 카드중에 냈던 카드가 포함되어 있지 않으면 바닥에 내려 놓는다.
            bool is_exist_player = this.cards_to_give_player.Exists(obj => obj.is_same_card(this.card_from_player.number, this.card_from_player.pae_type, this.card_from_player.position));
            if (!is_exist_player)
            {
                this.floor_manager.puton_card(this.card_from_player, this.current_player_index);
                this.cards_to_floor.Add(this.card_from_player);
            }
        }

        if (this.bonus_cards_to_give_player.Count != 0)
        {
            for (int i = 0; i < this.bonus_cards_to_give_player.Count; i++)
            {
                this.floor_manager.puton_bonus_card(this.bonus_cards_to_give_player[i], this.card_from_player.number, this.current_player_index);
                this.cards_to_floor.Add(this.card_from_player);
            }
        }

        // 뒤집은 카드에 대해서도 같은 방식으로 처리한다.
        bool is_exist_deck_card = this.cards_to_give_player.Exists(obj => obj.is_same_card(this.card_from_deck.number, this.card_from_deck.pae_type, this.card_from_deck.position));
        if (!is_exist_deck_card)
        {
            this.floor_manager.puton_card(this.card_from_deck, this.current_player_index);
            this.cards_to_floor.Add(this.card_from_deck);
        }

        // 싹쓸이 체크.
        if (this.floor_manager.is_empty())
        {
            if (this.player_agents[player_index].hand_pae.Count + this.player_agents[this.current_player_index].remain_bomb_count > 0)
            {
                this.flipped_card_event_type.Add(CARD_EVENT_TYPE.CLEAN);
                take_cards_from_others(1);
            }
        }
        calculate_players_score();
    }

    //바닥의 카드를 플레이어에게 준다.
    public void give_floor_cards_to_player(byte player_index)
    {
        for (int i = 0; i < this.cards_to_give_player.Count; ++i)
        {
            this.player_agents[player_index].add_card_to_floor(this.cards_to_give_player[i]);

            this.floor_manager.remove_card(this.cards_to_give_player[i]);
        }
    }

    public void give_bomb_bonus_cards_to_player(byte player_index, Card card)
    {
        this.player_agents[player_index].add_card_to_floor(card);
    }

    public PLAYER_SELECT_CARD_RESULT on_choose_card(byte player_index, PLAYER_SELECT_CARD_RESULT result_type, byte choice_index)
    {
        if (result_type != this.expected_result_type)
        {
            //받아야하는 타입이 아닌 오류로 인한 경우 
            Debug.Log(string.Format("Invalid result type. client {0}, expected {1}", result_type, this.expected_result_type));
        }

        // 클라이언트에서 엉뚱한 값을 보내올 수 있으므로 검증 후 이상이 있으면 첫번째 카드를 선택한다.
        Card player_choose_card = null;
        if (this.target_cards_to_choice.Count <= choice_index)
        {
            player_choose_card = this.target_cards_to_choice[0];
        }
        else
        {
            player_choose_card = this.target_cards_to_choice[choice_index];
        }

        PLAYER_SELECT_CARD_RESULT ret = PLAYER_SELECT_CARD_RESULT.COMPLETED;
        switch (this.expected_result_type)
        {
            case PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_PLAYER:
                this.cards_to_give_player.Add(this.card_from_player);
                this.cards_to_give_player.Add(player_choose_card);
                return this.expected_result_type;

            case PLAYER_SELECT_CARD_RESULT.CHOICE_ONE_CARD_FROM_DECK:
                this.cards_to_give_player.Add(this.card_from_deck);
                this.cards_to_give_player.Add(player_choose_card);
                after_flipped_card(player_index);
                break;
        }
        return ret;
    }

    //상대방의 카드를 뺏어온다.
    void take_cards_from_others(byte pee_count)
    {
        PlayerAgent attacker = this.player_agents[this.current_player_index];

        byte next_player = get_next_player_index();
        PlayerAgent victim = this.player_agents[next_player];

        if (!this.other_cards_to_player.ContainsKey(next_player))
        {
            this.other_cards_to_player.Add(next_player, new List<Card>());
        }

        List<Card> cards = victim.pop_card_from_floor(pee_count);

        if (cards == null)
        {
            return;
        }

        for (int i = 0; i < cards.Count; ++i)
        {
            attacker.add_card_to_floor(cards[i]);
            this.other_cards_to_player[next_player].Add(cards[i]);
        }
    }

    //플레이어들의 점수를 계산한다.
    void calculate_players_score()
    {
        for (int i = 0; i < this.player_agents.Count; ++i)
        {
            this.player_agents[i].calculate_score();
        }
    }

    //해당 플레이어가 고스톱을 결정할 점수가 되었는지 확인한다.
    public bool is_time_to_ask_gostop()
    {
        return this.player_agents[this.current_player_index].can_finish();
    }

    //해당 플레이어가 국진을 가지고있는지 확인한다.
    public bool has_kookjin(byte player_index)
    {
        if (this.player_agents[player_index].is_used_kookjin)
        {
            return false;
        }

        byte kookjin = this.player_agents[player_index].get_card_count(PAE_TYPE.YEOL, CARD_STATUS.KOOKJIN);
        if (kookjin <= 0)
        {
            return false;
        }

        return true;
    }

    //해당 플레이어의 PlayerAgent를 가져오는 함수
    PlayerAgent get_current_player()
    {
        return this.player_agents[this.current_player_index];
    }

    public void add_nagari()
    {
        nagari_value++;
    }

    public byte get_nagari()
    {
        return nagari_value;
    }

    public void reset_nagari()
    {
        nagari_value = 0;
    }

    #region get_next_player
    byte MAX_PLAYER_COUNT = 2;
    public byte get_next_player_index()
    {
        if (this.current_player_index < MAX_PLAYER_COUNT - 1)
        {
            return (byte)(this.current_player_index + 1);
        }

        return 0;
    }

    public byte find_next_player_index_of(byte prev_player_index)
    {
        if (prev_player_index < MAX_PLAYER_COUNT - 1)
        {
            return (byte)(prev_player_index + 1);
        }
        return 0;
    }

    public void move_to_next_player()
    {
        get_current_player().add_turn();
        this.current_player_index = get_next_player_index();
    }
    #endregion
}

public class PlayerPickCard
{
    public byte player;
    public byte card_slot;
    public Card card;

    public PlayerPickCard(byte player_index, byte slot_indx, Card pick_card)
    {
        this.player = player_index;
        this.card_slot = slot_indx;
        this.card = pick_card;
    }
}