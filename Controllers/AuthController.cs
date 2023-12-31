using System.Data;
using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;

    public AuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _authHelper = new(config);
    }

    [AllowAnonymous] // To bypass the authorization
    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
        if (userForRegistration.Password == userForRegistration.PasswordConfirm)
        {
            string sqlCheckUserExists = @"
                SELECT Email FROM TutorialAppSchema.Users 
                WHERE Email = '" + userForRegistration.Email + "'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

            if (!existingUsers.Any())
            {
                byte[] passwordSalt = new byte[128 / 8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                string sqlAddAuth = @"
                    INSERT INTO TutorialAppSchema.Auth (
                    [Email],
                    [PasswordHash],
                    [PasswordSalt]
                ) VALUES (
                    '" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt)";

                List<SqlParameter> sqlParameters = new();
                SqlParameter passwordSaltParameter = new("@PasswordSalt", SqlDbType.VarBinary)
                {
                    Value = passwordSalt
                };
                SqlParameter passwordHashParameter = new("@PasswordHash", SqlDbType.VarBinary)
                {
                    Value = passwordHash
                };
                sqlParameters.Add(passwordSaltParameter);
                sqlParameters.Add(passwordHashParameter);

                if (_dapper.ExecuteSqlWithParamters(sqlAddAuth, sqlParameters))
                {
                    string sqlAddUser = @"
                    INSERT INTO TutorialAppSchema.Users(
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active]
                    ) VALUES (
                        '" + userForRegistration.FirstName + @"',
                        '" + userForRegistration.LastName + @"',
                        '" + userForRegistration.Email + @"',
                        '" + userForRegistration.Gender + @"', 1)";

                    if (_dapper.ExecuteSql(sqlAddUser))
                    {
                        return Ok();
                    }
                    throw new Exception("Failed to add user");
                }
                throw new Exception("Failed to register");
            }
            throw new Exception("User already exists");
        }
        throw new Exception("Passwords do not match");
    }

    [AllowAnonymous] // To bypass the authorization
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        string sqlForHashAndSalt = @"
            SELECT [PasswordHash], [PasswordSalt] 
            FROM TutorialAppSchema.Auth WHERE Email = '" + userForLogin.Email + "'";
        UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

        byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

        // can't user direct equal check bcoz it will then match Object pointers
        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != userForConfirmation.PasswordHash[i])
            {
                return StatusCode(401, "Incorrect Password");
            }
        }

        string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users WHERE Email='" + userForLogin.Email + "'";
        int userId = _dapper.LoadDataSingle<int>(userIdSql);

        return Ok(new Dictionary<string, string>{
            {"token", _authHelper.CreateToken(userId)}
        });
    }

    [HttpGet("RefreshToken")]
    public string RefreshToken()
    {
        // string to find userId claim that we set while creating the token
        string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users WHERE UserId='" + User.FindFirst("userId")?.Value + "'";

        int userId = _dapper.LoadDataSingle<int>(userIdSql);

        return _authHelper.CreateToken(userId);
    }   
}