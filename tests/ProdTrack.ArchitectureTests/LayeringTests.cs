using System.Reflection;
using NetArchTest.Rules;
using ProdTrack.Application.Messaging;

namespace ProdTrack.ArchitectureTests;

/// <summary>Dependency rules from docs/02 section 3 and docs/09 section 2.</summary>
[Trait("Category", "Unit")]
[Trait("Story", "PT-003")]
public class LayeringTests
{
    private static readonly Assembly Domain = typeof(Domain.Common.Entity).Assembly;
    private static readonly Assembly Application = typeof(IDispatcher).Assembly;
    private static readonly Assembly Infrastructure = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly Contracts = typeof(Contracts.ApiRoutes).Assembly;

    [Fact]
    public void Domain_has_no_dependencies_on_other_layers_or_frameworks()
    {
        var result = Types.InAssembly(Domain).ShouldNot()
            .HaveDependencyOnAny("ProdTrack.Application", "ProdTrack.Infrastructure", "ProdTrack.Server", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_web()
    {
        var result = Types.InAssembly(Application).ShouldNot()
            .HaveDependencyOnAny("ProdTrack.Infrastructure", "ProdTrack.Server", "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore.SqlServer")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Contracts_depend_on_nothing_in_the_solution()
    {
        var result = Types.InAssembly(Contracts).ShouldNot()
            .HaveDependencyOnAny("ProdTrack.Domain", "ProdTrack.Application", "ProdTrack.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_the_web_host()
    {
        var result = Types.InAssembly(Infrastructure).ShouldNot().HaveDependencyOn("ProdTrack.Server").GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-010")]
    public void Every_command_and_query_declares_a_policy()
    {
        var requests = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>))))
            .ToList();

        requests.Should().NotBeEmpty();
        requests.Where(t => t.GetCustomAttribute<RequiresPolicyAttribute>() is null).Select(t => t.FullName)
            .Should().BeEmpty("every use case must be authorized in the Application layer");
    }

    [Fact]
    public void Handlers_are_internal()
    {
        var handlers = Application.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))))
            .ToList();

        handlers.Should().NotBeEmpty().And.OnlyContain(t => !t.IsPublic);
    }
}
