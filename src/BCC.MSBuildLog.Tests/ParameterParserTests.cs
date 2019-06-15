using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Interfaces.Build;
using BCC.MSBuildLog.Services;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Tests
{
    public class ParameterParserTests
    {
        static ParameterParserTests()
        {
            Faker = new Faker();
        }

        public static Faker Faker { get; }

        [Fact]
        public void ShouldGetFromBuildService()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            var buildService = Substitute.For<IBuildService>();

            var cloneRoot = Faker.Random.String(10);
            var commitHash = Faker.Random.String(10);
            var gitHubOwner = Faker.Random.String(10);
            var gitHubRepo = Faker.Random.String(10);
            var token = Faker.Random.String(10);

            buildService.CloneRoot.Returns(cloneRoot);
            buildService.CommitHash.Returns(commitHash);
            buildService.GitHubOwner.Returns(gitHubOwner);
            buildService.GitHubRepo.Returns(gitHubRepo);

            environmentProvider.GetEnvironmentVariable("BCC_TOKEN").Returns(token);

            var parameterParser = new ParameterParser(environmentProvider, buildService);
            var parameters = parameterParser.Parse(string.Empty);

            parameters.Should().NotBeNull();
            parameters.CloneRoot.Should().Be(cloneRoot);
            parameters.Hash.Should().Be(commitHash);
            parameters.Owner.Should().Be(gitHubOwner);
            parameters.Repo.Should().Be(gitHubRepo);
            parameters.Token.Should().Be(token);
        }

        [Fact]
        public void ShouldBeOverrideable()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            var buildService = Substitute.For<IBuildService>();

            buildService.CloneRoot.Returns(Faker.Random.String(10));
            buildService.CommitHash.Returns(Faker.Random.String(10));
            buildService.GitHubOwner.Returns(Faker.Random.String(10));
            buildService.GitHubRepo.Returns(Faker.Random.String(10));

            environmentProvider.GetEnvironmentVariable("BCC_TOKEN").Returns(Faker.Random.String(10));

            var cloneRoot = Faker.Random.String(10);
            var commitHash = Faker.Random.String(10);
            var gitHubOwner = Faker.Random.String(10);
            var gitHubRepo = Faker.Random.String(10);
            var token = Faker.Random.String(10);

            var parameterParser = new ParameterParser(environmentProvider, buildService);
            var parameters = parameterParser.Parse($"cloneroot={cloneRoot};hash={commitHash};owner={gitHubOwner};repo={gitHubRepo};token={token}");

            parameters.Should().NotBeNull();
            parameters.CloneRoot.Should().Be(cloneRoot);
            parameters.Hash.Should().Be(commitHash);
            parameters.Owner.Should().Be(gitHubOwner);
            parameters.Repo.Should().Be(gitHubRepo);
            parameters.Token.Should().Be(token);
        }

        [Fact]
        public void CanBeProvidedOnCommandForUnknownBuildServer()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            environmentProvider.GetEnvironmentVariable("BCC_TOKEN").Returns(Faker.Random.String(10));

            var cloneRoot = Faker.Random.String(10);
            var commitHash = Faker.Random.String(10);
            var gitHubOwner = Faker.Random.String(10);
            var gitHubRepo = Faker.Random.String(10);
            var token = Faker.Random.String(10);

            var parameterParser = new ParameterParser(environmentProvider, null);
            var parameters = parameterParser.Parse($"cloneroot={cloneRoot};hash={commitHash};owner={gitHubOwner};repo={gitHubRepo};token={token}");

            parameters.Should().NotBeNull();
            parameters.CloneRoot.Should().Be(cloneRoot);
            parameters.Hash.Should().Be(commitHash);
            parameters.Owner.Should().Be(gitHubOwner);
            parameters.Repo.Should().Be(gitHubRepo);
            parameters.Token.Should().Be(token);
        }
    }
}