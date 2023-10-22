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
    IUserRepository _userRepository;
    readonly IMapper _mapper;
    public UserEFController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _mapper = new Mapper(new MapperConfiguration(cfg => {
            cfg.CreateMap<UserToAddDto, User>();
        }));
    }

    [HttpGet("Getusers")]
    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetUsers();
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    // basically not returning any specific data, just telling if req was success or failure
    public IActionResult EditUser(User user)
    {
        User? userDb = _userRepository.GetSingleUser(user.UserId);
        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;

            if (_userRepository.SaveChanges()) return Ok();
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

        _userRepository.AddEntity<User>(userDb);
        if (_userRepository.SaveChanges()) return Ok();
        throw new Exception("Insertion failed");

    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _userRepository.GetSingleUser(userId);
        if(userDb != null){         
            _userRepository.RemoveEntity<User>(userDb);

            if(_userRepository.SaveChanges()) return Ok();
            throw new Exception("Deletion failed");
        }
        throw new Exception("Failed to get user");
    }
}
 