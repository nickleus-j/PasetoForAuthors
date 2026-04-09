using OpineHere.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using OpineHere.Data.entity;

namespace OpineHere.EntityFramework;

public class EfUnitOfWork: IDataUnitOfWork
{
    private readonly OpineContext _context;
    public MarkdownPostRepo MarkdownPostRepo{ get; private set; }
    public EfUnitOfWork(OpineContext context)
    {
        _context = context;
        MarkdownPostRepo = new MarkdownPostRepo(_context);
    }
    public void Dispose()
    {
        _context.Dispose();
    }
    public async Task<int> CompleteAsync()
    {
        return await  _context.SaveChangesAsync();
    }
}