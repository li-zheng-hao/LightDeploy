using LightApi.EFCore.EFCore.DbContext;
using Microsoft.EntityFrameworkCore;

namespace LightDeploy.ClientAgent.Domain;

public class LdAgentDbContext:AppDbContext
{
    public LdAgentDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }
}