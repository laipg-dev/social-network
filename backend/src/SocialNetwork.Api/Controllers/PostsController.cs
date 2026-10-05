using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICurrentUser _currentUser;

    public PostsController(
        IPostService postService,
        ICurrentUser currentUser)
    {
        _postService = postService;
        _currentUser = currentUser;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _postService.CreateAsync(
            _currentUser.UserId,
            request,
            cancellationToken);

        return Ok(result);
    }
}