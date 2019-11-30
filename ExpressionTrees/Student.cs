namespace ExpressionTrees
{
    public class Student
    {
        public Student()
        {
        }

        public Student(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public Sex Sex { get; set; }

        public int Age { get; set; }
    }
}
