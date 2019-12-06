using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionTrees.Descriptor
{

    public abstract class FieldDesriptorBase
    {
        public FieldDesriptorBase(string name, string description, ValueType fieldType, object fieldValue)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            FieldType = fieldType;
            FieldValue = fieldValue ?? throw new ArgumentNullException(nameof(fieldValue));
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public ValueType FieldType { get; set; }

        public object FieldValue { get; set; }

    }
}
