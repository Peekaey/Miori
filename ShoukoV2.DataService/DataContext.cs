using Microsoft.EntityFrameworkCore;

namespace ShoukoV2.DataService;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
}