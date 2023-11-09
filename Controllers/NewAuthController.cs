using System.Data;
using System.Security.Cryptography;
using Dapper;
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
public class NewAuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;

    public NewAuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _authHelper = new(config);
    }

    [AllowAnonymous] // To bypass the authorization
    [HttpPost("Register")]
    public IActionResult Register(NewUserForRegistrationDto userForRegistration)
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

                string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert 
                    @Email = @EmailParam, 
                    @PasswordHash = @PasswordHashParam, 
                    @PasswordSalt = @PasswordSaltParam";

                // Can use dynamic parameters as well 
                // to get lesser lines of code
                // just pass dynamic paramaters to Execute()
                // just like in LoadDataSingleWithParameters()
                // DyType for byte[] will be Binary
                List<SqlParameter> sqlParameters = new();


                SqlParameter emailParameter = new("@EmailParam", SqlDbType.VarChar)
                {
                    Value = userForRegistration.Email
                };
                SqlParameter passwordHashParameter = new("@PasswordHashParam", SqlDbType.VarBinary)
                {
                    Value = passwordHash
                };
                SqlParameter passwordSaltParameter = new("@PasswordSaltParam", SqlDbType.VarBinary)
                {
                    Value = passwordSalt
                };


                sqlParameters.Add(passwordSaltParameter);
                sqlParameters.Add(emailParameter);
                sqlParameters.Add(passwordHashParameter);

                if (_dapper.ExecuteSqlWithParamters(sqlAddAuth, sqlParameters))
                {
                    string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert 
                        @FirstName = '" + userForRegistration.FirstName +
                        "', @LastName = '" + userForRegistration.LastName +
                        "', @Email = '" + userForRegistration.Email +
                        "', @Gender = '" + userForRegistration.Gender +
                        "', @JobTitle = '" + userForRegistration.JobTitle +
                        "', @Department = '" + userForRegistration.Department +
                        "', @Salary = " + userForRegistration.Salary.ToString();
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

    [AllowAnonymous] 
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get 
            @Email = @EmailParam";


        DynamicParameters sqlParameters = new();

        sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

        
        UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

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