/*namespace BEACN.Mix.Create.Force.Default.Device
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            var service = new WorkerService();
            service.Start();
            System.Console.WriteLine("Press any key to exit.");
            System.Console.ReadLine();
            service.Stop();
#else
            System.ServiceProcess.ServiceBase.Run(new System.ServiceProcess.ServiceBase[]
            {
                new WorkerService()
            });
#endif
        }
    }
}

    */