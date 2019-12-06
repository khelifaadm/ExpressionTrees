using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionTrees.Descriptor
{

    public class FieldDescriptorRadioValue : FieldDesriptorBase
    {
        public FieldDescriptorRadioValue(string name, string description, string fieldValue) : base(name, description, ValueType.RadioValue, fieldValue)
        {
            FieldValues = new List<DescriptionValueItem>();
        }

        /// <summary>
        /// Enum values
        /// </summary>
        public List<DescriptionValueItem> FieldValues { get; set; }
    }

}
