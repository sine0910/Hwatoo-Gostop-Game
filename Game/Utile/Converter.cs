using System;
using UnityEngine;

public static class Converter
{
    public static PAE_TYPE PaeType(string pea_type)
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
        Debug.Log("Converter error PaeType " + pea_type);
        return 0;
    }

    public static CARD_EVENT_TYPE EventType(string event_type)
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

    public static PLAYER_SELECT_CARD_RESULT Card_Result(string card_result)
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

    public static TIER Tier(string tier)
    {
        switch (tier)
        {
            case "CHO1":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO1");
                }

            case "CHO2":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO2");
                }

            case "CHO3":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO3");
                }

            case "CHO4":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO4");
                }

            case "CHO5":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO5");
                }

            case "CHO6":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "CHO6");
                }

            case "JONG1":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "JONG1");
                }

            case "JONG2":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "JONG2");
                }

            case "JONG3":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "JONG3");
                }

            case "KO1":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "KO1");
                }

            case "KO2":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "KO2");
                }

            case "KO3":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "KO3");
                }

            case "DE1":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "DE1");
                }

            case "DE2":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "DE2");
                }

            case "DE3":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "DE3");
                }

            case "DE4":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "DE4");
                }

            case "SEOKSA":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "SEOKSA");
                }

            case "HACKSA":
                {
                    return (TIER)Enum.Parse(typeof(TIER), "HACKSA");
                }
        }
        return 0;
    }

    public static string TierToString(TIER tier)
    {
        switch (tier)
        {
            case TIER.CHO1:
                {
                    return "��1";
                }

            case TIER.CHO2:
                {
                    return "��2";
                }

            case TIER.CHO3:
                {
                    return "��3";
                }

            case TIER.CHO4:
                {
                    return "��4";
                }

            case TIER.CHO5:
                {
                    return "��5";
                }

            case TIER.CHO6:
                {
                    return "��6";
                }

            case TIER.JONG1:
                {
                    return "��1";
                }

            case TIER.JONG2:
                {
                    return "��2";
                }

            case TIER.JONG3:
                {
                    return "��3";
                }

            case TIER.KO1:
                {
                    return "��1";
                }

            case TIER.KO2:
                {
                    return "��2";
                }

            case TIER.KO3:
                {
                    return "��3";
                }

            case TIER.DE1:
                {
                    return "��1";
                }

            case TIER.DE2:
                {
                    return "��2";
                }

            case TIER.DE3:
                {
                    return "��3";
                }

            case TIER.DE4:
                {
                    return "��4";
                }

            case TIER.SEOKSA:
                {
                    return "����";
                }

            case TIER.BACKSA:
                {
                    return "�ڻ�";
                }
        }
        return "��1";
    }

    public static OLD Old(string old)
    {
        switch (old)
        {
            case "TEN":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "TEN");
                }

            case "TWENTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "TWENTY");
                }

            case "THIRTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "THIRTY");
                }

            case "FORTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "FORTY");
                }

            case "FIFTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "FIFTY");
                }

            case "SIXTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "SIXTY");
                }

            case "SEVENTY":
                {
                    return (OLD)Enum.Parse(typeof(OLD), "SEVENTY");
                }
        }
        return OLD.NONE;
    }

    public static string OldToString(OLD old)
    {
        switch (old)
        {
            case OLD.TEN:
                {
                    return "10��";
                }

            case OLD.TWENTY:
                {
                    return "20��";
                }

            case OLD.THIRTY:
                {
                    return "30��";
                }

            case OLD.FORTY:
                {
                    return "40��";
                }

            case OLD.FIFTY:
                {
                    return "50��";
                }

            case OLD.SIXTY:
                {
                    return "60��";
                }

            case OLD.SEVENTY:
                {
                    return "70�� �̻�";
                }
        }
        return "";
    }

    public static GENDER Gender(string gender)
    {
        switch (gender)
        {
            case "MALE":
                {
                    return (GENDER)Enum.Parse(typeof(GENDER), "MALE");
                }

            case "FEMALE":
                {
                    return (GENDER)Enum.Parse(typeof(GENDER), "FEMALE");
                }
        }
        return GENDER.NONE;
    }

    public static string GenderToString(GENDER gender)
    {
        switch (gender)
        {
            case GENDER.MALE:
                {
                    return "��";
                }

            case GENDER.FEMALE:
                {
                    return "��";
                }
        }
        return "";
    }

    public static PLAYER_RATING PlayerRating(string player_rating)
    {
        switch (player_rating)
        {
            case "VIP":
                {
                    return PLAYER_RATING.VIP;
                }

            case "VVIP":
                {
                    return PLAYER_RATING.VVIP;
                }
        }
        return PLAYER_RATING.NOMAL;
    }

    public static string PlayerRatingToString(PLAYER_RATING player_rating)
    {
        switch (player_rating)
        {
            case PLAYER_RATING.VIP:
                {
                    return "VIP";
                }

            case PLAYER_RATING.VVIP:
                {
                    return "VVIP";
                }
        }
        return "�Ϲ�";
    }

    public static ITEM Item(string item)
    {
        switch (item)
        {
            case "MONEY":
                {
                    return ITEM.MONEY;
                }

            case "KEY":
                {
                    return ITEM.KEY;
                }
        }
        return 0;
    }

    public static string PurchaseItem(PRODUCT item)
    {
        switch (item)
        {
            case PRODUCT.Million1:
                {
                    return "1���";
                }
            case PRODUCT.Million5:
                {
                    return "5���";
                }
            case PRODUCT.Million10:
                {
                    return "10���";
                }
            case PRODUCT.Million50:
                {
                    return "50���";
                }

            case PRODUCT.Key1:
                {
                    return "Ȳ�ݿ��� 1��";
                }
            case PRODUCT.Key5:
                {
                    return "Ȳ�ݿ��� 5��";
                }
            case PRODUCT.Key10:
                {
                    return "Ȳ�ݿ��� 10��";
                }

            case PRODUCT.VIP:
                {
                    return "VIPȸ����";
                }
            case PRODUCT.VVIP:
                {
                    return "VVIPȸ����";
                }
        }
        return "";
    }

    public static string ItemName(ITEM item)
    {
        switch (item)
        {
            case ITEM.MONEY:
                {
                    return "��";
                }
            case ITEM.KEY:
                {
                    return "Ȳ�ݿ���";
                }
            case ITEM.VIP_COUPON:
                {
                    return "VIPȸ����";
                }
            case ITEM.VVIP_COUPON:
                {
                    return "VVIPȸ����";
                }
            case ITEM.BRONZE_ELEPHANT:
                {
                    return "û�� �ڳ�������";
                }
            case ITEM.SLIVER_ELEPHANT:
                {
                    return "û�� �ڳ�������";
                }
            case ITEM.GOLDEN_ELEPHANT:
                {
                    return "Ȳ�� �ڳ�������";
                }
        }
        return "";
    }

    public static string ItemInfo(ITEM item)
    {
        switch (item)
        {
            case ITEM.MONEY:
                {
                    return "������ �ϰų� �������� �������� ������ �� �ַ� ���Ǵ� ��ȭ�Դϴ�";
                }
            case ITEM.KEY:
                {
                    return "���ڸ� ���� ���� ���Ǵ� ��ȭ�Դϴ�";
                }
            case ITEM.VIP_COUPON:
                {
                    return "1�ް� �����Ǹ� ���ϸ��� 1�� 5000�ɰ� ���� 1���� ������ ���� �� �ֽ��ϴ�";
                }
            case ITEM.VVIP_COUPON:
                {
                    return "1�ް� �����Ǹ� ���ϸ��� 5��ɰ� ���� 3���� ���� ���� ������ ���� �� �ֽ��ϴ�";
                }
            case ITEM.BRONZE_ELEPHANT:
                {
                    return "�ʵ� ����̻� 1���� Ȳ�ݿ���� �� �� ������ ���� Ȯ���� ū �ݾ��� ���� �� �ֽ��ϴ�\n�ּ� ȹ�� �ݾ� 1��5000����\n�ִ� ȹ�� �ݾ� 300���";
                }
            case ITEM.SLIVER_ELEPHANT:
                {
                    return "�ߵ� ����̻� 2���� Ȳ�ݿ���� �� �� ������ ���� Ȯ���� �ſ� ū �ݾ��� ���� �� �ֽ��ϴ�\n�ּ� ȹ�� �ݾ� 3���\n�ִ� ȹ�� �ݾ� 700���";
                }
            case ITEM.GOLDEN_ELEPHANT:
                {
                    return "��� ����̻� 3���� Ȳ�ݿ���� �� �� ������ ���� Ȯ���� �������� ū �ݾ��� ���� �� �ֽ��ϴ�\n�ּ� ȹ�� �ݾ� 5���\n�ִ� ȹ�� �ݾ� 1500���";
                }
        }
        return "";
    }
}
