using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public class TierManager
    {
        public static string TierCheck(int tier)
        {
            switch (tier)
            {
                case 3:
                    {
                        return "하수";
                    }
                case 4:
                    {
                        return "중수";
                    }
                case 5:
                    {
                        return "고수";
                    }
                case 6:
                    {
                        return "장인";
                    }
                case 7:
                    {
                        return "전설";
                    }
                case 8:
                    {
                        return "신선";
                    }
                case 9:
                    {
                        return "신";
                    }
            }
            return "";
        }

        public int StringTierCheck(string tier)
        {
            switch (tier)
            {
                case "하수":
                    {
                        return 3;
                    }
                case "중수":
                    {
                        return 4;
                    }
                case "고수":
                    {
                        return 5;
                    }
                case "장인":
                    {
                        return 6;
                    }
                case "전설":
                    {
                        return 7;
                    }
                case "신선":
                    {
                        return 8;
                    }
                case "신":
                    {
                        return 9;
                    }
            }
            return 0;
        }
    }
}
