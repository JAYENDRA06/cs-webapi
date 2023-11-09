using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("Getusers/{userId}/{active}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool active)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string paramters = "";

        if (userId != 0)
        {
            paramters += ", @UserId = " + userId.ToString();
        }
        if (active)
        {
            paramters += ", @Active = " + active;
        }

        sql += paramters[1..]; // starting at 1 and ending at last

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert 
            @FirstName = '" + user.FirstName +
            "', @LastName = '" + user.LastName + 
            "', @Email = '" + user.Email + 
            "', @Gender = '" + user.Gender +
            "', @JobTitle = '" + user.JobTitle +
            "', @Department = '" + user.Department + 
            "', @Salary = '" + user.Salary.ToString() + "'";
        
        string parameters = "";
        
        if(user.UserId != 0){
            parameters += ", @UserId = " + user.UserId.ToString();
        }
        if(user.Active){
            parameters += ", @Active = " + user.Active.ToString();
        }
        sql += parameters;

        bool res = _dapper.ExecuteSql(sql);

        if (res) return Ok();
        throw new Exception("Upsert failed");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = " + userId.ToString();

        bool res = _dapper.ExecuteSql(sql);

        if (res) return Ok();
        throw new Exception("Deletion failed");
    }
}
