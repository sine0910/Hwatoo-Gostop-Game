using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 프로토콜 정의.
// 서버에서 클라이언트로 가는 패킷 : S -> C
// 클라이언트에서 서버로 가는 패킷 : C -> S
public enum PROTOCOL : short
{
    // 시스템 프로토콜.
    READY_TO_SERVER_START = 1,   // S -> C//
    LOCAL_SERVER_STARTED = 2,
    // 게임 프로토콜.
    READY_TO_START = 10,        // C -> S
    BEGIN_CARD_INFO = 11,       // S -> C//
    DISTRIBUTED_ALL_CARDS = 12, // C -> S

    SELECT_CARD_REQ = 13,       // C -> S
    SELECT_CARD_ACK = 14,       // S -> C//

    // 플레이어가 두개의 카드 중 하나를 선택해야 하는 경우.
    CHOOSE_CARD = 15,			// C -> S

    FLIP_BOMB_CARD_REQ = 17,	// C -> S

    FLIP_DECK_CARD_REQ = 18,    // C -> S
    FLIP_DECK_CARD_ACK = 19,    // S -> C//

    TURN_RESULT = 20,           // S -> C//

    ASK_GO_OR_STOP = 21,        // S -> C//
    ANSWER_GO_OR_STOP = 22,     // C -> S

    UPDATE_PLAYER_STATISTICS = 23,  // S -> C

    ASK_KOOKJIN_TO_PEE = 24,        // S -> C//
    ANSWER_KOOKJIN_TO_PEE = 25,     // C -> S

    MOVE_KOOKJIN_TO_PEE = 26,       // S -> C

    GAME_RESULT = 27,  // S -> C//

    START_BONUSPEE = 33,

    SELECT_BONUS_CARD_ACK = 44,
    FLIP_DECK_BONUS_CARD_REQ = 45,
    FLIP_DECK_BONUS_CARD_ACK = 46,

    FLIP_PLUS_BONUS_CARD_ACK = 55,
    FLIP_BOMB_BONUS_CARD_REQ = 56,
    FLIP_BOMB_BONUS_CARD_ACK = 57,

    START_TURN = 98,
    BONUS_START = 77,
    BONUS_TURN = 99,
    TURN_END = 100,

    SET_START_USER = 81,//R
    ON_SET_START_USER = 82,//U
    READY_TO_USER_SELET_START = 83,//R
    START_USER_SELET_START = 84,//U
    SET_START_USER_SELET_START = 85,//R
    SET_START_USER_CARD_REQ = 86,//U
    SET_START_USER_CARD_ACK = 87,//R
    SET_START_USER_TURN_END = 88,//U
    END_SET_START_USER = 89,//U

    END
}

// 플레이어가 낸 카드에 대한 결과.
public enum PLAYER_SELECT_CARD_RESULT : byte
{
    // 완료.
    COMPLETED,

    // 카드 한장 선택해야 함(플레이어가 낸 경우).
    CHOICE_ONE_CARD_FROM_PLAYER,

    // 카드 한장 선택해야 함(덱에서 뒤집은 경우).
    CHOICE_ONE_CARD_FROM_DECK,

    //덱 카드를 한장 해당 플레이어에게 주어야 함
    BONUSCARD,

    // 에러.
    ERROR_INVALID_CARD
}

// 카드 이벤트 정의.
public enum CARD_EVENT_TYPE : byte
{
    NONE,

    // 쪽.
    KISS,

    // 뻑.
    PPUK,

    // 따닥.
    DDADAK,

    // 폭탄.
    BOMB,

    // 싹쓸이.
    CLEAN,

    // 뻑 먹기.
    EAT_PPUK,

    // 자뻑.
    SELF_EAT_PPUK,

    // 흔들기.
    SHAKING,

    //이벤트 피
    BONUSCARD,

    //첫 뻑
    START_PPUK
}


public enum TURN_RESULT_TYPE : byte
{
    // 일반 카드를 낸 후의 결과.
    RESULT_OF_NORMAL_CARD,

    // 폭탄 카드를 낸 후의 결과.
    RESULT_OF_BOMB_CARD,

    STARTBONUSCARD,

    BONUSCARD
}

