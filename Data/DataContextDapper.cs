using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotnetAPI.Data
{
    public class DataContextDapper
    {
        private readonly IConfiguration _config; 
        private readonly string? _connectionString; 

        public DataContextDapper(IConfiguration config){
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }
        
        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Query<T>(sql);
        }
        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QuerySingle<T>(sql);
        }
        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql) > 0;
        }
        public int ExecuteSqlInt(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql);
        }
        public bool ExecuteSqlWithParamters(string sql, List<SqlParameter> sqlParameters)
        {
            SqlCommand commandWithParams = new(sql);

            foreach(SqlParameter paramter in sqlParameters){
                commandWithParams.Parameters.Add(paramter);
            }

            SqlConnection dbConnection = new(_connectionString);
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = commandWithParams.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }
    }
}