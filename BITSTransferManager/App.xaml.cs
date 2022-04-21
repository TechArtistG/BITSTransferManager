using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
//using SharpBits.Base;



// Set up the needed BITS namespaces
using BITS = BITSReference1_5;


namespace BITSTransferManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex singletonMutex = null;

        private TaskbarIcon notifyIcon;
        
        public BITS.IBackgroundCopyManager bitsManager { get; private set; } = null;

        public const uint BG_JOB_ENUM_ALL_USERS = 1;
        public uint jobEnumType = 0; // is either 0 or BG_JOB_ENUM_ALL_USERS
        public jobManager manager;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Test if app already open
            bool singletonCreatedNew;
            singletonMutex = new Mutex(true, this.GetType().Namespace.ToString(), out singletonCreatedNew);
            
            if (!singletonCreatedNew)
            {
                //app is already running! Exiting the application
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
            
            this.manager = new jobManager();

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
