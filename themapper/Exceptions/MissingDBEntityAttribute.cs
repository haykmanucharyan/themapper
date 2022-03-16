using System;

namespace themapper.Exceptions
{
    public class MissingDBEntityAttribute : Exception
    {
        public MissingDBEntityAttribute(Type type) : base($"themapper: missing DBEntity attribute for [{type}].")
        {

        }
    }
}
