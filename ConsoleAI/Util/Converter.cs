using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    class Converter
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
    }
}
