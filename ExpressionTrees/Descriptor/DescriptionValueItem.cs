using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionTrees.Descriptor
{

    public class DescriptionValueItem
    {
        public DescriptionValueItem(int index, string name, string description)
        {
            Index = index;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Index { get; set; }

    }
   
}
