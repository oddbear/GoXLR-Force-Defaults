using System.ServiceProcess;
using System.Threading;

namespace GoXLR.Force.Defaults
{
    public class WorkerService : ServiceBase
    {
        private readonly NotificationClient _notificationClient;

        public WorkerService()
        {
            var name = "GoXLR Force Defaults";

            base.ServiceName = name;
            base.CanStop = true;
            base.CanPauseAndContinue = true;
            base.AutoLog = true;

            _notificationClient = new NotificationClient();
        }
        
#if DEBUG
        public void Start()
        {
            OnStart(null);
        }
#endif

        protected override void OnStart(string[] args)
        {
            //TODO: Re-implement
        }

        protected override void OnContinue()
        {
            //TODO: Re-implement
        }

        protected override void OnPause()
        {
            //TODO: Re-implement
        }

        protected override void OnStop()
        {
            //TODO: Re-implement
        }
    }
}
