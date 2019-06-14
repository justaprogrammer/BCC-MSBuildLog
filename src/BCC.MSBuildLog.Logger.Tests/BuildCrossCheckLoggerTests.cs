using System;
using System.Linq;
using BCC.Submission.Interfaces;
using Bogus;
using FluentAssertions;
using Microsoft.Build.Framework;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Logger.Tests
{
    public class BuildCrossCheckLoggerTests
    {
        [Fact]
        public void ShouldNotInitialize()
        {
            var faker = new Faker();

            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironment>();
            var submissionService = Substitute.For<ISubmissionService>();

            buildCrossCheckLogger.Initialize(eventSource, environment, submissionService);

            environment.Received(1).GetEnvironmentVariable(Arg.Is("BCC_TOKEN"));
            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Token is not present"));
        }

        [Fact]
        public void ShouldInitialize()
        {
            var faker = new Faker();

            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironment>();
            var submissionService = Substitute.For<ISubmissionService>();

            var bccToken = faker.Random.String(12);
            environment.GetEnvironmentVariable(Arg.Is("BCC_TOKEN"))
                .Returns(bccToken);

            buildCrossCheckLogger.Initialize(eventSource, environment, submissionService);

            environment.Received(1).GetEnvironmentVariable(Arg.Is("BCC_TOKEN"));
            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Enabled"));
        }

        [Fact]
        public void ShouldStartBeforeOtherEvents()
        {
            var faker = new Faker();

            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironment>();
            var submissionService = Substitute.For<ISubmissionService>();

            var bccToken = faker.Random.String(12);
            environment.GetEnvironmentVariable(Arg.Is("BCC_TOKEN"))
                .Returns(bccToken);

            buildCrossCheckLogger.Initialize(eventSource, environment, submissionService);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var buildMessageEventArgs = new BuildMessageEventArgs(faker.Random.String(), faker.Random.String(),
                    faker.Random.String(), faker.PickRandom<MessageImportance>(), faker.Date.Past());

                eventSource.MessageRaised +=
                    Raise.Event<BuildMessageEventHandler>(buildMessageEventArgs);
            }).Message.Should().Be("Build not started");

            //TODO Check other event types
        }

        [Fact]
        public void ShouldNotStartTwice()
        {
            var faker = new Faker();

            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironment>();
            var submissionService = Substitute.For<ISubmissionService>();

            var bccToken = faker.Random.String(12);
            environment.GetEnvironmentVariable(Arg.Is("BCC_TOKEN"))
                .Returns(bccToken);

            buildCrossCheckLogger.Initialize(eventSource, environment, submissionService);

            environment.Received(1).GetEnvironmentVariable(Arg.Is("BCC_TOKEN"));
            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Enabled"));

            var buildStartedEventArgs = new BuildStartedEventArgs(faker.Random.String(), faker.Random.String());
            eventSource.BuildStarted +=
                Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);

            buildStartedEventArgs = new BuildStartedEventArgs(faker.Random.String(), faker.Random.String());

            Assert.Throws<InvalidOperationException>(() =>
            {
                eventSource.BuildStarted +=
                    Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);
            }).Message.Should().Be("Build already started");
        }


        [Fact(Skip = "Not Complete")]
        public void ShouldSubmitOnComplete()
        {
            var faker = new Faker();

            var buildCrossCheckLogger = new BuildCrossCheckLogger();
            var eventSource = Substitute.For<IEventSource>();
            var environment = Substitute.For<IEnvironment>();
            var submissionService = Substitute.For<ISubmissionService>();

            var bccToken = faker.Random.String(12);
            environment.GetEnvironmentVariable(Arg.Is("BCC_TOKEN"))
                .Returns(bccToken);

            buildCrossCheckLogger.Initialize(eventSource, environment, submissionService);

            environment.Received(1).GetEnvironmentVariable(Arg.Is("BCC_TOKEN"));
            environment.Received(1).WriteLine(Arg.Is("BuildCrossCheck Enabled"));

            var buildStartedEventArgs = new BuildStartedEventArgs(faker.Random.String(), faker.Random.String());
            eventSource.BuildStarted +=
                Raise.Event<BuildStartedEventHandler>(buildStartedEventArgs);

            var buildFinishedEventArgs = new BuildFinishedEventArgs(faker.Random.String(), faker.Random.String(), true);
            eventSource.BuildFinished +=
                Raise.Event<BuildFinishedEventHandler>(buildFinishedEventArgs);

            submissionService.Received(1).SubmitAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}