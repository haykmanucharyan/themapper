using System;

namespace themapper.Exceptions
{
    public class ConstructorNotFoundException : Exception
    {
        public ConstructorNotFoundException(Type type) : base($"Default constructor for [{type}] not found")
        { 
        
        }
    }
}
