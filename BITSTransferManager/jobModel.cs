using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Aga.Controls.Tree;
using System.Collections;

//using SharpBits.Base;

// Set up the needed BITS namespaces
using BITS = BITSReference1_5;

namespace BITSTransferManager
{
    public class jobModel : ITreeModel
    {
        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {
                // Parent is base so return Jobs
                var app = (App)Application.Current;

                BITS.IEnumBackgroundCopyJobs jobsEnum = null;
                app.bitsManager.EnumJobs(app.jobEnumType, out jobsEnum);

                uint jobFetchedCount = 0;
                do
                {
                    BITS.IBackgroundCopyJob job = null;
                    jobsEnum.Next(1, out job, ref jobFetchedCount); // Can only pull a single job out at a time
                    if (jobFetchedCount > 0)
                    {
                        
                        yield return new jobWrapper(job);
                    }
                }
                while (jobFetchedCount > 0);
                
            }
            else if (parent is jobWrapper)
            {
                // Parent is Job so return files in job
                var jobw = parent as jobWrapper;

                BITS.IEnumBackgroundCopyFiles filesEnum;
                jobw.job.EnumFiles(out filesEnum);

                uint nfilesFetched = 0;
                BITS.IBackgroundCopyFile file = null;

                do
                {
                    filesEnum.Next(1, out file, ref nfilesFetched);
                    if (nfilesFetched > 0)
                    {
                        var curFilew = new fileWrapper(file);
                        jobw.files.Append(curFilew);
                        yield return curFilew;
                    }
                }
                while (nfilesFetched > 0);              
            }
        }

        public bool HasChildren(object parent)
        {
            return parent is jobWrapper;
        }
    }
}