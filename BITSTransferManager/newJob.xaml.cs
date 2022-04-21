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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace BITSTransferManager
{
    /// <summary>
    /// Interaction logic for newJob.xaml
    /// </summary>
    public partial class newJob : Window
    {
        public string srcPathRet;
        public string dstPathRet;        
        
        public newJob()
        {
            InitializeComponent();
        }        

        private void dstBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Copy Destination location";
            var result = fd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.dstPath.Text = fd.SelectedPath;
            }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            //this.srcPathRet = this.srcPath.Text;
            
            if (Directory.Exists(this.dstPath.Text))
            {
                this.dstPathRet = this.dstPath.Text;
                this.DialogResult = true;
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                System.Windows.Forms.MessageBox.Show("The destination path must be a valid folder", "Alert", buttons, MessageBoxIcon.Warning);
            }

        }

       
    }
}
