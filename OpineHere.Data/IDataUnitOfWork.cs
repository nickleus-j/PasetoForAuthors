using System;
using System.Collections.Generic;
using System.Text;
namespace OpineHere.Data;

public interface IDataUnitOfWork
{
    void Dispose();
    Task<int> CompleteAsync();
    IMarkdownPostRepo MarkdownPostRepo { get; }
}