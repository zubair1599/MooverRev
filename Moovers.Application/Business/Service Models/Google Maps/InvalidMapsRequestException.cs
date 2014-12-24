using System;

namespace Business
{
    [Serializable]
    public class InvalidMapsRequestException : Exception
    {
        public InvalidMapsRequestException(string msg) : base(msg) 
        {
        }
    }
}