using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NewPostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public NewPostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new(config);
        }

        [HttpGet("GetPosts/{userId}/{postId}/{searchValue}")]
        public IEnumerable<Post> GetPosts(int userId = 0, int postId = 0, string searchValue = "none")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string parameters = "";

            if (searchValue != "none")
            {
                parameters += ", @SearchValue = '" + searchValue + "'";
            }
            if (userId != 0)
            {
                parameters += ", @UserId = " + userId.ToString();
            }
            if (postId != 0)
            {
                parameters += ", @PostId = " + postId.ToString();
            }

            if (parameters.Length > 0) sql += parameters[1..];

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("GetMyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = " + this.User.FindFirst("userId")?.Value;

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(PostToUpsertDto postToUpsert)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId = " + User.FindFirst("userId")?.Value
                + ", @PostTitle = '" + postToUpsert.PostTitle
                + "', @PostContent = '" + postToUpsert.PostContent + "'";

            if (postToUpsert.PostId != 0)
            {
                sql += ", @PostId = " + postToUpsert.PostId.ToString();
            }

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to create new post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_Delete 
                @PostId = " + postId.ToString() 
                + ", @UserId = " + User.FindFirst("userId")?.Value;

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post");
        }

    }
}