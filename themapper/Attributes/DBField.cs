using System;

namespace themapper.Attributes
{
    public class DBField : Attribute
    {
        public string FieldName { get; set; }

        public short Order { get; set; }
    }
}
