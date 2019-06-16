using System;
using BCC.MSBuildLog.Interfaces;

namespace BCC.MSBuildLog.Services
{
    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly bool _isDebug;

        public EnvironmentProvider()
        {
            _isDebug = !string.IsNullOrEmpty(GetEnvironmentVariable("BCC_DEBUG"));
        }

        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public void DebugLine(string line)
        {
            if(_isDebug) WriteLine(line);
        }
    }
}