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
                   .Where(x => x.Age >= 7)
                   .AndWhere(x => x.Age < 10)
                   .OrWhere(x => x.FirstName.Contains("ade"))
                   .Select(x => new { x.FirstName, x.Age, NewField = 10 })
            );
        }
    }
}
