using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class TenantResolutionTests
{
    private readonly IConfiguration _configuration;

    public TenantResolutionTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Tenancy:BaseDomain", "scoutsclm.org"},
            {"Tenancy:FallbackTenantSlug", "lestonnac"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Theory]
    [InlineData("lestonnac.scoutsclm.org", "lestonnac")]
    [InlineData("sanvicente.scoutsclm.org", "sanvicente")]
    [InlineData("LESTONNAC.scoutsclm.org", "lestonnac")]
    [InlineData("lestonnac.localhost", "lestonnac")]
    [InlineData("sanvicente.localhost", "sanvicente")]
    [InlineData("admin.localhost", "admin")]
    [InlineData("sclm.scoutsclm.org", "sclm")]
    [InlineData("localhost", null)]
    [InlineData("scoutsclm.org", null)]
    [InlineData("www.scoutsclm.org", null)]
    [InlineData("www.localhost", null)]
    public void ExtractTenantSlug_FromSubdomain_ShouldResolveExpectedSlug(string host, string? expectedSlug)
    {
        var middleware = new TenantResolutionMiddleware(
            _ => Task.CompletedTask,
            _configuration,
            NullLogger<TenantResolutionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);

        var slug = middleware.ExtractTenantSlug(context);

        Assert.Equal(expectedSlug, slug);
    }

    [Fact]
    public void ExtractTenantSlug_FromHttpHeader_ShouldHavePrecedence()
    {
        var middleware = new TenantResolutionMiddleware(
            _ => Task.CompletedTask,
            _configuration,
            NullLogger<TenantResolutionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("sanvicente.scoutsclm.org");
        context.Request.Headers["X-Tenant-Slug"] = "lestonnac";

        var slug = middleware.ExtractTenantSlug(context);

        Assert.Equal("lestonnac", slug);
    }

    [Fact]
    public void ExtractTenantSlug_FromQueryString_ShouldResolveSlug()
    {
        var middleware = new TenantResolutionMiddleware(
            _ => Task.CompletedTask,
            _configuration,
            NullLogger<TenantResolutionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost");
        context.Request.QueryString = new QueryString("?tenant=sanvicente");

        var slug = middleware.ExtractTenantSlug(context);

        Assert.Equal("sanvicente", slug);
    }
}
