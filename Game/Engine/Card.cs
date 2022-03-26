using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ī�� Ÿ��
public enum PAE_TYPE : byte
{
    PEE,
    KWANG,
    TEE,
    YEOL
}

//ī�� �Ӽ�
public enum CARD_STATUS : byte
{
    NONE,
    GODORI,     // ����
    TWO_PEE,        // ����
    CHEONG_DAN,     // û��
    HONG_DAN,       // ȫ��
    CHO_DAN,        // �ʴ�
    KOOKJIN,        // ����
    BONUS_PEE       //���ʽ���
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

    //������ ���Ƿ� ��ȯ�� �� ���
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
