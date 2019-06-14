using System;

namespace BCC.MSBuildLog.Logger
{
    public class DefaultEnvironment : IEnvironment
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