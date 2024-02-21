using System.ComponentModel;

namespace OnlineService.Services
{
    public class BackgroundWorkerService
    {
        private BackgroundWorker backgroundWorker;

        public event DoWorkEventHandler DoWorkChanged;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        public BackgroundWorkerService()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (sender, e) => DoWorkChanged?.Invoke(sender, e);
            backgroundWorker.RunWorkerCompleted += (sender, e) => RunWorkerCompleted?.Invoke(sender, e);
        }

        public void StartBackgroundWork()
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }
        public void StopBackgroundWork()
        {
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.CancelAsync();
        }
        public bool IsCancellationPending()
        {
            return backgroundWorker.CancellationPending;
        }
        public bool IsBusy()
        {
            return backgroundWorker.IsBusy;
        }
    }
}
