using System;
using System.IO.Abstractions;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using Bogus;
using FluentAssertions;
using Microsoft.Build.Framework;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Tests
{
    public class BuildCrossCheckLoggerTests
    {
        public Faker Faker { get; }

        public BuildCrossCheckLoggerTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldNotInitialize()
        {
            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var fileSystem = Substitute.For<IFileSystem>();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironmentProvider>();
            var submissionService = Substitute.For<ISubmissionService>();
            var parameterParser = Substitute.For<IParameterParser>();
            var logDataBuilderFactory = Substitute.For<ILogDataBuilderFactory>();

            parameterParser.Parse(Arg.Any<string>()).Returns(new Parameters());
            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Token is not present"));
        }

        [Fact]
        public void ShouldInitialize()
        {
            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var fileSystem = Substitute.For<IFileSystem>();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironmentProvider>();
            var submissionService = Substitute.For<ISubmissionService>();
            var parameterParser = Substitute.For<IParameterParser>();
            var logDataBuilderFactory = Substitute.For<ILogDataBuilderFactory>();

            var parameters = new Parameters
            {
                Token = Faker.Random.String(12)
            };

            parameterParser.Parse(Arg.Any<string>()).Returns(parameters);

            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Enabled"));
        }

        [Fact]
        public void ShouldStartBeforeOtherEvents()
        {
            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var fileSystem = Substitute.For<IFileSystem>();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironmentProvider>();
            var submissionService = Substitute.For<ISubmissionService>();
            var parameterParser = Substitute.For<IParameterParser>();
            var logDataBuilderFactory = Substitute.For<ILogDataBuilderFactory>();

            var parameters = new Parameters
            {
                Token = Faker.Random.String(12)
            };

            parameterParser.Parse(Arg.Any<string>()).Returns(parameters);

            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var buildMessageEventArgs = new BuildWarningEventArgs(Faker.Random.String(), Faker.Random.String(),
                    Faker.Random.String(), Faker.Random.Int(), Faker.Random.Int(), Faker.Random.Int(),
                    Faker.Random.Int(), Faker.Random.String(), Faker.Random.String(), Faker.Random.String(),
                    Faker.Date.Past());

                eventSource.WarningRaised +=
                    Raise.Event<BuildWarningEventHandler>(buildMessageEventArgs);
            }).Message.Should().Be("Build not started");
        }

        [Fact]
        public void ShouldNotStartTwice()
        {
            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var fileSystem = Substitute.For<IFileSystem>();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironmentProvider>();
            var submissionService = Substitute.For<ISubmissionService>();
            var parameterParser = Substitute.For<IParameterParser>();
            var logDataBuilderFactory = Substitute.For<ILogDataBuilderFactory>();

            var parameters = new Parameters
            {
                Token = Faker.Random.String(12)
            };

            parameterParser.Parse(Arg.Any<string>()).Returns(parameters);

            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            var buildStartedEventArgs = new BuildStartedEventArgs(Faker.Random.String(), Faker.Random.String());
            eventSource.BuildStarted +=
                Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);

            buildStartedEventArgs = new BuildStartedEventArgs(Faker.Random.String(), Faker.Random.String());

            Assert.Throws<InvalidOperationException>(() =>
            {
                eventSource.BuildStarted +=
                    Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);
            }).Message.Should().Be("Build already started");
        }


        [Fact(Skip = "Not Complete")]
        public void ShouldSubmitOnComplete()
        {
            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var fileSystem = Substitute.For<IFileSystem>();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironmentProvider>();
            var submissionService = Substitute.For<ISubmissionService>();
            var parameterParser = Substitute.For<IParameterParser>();
            var logDataBuilderFactory = Substitute.For<ILogDataBuilderFactory>();

            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            var bccToken = Faker.Random.String(12);
            environment.GetEnvironmentVariable(Arg.Is("BCC_TOKEN"))
                .Returns(bccToken);

            buildCrossCheckLogger.Initialize(fileSystem, eventSource, environment, submissionService, parameterParser, logDataBuilderFactory);

            environment.Received(1).GetEnvironmentVariable(Arg.Is("BCC_TOKEN"));
            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Enabled"));

            var buildStartedEventArgs = new BuildStartedEventArgs(Faker.Random.String(), Faker.Random.String());
            eventSource.BuildStarted +=
                Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);

            var buildFinishedEventArgs = new BuildFinishedEventArgs(Faker.Random.String(), Faker.Random.String(), true);
            eventSource.BuildFinished +=
                Raise.Event<BuildFinishedEventHandler>(buildFinishedEventArgs);

            submissionService.Received(1).SubmitAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}