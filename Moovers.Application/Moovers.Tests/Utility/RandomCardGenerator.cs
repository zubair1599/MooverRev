using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moovers.Tests.Utility
{
    public enum CreditCardType
    {
        /// <summary>American Express</summary>
        AMEX,

        /// <summary>Diner's Club</summary>
        DINERS,

        /// <summary>Diner's Club Enroute</summary>
        ENROUTE,

        /// <summary>JCB 15 Digit</summary>
        JCB_15,

        /// <summary>JCB 16 Digit</summary>
        JCB_16,

        DISCOVER,
        MASTERCARD,
        VISA,
        VOYAGER
    }

    public static class RandomCreditCardNumberGenerator
    {
        private static readonly Random Random = new Random();

        private static readonly string[] AMEX_PREFIX_LIST = { "34", "37" };

        private static readonly string[] DINERS_PREFIX_LIST = { "300", "301", "302", "303", "36", "38" };

        private static readonly string[] DISCOVER_PREFIX_LIST = { "6011" };

        private static readonly string[] ENROUTE_PREFIX_LIST = { "2014", "2149" };

        private static readonly string[] JCB_15_PREFIX_LIST = { "2100", "1800" };

        private static readonly string[] JCB_16_PREFIX_LIST = { "3088", "3096", "3112", "3158", "3337", "3528" };

        private static readonly string[] MASTERCARD_PREFIX_LIST =  { "51", "52", "53", "54", "55" };

        private static readonly string[] VISA_PREFIX_LIST = { "4539", "4556", "4916", "4532", "4929", "40240071", "4485", "4716", "4" };

        private static readonly string[] VOYAGER_PREFIX_LIST = { "8699" };

        private static string GetRandomPrefix(CreditCardType type)
        {
            var list = new string[0];
            switch (type)
            {
                case CreditCardType.AMEX:
                    list = AMEX_PREFIX_LIST;
                    break;
                case CreditCardType.DINERS:
                    list = DINERS_PREFIX_LIST;
                    break;
                case CreditCardType.DISCOVER:
                    list = DISCOVER_PREFIX_LIST;
                    break;
                case CreditCardType.ENROUTE:
                    list = ENROUTE_PREFIX_LIST;
                    break;
                case CreditCardType.JCB_15:
                    list = JCB_15_PREFIX_LIST;
                    break;
                case CreditCardType.JCB_16:
                    list = JCB_16_PREFIX_LIST;
                    break;
                case CreditCardType.MASTERCARD:
                    list = MASTERCARD_PREFIX_LIST;
                    break;
                case CreditCardType.VISA:
                    list = VISA_PREFIX_LIST;
                    break;
                case CreditCardType.VOYAGER:
                    list = VOYAGER_PREFIX_LIST;
                    break;
            }

            lock (Random)
            {
                return list[Random.Next(list.Count())];
            }
        }

        private static string CreateFakeCreditCardNumber(string prefix, int length)
        {
            string ccnumber = prefix;

            while (ccnumber.Length < (length - 1))
            {
                lock (Random)
                {
                    double rnd = (Random.NextDouble() * 1.0f - 0f);
                    ccnumber += Math.Floor(rnd * 10);
                    System.Threading.Thread.Sleep(20);
                }
            }

            // reverse number and convert to int
            var reversedCCnumberstring = ccnumber.ToCharArray().Reverse();
            var reversedCCnumberList = reversedCCnumberstring.Select(c => Convert.ToInt32(c.ToString()));

            // calculate sum
            int sum = 0;
            int pos = 0;
            int[] reversedCCnumber = reversedCCnumberList.ToArray();
            while (pos < length - 1)
            {
                int odd = reversedCCnumber[pos] * 2;

                if (odd > 9)
                {
                    odd -= 9;
                }

                sum += odd;
                if (pos != (length - 2))
                {
                    sum += reversedCCnumber[pos + 1];
                }

                pos += 2;
            }

            // calculate check digit
            int checkdigit = Convert.ToInt32((Math.Floor((decimal)sum / 10) + 1) * 10 - sum) % 10;
            ccnumber += checkdigit;
            return ccnumber;
        }

        public static string GetInvalidNumber()
        {
            var tmp = CreateFakeCreditCardNumber("11", 16);
            var ret = tmp.Substring(0, tmp.Length - 1);
            var end = (int.Parse(tmp.Substring(tmp.Length - 1)) + 1).ToString().Substring(0, 1);
            return ret + end;
        }

        public static string GetNumber(CreditCardType type)
        {
            string prefix = GetRandomPrefix(type);
            var length = 16;
            if (type == CreditCardType.JCB_15 || type == CreditCardType.AMEX)
            {
                length = 15;
            }

            return CreateFakeCreditCardNumber(prefix, length);
        }
    }
}
