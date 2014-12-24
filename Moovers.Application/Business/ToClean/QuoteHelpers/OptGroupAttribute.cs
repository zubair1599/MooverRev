using System;

namespace Business.ToClean.QuoteHelpers
{
    public class OptGroupAttribute : Attribute
    {
        public string Name { get; set; }

        public OptGroupAttribute(string name)
        {
            this.Name = name;
        }
    }
}