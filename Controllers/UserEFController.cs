// Providing separate namespaces and using them to optimize and not load everything
using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserEFController : ControllerBase
{
    readonly DataContextEF _enitiyFramework;
    readonly IMapper _mapper;
    public UserEFController(IConfiguration config)
    {
        _enitiyFramework = new DataContextEF(config);
        _mapper = new Mapper(new MapperConfiguration(cfg => {
            cfg.CreateMap<UserToAddDto, User>();
        }));
    }

    [HttpGet("Getusers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _enitiyFramework.Users.ToList<User>();
        return users;
    }

    [HttpGet("Getuser/{userId}")]
    public User GetUser(int userId)
    {
        User? user = _enitiyFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();
        if (user != null) return user;
        throw new Exception("Failed to get user");
    }

    [HttpPut("EditUser")]
    // basically not returning any specific data, just telling if req was success or failure
    public IActionResult EditUser(User user)
    {
        User? userDb = _enitiyFramework.Users.Where(u => u.UserId == user.UserId).FirstOrDefault<User>();
        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;

            int res = _enitiyFramework.SaveChanges();
            if (res > 0) return Ok();
            throw new Exception("Updation failed");
        }
        throw new Exception("Failed to get user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User userDbWithoutMapper = new()
        {
            Active = user.Active,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Gender = user.Gender
        };

        // using AutoMapper
        User userDb = _mapper.Map<User>(user);

        _enitiyFramework.Add(userDb);
        int res = _enitiyFramework.SaveChanges();
        if (res > 0) return Ok();
        throw new Exception("Insertion failed");

    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _enitiyFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();
        if(userDb != null){         
            _enitiyFramework.Users.Remove(userDb);

            int res = _enitiyFramework.SaveChanges();
            if(res > 0) return Ok();
            throw new Exception("Deletion failed");
        }
        throw new Exception("Failed to get user");
    }
}
