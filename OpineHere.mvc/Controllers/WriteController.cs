using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpineHere.Data;
using OpineHere.mvc.Mapping;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Controllers;

public class WriteController : Controller
{
    private IDataUnitOfWork UnitOfWork;
    public WriteController(IDataUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
    }
    // GET
    [PasetoAuthorize]
    public IActionResult Create()
    {
        return View();
    }
    [PasetoAuthorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarkdownPostDto post)
    {
        if (ModelState.IsValid)
        {
            post.UserId = new Guid(HttpContext.Items["UserId"].ToString());
            post.LastUpdate = DateTime.UtcNow;
            post.PostDate = DateTime.UtcNow;
            post.AuthorName = HttpContext.Items["UserDisplayName"].ToString();
            await UnitOfWork.MarkdownPostRepo.AddAsync(MarkdownPostMapper.ToEntity(post));
            await UnitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("Index", "Home");
    }    
}