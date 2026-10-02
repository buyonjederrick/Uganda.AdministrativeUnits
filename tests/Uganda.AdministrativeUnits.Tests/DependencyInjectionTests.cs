using Microsoft.Extensions.DependencyInjection;
using Uganda.AdministrativeUnits;
using Xunit;

namespace Uganda.AdministrativeUnits.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Registers_a_single_shared_directory_and_is_idempotent()
    {
        var services = new ServiceCollection();
        services.AddUgandaAdministrativeUnits().AddUgandaAdministrativeUnits();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Same(AdministrativeUnitDirectory.Default, provider.GetRequiredService<IAdministrativeUnitDirectory>());
        Assert.Single(services, d => d.ServiceType == typeof(IAdministrativeUnitDirectory));
    }
}
