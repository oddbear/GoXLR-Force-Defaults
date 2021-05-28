using System.ServiceProcess;

namespace GoXLR.Force.Defaults
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceBase.Run(new ServiceBase[]
            {
                new WorkerService()
            });
        }
    }
}
