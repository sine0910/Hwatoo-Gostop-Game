using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카드 타입
public enum PAE_TYPE : byte
{
    PEE,
    KWANG,
    TEE,
    YEOL
}

//카드 속성
public enum CARD_STATUS : byte
{
    NONE,
    GODORI,     // 고도리
    TWO_PEE,        // 쌍피
    CHEONG_DAN,     // 청단
    HONG_DAN,       // 홍단
    CHO_DAN,        // 초단
    KOOKJIN,        // 국진
    BONUS_PEE       //보너스피
}

public class Card
{
    public byte number { get; private set; }
    public PAE_TYPE pae_type { get; private set; }
    public byte position { get; private set; }

    public CARD_STATUS status { get; private set; }

    public Card(byte number, PAE_TYPE pae_type, byte position)
    {
        this.number = number;
        this.pae_type = pae_type;
        this.position = position;
        this.status = CARD_STATUS.NONE;
    }

    public void set_card_status(CARD_STATUS status)
    {
        this.status = status;
    }

    //국진을 쌍피로 변환할 때 사용
    public void change_pae_type(PAE_TYPE pae_type_to_change)
    {
        this.pae_type = pae_type_to_change;
    }

    public bool is_same_card(byte number, PAE_TYPE pae_type, byte position)
    {
        return this.number == number && this.pae_type == pae_type && this.position == position;
    }

    public bool is_same_number(byte number)
    {
        return this.number == number;
    }

    public bool is_same_status(CARD_STATUS status)
    {
        return this.status == status;
    }
}
