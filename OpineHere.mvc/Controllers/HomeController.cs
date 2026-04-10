using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpineHere.Data;
using OpineHere.Data.entity;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Controllers;

public class HomeController : Controller
{
    private IDataUnitOfWork UnitOfWork;
    public HomeController(IDataUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
    }
    public async Task<IActionResult> Index()
    {
        
        return View(await UnitOfWork.MarkdownPostRepo.GetFromPageAsync(1,10,"Title"));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AnnonymousPost()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnnonymousPost(MarkdownPost post)
    {
        if (ModelState.IsValid)
        {
            post.UserId = null;
            post.LastUpdate = DateTime.UtcNow;
            post.PostDate = DateTime.UtcNow;
            await UnitOfWork.MarkdownPostRepo.AddAsync(post);
            await UnitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(post);
    }    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}