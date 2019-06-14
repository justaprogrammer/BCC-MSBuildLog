using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Serialization;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace BCC.Core.Tests.Serialization
{
    public class CreateCheckRunSerializerTests
    {
        protected internal static readonly CreateCheckRun CreateCheckRun = new CreateCheckRun(
            "MSBuild Log", 
            "0 errors - 1 warning", 
            "- [TestConsoleApp1/Program.cs(13)](https://github.com/justaprogrammer/TestConsoleApp1/tree/815aa7a3051cc0d0ae6c6c2c89cba125e8027d86/TestConsoleApp1/Program.cs#L13) **Warning - CS0219** The variable 'hello' is assigned but its value is never used\r\n", 
            CheckConclusion.Success, 
            DateTimeOffset.Parse("2018-12-07T08:39:22.5858425-05:00"),
            DateTimeOffset.Parse("2018-12-07T08:39:22.6347882-05:00"))
        {
            Annotations = new[]
            {
                new Annotation("TestConsoleApp1/Program.cs", 13, 13, AnnotationLevel.Warning, "The variable 'hello' is assigned but its value is never used")
                {
                    Title = "CS0219: TestConsoleApp1/Program.cs(13)"
                }
            }
        };

        static CreateCheckRunSerializerTests()
        {
            Faker = new Faker();
        }
        public static Faker Faker { get; }

        [Fact]
        public void TestSerializer()
        {
            var serialize = CreateCheckRunSerializer.Serialize(CreateCheckRun);
            serialize.Should().Be(Resources.CheckRunSample);
        }

        [Fact]
        public void TestDeSerializer()
        {
            var createCheckRun = CreateCheckRunSerializer.DeSerialize(Resources.CheckRunSample);
            createCheckRun.Should().BeEquivalentTo(CreateCheckRun);
        }

        [Fact]
        public void TestDeSerializerAgainstOriginal()
        {
            var createCheckRun = CreateCheckRunSerializer.DeSerialize(Resources.CheckRunSampleOriginal);
            createCheckRun.Should().BeEquivalentTo(CreateCheckRun);
        }

        [Fact]
        public void TestDeSerializerAgainstWrong()
        {
            new Action(() => CreateCheckRunSerializer.DeSerialize(Resources.CheckRunSampleWrong))
                .Should()
                .Throw<JsonSerializationException>()
                .WithMessage("Could not find member 'names' on object of type 'CreateCheckRun'. Path 'names', line 1, position 9.");
        }
    }
}
