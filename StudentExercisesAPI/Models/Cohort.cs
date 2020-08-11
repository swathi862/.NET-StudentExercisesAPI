using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> listOfStudents { get; set; } = new List<Student>();
        public List<Instructor> listOfInstructors { get; set; } = new List<Instructor>();
    }
}
