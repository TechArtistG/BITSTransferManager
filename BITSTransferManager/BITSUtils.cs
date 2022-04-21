using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Diagnostics;

// Set up the needed BITS namespaces
using BITS = BITSReference1_5;
//using System.Management.Automation;


using System.Windows.Controls;

namespace BITSTransferManager
{
    public class jobManager
    {
        public static bool autoCompeteAsyncJobs = true;

        public ObservableCollection<jobItem> JobItems { get; set; }
        //private App _app;
        public BITS.IBackgroundCopyManager bitsManager { get; private set; } = null;
        public const uint BG_JOB_ENUM_ALL_USERS = 1;
        public uint jobEnumType = 0; // is either 0 or BG_JOB_ENUM_ALL_USERS

        private DispatcherTimer _timer;

        private string getExeBasePath()
        {
            string execPath = Assembly.GetEntryAssembly().Location;
            return (Path.GetDirectoryName(execPath));
        }

        public jobManager()
        {
            // _app = appRef;
            this.JobItems = new ObservableCollection<jobItem>();

            try
            {
                // Setup BITS manager
                this.bitsManager = new BITS.BackgroundCopyManager1_5();
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                MessageBox.Show("Error loading BITS interface");
                return;
            }


            this._timer = new DispatcherTimer();
            this._timer.Tick += this.Timer_Tick;
            this._timer.Interval = new TimeSpan(0, 0, 1);
            this._timer.Start();

            this.updateData();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.updateData();
        }

        public bool BITSGUIDCompare(BITS.GUID a, BITS.GUID b)
        {
            return (a.Data1 == b.Data1 && a.Data2 == b.Data2 && a.Data3 == b.Data3 && a.Data4 == b.Data4);
        }


        public jobItem getBITSJobItemFromId(BITS.GUID id)
        {
            jobItem retItem = null;

            foreach (jobItem i in this.JobItems)
            {
                if (i.jobId.GuidEquals(id)) return (i);
            }

            return (retItem);
        }


        public void updateData()
        {
            // Since all BITS calls are not persistant need to recall all data when refreshing
            BITS.IEnumBackgroundCopyJobs jobsEnum = null;
            this.bitsManager.EnumJobs(this.jobEnumType, out jobsEnum);

            foreach (jobItem i in this.JobItems)
            {
                i.wasUpdated = false;
            }

            uint jobFetchedCount = 0;
            do
            {
                BITS.IBackgroundCopyJob job = null;
                jobsEnum.Next(1, out job, ref jobFetchedCount); // Can only pull a single job out at a time
                if (jobFetchedCount > 0)
                {
                    BITS.GUID searchFor;
                    job.GetId(out searchFor);
                    jobItem curJob = getBITSJobItemFromId(searchFor);

                    if (curJob != null)
                    {
                        // Job already exists in data stuctures
                        curJob.updateData(job);
                    }
                    else
                    {
                        // Need to add job
                        jobItem newJob = new jobItem(job);
                        this.JobItems.Add(newJob);
                    }
                }
            }
            while (jobFetchedCount > 0);

            // Find items that weren't updated and therefore are orphaned
            var jobItemsList = this.JobItems.Cast<jobItem>().ToList();

            // In ForEach Loop remove Items from ObservableCollection
            foreach (jobItem job in jobItemsList)
            {
                // Remove from ObservableCollection
                if (!job.wasUpdated)
                    this.JobItems.Remove(job);
            }
        }

        // Takes a list of files paths, copies them to the clipboard then calls PS script
        public void CreateBackgroundCopyJob(List<string> src, string dst)
        {
            // Copy files to clipbaord so they can be used by ps script
            StringCollection paths = new StringCollection();
            paths.AddRange(src.ToArray());
            Clipboard.SetFileDropList(paths);

            // Call ps script
            string basePath = getExeBasePath();
            string scriptPath = basePath + "\\BITSPasteFiles.ps1";            
            string args = String.Format("-NoProfile -ExecutionPolicy Bypass -File \"{0}\" \"{1}\"", scriptPath, dst);
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "Powershell",
                    WorkingDirectory = basePath,
                    Arguments = args
                }
            };
            p.Start();
        }
    }
}
