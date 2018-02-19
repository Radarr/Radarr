using System;
using NLog;
using NzbDrone.Core.Lifecycle;

namespace Radarr.Host
{
    public interface ICancelHandler
    {
        void Attach();
    }
    
    class CancelHandler : ICancelHandler
    {
        private object _syncRoot;
        private volatile bool _cancelInitiated;
        private readonly ILifecycleService _lifecycleService;
  
        public CancelHandler(ILifecycleService lifecycleService)
        {
            _lifecycleService = lifecycleService;
        }

        public void Attach()
        {
            Console.CancelKeyPress += HandlerCancelKeyPress;
            _syncRoot = new object();
        }
  
        private void HandlerCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Tell system to ignore the Ctrl+C and not terminate. We'll do that.
            e.Cancel = true;
	
            var shouldTerminate = false;
            lock (_syncRoot)
            {
                shouldTerminate = _cancelInitiated;
                _cancelInitiated = true;
            }
    
            // TODO: Probably should schedule these on the threadpool.
            if (shouldTerminate)
            {
                UngracefulShutdown();
            }
            else
            {
                GracefulShutdown();
            }	
        }
  
        private void GracefulShutdown()
        {
            Console.WriteLine("Shutdown requested, press Ctrl+C again to terminate directly.");
            // TODO: Sent ApplicationShutdownRequested event or something like it.
            _lifecycleService.Shutdown();
        }
  
        private void UngracefulShutdown()
        {
            Console.WriteLine("Termination requested.");
            // TODO: Kill it. Shutdown NLog and invoke Environment.Exit.
            LogManager.Configuration = null;
            Environment.Exit(0);
        }
    }
}
