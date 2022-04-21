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
using System.Windows.Media.Media3D;

namespace BITSTransferManager
{
    /// <summary>
    /// Interaction logic for jobsTreeview.xaml
    /// </summary>
    public partial class jobsTreeview : UserControl
    {
        public baseItem2 selectedItem = null;

        public jobsTreeview()
        {
            InitializeComponent();
            var app = (App)Application.Current;
            
            // Linking to object on app since job list can be accessed from multiple sources
            trvJobs.ItemsSource = app.manager.JobItems;
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }

        void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            DependencyObject returnVal = source;

            while (returnVal != null && !(returnVal is T))
            {
                DependencyObject tempReturnVal = null;
                if (returnVal is Visual || returnVal is Visual3D)
                {
                    tempReturnVal = VisualTreeHelper.GetParent(returnVal);
                }
                if (tempReturnVal == null)
                {
                    returnVal = LogicalTreeHelper.GetParent(returnVal);
                }
                else returnVal = tempReturnVal;
            }

            return returnVal as T;
        }



        private void jobContextMenu_Click(object sender, RoutedEventArgs e)
        {            
            if (sender is MenuItem menuItem && this.selectedItem is jobItem)
            {
                MenuItem mSender = (MenuItem)sender;
                jobItem curJobItem = (jobItem)this.selectedItem;

                switch (mSender.Header)
                {
                    case "Pause":                        
                        curJobItem.jobRef.Suspend();
                        break;
                    case "Resume":
                        curJobItem.jobRef.Resume();
                        break;
                    case "Delete":
                        curJobItem.jobRef.Cancel();
                        break;
                }
            }
        }

        private void trvJobs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.selectedItem = (baseItem2)e.NewValue;
        }
    }
}
