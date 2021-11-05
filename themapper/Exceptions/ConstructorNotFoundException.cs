using System;

namespace themapper.Exceptions
{
    public class ConstructorNotFoundException : Exception
    {
        public ConstructorNotFoundException(Type type) : base($"themapper: default constructor for [{type}] not found")
        { 
        
        }
    }
}
