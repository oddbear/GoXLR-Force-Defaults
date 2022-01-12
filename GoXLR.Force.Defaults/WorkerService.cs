using System.ServiceProcess;

namespace GoXLR.Force.Defaults
{
    public class WorkerService : ServiceBase
    {
        private NotificationClient _notificationClient;

        public WorkerService()
        {
            var name = "GoXLR Force Defaults";

            base.ServiceName = name;
            base.CanStop = true;
            base.CanPauseAndContinue = true;
            base.AutoLog = true;

            _notificationClient = new NotificationClient();
        }

        public void OnStart()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            _notificationClient.IsActive = true;
            _notificationClient.EnsureDefaultState();
        }

        protected override void OnContinue()
        {
            _notificationClient.IsActive = true;
            _notificationClient.EnsureDefaultState();
        }

        protected override void OnPause()
        {
            _notificationClient.IsActive = false;
        }

        protected override void OnStop()
        {
            _notificationClient.IsActive = false;
        }
    }
}
