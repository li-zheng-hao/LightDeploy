using LightApi.EFCore.EFCore.DbContext;
using Microsoft.EntityFrameworkCore;

namespace LightDeploy.Server.Core;

public class LdDbContext:AppDbContext
{
    public LdDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }
}