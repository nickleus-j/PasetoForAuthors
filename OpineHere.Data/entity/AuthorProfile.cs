using System.ComponentModel.DataAnnotations;

namespace OpineHere.Data.entity;

public class AuthorProfile
{
    [Key]
    public int Id { get; set; }
    [MinLength(1)]
    [Display(Name = "First Name")]
    public string GivenName{get; set;}
    [MinLength(1)]
    [Display(Name = "Last Name")]
    public string Surname{get; set;}
}