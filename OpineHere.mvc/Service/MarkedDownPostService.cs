using OpineHere.Data;
using OpineHere.mvc.Mapping;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Service;

public class MarkedDownPostService
{
    IDataUnitOfWork unitOfWork;

    public MarkedDownPostService(IDataUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<MarkdownPostDto> GetPostAsync(string id)
    {
        Guid guid = Guid.Parse(id);
        var post=await unitOfWork.MarkdownPostRepo.SingleAsync(p => p.Id == guid);
        return MarkdownPostMapper.ToDto(post);
    }
}