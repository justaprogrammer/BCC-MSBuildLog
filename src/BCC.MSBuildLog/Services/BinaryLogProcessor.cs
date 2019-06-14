extern alias StructuredLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Extensions;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StructuredLogger::Microsoft.Build.Logging;

namespace BCC.MSBuildLog.Services
{
    public class BinaryLogProcessor : IBinaryLogProcessor
    {
        private readonly IBinaryLogReader _binaryLogReader;
        private ILogger<BinaryLogProcessor> Logger { get; }

        public BinaryLogProcessor(IBinaryLogReader binaryLogReader, ILogger<BinaryLogProcessor> logger = null)
        {
            _binaryLogReader = binaryLogReader;
            Logger = logger ?? new NullLogger<BinaryLogProcessor>();
        }

        /// <inheritdoc />
        public LogData ProcessLog(string binLogPath, string cloneRoot, string owner, string repo, string hash, CheckRunConfiguration configuration = null)
        {
            Logger.LogInformation("ProcessLog binLogPath:{0} cloneRoot:{1}", binLogPath, cloneRoot);

            var ruleDictionary = 
                configuration?.Rules?.ToDictionary(rule => rule.Code, rule => rule.ReportAs);

            var reportTotalBytes = 0.0;
            var reportingMaxed = false;

            var warningCount = 0;
            var errorCount = 0;
            var annotations = new List<Annotation>();
            var report = new StringBuilder();
            foreach (var record in _binaryLogReader.ReadRecords(binLogPath))
            {
                var annotationAndLine = GetAnnotationAndLine(cloneRoot, owner, repo, hash, ruleDictionary, record.Args);
                if (annotationAndLine.Item1 == null) continue;

                annotations.Add(annotationAndLine.Item1);

                if (annotationAndLine.Item1.AnnotationLevel == AnnotationLevel.Warning)
                {
                    warningCount++;
                }
                else if(annotationAndLine.Item1.AnnotationLevel == AnnotationLevel.Failure)
                {
                    errorCount++;
                }

                if (!reportingMaxed)
                { 
                    var lineBytes = Encoding.Unicode.GetByteCount(annotationAndLine.Item2) / 1024.0;

                    if (reportTotalBytes + lineBytes < 128.0)
                    {
                        report.Append(annotationAndLine.Item2);
                        reportTotalBytes += lineBytes;
                    }
                    else
                    {
                        reportingMaxed = true;
                    }
                }
            }

            return new LogData
            {
                Annotations = annotations.ToArray(),
                WarningCount = warningCount,
                ErrorCount = errorCount,
                Report = report.ToString()
            };
        }

        private (Annotation, string) GetAnnotationAndLine(string cloneRoot, string owner, string repo, string hash,
            IReadOnlyDictionary<string, ReportAs> ruleDictionary, BuildEventArgs recordArgs)
        {
            int lineNumber;
            int endLineNumber;

            var buildWarning = recordArgs as BuildWarningEventArgs;
            var buildError = recordArgs as BuildErrorEventArgs;

            if (buildWarning == null && buildError == null) return (null, null);

            AnnotationLevel checkWarningLevel;
            BuildEventLevel buildEventLevel;
            string buildCode;
            string projectFile;
            string file;
            string message;
            string code;
            string recordTypeString;
            if (buildWarning != null)
            {
                recordTypeString = "Warning";

                checkWarningLevel = AnnotationLevel.Warning;
                buildCode = buildWarning.Code;
                projectFile = buildWarning.ProjectFile;
                file = buildWarning.File;
                code = buildWarning.Code;
                message = buildWarning.Message;
                lineNumber = buildWarning.LineNumber;
                endLineNumber = buildWarning.EndLineNumber;
            }
            else
            {
                recordTypeString = "Error";

                checkWarningLevel = AnnotationLevel.Failure;
                buildCode = buildError.Code;
                projectFile = buildError.ProjectFile;
                file = buildError.File;
                code = buildError.Code;
                message = buildError.Message;
                lineNumber = buildError.LineNumber;
                endLineNumber = buildError.EndLineNumber;
            }


            endLineNumber = endLineNumber == 0 ? lineNumber : endLineNumber;

            if (buildCode.StartsWith("MSB"))
            {
                if (projectFile == null)
                {
                    projectFile = file;
                }
                else
                {
                    file = projectFile;
                }
            }

            var filePath = GetFilePath(cloneRoot, projectFile ?? file, file);
            var title = $"{code}: {filePath}({lineNumber})";

            ReportAs reportAs = ReportAs.AsIs;
            if (ruleDictionary?.TryGetValue(buildCode, out reportAs) ?? false)
            {
                switch (reportAs)
                {
                    case ReportAs.Ignore:
                        return (null, null);
                    case ReportAs.AsIs:
                        break;
                    case ReportAs.Notice:
                        checkWarningLevel = AnnotationLevel.Notice;
                        break;
                    case ReportAs.Warning:
                        checkWarningLevel = AnnotationLevel.Warning;
                        break;
                    case ReportAs.Error:
                        checkWarningLevel = AnnotationLevel.Failure;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var annotation = CreateAnnotation(checkWarningLevel,
                cloneRoot,
                title,
                message,
                lineNumber,
                endLineNumber,
                filePath);

            var lineReference = lineNumber != endLineNumber ? $"L{lineNumber}-L{endLineNumber}" : $"L{lineNumber}";

            var line = $"- [{filePath}({lineNumber})](https://github.com/{owner}/{repo}/tree/{hash}/{filePath}#{lineReference}) **{recordTypeString} - {code}** {message}{Environment.NewLine}";

            return (annotation, line);
        }

        private Annotation CreateAnnotation(AnnotationLevel checkWarningLevel, [NotNull] string cloneRoot, [NotNull] string title, [NotNull] string message, int lineNumber, int endLineNumber, string getFilePath)
        {
            if (cloneRoot == null)
            {
                throw new ArgumentNullException(nameof(cloneRoot));
            }

            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new Annotation(
                getFilePath,
                lineNumber,
                endLineNumber,
                checkWarningLevel,
                message)
            {
                Title = title
            };
        }

        private string GetFilePath(string cloneRoot, string projectFile, string file)
        {
            if (projectFile == null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var filePath = Path.Combine(Path.GetDirectoryName(projectFile), file);
            if (filePath.IsSubPathOf(cloneRoot))
            {
                return GetRelativePath(filePath, cloneRoot).Replace("\\", "/");
            }

            var dotNugetPosition = filePath.IndexOf(".nuget");
            if (dotNugetPosition != -1)
            {
                return filePath.Substring(dotNugetPosition).Replace("\\", "/");
            }

            throw new InvalidOperationException($"FilePath `{filePath}` is not a child of `{cloneRoot}`");
        }

        private string GetRelativePath(string filespec, string folder)
        {
            //https://stackoverflow.com/a/703292/104877

            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
