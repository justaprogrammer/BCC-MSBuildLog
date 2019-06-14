using System;
using System.Diagnostics.CodeAnalysis;

namespace BCC.Core.Model.CheckRunSubmission
{
    public class Annotation : IEquatable<Annotation>
    {
        public string Filename { get; set;  }
        public AnnotationLevel AnnotationLevel { get; set; }
        public BuildEventLevel BuildEventLevel { get; set; }
        public string Title { get; set; }
        public string RawDetails { get; set; }
        public string Message { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public Annotation()
        {
        }

        public Annotation(string filename,
            int startLine, int endLine, AnnotationLevel annotationLevel, BuildEventLevel buildEventLevel, string message)
        {
            Filename = filename;
            AnnotationLevel = annotationLevel;
            BuildEventLevel = buildEventLevel;
            Message = message;
            StartLine = startLine;
            EndLine = endLine;
        }

        public bool Equals(Annotation other)
        {
            if (other == null)
                return false;

            return Filename == other.Filename &&
                   AnnotationLevel == other.AnnotationLevel &&
                   BuildEventLevel == other.BuildEventLevel &&
                   Title == other.Title &&
                   RawDetails == other.RawDetails &&
                   Message == other.Message &&
                   StartLine == other.StartLine &&
                   EndLine == other.EndLine;
        }
    }
}