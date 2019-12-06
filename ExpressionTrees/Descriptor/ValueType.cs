using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionTrees.Descriptor
{
    public enum ValueType
    {
        /// <summary>
        /// Simple value (string,int,double..)
        /// </summary>
        OneValue,

        /// <summary>
        /// Simple value from selection (Enum)
        /// </summary>
        RadioValue,

        /// <summary>
        /// Array Value from selection (bit enum)
        /// </summary>
        //CheckValue
    }
   
}
