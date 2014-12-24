using System;
using System.Collections.Generic;
using System.Text;
using Business.Utility;

namespace Business.Models
{
    public struct TransactionRequestStore
    {
        private string _card_number;

        public string Card_Number 
        { 
            get
            {
                return _card_number; 
            }

            set 
            {
                if (String.IsNullOrEmpty(value))
                {
                    _card_number = value;
                    return;
                }

                _card_number = value.Substring(value.Length - 4).PadLeft(value.Length, '*');
            }
        }

        public string CardType { get; set; }

        public string Currency { get; set; }

        public string Transaction_Type { get; set; }

        public string Expiry_Date { get; set; }

        public string CardHoldersName { get; set; }

        public string VerificationStr2
        {
            get { return "XXX"; }

            set { }
        }

        public string CVD_Presence_Ind { get; set; }
        
        public string ZipCode
        {
            get { return "XXXXX"; }

            set { }
        }

        public string TransArmorToken { get; set; }

        public string Customer_Ref { get; set; }

        public string Reference_No { get; set; }

        public string DollarAmount { get; set; }
    }

    public struct TransactionResultStore
    {
        public bool Transaction_Error { get; set; }

        public bool Transaction_Approved { get; set; }

        public string EXact_Resp_Code { get; set; }

        public string EXact_Message { get; set; }

        public string Bank_Resp_Code { get; set; }

        public string Bank_Message { get; set; }

        public string Bank_Resp_Code_2 { get; set; }

        public string SequenceNo { get; set; }

        public string AVS { get; set; }

        public string CVV2 { get; set; }

        public string Retrieval_Ref_No { get; set; }

        public string CAVV_Response { get; set; }

        public string AmountRequested { get; set; }

        public string MerchantName { get; set; }

        public string MerchantAddress { get; set; }

        public string MerchantCity { get; set; }

        public string MerchantProvince { get; set; }

        public string MerchantCountry { get; set; }

        public string MerchantPostal { get; set; }

        public string MerchantURL { get; set; }

        public string CurrentBalance { get; set; }

        public string PreviousBalance { get; set; }
    }

    public partial class TransactionLog
    {
        public TransactionLog() { }

        public FirstData.TransactionResult GetResponse()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FirstData.TransactionResult>(Utility.Security.Decrypt(this.Response));
        }

        public TransactionLog(FirstData.Transaction tran, FirstData.TransactionResult result, string chargeType, string refnum, string custNum)
        {
            var reqstore = new TransactionRequestStore()
            {
                Card_Number = tran.Card_Number,
                CardType = tran.CardType,
                Currency = tran.Currency,
                Transaction_Type = tran.Transaction_Type,
                Expiry_Date = tran.Expiry_Date,
                CardHoldersName = tran.CardHoldersName,
                VerificationStr2 = tran.VerificationStr2,
                CVD_Presence_Ind = tran.CVD_Presence_Ind,
                ZipCode = tran.ZipCode,
                Customer_Ref = tran.Customer_Ref,
                Reference_No = tran.Reference_No,
                DollarAmount = tran.DollarAmount,
                TransArmorToken = tran.TransarmorToken
            };

            var resultstore = new TransactionResultStore()
            {
                Transaction_Error = result.Transaction_Error,
                Transaction_Approved = result.Transaction_Approved,
                EXact_Resp_Code = result.EXact_Resp_Code,
                EXact_Message = result.EXact_Message,
                Bank_Resp_Code = result.Bank_Resp_Code,
                Bank_Message = result.Bank_Message,
                Bank_Resp_Code_2 = result.Bank_Resp_Code_2,
                SequenceNo = result.SequenceNo,
                AVS = result.AVS,
                CVV2 = result.CVV2,
                Retrieval_Ref_No = result.Retrieval_Ref_No,
                CAVV_Response = result.CAVV_Response,
                AmountRequested = result.AmountRequested,
                MerchantName = result.MerchantName,
                MerchantAddress = result.MerchantAddress,
                MerchantCity = result.MerchantCity,
                MerchantProvince = result.MerchantProvince,
                MerchantCountry = result.MerchantCountry,
                MerchantPostal = result.MerchantPostal,
                MerchantURL = result.MerchantURL,
                CurrentBalance = result.CurrentBalance,
                PreviousBalance = result.PreviousBalance,
            };

            this.Response = Utility.Security.Encrypt(resultstore.SerializeToJson());
            this.TransactionStore = reqstore.SerializeToJson();
            this.Date = DateTime.Now;
            this.ChargeType = chargeType;
            this.ReferenceNum = refnum;
            this.CustomerNum = custNum;
        }
    }
}
