extern alias StructuredLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Legacy.MSBuild.Extensions;
using BCC.MSBuildLog.Legacy.MSBuild.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Model;
using JetBrains.Annotations;
using Microsoft.Build.Framework;

namespace BCC.MSBuildLog.Legacy.MSBuild.Services
{
    public class LogDataBuilder : ILogDataBuilder
    {
        private readonly string _cloneRoot;
        private readonly string _owner;
        private readonly string _repo;
        private readonly string _hash;
        private readonly Dictionary<string, ReportAs> _ruleDictionary;

        private double _reportTotalBytes;
        private bool _reportingMaxed;
        private int _warningCount;
        private int _errorCount;
        private List<Annotation> _annotations;
        private StringBuilder _report;

        public LogDataBuilder(string cloneRoot, string owner, string repo, string hash, CheckRunConfiguration configuration)
        {
            _cloneRoot = cloneRoot;
            _owner = owner;
            _repo = repo;
            _hash = hash;
            _ruleDictionary =
                configuration?.Rules?.ToDictionary(rule => rule.Code, rule => rule.ReportAs);

            _annotations = new List<Annotation>();
            _report = new StringBuilder();
        }

        public LogData Build()
        {
            return new LogData
            {
                Annotations = _annotations.ToArray(),
                WarningCount = _warningCount,
                ErrorCount = _errorCount,
                Report = _report.ToString()
            };
        }

        public void ProcessRecord(BuildEventArgs recordArgs)
        {
            int lineNumber;
            int endLineNumber;

            var buildWarning = recordArgs as BuildWarningEventArgs;
            var buildError = recordArgs as BuildErrorEventArgs;

            if (buildWarning == null && buildError == null)
            {
                return;
            }

            AnnotationLevel checkWarningLevel;
            var buildEventLevel = BuildEventLevel.Warning;
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
                buildEventLevel = BuildEventLevel.Error;
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

            var filePath = GetFilePath(projectFile ?? file, file);
            var title = $"{code}: {filePath}({lineNumber})";

            var reportAs = ReportAs.AsIs;
            if (_ruleDictionary?.TryGetValue(buildCode, out reportAs) ?? false)
            {
                switch (reportAs)
                {
                    case ReportAs.Ignore:
                        return;
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
                _cloneRoot,
                title,
                message,
                lineNumber,
                endLineNumber,
                filePath,
                buildEventLevel);

            var lineReference = lineNumber != endLineNumber ? $"L{lineNumber}-L{endLineNumber}" : $"L{lineNumber}";

            var line = $"- [{filePath}({lineNumber})](https://github.com/{_owner}/{_repo}/tree/{_hash}/{filePath}#{lineReference}) **{recordTypeString} - {code}** {message}{Environment.NewLine}";

            _annotations.Add(annotation);

            if (annotation.BuildEventLevel == BuildEventLevel.Warning)
            {
                _warningCount++;
            }
            else if (annotation.BuildEventLevel == BuildEventLevel.Error)
            {
                _errorCount++;
            }

            if (!_reportingMaxed)
            {
                var lineBytes = Encoding.Unicode.GetByteCount(line) / 1024.0;

                if (_reportTotalBytes + lineBytes < 128.0)
                {
                    _report.Append(line);
                    _reportTotalBytes += lineBytes;
                }
                else
                {
                    _reportingMaxed = true;
                }
            }
        }

        public void Clear()
        {
            _reportTotalBytes = 0.0;
            _reportingMaxed = false;

            _warningCount = 0;
            _errorCount = 0;
            _annotations = new List<Annotation>();
            _report = new StringBuilder();
        }

        private Annotation CreateAnnotation(AnnotationLevel checkWarningLevel, [NotNull] string cloneRoot,
            [NotNull] string title, [NotNull] string message, int lineNumber, int endLineNumber, string getFilePath,
            BuildEventLevel buildEventLevel)
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
                buildEventLevel,
                message)
            {
                Title = title
            };
        }

        private string GetFilePath(string projectFile, string file)
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
            if (filePath.IsSubPathOf(_cloneRoot))
            {
                return GetRelativePath(filePath, _cloneRoot).Replace("\\", "/");
            }

            var dotNugetPosition = filePath.IndexOf(".nuget");
            if (dotNugetPosition != -1)
            {
                return filePath.Substring(dotNugetPosition).Replace("\\", "/");
            }

            throw new InvalidOperationException($"FilePath `{filePath}` is not a child of `{_cloneRoot}`");
        }

        private static string GetRelativePath(string filespec, string folder)
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