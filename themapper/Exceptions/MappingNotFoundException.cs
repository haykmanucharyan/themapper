using System;

namespace themapper.Exceptions
{
    public class MappingNotFoundException : Exception
    {
        public MappingNotFoundException(Type type) : base($"hmapper mapping for [{type}] not found")
        {
            
        }
    }
}
