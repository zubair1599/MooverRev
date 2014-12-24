using System;

namespace Business
{
    [Serializable]
    public class MapNotFoundException : Exception
    {
        public MapNotFoundException(string msg) : base(msg) 
        {
        }
    }
}