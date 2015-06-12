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

namespace ImageEvolution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Variables

        private Tracker tracker;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// intitialize objects and camera.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        /// <summary>
        /// Button action is clicked.
        /// </summary>
        private void ButtonAction_Click(object sender, RoutedEventArgs e)
        {
            if (tracker.IsProcessing())
            {
                buttonAction.Content = "Start";
                tracker.StopProcessing();
            }
            else
            {
                buttonAction.Content = "Stop";
                tracker.StartProcessing();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// intitialize objects and camera.
        /// </summary>
        private void Initialize()
        {
            tracker = new Tracker(labelFrameCounter);
        }

        #endregion
    }
}
