using Microsoft.AspNetCore.Mvc;
using OpineHere.Data;
using OpineHere.mvc.Mapping;

namespace OpineHere.mvc.Controllers;

public class AuthorController : Controller
{
    private IDataUnitOfWork UnitOfWork;
    public AuthorController(IDataUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
    }
    // GET
    public async Task<IActionResult> Index(string penName)
    {
        var posts = await UnitOfWork.MarkdownPostRepo.GetPostsWithPenName(penName);
        return View(MarkdownPostMapper.ToDto(posts));
    }
    public async Task<IActionResult> Profile(string authorId)
    {
        var author = await UnitOfWork.AuthorProfileRepo.GetProfileAsync(authorId);
        return PartialView(AuthorProfileMapper.ToDto(author));
    }
}