using BigElephant.Data;
using BigElephant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

public static class TestHelpers
{
    public static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    public static IConfiguration BuildJwtConfig()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "THIS_IS_A_TEST_KEY_32_CHARS_MINIMUM!!",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }
}
