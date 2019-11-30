using System;
namespace ExpressionTrees
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var studentsQuery = new QueryBuilder<Student>();
            Console.WriteLine(
            studentsQuery
                   .Where(x => x.Age >= 7 )
                   .AndWhere(x => x.FirstName.StartsWith("M") || x.FirstName.Contains("M") || x.FirstName.EndsWith("M"))
                   .Select(x => new { x.FirstName, x.Age, NewField = 10 })
            );
        }
    }
}
