using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SharpBits.Base;
using System.IO;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
// Set up the needed BITS namespaces
using BITS = BITSReference1_5;
using System.Windows.Media.Animation;


namespace BITSTransferManager
{
    public abstract class baseItem : INotifyPropertyChanged
    {
        // Bindings
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

        private string _Stage;        
        public string State
        {
            get { return this._Stage; }
            protected set
            {
                this._Stage = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _Priority;
        public string Priority
        {
            get { return this._Priority; }
            protected set
            {
                this._Priority = value;
                this.NotifyPropertyChanged();
            }
        }

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

        public abstract void updateBindings();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class jobWrapper : baseItem
    {
        public BITS.IBackgroundCopyJob job { get; protected set; } = null;
        public List<fileWrapper> files;

        private DispatcherTimer _timer;

        public jobWrapper(BITS.IBackgroundCopyJob bj)
        {
            this.files = new List<fileWrapper>();
            this.job = bj;

            _timer = new DispatcherTimer();
            _timer.Tick += this.Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();

            this.updateBindings();
        }

        public jobWrapper()
        {
            this.Name = "test Item";
            this.State = "test";
            this.Priority = "High";
            this.Transfered = 11111;
            this.Percent = 222222;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.updateBindings();

            


            foreach(fileWrapper f in this.files)
            {
                f.updateBindings();
            }
        }


        public override void updateBindings()
        {
            string displayName;
            this.job.GetDisplayName(out displayName);

            this.Name = displayName;
            this.Transfered += 1;


            //this.State = this.job.State.ToString();
            //this.Priority = this.job.Priority.ToString();
        }
    }

    public class fileWrapper : baseItem
    {
        public BITS.IBackgroundCopyFile file { get; protected set; } = null;

        public fileWrapper(BITS.IBackgroundCopyFile f)
        {
            this.file = f;
            this.updateBindings();
        }

        public override void updateBindings()
        {
            string localName;
            file.GetLocalName(out localName);
            this.Name = Path.GetFileName(localName);

            BITS._BG_FILE_PROGRESS progress;
            file.GetProgress(out progress);

            this.Transfered = progress.BytesTransferred / 1000000.0f;

            if(progress.BytesTotal != ulong.MaxValue)
            {
                this.Percent = progress.BytesTotal / progress.BytesTransferred;
            }
            


            //this.Transfered = file.Progress.BytesTransferred / 1000000.0f;
            //this.Percent = file.Progress.BytesTotal / file.Progress.BytesTransferred;
        }
    }
}
