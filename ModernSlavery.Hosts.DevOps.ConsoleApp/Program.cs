using ModernSlavery.Infrastructure.DevOps.Cloud;
using System;
using System.IO;
using System.Reflection;

namespace ModernSlavery.Hosts.DevOps.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Set the console title to the exe name
            Console.Title=Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            //Process the command line
            CommandLineParser.ExecuteCommandLine(args);
        }
    }
}
