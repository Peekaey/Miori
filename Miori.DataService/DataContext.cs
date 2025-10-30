using Microsoft.EntityFrameworkCore;

namespace Miori.DataService;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
}