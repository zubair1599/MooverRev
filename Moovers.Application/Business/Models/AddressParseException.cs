using System;

namespace Business.Models
{
    [Serializable]
    public class AddressParseException : Exception
    {
        public AddressParseException(string msg, Exception inner) : base(msg, inner) { }
    }
}