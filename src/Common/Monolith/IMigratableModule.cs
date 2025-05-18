using System;
using System.Threading.Tasks;

namespace Common.Monolith;

public interface IMigratable
{
    Task Migrate(IServiceProvider services);
}
