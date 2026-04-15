using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpineHere.Data.entity;
namespace OpineHere.EntityFramework;

public class OpineContext: IdentityDbContext
{
    public OpineContext()
    {
    }

    public OpineContext(DbContextOptions<OpineContext> options)
        : base(options)
    {
    }
    public virtual DbSet<MarkdownPost> MarkdownPost { get; set; }
    public virtual DbSet<PopularityApproval> PopularityApproval { get; set; }
    public virtual DbSet<AuthorProfile> AuthorProfile { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        string[] emails = ["primary@default.co", "beta@man.com"];
        string[] userIds = ["8e445800-0000-4543-0000-9443d048cdb9", "8e445800-1111-4543-1111-9443d048cdb9","8e445801-0000-4543-0000-9443d048cdb9", "8e445801-1111-4543-1111-9443d048cdb9"
        ];
        modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole { Id = "00445865-0000-0000-0000-9443d048cdb9", Name = "General", NormalizedName = "General", ConcurrencyStamp = "00445865-0000-0000-0000-9443d048cd00" });
        
        modelBuilder.Entity<IdentityUser>().HasData(new IdentityUser
        {
            Id = userIds[0], // Static GUID
            UserName = emails[0],
            NormalizedUserName = emails[0].ToUpper(),
            Email = emails[0],
            NormalizedEmail = emails[0].ToUpper(),
            PasswordHash = "aPassword123!",
            SecurityStamp= "8e445865-0000-aaaa-0000-9443d048cdb9",
            ConcurrencyStamp= "8e445865-0000-aaaa-0000-9443d048cd00"
        });
        modelBuilder.Entity<AuthorProfile>().HasData(
            new AuthorProfile
            {
                Id = 1,
                UserId =new Guid(userIds[0]),
                GivenName =  "Default",
                Surname =  "User",
            });
    }
}