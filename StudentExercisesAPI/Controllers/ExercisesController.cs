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
    public class ExercisesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
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

        // GET: api/<ExercisesController>
        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string query = "SELECT Exercise.Id AS ExerciseID, Exercise.Name AS ExerciseName, Language FROM Exercise ";

                    if (include == "students")
                    {
                        query = "SELECT Exercise.Id AS ExerciseID, Exercise.Name AS ExerciseName, Exercise.Language, Student.Id AS StudentID, Student.FirstName, Student.LastName, Student.SlackHandle, Student.CohortId, Cohort.Name AS CohortName FROM Exercise LEFT JOIN AssignedExercise ON Exercise.Id = AssignedExercise.ExerciseId LEFT JOIN Student ON Student.Id = AssignedExercise.StudentId LEFT JOIN Cohort ON Student.CohortId = Cohort.Id  ";
                    }

                    if (q != " ")
                    {
                        query += $"WHERE Exercise.Name LIKE '%{q}%' OR Language LIKE '%{q}%'";
                    }

                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                            Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                        if (exercises.Any(e => e.Id == exercise.Id) == false)
                        {
                            if (include == "students")
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("StudentID")))
                                {
                                    Student student = new Student
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                        CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Cohort = new Cohort { Id = reader.GetInt32(reader.GetOrdinal("CohortId")), Name = reader.GetString(reader.GetOrdinal("CohortName")), }
                                    };

                                    exercise.assignedStudents.Add(student);
                                }
                            }

                            exercises.Add(exercise);
                        }

                        else
                        {
                            if (include == "students")
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("StudentID")))
                                {
                                    Student student = new Student
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                        CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Cohort = new Cohort { Id = reader.GetInt32(reader.GetOrdinal("CohortId")), Name = reader.GetString(reader.GetOrdinal("CohortName")), }
                                    };

                                    exercises.FirstOrDefault(e => e.Id == exercise.Id).assignedStudents.Add(student);
                                }
                            }

                        }
                        
                    }
                    reader.Close();

                    return Ok(exercises);
                }
            }
        }

        // GET api/<ExercisesController>/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };
                    }
                    reader.Close();

                    return Ok(exercise);
                }
            }
        }

        //POST api/<ExercisesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (Name, Language)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));

                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        // PUT api/<ExercisesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET Name = @name,
                                                Language = @language
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
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
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<ExercisesController>/5
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
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
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
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}
