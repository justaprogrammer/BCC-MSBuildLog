using System;
using BCC.MSBuildLog.Logger.Interfaces;
using BCC.MSBuildLog.Logger.Interfaces.Build;
using BCC.MSBuildLog.Logger.Model;
using BCC.MSBuildLog.Logger.Services.Build;

namespace BCC.MSBuildLog.Logger.Services
{
    public  class ParameterParser: IParameterParser
    {
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IBuildService _buildService;

        public ParameterParser(IEnvironmentProvider environmentProvider, IBuildService buildService)
        {
            _environmentProvider = environmentProvider;
            _buildService = buildService;
        }

        public Parameters Parse(string input)
        {
            var buildServerMessage = _buildService != null ? $"Detected {_buildService.BuildServiceName}" : " - No Build Service Detected";
            _environmentProvider.DebugLine($"BuildCrossCheck {buildServerMessage}");
            _environmentProvider.DebugLine($"BuildCrossCheck Parameters `{input}`");

            var parameters = new Parameters();

            if (_buildService != null)
            {
                parameters.CloneRoot = _buildService.CloneRoot;
                parameters.Hash = _buildService.CommitHash;
                parameters.Owner = _buildService.GitHubOwner;
                parameters.Repo = _buildService.GitHubRepo;
            }

            parameters.Token = _environmentProvider.GetEnvironmentVariable("BCC_TOKEN");

            if (!string.IsNullOrEmpty(input))
            {
                var groups = input.Split(new[]{ ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var group in groups)
                {
                    var split = group.Split(new[] { '=' });
                    if (split.Length != 2)
                    {
                        throw new ArgumentException($"Invalid input `{group}`");
                    }

                    var key = split[0].ToLower();
                    if (key == "cloneroot")
                    {
                        parameters.CloneRoot = split[1];
                    }
                    else if (key == "hash")
                    {
                        parameters.Hash = split[1];
                    }
                    else if (key == "owner")
                    {
                        parameters.Owner = split[1];
                    }
                    else if (key == "repo")
                    {
                        parameters.Repo = split[1];
                    }
                    else if (key == "token")
                    {
                        parameters.Token = split[1];
                    }
                    else if (key == "configuration")
                    {
                        parameters.ConfigurationFile = split[1];
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown key `{split[0]}`");
                    }
                }
            }

            return parameters;
        }
    }
}