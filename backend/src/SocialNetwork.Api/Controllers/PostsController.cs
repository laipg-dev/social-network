using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;

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
    public async Task<ActionResult<PostResponse>> Create(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _postService.CreateAsync(
            _currentUser.UserId,
            request,
            cancellationToken);

        return Created($"/api/posts/{result.Id}", result);
    }

    [HttpGet]
    public async Task<ActionResult<PostPageResponse>> GetPage(
        [FromQuery] Guid? communityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await _postService.GetPageAsync(communityId, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _postService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostResponse>> Update(
        Guid id,
        UpdatePostRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _postService.UpdateAsync(id, _currentUser.UserId, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _postService.DeleteAsync(id, _currentUser.UserId, cancellationToken);
        return NoContent();
    }
}