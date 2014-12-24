using System;
using System.Collections.Generic;
using System.Text;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class PasswordReset
    {
        private const int KeyLength = 12;

        /// <summary>
        /// try to make password reset keys fairly typable/readable
        /// </summary>
        private static readonly char[] ValidCharacters = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'k',
            'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
            'x', 'y', 'z', '2', '3', '4', '5', '6', '9'
        };

        private static string GenerateResetKey()
        {
            var sb = new StringBuilder();
            var rand = new Random();
            while (sb.Length < KeyLength)
            {
                lock (rand)
                {
                    sb.Append(ValidCharacters[rand.Next(0, ValidCharacters.Length - 1)]);
                }
            }

            return sb.ToString();
        }

        public PasswordReset()
        {
            var repo = new PasswordResetRepository();
            string key = String.Empty;
            while (String.IsNullOrEmpty(key) || repo.Get(key) != null)
            {
                key = GenerateResetKey();
            }

            this.ResetKey = key;
            this.DateRequested = DateTime.Now;
        }
    }
}
