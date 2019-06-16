using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Services.Build;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BCC.MSBuildLog.Tests
{
    public class BuildServiceProviderTests
    {
        static BuildServiceProviderTests()
        {
            Faker = new Faker();
        }

        public static Faker Faker { get; }

        [Fact]
        public void ShouldReturnNull()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            var environmentServiceProvider = new BuildServiceProvider(environmentProvider);
            environmentServiceProvider.GetBuildService().Should().BeNull();
        }

        [Fact]
        public void ShouldDetectAppVeyor()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            environmentProvider.GetEnvironmentVariable("APPVEYOR").Returns("True");

            var environmentServiceProvider = new BuildServiceProvider(environmentProvider);
            var environmentService = environmentServiceProvider.GetBuildService();
            environmentService.Should().BeOfType<AppVeyorBuildService>();
        }

        [Fact]
        public void ShouldDetectTravis()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            environmentProvider.GetEnvironmentVariable("TRAVIS").Returns("True");

            var environmentServiceProvider = new BuildServiceProvider(environmentProvider);
            var environmentService = environmentServiceProvider.GetBuildService();
            environmentService.Should().BeOfType<TravisBuildService>();
        }

        [Fact]
        public void ShouldDetectCircle()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            environmentProvider.GetEnvironmentVariable("CIRCLECI").Returns("True");

            var environmentServiceProvider = new BuildServiceProvider(environmentProvider);
            var environmentService = environmentServiceProvider.GetBuildService();
            environmentService.Should().BeOfType<CircleBuildService>();
        }

        [Fact]
        public void ShouldDetectJenkins()
        {
            var environmentProvider = Substitute.For<IEnvironmentProvider>();
            environmentProvider.GetEnvironmentVariable("JENKINS_HOME").Returns(Faker.System.DirectoryPath());

            var environmentServiceProvider = new BuildServiceProvider(environmentProvider);
            var environmentService = environmentServiceProvider.GetBuildService();
            environmentService.Should().BeOfType<JenkinsBuildService>();
        }
    }
}
