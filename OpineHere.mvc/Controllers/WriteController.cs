using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpineHere.Data;
using OpineHere.mvc.Mapping;
using OpineHere.mvc.Models;
using OpineHere.mvc.Service;

namespace OpineHere.mvc.Controllers;

public class WriteController : Controller
{
    private IDataUnitOfWork UnitOfWork;
    public WriteController(IDataUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
    }
    public async Task<IActionResult> leer(string id)
    {
        MarkedDownPostService markedDownPostService = new MarkedDownPostService(UnitOfWork);
        return View(await markedDownPostService.GetPostAsync(id));
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
            return RedirectToAction("Index", "Home");
        }
        return RedirectToAction("Index", "Home");
    }    
}