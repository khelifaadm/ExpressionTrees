# ExpressionTrees
C# Expression trees practice, SQL builder from class
>QueryBuilder\<Student>()  
.where(x => x.Age >= 7 ||  x => x.FirstName.Contains("M"))  
.select(x => new { x.FirstName, x.Age, NewField = 10 })

Produce   
>Select FirstName, Age, 10 as NewField   
from Students   
where (Age >= 7) or (firstName like '%M%')
