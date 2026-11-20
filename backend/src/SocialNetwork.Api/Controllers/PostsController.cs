using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Common.Exceptions;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICurrentUser _currentUser;
    private readonly IPostMediaService _mediaService;

    public PostsController(
        IPostService postService,
        ICurrentUser currentUser,
        IPostMediaService mediaService)
    {
        _postService = postService;
        _currentUser = currentUser;
        _mediaService = mediaService;
    }

    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(55_000_000)]
    public async Task<ActionResult<PostResponse>> CreateWithImages([FromForm] CreatePostForm form, CancellationToken ct)
    {
        if (form.Images.Count > 5) throw new ValidationException(new Dictionary<string, string[]> { ["images"] = ["A post can contain at most 5 images."] });
        if (string.IsNullOrWhiteSpace(form.Content) && form.Images.Count == 0)
            throw new ValidationException(new Dictionary<string, string[]> { ["content"] = ["Provide text or at least one image."] });
        var request = new CreatePostRequest { Content = form.Content, Visibility = form.Visibility, CommunityId = form.CommunityId };
        var files = new List<FileUploadRequest>(form.Images.Count);
        try
        {
            foreach (var image in form.Images)
            {
                files.Add(new FileUploadRequest(image.OpenReadStream(), Path.GetFileName(image.FileName), image.ContentType, image.Length));
            }
            var post = files.Count == 0
                ? await _postService.CreateAsync(_currentUser.UserId, request, ct)
                : await _mediaService.CreatePostWithMediaAsync(_currentUser.UserId, request, files,
                    new Uri($"{Request.Scheme}://{Request.Host}"), ct);
            return Created($"/api/posts/{post.Id}", post);
        }
        finally
        {
            foreach (var file in files) await file.Content.DisposeAsync();
        }
    }

    [Authorize]
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
    [AllowAnonymous]
    public async Task<ActionResult<PostPageResponse>> GetPage(
        [FromQuery] Guid? communityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await _postService.GetPageAsync(communityId, page, pageSize, _currentUser.IsAuthenticated ? _currentUser.UserId : null, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _postService.GetByIdAsync(id, _currentUser.IsAuthenticated ? _currentUser.UserId : null, cancellationToken));

    [HttpGet("users/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostPageResponse>> GetByUser(Guid userId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await _postService.GetByUserAsync(userId,_currentUser.IsAuthenticated?_currentUser.UserId:null,page,pageSize,ct));

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PostPageResponse>> Search([FromQuery]string q,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await _postService.SearchAsync(q,_currentUser.IsAuthenticated?_currentUser.UserId:null,page,pageSize,ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostResponse>> Update(
        Guid id,
        UpdatePostRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _postService.UpdateAsync(id, _currentUser.UserId, request, cancellationToken));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _postService.DeleteAsync(id, _currentUser.UserId, cancellationToken);
        await _mediaService.DeleteForPostAsync(id, _currentUser.UserId, cancellationToken);
        return NoContent();
    }
}

public sealed class CreatePostForm
{
    public string? Content { get; set; }
    public string Visibility { get; set; } = "PUBLIC";
    public Guid? CommunityId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}
