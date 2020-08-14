using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPITest
{
    public class DatabaseFixture : IDisposable
    {
        private readonly string ConnectionString = @$"Server=localhost\SQLEXPRESS;Database=StudentExercises;Trusted_Connection=True;";
        public Student TestStudent { get; set; }
        public DatabaseFixture()
        {
            Student newStudent = new Student
            {
                
            };
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @$"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES ('{newStudent.FirstName}', '{newStudent.LastName}', '{newStudent.SlackHandle}', '{newStudent.CohortId}')";
                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    TestStudent = newStudent;
                }
            }
        }
        public void Dispose()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @$"DELETE FROM Coffee WHERE Title='Test Coffee'";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
