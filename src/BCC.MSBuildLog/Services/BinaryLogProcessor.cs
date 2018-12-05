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

            var messageCount = 0;
            var warningCount = 0;
            var errorCount = 0;
            var annotations = new List<Annotation>();
            var report = new StringBuilder();
            foreach (var record in _binaryLogReader.ReadRecords(binLogPath))
            {
                var buildEventArgs = record.Args;
                var buildMessage = buildEventArgs as BuildMessageEventArgs;
                var buildWarning = buildEventArgs as BuildWarningEventArgs;
                var buildError = buildEventArgs as BuildErrorEventArgs;

                if (buildMessage == null && buildWarning == null && buildError == null)
                    continue;

                CheckWarningLevel checkWarningLevel;
                string buildCode;
                string projectFile;
                string file;
                string message;
                int lineNumber;
                int endLineNumber;
                string code;
                string recordTypeString;

                if (buildMessage != null)
                {
                    if (buildMessage.Code == null)
                    {
                        continue;
                    }

                    messageCount++;
                    recordTypeString = "Message";

                    checkWarningLevel = CheckWarningLevel.Notice;
                    buildCode = buildMessage.Code;
                    projectFile = buildMessage.ProjectFile;
                    file = buildMessage.File;
                    code = buildMessage.Code;
                    message = buildMessage.Message;
                    lineNumber = buildMessage.LineNumber;
                    endLineNumber = buildMessage.EndLineNumber;
                }
                else if (buildWarning != null)
                {
                    warningCount++;
                    recordTypeString = "Warning";

                    checkWarningLevel = CheckWarningLevel.Warning;
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
                    errorCount++;
                    recordTypeString = "Error";

                    checkWarningLevel = CheckWarningLevel.Failure;
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

                var messageFilePath = projectFile ?? file;
                var fileIsCloneSubroot = true;
                var filePath = GetFilePath(cloneRoot, messageFilePath, file);
                if (filePath == null)
                {
                    fileIsCloneSubroot = false;
                    filePath = GetSpecialFilePath(messageFilePath);
                    if (filePath == null)
                    {
                        throw new InvalidOperationException($"FilePath `{messageFilePath}` is not a child of `{cloneRoot}`");
                    }
                }

                var title = $"{code}: {filePath}({lineNumber})";

                ReportAs reportAs = ReportAs.AsIs;
                if (ruleDictionary?.TryGetValue(buildCode, out reportAs) ?? false)
                {
                    switch (reportAs)
                    {
                        case ReportAs.Ignore:
                            continue;
                        case ReportAs.AsIs:
                            break;
                        case ReportAs.Notice:
                            checkWarningLevel = CheckWarningLevel.Notice;
                            break;
                        case ReportAs.Warning:
                            checkWarningLevel = CheckWarningLevel.Warning;
                            break;
                        case ReportAs.Error:
                            checkWarningLevel = CheckWarningLevel.Failure;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (fileIsCloneSubroot)
                {
                    annotations.Add(CreateAnnotation(checkWarningLevel,
                        cloneRoot,
                        title,
                        message,
                        lineNumber,
                        endLineNumber,
                        filePath));
                }

                if (!reportingMaxed)
                {
                    var lineReference = lineNumber != endLineNumber ? $"L{lineNumber}-L{endLineNumber}" : $"L{lineNumber}";

                    var line = $"- [{filePath}({lineNumber})](https://github.com/{owner}/{repo}/tree/{hash}/{filePath}#{lineReference}) **{recordTypeString} - {code}** {message}{Environment.NewLine}";
                    var lineBytes = Encoding.Unicode.GetByteCount(line) / 1024.0;

                    if (reportTotalBytes + lineBytes < 128.0)
                    {
                        report.Append(line);
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
                MessageCount = messageCount,
                WarningCount = warningCount,
                ErrorCount = errorCount,
                Report = report.ToString()
            };
        }

        private Annotation CreateAnnotation(CheckWarningLevel checkWarningLevel, [NotNull] string cloneRoot, [NotNull] string title, [NotNull] string message, int lineNumber, int endLineNumber, string getFilePath)
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

            return null;
        }

        private string GetSpecialFilePath(string filePath)
        {
            string nugetPath = ClipPath(filePath, ".nuget");
            if (nugetPath != null)
            {
                return nugetPath;
            }

            string dotnetPath = ClipPath(filePath, "dotnet");
            if (dotnetPath != null)
            {
                return dotnetPath;
            }

            string msbuildPath = ClipPath(filePath, "msbuild\\microsoft");
            if (msbuildPath != null)
            {
                return msbuildPath;
            }

            return null;
        }

        private static string ClipPath(string filePath, string containsPath)
        {
            var position = filePath.IndexOf(containsPath, StringComparison.InvariantCultureIgnoreCase);
            string result = null;
            if (position != -1)
            {
                result = filePath.Substring(position).Replace("\\", "/");
            }

            return result;
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
