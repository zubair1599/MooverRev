using System.Collections.Generic;
using System.Text;
using Business.FirstData;

namespace Business.Models
{
    public partial class Account_CreditCard
    {
        public FirstDataCardTypes GetCardType()
        {
            if (this.CardType.ToLower() == "visa")
            {
                return FirstDataCardTypes.Visa;
            }

            if (this.CardType.ToLower() == "american express")
            {
                return FirstDataCardTypes.Amex;
            }

            if (this.CardType.ToLower() == "jcb")
            {
                return FirstDataCardTypes.JCB;
            }

            if (this.CardType.ToLower() == "discover")
            {
                return FirstDataCardTypes.Discover;
            }

            if (this.CardType.ToLower() == "mastercard")
            {
                return FirstDataCardTypes.Mastercard;
            }

            return FirstDataCardTypes.DinersClub;
        }

        public void SetCard(string val)
        {
            this.Card = Utility.Security.Encrypt(val);
        }

        public string GetCard()
        {
            return Utility.Security.Decrypt(this.Card);
        }

        public void SetCardHolder(string val)
        {
            this.CardHolder = Utility.Security.Encrypt(val);
        }

        public string GetCardHolder()
        {
            return Utility.Security.Decrypt(this.CardHolder);
        }

        public void SetExpiration(string val)
        {
            this.Expiration = Utility.Security.Encrypt(val);
        }

        public string GetExpiration()
        {
            return Utility.Security.Decrypt(this.Expiration);
        }

        public string DisplayCard()
        {
            var card = this.GetCard();
            return card.Substring(card.Length - 4).PadLeft(card.Length, '*');
        }

        public string GetLast4()
        {
            return this.GetCard().Substring(this.GetCard().Length - 4);
        }

        public string DisplayExpiration()
        {
            return this.GetExpiration().Substring(0, 2) + "/" + this.GetExpiration().Substring(2, 2);
        }

        public object ToJsonObject()
        {
            return new
            {
                ID = this.CreditCardID,
                CardType = this.CardType,
                DisplayCard = this.DisplayCard(),
                DisplayExpiration = this.DisplayExpiration(),
                Expiration = this.Expiration,
                AccountID = this.AccountID
            };
        }
    }
}
