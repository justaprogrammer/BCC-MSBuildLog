using System;
using System.Linq;
using System.Threading;

namespace BCC.Core.Model.CheckRunSubmission
{
    public class CreateCheckRun : IEquatable<CreateCheckRun>
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Text { get; set; }
        public CheckConclusion Conclusion { get; set; }
        public Annotation[] Annotations { get; set; }
        public CheckRunImage[] Images { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }

        public CreateCheckRun()
        {
        }

        public CreateCheckRun(string name, string title, string summary, CheckConclusion conclusion, DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Conclusion = conclusion;
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }

        public bool Equals(CreateCheckRun other)
        {
            if (other == null)
                return false;

            var b = Name == other.Name &&
                    Title == other.Title &&
                    Summary == other.Summary &&
                    Text == other.Text &&
                    Conclusion == other.Conclusion &&
                    StartedAt == other.StartedAt &&
                    CompletedAt == other.CompletedAt;

            var c = (Annotations == null) == (other.Annotations == null) &&
                    (Annotations == null || Annotations.SequenceEqual(other.Annotations));

            var d = (Images == null) == (other.Images == null) &&
                    (Images == null || Images.SequenceEqual(other.Images));

            return b && c && d;
        }
    }
}