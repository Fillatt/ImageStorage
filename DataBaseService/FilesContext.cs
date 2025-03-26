using DataBaseService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBaseService
{
    public class FilesContext : DbContext
    {
        public DbSet<Image> Images => Set<Image>();

        public FilesContext(DbContextOptions<FilesContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
