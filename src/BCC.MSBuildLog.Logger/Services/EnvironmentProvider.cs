using System;
using BCC.MSBuildLog.Logger.Interfaces;

namespace BCC.MSBuildLog.Logger.Services
{
    public class EnvironmentProvider : IEnvironmentProvider
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}