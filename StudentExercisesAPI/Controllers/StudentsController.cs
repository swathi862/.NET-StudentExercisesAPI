using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: api/<StudentsController>
        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string query = "SELECT Student.Id, FirstName, LastName, SlackHandle, CohortId, Cohort.Name AS CohortName FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id ";

                    if(include == "exercise")
                    {
                        query = "SELECT Student.Id, FirstName, LastName, SlackHandle, CohortId, Cohort.Name AS CohortName, Exercise.Id AS ExerciseID, Exercise.Name AS ExerciseName, Exercise.Language FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id LEFT JOIN AssignedExercise ON Student.Id = AssignedExercise.StudentId LEFT JOIN Exercise ON Exercise.Id = AssignedExercise.ExerciseId ";
                    }

                    if(q != " ")
                    {
                        query += $"WHERE FirstName LIKE '%{q}%' OR LastName LIKE '%{q}%' OR SlackHandle LIKE '%{q}%'";
                    }

                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort { Id = reader.GetInt32(reader.GetOrdinal("CohortId")), Name = reader.GetString(reader.GetOrdinal("CohortName")), }
                        };

                        if (students.Any(stud => stud.Id == student.Id) == false)
                        {
                            if (include == "exercise")
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("ExerciseID")))
                                {
                                    Exercise exercise = new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                                        Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("Language"))
                                    };

                                    student.assignedExercises.Add(exercise);
                                }
                            }

                            students.Add(student);
                        }

                        else
                        {
                            if (include == "exercise")
                            {
                                if(!reader.IsDBNull(reader.GetOrdinal("ExerciseID")))
                                {
                                    Exercise exercise = new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                                        Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("Language"))
                                    };

                                    students.FirstOrDefault(s => s.Id == student.Id).assignedExercises.Add(exercise);
                                }
                            }

                        }

                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }

        // GET api/<StudentsController>/5
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                           Student.Id, FirstName, LastName, SlackHandle, CohortId, Cohort.Name 
                        FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id
                        WHERE Student.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                       student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort { Id = reader.GetInt32(reader.GetOrdinal("CohortId")), Name = reader.GetString(reader.GetOrdinal("Name")), }
                        };
                    }
                    reader.Close();

                    return Ok(student);
                }
            }
        }

        // POST api/<StudentsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slack, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slack", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));


                    int newId = (int)cmd.ExecuteScalar();
                    student.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, student);
                }
            }
        }

        // PUT api/<StudentsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slack,
                                                CohortId = @cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slack", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<StudentsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Student.Id, FirstName, LastName, SlackHandle, CohortId, Cohort.Name FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id
                        WHERE Student.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
