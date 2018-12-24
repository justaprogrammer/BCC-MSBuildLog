using BCC.Core.Services;
using BCC.MSBuildLog.Services;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Tests.Services
{
    public class CommandLineParserTests
    {
        private static readonly Faker Faker;

        static CommandLineParserTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldCallForHelpIfNothingSent()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);
            var applicationArguments = commandLineParser.Parse(new string[0]);

            applicationArguments.Should().BeNull();

            listener.Received(1).Callback(Arg.Any<string>());
        }

        [Fact]
        public void ShouldCallForHelpIfNoOwnership()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var hash = Faker.Random.String(10);

            var applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
                "--cloneRoot", $@"""{cloneRoot}""",
                "--hash", $@"""{hash}""",
            });

            applicationArguments.Should().BeNull();

            listener.Received(1).Callback(Arg.Any<string>());
        }

        [Fact]
        public void ShouldCallForHelpIfOwnerRepoIsInvalid()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var hash = Faker.Random.String(10);

            var applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
                "--cloneRoot", $@"""{cloneRoot}""",
                "--ownerrepo", "invalid_Format",
                "--hash", $@"""{hash}""",
            });

            applicationArguments.Should().BeNull();

            listener.Received(1).Callback(Arg.Any<string>());
        }

        [Fact]
        public void ShouldRequireRequiredArguments()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var owner = Faker.Random.Word();
            var repo = Faker.Random.Word();
            var hash = Faker.Random.String(10);

            var applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
                "--cloneRoot", $@"""{cloneRoot}""",
                "--owner", $@"""{owner}""",
                "--repo", $@"""{repo}""",
                "--hash", $@"""{hash}""",
            });

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);
            applicationArguments.OwnerRepo.Should().BeNull();
            applicationArguments.Owner.Should().Be(owner);
            applicationArguments.Repo.Should().Be(repo);
            applicationArguments.Hash.Should().Be(hash);
            applicationArguments.ConfigurationFile.Should().BeNull();

            commandLineParser = new CommandLineParser(listener.Callback, environmentService);
            applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
                "--cloneRoot", $@"""{cloneRoot}""",
                "--ownerRepo", $@"""{owner}/{repo}""",
                "--hash", $@"""{hash}""",
            });

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);
            applicationArguments.OwnerRepo.Should().Be($"{owner}/{repo}");
            applicationArguments.Owner.Should().Be(owner);
            applicationArguments.Repo.Should().Be(repo);
            applicationArguments.Hash.Should().Be(hash);
            applicationArguments.ConfigurationFile.Should().BeNull();
        }

        [Fact]
        public void ShouldBeAbleToSetConfigurationArgument()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var owner = Faker.Random.Word();
            var repo = Faker.Random.Word();
            var hash = Faker.Random.String(10);
            var configurationFile = Faker.System.FilePath();

            var applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
                "--cloneRoot", $@"""{cloneRoot}""",
                "--owner", $@"""{owner}""",
                "--repo", $@"""{repo}""",
                "--hash", $@"""{hash}""",
                "--configuration", $@"""{configurationFile}"""
            });

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);
            applicationArguments.Owner.Should().Be(owner);
            applicationArguments.Repo.Should().Be(repo);
            applicationArguments.Hash.Should().Be(hash);
            applicationArguments.ConfigurationFile.Should().Be(configurationFile);
        }

        [Fact]
        public void ShouldPullArgumentsFromEnvironment()
        {
            var listener = Substitute.For<ICommandLineParserCallBackListener>();
            var environmentService = Substitute.For<IEnvironmentService>();
            var commandLineParser = new CommandLineParser(listener.Callback, environmentService);

            var inputPath = Faker.System.FilePath();
            var outputPath = Faker.System.FilePath();
            var cloneRoot = Faker.System.DirectoryPath();
            var owner = Faker.Random.Word();
            var repo = Faker.Random.Word();
            var hash = Faker.Random.String(10);

            environmentService.BuildFolder.Returns(cloneRoot);
            environmentService.GitHubOwner.Returns(owner);
            environmentService.GitHubRepo.Returns(repo);
            environmentService.CommitHash.Returns(hash);

            var applicationArguments = commandLineParser.Parse(new[]
            {
                "--input", $@"""{inputPath}""",
                "--output", $@"""{outputPath}""",
            });

            listener.DidNotReceive().Callback(Arg.Any<string>());

            applicationArguments.Should().NotBeNull();
            applicationArguments.InputFile.Should().Be(inputPath);
            applicationArguments.OutputFile.Should().Be(outputPath);
            applicationArguments.CloneRoot.Should().Be(cloneRoot);
            applicationArguments.OwnerRepo.Should().BeNull();
            applicationArguments.Owner.Should().Be(owner);
            applicationArguments.Repo.Should().Be(repo);
            applicationArguments.Hash.Should().Be(hash);
            applicationArguments.ConfigurationFile.Should().BeNull();
        }

        public interface ICommandLineParserCallBackListener
        {
            void Callback(string obj);
        }
    }
}