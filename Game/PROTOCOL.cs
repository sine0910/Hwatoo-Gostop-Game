using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �������� ����.
// �������� Ŭ���̾�Ʈ�� ���� ��Ŷ : S -> C
// Ŭ���̾�Ʈ���� ������ ���� ��Ŷ : C -> S
public enum PROTOCOL : short
{
    // �ý��� ��������.
    READY_TO_SERVER_START = 1,   // S -> C//
    LOCAL_SERVER_STARTED = 2,
    // ���� ��������.
    READY_TO_START = 10,        // C -> S
    BEGIN_CARD_INFO = 11,       // S -> C//
    DISTRIBUTED_ALL_CARDS = 12, // C -> S

    SELECT_CARD_REQ = 13,       // C -> S
    SELECT_CARD_ACK = 14,       // S -> C//

    // �÷��̾ �ΰ��� ī�� �� �ϳ��� �����ؾ� �ϴ� ���.
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

// �÷��̾ �� ī�忡 ���� ���.
public enum PLAYER_SELECT_CARD_RESULT : byte
{
    // �Ϸ�.
    COMPLETED,

    // ī�� ���� �����ؾ� ��(�÷��̾ �� ���).
    CHOICE_ONE_CARD_FROM_PLAYER,

    // ī�� ���� �����ؾ� ��(������ ������ ���).
    CHOICE_ONE_CARD_FROM_DECK,

    //�� ī�带 ���� �ش� �÷��̾�� �־�� ��
    BONUSCARD,

    // ����.
    ERROR_INVALID_CARD
}

// ī�� �̺�Ʈ ����.
public enum CARD_EVENT_TYPE : byte
{
    NONE,

    // ��.
    KISS,

    // ��.
    PPUK,

    // ����.
    DDADAK,

    // ��ź.
    BOMB,

    // �Ͼ���.
    CLEAN,

    // �� �Ա�.
    EAT_PPUK,

    // �ڻ�.
    SELF_EAT_PPUK,

    // ����.
    SHAKING,

    //�̺�Ʈ ��
    BONUSCARD,

    //ù ��
    START_PPUK
}


public enum TURN_RESULT_TYPE : byte
{
    // �Ϲ� ī�带 �� ���� ���.
    RESULT_OF_NORMAL_CARD,

    // ��ź ī�带 �� ���� ���.
    RESULT_OF_BOMB_CARD,

    STARTBONUSCARD,

    BONUSCARD
}

