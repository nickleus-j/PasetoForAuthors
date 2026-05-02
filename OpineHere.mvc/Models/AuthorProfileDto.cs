using System.ComponentModel.DataAnnotations;

namespace OpineHere.mvc.Models;

public class AuthorProfileDto
{
    [Key]
    public int Id { get; set; }
    [MinLength(1)]
    [Display(Name = "Forename")]
    public string Forename{get; set;}
    [MinLength(1)]
    [Display(Name = "Last Name")]
    public string Surname{get; set;}
}