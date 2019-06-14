using BCC.MSBuildLog.Interfaces;
using Bogus;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Tests
{
    public class ProgramTests
    {
        private static readonly Faker Faker;

        static ProgramTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldNotCallForInvalidArguments()
        {
            var buildLogProcessor = Substitute.For<IBuildLogProcessor>();
            var commandLineParser = Substitute.For<ICommandLineParser>();
            var program = new Program(commandLineParser, buildLogProcessor);

            program.Run(new string[0]);
            commandLineParser.Received(1).Parse(Arg.Any<string[]>());
            buildLogProcessor.DidNotReceive().Process(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
        }

        [Fact]
        public void ShouldCallForValidArguments()
        {
            var buildLogProcessor = Substitute.For<IBuildLogProcessor>();
            var commandLineParser = Substitute.For<ICommandLineParser>();
            var applicationArguments = new ApplicationArguments()
            {
                OutputFile = Faker.System.FilePath(),
                InputFile = Faker.System.FilePath(),
                Repo = Faker.Random.Word(),
                Owner = Faker.Random.Word(),
                Hash = Faker.Random.String(10),
                CloneRoot = Faker.System.DirectoryPath()
            };

            commandLineParser.Parse(Arg.Any<string[]>()).Returns(applicationArguments);

            var program = new Program(commandLineParser, buildLogProcessor);

            program.Run(new string[0]);
            buildLogProcessor.Received(1).Process(
                applicationArguments.InputFile,
                applicationArguments.OutputFile,
                applicationArguments.CloneRoot,
                applicationArguments.Owner,
                applicationArguments.Repo,
                applicationArguments.Hash);
        }
    }
}
