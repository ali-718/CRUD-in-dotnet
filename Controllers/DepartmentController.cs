using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"
                select DepartmentId, DepartmentName 
                from dbo.department
               ";
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            SqlDataReader myReader;
            using(SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    myReader = sqlCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < myReader.FieldCount; i++)
                        {
                            string columnName = myReader.GetName(i);
                            object value = myReader.GetValue(i);
                            row[columnName] = value;
                        }
                        result.Add(row);
                    };
                    myReader.Close();
                    sqlConnection.Close();
                }
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult Post(DepartmentModel dep)
        {
            string query = @"
                insert into dbo.department
                values (@DepartmentName)
               ";
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@DepartmentName", dep.DepartmentName);
                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }

            return Get();
        }

        [HttpPost]
        [Route("updateName")]
        public JsonResult updateName(DepartmentModel dep)
        {
            string query = @"
                update dbo.department
                set DepartmentName = @DepartmentName
                where DepartmentId = @DepartmentId
               ";
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@DepartmentId", dep.DepartmentId);
                    sqlCommand.Parameters.AddWithValue("@DepartmentName", dep.DepartmentName);
                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }

            return fetchDepartmentById(dep);
        }

        private JsonResult fetchDepartmentById(DepartmentModel dep)
        {
            string query = @"
                select DepartmentId, DepartmentName 
                from dbo.department
                where DepartmentId = @DepartmentId
               ";
            SqlDataReader reader;
            Dictionary<string, object> department = new Dictionary<string, object>(); 
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@DepartmentId", dep.DepartmentId);
                    reader = sqlCommand.ExecuteReader();
                    if(reader.Read())
                    {
                        department[reader.GetName(0)] = reader.GetValue(0);
                        department[reader.GetName(1)] = reader.GetValue(1);
                    }
                    sqlConnection.Close();
                }
            }

            return new JsonResult(department);
        }

        [HttpPost]
        [Route("delete/{id}")]
        public string deleteDepartment(int id)
        {
            string query = @"
                delete from dbo.department
                where DepartmentId = @DepartmentId
               ";
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@DepartmentId", id);
                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }

            return "Department has been deleted";
        }

        [HttpGet]
        [Route("paginated/{page}/{quantity}")]
        public JsonResult paginatedDepartment(int page, int quantity)
        {
            int offset = page * quantity;
            string query = @"
               select DepartmentName, DepartmentId
                from testdb.dbo.department
                order by DepartmentId
                offset @offset rows fetch next @quantity rows only
               ";
            SqlDataReader reader;
            List<Dictionary<string, object>> DataColumn = new List<Dictionary<string, object>>();
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@offset", offset);
                    sqlCommand.Parameters.AddWithValue("@quantity", quantity);
                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        DataColumn.Add(row);
                    }
                    sqlConnection.Close();
                }
            }

            return new JsonResult(DataColumn);
        }

        [HttpGet]
        [Route("search/{search}")]
        public JsonResult searchedQuery(string search)
        {
            string query = @"
               select * 
                from dbo.department
                where DepartmentName like @search
               ";
            SqlDataReader reader;
            List<Dictionary<string, object>> DataColumn = new List<Dictionary<string, object>>();
            string sqlDatasource = _configuration.GetConnectionString("EmployeeCon");
            using (SqlConnection sqlConnection = new SqlConnection(sqlDatasource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@search", '%' + search + '%');
                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        DataColumn.Add(row);
                    }
                    sqlConnection.Close();
                }
            }

            return new JsonResult(DataColumn);
        }
    }
}
