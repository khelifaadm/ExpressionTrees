using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionTrees.Descriptor
{
    public class FieldDescriptorOneValue : FieldDesriptorBase
    {
        public FieldDescriptorOneValue(string name, string description, object fieldValue) : base(name, description, ValueType.OneValue, fieldValue)
        {
        }
    }
}
