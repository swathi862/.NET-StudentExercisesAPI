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
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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

        // GET: api/<CohortController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Cohort.Id AS CohortID, Cohort.Name, Student.Id AS StudentID, Student.FirstName AS StudentFirstName, Student.LastName AS StudentLastName, Student.SlackHandle AS StudentSlack, Student.CohortId AS StudentCohort, Instructor.Id AS InstructorID, Instructor.FirstName AS InstructorFirstName, Instructor.LastName AS InstructorLastName, Instructor.SlackHandle AS InstructorSlack, Instructor.CohortId AS InstructorCohort FROM Student JOIN Cohort ON Student.CohortId = Cohort.Id JOIN Instructor ON Instructor.CohortId = Cohort.Id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };


                        if (cohorts.Any(c => c.Id == cohort.Id) == false)
                        {
                            if (cohort.listOfStudents.Any(stud => stud.Id == reader.GetInt32(reader.GetOrdinal("StudentID"))) == false)
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohort"))
                                };

                                cohort.listOfStudents.Add(student);

                            }

                            if (cohort.listOfInstructors.Any(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorID"))) == false)
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohort"))
                                };

                                cohort.listOfInstructors.Add(instructor);

                            }

                            cohorts.Add(cohort);
                        }
                        else
                        {
                            if (cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfStudents.Any(stud => stud.Id == reader.GetInt32(reader.GetOrdinal("StudentID"))) == false)
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohort"))
                                };

                                cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfStudents.Add(student);

                            }

                            if (cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfInstructors.Any(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorID"))) == false)
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohort"))
                                };

                                cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfInstructors.Add(instructor);

                            }

                        }

                    }

                    reader.Close();

                    return Ok(cohorts);
                }

            }
        }

        // GET api/<CohortController>/5
        [HttpGet("{id}", Name ="GetCohort")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Cohort.Id AS CohortID, Cohort.Name, Student.Id AS StudentID, Student.FirstName AS StudentFirstName, Student.LastName AS StudentLastName, Student.SlackHandle AS StudentSlack, Student.CohortId AS StudentCohort, Instructor.Id AS InstructorID, Instructor.FirstName AS InstructorFirstName, Instructor.LastName AS InstructorLastName, Instructor.SlackHandle AS InstructorSlack, Instructor.CohortId AS InstructorCohort FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id LEFT JOIN Instructor ON Instructor.CohortId = Cohort.Id
                        WHERE Cohort.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    while (reader.Read())
                    {
                        if (cohort == null)
                        {
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                            };
                        }
                        else
                        {
                            if (cohort.listOfStudents.Any(stud => stud.Id == reader.GetInt32(reader.GetOrdinal("StudentID"))) == false)
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohort"))
                                };

                                cohort.listOfStudents.Add(student);

                            }

                            if (cohort.listOfInstructors.Any(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorID"))) == false)
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohort"))
                                };

                                cohort.listOfInstructors.Add(instructor);

                            }
                        }
                    }
                    reader.Close();

                    return Ok(cohort);
                }
            }
        }

        // POST api/<CohortController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohort(Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));


                    int newId = (int)cmd.ExecuteScalar();
                    cohort.Id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
                }
            }
        }

        // PUT api/<CohortController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET Name = @name,

                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<CohortController>/5
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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        Cohort.Id AS CohortID, Cohort.Name, Student.Id AS StudentID, Student.FirstName AS StudentFirstName, Student.LastName AS StudentLastName, Student.SlackHandle AS StudentSlack, Student.CohortId AS StudentCohort, Instructor.Id AS InstructorID, Instructor.FirstName AS InstructorFirstName, Instructor.LastName AS InstructorLastName, Instructor.SlackHandle AS InstructorSlack, Instructor.CohortId AS InstructorCohort FROM Student LEFT JOIN Cohort ON Student.CohortId = Cohort.Id LEFT JOIN Instructor ON Instructor.CohortId = Cohort.Id
                          WHERE Cohort.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
