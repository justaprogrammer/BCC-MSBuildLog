using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BCC.Core.Model.CheckRunSubmission;
using Bogus;
using FluentAssertions;
using Xunit;

namespace BCC.Core.Tests.Model
{
    public class CreateCheckRunTests
    {
        [SuppressMessage("ReSharper", "ArgumentsStyleOther")]
        [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
        static CreateCheckRunTests()
        {
            Faker = new Faker();
            FakeAnnotation = new Faker<Annotation>()
                .CustomInstantiator(f =>
                {
                    var lineNumber = f.Random.Int(1);
                    return new Annotation(
                        filename: f.System.FileName(),
                        startLine: lineNumber,
                        endLine: lineNumber,
                        annotationLevel: f.PickRandom<AnnotationLevel>(),
                        message: f.Lorem.Word())
                    {
                        Title = f.Random.Words(3)
                    };
                });

            FakeCheckRunImage = new Faker<CheckRunImage>()
                .CustomInstantiator(f => new CheckRunImage(alt:f.Random.Words(3), imageUrl:f.Internet.Url())
                {
                    Caption = f.Random.Words(3)
                });

            FakeCheckRun = new Faker<CreateCheckRun>()
                .CustomInstantiator(f => new CreateCheckRun(
                    name: f.Random.Word(),
                    title: f.Random.Word(),
                    summary: f.Random.Word(),
                    conclusion: f.Random.Enum<CheckConclusion>(),
                    startedAt: f.Date.PastOffset(2),
                    completedAt: f.Date.PastOffset())
                {
                    Annotations = f.Random.Bool() ? null : FakeAnnotation.Generate(f.Random.Int(2, 10)).ToArray(),
                    Images = f.Random.Bool() ? null : FakeCheckRunImage.Generate(f.Random.Int(2, 10)).ToArray()
                });
        }

        public static Faker<CreateCheckRun> FakeCheckRun { get; set; }
        public static Faker<Annotation> FakeAnnotation { get; }
        public static Faker<CheckRunImage> FakeCheckRunImage { get; set; }
        public static Faker Faker { get; }

        [Fact]
        public void EquatableTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var createCheckRun = FakeCheckRun.Generate();

                var copy = new CreateCheckRun()
                {
                    Name = createCheckRun.Name,
                    Title = createCheckRun.Title,
                    StartedAt = createCheckRun.StartedAt,
                    CompletedAt = createCheckRun.CompletedAt,
                    Conclusion = createCheckRun.Conclusion,
                    Summary = createCheckRun.Summary,
                    Annotations = createCheckRun.Annotations?.Select(annotation => annotation).ToArray(),
                    Images = createCheckRun.Images?.Select(annotation => annotation).ToArray(),
                };

                createCheckRun.Equals(copy).Should().BeTrue();
            }
        }
    }
}
