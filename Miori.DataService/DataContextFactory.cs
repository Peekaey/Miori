using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Miori.DataService;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<DataContext> optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        
        optionsBuilder.UseNpgsql(); 
        return new DataContext(optionsBuilder.Options);

    }

    private string GetConnectionString()
    {
        return "";
    }
}