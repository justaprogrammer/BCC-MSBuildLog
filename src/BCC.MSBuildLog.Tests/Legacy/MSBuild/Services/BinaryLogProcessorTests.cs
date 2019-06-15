using System;
using System.IO;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Legacy.MSBuild.Model;
using BCC.MSBuildLog.Legacy.MSBuild.Services;
using BCC.MSBuildLog.Tests.Legacy.MSBuild.Util;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BCC.MSBuildLog.Tests.Legacy.MSBuild.Services
{
    public class BinaryLogProcessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<BinaryLogProcessorTests> _logger;

        private static readonly Faker Faker;

        public BinaryLogProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<BinaryLogProcessorTests>(testOutputHelper);
        }

        static BinaryLogProcessorTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);
            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Warning_For_Other_Code()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS02191",
                        ReportAs = ReportAs.Error
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10), checkRunConfiguration);

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);
            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Warning()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Warning
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10), checkRunConfiguration);

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);
            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }
         
        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Notice()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Notice
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10), checkRunConfiguration);

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);
            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Notice,
                    BuildEventLevel.Warning,
                    "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Error()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Error
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10), checkRunConfiguration);

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);
            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Failure,
                    BuildEventLevel.Warning,
                    "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_Warning_ConfigureAs_Ignore()
        {
            var checkRunConfiguration = new CheckRunConfiguration()
            {
                Rules = new LogAnalyzerRule[]
                {
                    new LogAnalyzerRule()
                    {
                        Code = "CS0219",
                        ReportAs = ReportAs.Ignore
                    }
                }
            };

            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1warning.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10), checkRunConfiguration);
            logData.Annotations.Should().BeEmpty();

            logData.Report.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_Error()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-1error.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(1);
            logData.WarningCount.Should().Be(0);

            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    13,
                    13,
                    AnnotationLevel.Failure,
                    BuildEventLevel.Error,
                    "; expected")
                {
                    Title = "CS1002: TestConsoleApp1/Program.cs(13)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_TestConsoleApp1_CodeAnalysis()
        {
            var cloneRoot = @"C:\projects\testconsoleapp1\";
            var logData = ProcessLog("testconsoleapp1-codeanalysis.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1);

            logData.Annotations.Should().AllBeEquivalentTo(
                new Annotation(
                    "TestConsoleApp1/Program.cs",
                    20,
                    20,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "Microsoft.Usage : 'Program.MyClass' contains field 'Program.MyClass._inner' that is of IDisposable type: 'Program.MyOTherClass'. Change the Dispose method on 'Program.MyClass' to call Dispose or Close on this field.")
                {
                    Title = "CA2213: TestConsoleApp1/Program.cs(20)"
                }
            );

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_MSBLOC()
        {
            var cloneRoot = @"C:\projects\msbuildlogoctokitchecker\";
            var logData = ProcessLog("msbloc.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(10);

            logData.Annotations.Length.Should().Be(10);

            logData.Annotations[0].Should().BeEquivalentTo(
                new Annotation(
                    "MSBLOC.Core.Tests/Services/BinaryLogProcessorTests.cs",
                    56,
                    56,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "The variable 'filename' is assigned but its value is never used")
                {
                    Title = "CS0219: MSBLOC.Core.Tests/Services/BinaryLogProcessorTests.cs(56)"
                });

            logData.Annotations[1].Should().BeEquivalentTo(
                new Annotation(
                    "MSBLOC.Core.Tests/Services/BinaryLogProcessorTests.cs",
                    83,
                    83,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "The variable 'filename' is assigned but its value is never used")
                {
                    Title = "CS0219: MSBLOC.Core.Tests/Services/BinaryLogProcessorTests.cs(83)"
                });

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Parse_OctokitGraphQL()
        {
            var cloneRoot = @"C:\projects\octokit-graphql\";
            var logData = ProcessLog("octokit.graphql.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(803);

            logData.Annotations.Length.Should().Be(803);

            logData.Annotations[0].Should().BeEquivalentTo(
                new Annotation(
                    "Octokit.GraphQL.Core/Connection.cs",
                    43,
                    43,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "Missing XML comment for publicly visible type or member 'Connection.Uri'")
                {
                    Title = "CS1591: Octokit.GraphQL.Core/Connection.cs(43)"
                });

            logData.Annotations[1].Should().BeEquivalentTo(
                new Annotation(
                    "Octokit.GraphQL.Core/Connection.cs",
                    44,
                    44,
                    AnnotationLevel.Warning,
                    BuildEventLevel.Warning,
                    "Missing XML comment for publicly visible type or member 'Connection.CredentialStore'")
                {
                    Title = "CS1591: Octokit.GraphQL.Core/Connection.cs(44)"
                });

            logData.Report.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Parse_GitHubVisualStudio()
        {
            var cloneRoot = @"c:\users\spade\projects\github\visualstudio\";
            var logData = ProcessLog("visualstudio.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1556);

            logData.Report.Should().NotBeNullOrWhiteSpace();

            var reportKbytes = Encoding.Unicode.GetByteCount(logData.Report) / 1024.0;
            reportKbytes.Should().BeLessThan(128.0);
        }

        [Fact]
        public void Should_Parse_GitHubVisualStudio_Recent()
        {
            var cloneRoot = @"c:\users\spade\projects\github\visualstudio\";
            var logData = ProcessLog("visualstudio-recent.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(1304);

            logData.Report.Should().NotBeNullOrWhiteSpace();

            var reportKbytes = Encoding.Unicode.GetByteCount(logData.Report) / 1024.0;
            reportKbytes.Should().BeLessThan(128.0);
        }

        [Fact]
        public void Should_Parse_DBATools()
        {
            var cloneRoot = @"c:\github\dbatools\bin\projects\dbatools\";
            var logData = ProcessLog("dbatools.binlog", cloneRoot, Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));

            logData.ErrorCount.Should().Be(0);
            logData.WarningCount.Should().Be(0);

            logData.Annotations.Length.Should().Be(0);

            logData.Report.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_ThrowWhen_BuildPath_Outisde_CloneRoot()
        {
            var invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                ProcessLog("testconsoleapp1-1warning.binlog", @"C:\projects\testconsoleapp2\", Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));
            });

            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");

            invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
            {
                ProcessLog("testconsoleapp1-1error.binlog", @"C:\projects\testconsoleapp2\", Faker.Internet.UserName(), Faker.Random.Word(), Faker.Random.String(10));
            });
            invalidOperationException.Message.Should().Be(@"FilePath `C:\projects\testconsoleapp1\TestConsoleApp1\Program.cs` is not a child of `C:\projects\testconsoleapp2\`");
        }

        private LogData ProcessLog(string resourceName, string cloneRoot, string userName, string repo, string hash, CheckRunConfiguration checkRunConfiguration = null)
        {
            var resourcePath = TestUtils.GetResourcePath(resourceName);
            File.Exists(resourcePath).Should().BeTrue();

            var binaryLogReader = new BinaryLogReader();
            var logProcessor = new BinaryLogProcessor(binaryLogReader, TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            return logProcessor.ProcessLog(resourcePath, cloneRoot, userName, repo, hash, checkRunConfiguration);
        }
    }
}