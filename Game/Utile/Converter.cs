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
                    return "초1";
                }

            case TIER.CHO2:
                {
                    return "초2";
                }

            case TIER.CHO3:
                {
                    return "초3";
                }

            case TIER.CHO4:
                {
                    return "초4";
                }

            case TIER.CHO5:
                {
                    return "초5";
                }

            case TIER.CHO6:
                {
                    return "초6";
                }

            case TIER.JONG1:
                {
                    return "중1";
                }

            case TIER.JONG2:
                {
                    return "중2";
                }

            case TIER.JONG3:
                {
                    return "중3";
                }

            case TIER.KO1:
                {
                    return "고1";
                }

            case TIER.KO2:
                {
                    return "고2";
                }

            case TIER.KO3:
                {
                    return "고3";
                }

            case TIER.DE1:
                {
                    return "대1";
                }

            case TIER.DE2:
                {
                    return "대2";
                }

            case TIER.DE3:
                {
                    return "대3";
                }

            case TIER.DE4:
                {
                    return "대4";
                }

            case TIER.SEOKSA:
                {
                    return "석사";
                }

            case TIER.BACKSA:
                {
                    return "박사";
                }
        }
        return "초1";
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
                    return "10대";
                }

            case OLD.TWENTY:
                {
                    return "20대";
                }

            case OLD.THIRTY:
                {
                    return "30대";
                }

            case OLD.FORTY:
                {
                    return "40대";
                }

            case OLD.FIFTY:
                {
                    return "50대";
                }

            case OLD.SIXTY:
                {
                    return "60대";
                }

            case OLD.SEVENTY:
                {
                    return "70대 이상";
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
                    return "남";
                }

            case GENDER.FEMALE:
                {
                    return "여";
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
        return "일반";
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
                    return "1억냥";
                }
            case PRODUCT.Million5:
                {
                    return "5억냥";
                }
            case PRODUCT.Million10:
                {
                    return "10억냥";
                }
            case PRODUCT.Million50:
                {
                    return "50억냥";
                }

            case PRODUCT.Key1:
                {
                    return "황금열쇠 1개";
                }
            case PRODUCT.Key5:
                {
                    return "황금열쇠 5개";
                }
            case PRODUCT.Key10:
                {
                    return "황금열쇠 10개";
                }

            case PRODUCT.VIP:
                {
                    return "VIP회원권";
                }
            case PRODUCT.VVIP:
                {
                    return "VVIP회원권";
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
                    return "돈";
                }
            case ITEM.KEY:
                {
                    return "황금열쇠";
                }
            case ITEM.VIP_COUPON:
                {
                    return "VIP회원권";
                }
            case ITEM.VVIP_COUPON:
                {
                    return "VVIP회원권";
                }
            case ITEM.BRONZE_ELEPHANT:
                {
                    return "청동 코끼리상자";
                }
            case ITEM.SLIVER_ELEPHANT:
                {
                    return "청은 코끼리상자";
                }
            case ITEM.GOLDEN_ELEPHANT:
                {
                    return "황금 코끼리상자";
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
                    return "게임을 하거나 상점에서 아이템을 구매할 때 주로 사용되는 재화입니다";
                }
            case ITEM.KEY:
                {
                    return "상자를 열기 위해 사용되는 재화입니다";
                }
            case ITEM.VIP_COUPON:
                {
                    return "1달간 유지되며 매일마다 1억 5000냥과 열쇠 1개의 혜택을 받을 수 있습니다";
                }
            case ITEM.VVIP_COUPON:
                {
                    return "1달간 유지되며 매일마다 5억냥과 열쇠 3개의 더욱 많은 혜택을 받을 수 있습니다";
                }
            case ITEM.BRONZE_ELEPHANT:
                {
                    return "초등 등급이상만 1개의 황금열쇠로 열 수 있으며 낮은 확률로 큰 금액을 받을 수 있습니다\n최소 획득 금액 1억5000만냥\n최대 획득 금액 300억냥";
                }
            case ITEM.SLIVER_ELEPHANT:
                {
                    return "중등 등급이상만 2개의 황금열쇠로 열 수 있으며 낮은 확률로 매우 큰 금액을 받을 수 있습니다\n최소 획득 금액 3억냥\n최대 획득 금액 700억냥";
                }
            case ITEM.GOLDEN_ELEPHANT:
                {
                    return "고등 등급이상만 3개의 황금열쇠로 열 수 있으며 낮은 확률로 전설적인 큰 금액을 받을 수 있습니다\n최소 획득 금액 5억냥\n최대 획득 금액 1500억냥";
                }
        }
        return "";
    }
}
