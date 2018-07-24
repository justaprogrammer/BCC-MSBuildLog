﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Submitter : ISubmitter
    {
        private ICheckRunsClient CheckRunsClient { get; }
        private ILogger<Submitter> Logger { get; }

        public Submitter(ICheckRunsClient checkRunsClient,
            ILogger<Submitter> logger = null)
        {
            CheckRunsClient = checkRunsClient;
            Logger = logger ?? new NullLogger<Submitter>();
        }

        public async Task<CheckRun> SubmitCheckRun(string owner, string name, string headSha,
            string checkRunName, BuildDetails buildDetails, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt,
            DateTimeOffset completedAt, string cloneRoot)
        {
            var newCheckRunAnnotations = buildDetails.Annotations.Select(annotation =>
            {
                CheckWarningLevel warningLevel;
                if (annotation.AnnotationWarningLevel == AnnotationWarningLevel.Warning)
                {
                    warningLevel = CheckWarningLevel.Warning;
                }
                else if (annotation.AnnotationWarningLevel == AnnotationWarningLevel.Notice)
                {
                    warningLevel = CheckWarningLevel.Notice;
                }
                else
                {
                    warningLevel = CheckWarningLevel.Failure;
                }

                var blobHref = BlobHref(owner, name, headSha, annotation.Filename);
                var newCheckRunAnnotation = new NewCheckRunAnnotation(annotation.Filename, blobHref, annotation.LineNumber, annotation.EndLine, warningLevel, annotation.Message)
                {
                    Title = annotation.Title
                };

                return newCheckRunAnnotation;
            }).ToList();

            var newCheckRun = new NewCheckRun(checkRunName, headSha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = newCheckRunAnnotations
                },
                Status = CheckStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Conclusion = buildDetails.Annotations
                    .Any(annotation => annotation.AnnotationWarningLevel == AnnotationWarningLevel.Failure) ? CheckConclusion.Failure : CheckConclusion.Success
            };

            return await CheckRunsClient.Create(owner, name, newCheckRun);
        }

        private static string BlobHref(string owner, string repository, string sha, string file)
        {
            return $"https://github.com/{owner}/{repository}/blob/{sha}/{file.Replace(@"\", "/")}";
        }
    }
}