using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
// Set up the needed BITS namespaces
using BITS = BITSReference1_5;
using System.Windows.Controls;

namespace BITSTransferManager
{
    
    
    
    
    
    public abstract class baseItem2 : INotifyPropertyChanged
    {
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            protected set
            {
                this._Name = value;
                this.NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class jobItem : baseItem2
    {
        class stateAttrib
        {
            public string name { get; set; }
            public Brush color { get; set; }
        }
        
        private Dictionary<BITS.BG_JOB_STATE, stateAttrib> stateAttribs = new Dictionary<BITS.BG_JOB_STATE, stateAttrib>()
        {
            {BITS.BG_JOB_STATE.BG_JOB_STATE_ACKNOWLEDGED,  new stateAttrib { name="[ACKNOWLEDGED]", color=Brushes.DarkBlue } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_CANCELLED,  new stateAttrib { name="[CANCELLED]", color=Brushes.DarkRed } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_CONNECTING, new stateAttrib { name="[CONNECTING]", color=Brushes.DarkGray } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_ERROR, new stateAttrib { name="[ERROR]", color=Brushes.Red } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_QUEUED, new stateAttrib { name="[QUEUED]", color=Brushes.Blue } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_SUSPENDED, new stateAttrib { name="[SUSPENDED]", color=Brushes.DarkOrange } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED, new stateAttrib { name="[TRANSFERRED]", color=Brushes.DarkGreen } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRING, new stateAttrib { name="[TRANSFERRING]", color=Brushes.Green } },
            {BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR, new stateAttrib { name="[TRANSIENT ERROR]", color=Brushes.Red } }
        };

        private Brush _StateColor;

        public Brush StateColor
        {
            get { return _StateColor; }
            protected set
            {
                if (this._StateColor != value)
                {
                    this._StateColor = value;
                    this.NotifyPropertyChanged();
                }     
            }
        }


        private BITS.BG_JOB_STATE _rawState;
        public BITS.BG_JOB_STATE rawState
        {
            get { return _rawState; }
            protected set
            {
                this._rawState = value;
                this.State = stateAttribs[this._rawState].name;
                this.StateColor = stateAttribs[this._rawState].color;                      
            }
        }


        private string _State;
        public string State
        {
            get { return _State; }
            protected set
            {
                if (this._State != value)
                {
                    this._State = value;
                    this.NotifyPropertyChanged();
                }                   
            }
        }


        public bool wasUpdated;

        public BITS.IBackgroundCopyJob jobRef { get; protected set; } = null;

        public BITS.GUID jobId { get; protected set; }
        
        public ObservableCollection<fileItem> FileItems { get; set; }

        public jobItem(BITS.IBackgroundCopyJob job)
        {
            this.jobRef = job;
            this.FileItems = new ObservableCollection<fileItem>();
            BITS.GUID IdRef;
            job.GetId(out IdRef);
            this.jobId = IdRef;
            
            this.updateData(job);
        }


        public fileItem getBITSFileItemFromId(string fileName)
        {
            fileItem retFile = null;

            foreach(fileItem f in this.FileItems)
            {
                if (f.Name == fileName) return (f);
            }

            return (retFile);
        }

        public void updateData(BITS.IBackgroundCopyJob job)
        {
            this.wasUpdated = true;
            
            // Name
            string displayName;
            job.GetDisplayName(out displayName);
            this.Name = displayName;

            BITS.BG_JOB_STATE js;
            job.GetState(out js);

            // Check if just completed
            if(js == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED && this.rawState != BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED && jobManager.autoCompeteAsyncJobs)
            {
                this.jobRef.Complete();
            }

            this.rawState = js;

            //foreach (fileItem fi in this.FileItems)
            //{
                // Refreshing files data
                BITS.IEnumBackgroundCopyFiles filesEnum;
                job.EnumFiles(out filesEnum);

                uint nfilesFetched = 0;
                BITS.IBackgroundCopyFile file = null;

                do
                {
                    filesEnum.Next(1, out file, ref nfilesFetched);
                    if (nfilesFetched > 0)
                    {
                        string fileName;
                        file.GetLocalName(out fileName);
                        fileItem curFile = getBITSFileItemFromId(fileName);

                        if(curFile != null)
                        {
                            // File already exists in data stuctures
                            if(this.rawState == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRING)
                            {
                                curFile.updateData(file);
                            }                            
                        }
                        else
                        {
                            // Need to add file
                            fileItem newFile = new fileItem(file);
                            this.FileItems.Add(newFile);
                        }                       
                    }
                }
                while (nfilesFetched > 0);
            //}
        }
    }

    public class fileItem : baseItem2
    {
        private float _Transfered;
        public float Transfered
        {
            get { return this._Transfered; }
            protected set
            {
                this._Transfered = value;
                this.NotifyPropertyChanged();
            }
        }

        private float _Percent;
        public float Percent
        {
            get { return this._Percent; }
            protected set
            {
                this._Percent = value;
                this.NotifyPropertyChanged();
            }
        }



        public int fileHash;
        public fileItem(BITS.IBackgroundCopyFile file)
        {
            // Name
            string fileName;
            file.GetLocalName(out fileName);
            this.Name = fileName;

            this.fileHash = file.GetHashCode();
            this.updateData(file);
        }

        public void updateData(BITS.IBackgroundCopyFile file)
        { 
            // Transfered
            BITS._BG_FILE_PROGRESS progress;
            file.GetProgress(out progress);
            this.Transfered = progress.BytesTransferred / 1000000.0f;
            if(progress.BytesTransferred > 0)
            {
                //<TextBlock Text="{Binding Transfered}" Foreground="Blue" />

                this.Percent = ((float)progress.BytesTransferred / (float)progress.BytesTotal) * 100;
            }            
        }
    }

}
