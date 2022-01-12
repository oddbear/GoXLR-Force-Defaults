
using System;
using System.Linq;
using System.ServiceProcess;

namespace GoXLR.Force.Defaults
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var service = new WorkerService();
            if (args.Contains("standalone", StringComparer.OrdinalIgnoreCase))
            {
                service.OnStart();
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                service.Stop();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { service });
            }
        }
    }
}
