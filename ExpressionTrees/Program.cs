using ExpressionTrees.Descriptor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionTrees
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // SqlBuilderSample();
            var desc = new FieldMapper<Student>();
            Console.WriteLine(desc.Add(x => x.Sex)
                .Add(x => x.Age));
        }

        private static void SqlBuilderSample()
        {
            var studentsQuery = new QueryBuilder<Student>();
            Console.WriteLine(
            studentsQuery
                   .Where(x => x.Age >= 7)
                   .AndWhere(x => x.FirstName.StartsWith("M") || x.FirstName.Contains("M") || x.FirstName.EndsWith("M"))
                   .Select(x => new { x.FirstName, x.Age, NewField = 10 })
            );
        }
    }

    public class FieldMapper<T> where T : class
    {
        #region private
        private List<FieldDesriptorBase> _fieldsDescriptors;
        #endregion

        public FieldMapper()
        {
            _fieldsDescriptors = new List<FieldDesriptorBase>();
        }

        public FieldMapper<T> Add(Expression<Func<T, object>> expr)
        {

            if (expr.Body.NodeType == ExpressionType.Convert)
            {
                var exprUnary = (UnaryExpression)expr.Body;
                if (exprUnary.Operand.NodeType == ExpressionType.MemberAccess)
                {
                    var exprMemb = (MemberExpression)exprUnary.Operand;
                    if (exprMemb.Type.IsEnum)
                    {

                        var fieldDescriptor = new FieldDescriptorRadioValue(exprMemb.Member.Name, exprMemb.Member.Name, "0");
                        int index = 0;
                        foreach (var enumValue in Enum.GetValues(exprMemb.Type))
                        {
                            fieldDescriptor.FieldValues.Add(new DescriptionValueItem(index, enumValue.ToString(), enumValue.ToString()));
                            index++;
                        }

                        _fieldsDescriptors.Add(fieldDescriptor);
                    }
                    else
                    {
                        var fieldDescriptor = new FieldDescriptorOneValue(exprMemb.Member.Name, exprMemb.Member.Name, string.Empty);
                        _fieldsDescriptors.Add(fieldDescriptor);
                    }
                }
            }
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var fieldDescriptor in this._fieldsDescriptors)
            {
                switch (fieldDescriptor.FieldType)
                {
                    case Descriptor.ValueType.OneValue:                        
                        sb.AppendLine($"OneVlue Field name:{fieldDescriptor.Name}, descrip:{fieldDescriptor.Description}");
                        break;
                    case Descriptor.ValueType.RadioValue:
                        sb.AppendLine($"RadioValue Field name:{fieldDescriptor.Name}, descrip:{fieldDescriptor.Description}");
                        foreach (var item in (fieldDescriptor as FieldDescriptorRadioValue).FieldValues)
                        {
                            sb.AppendLine($" - {item.Name}");
                        }
                        break;
                    default:
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
