using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BITSTransferManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {           
        public MainWindow()
        {
            InitializeComponent();
        }


        private void populateListview()
        {


        }




        private void but_newJob_Click(object sender, RoutedEventArgs e)
        {
            //this.jobsListView.Items.Add(new MyItem { Id = 1, Name = "David" });
            newJob inputDialog = new newJob();
            inputDialog.Owner = this;
            inputDialog.ShowDialog();
        }

       

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            jobManager.autoCompeteAsyncJobs = (bool)((CheckBox)e.Source).IsChecked;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> files = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));

            if (files != null && files.Count > 0)
            {
                newJob inputDialog = new newJob();
                inputDialog.Owner = this;
                if(inputDialog.ShowDialog() == true)
                {
                    var app = (App)Application.Current;

                    app.manager.CreateBackgroundCopyJob(files, inputDialog.dstPathRet);
                }
            }
        }

        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }
    }
}
