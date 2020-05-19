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

namespace BattleCityAlpha_Updater
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Updater _updater;
        
        public MainWindow(Updater updater)
        {
            InitializeComponent();
            this.MouseDown += Window_MouseDown;
            this.Loaded += MainWindow_Loaded;

            this.progressBar_update.Value = 0;
            this.tb_updateleft.Visibility = Visibility.Hidden;
            this.tb_slash.Visibility = Visibility.Hidden;
            this.tb_updateright.Visibility = Visibility.Hidden;

            _updater = updater;

            _updater.InitUpdating += _updater_InitUpdating;
            _updater.ChangeMainText += _updater_ChangeMainText;
            _updater.StartUpdate += _updater_StartUpdate;
            _updater.SetProgess += _updater_SetProgess;
            _updater.EndUpdating += _updater_EndUpdating;
        }

        private void _updater_EndUpdating()
        {
            this.Close();
        }

        private void _updater_SetProgess(int progress)
        {
            this.progressBar_update.Value = progress;
        }

        private void _updater_StartUpdate(int value)
        {
            this.tb_updateleft.Visibility = Visibility.Visible;
            this.tb_slash.Visibility = Visibility.Visible;
            this.tb_updateright.Visibility = Visibility.Visible;

            this.tb_updateleft.Text = value.ToString();
        }

        private void _updater_ChangeMainText(string txt)
        {
            this.tb_maj.Text = txt;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _updater.Start();
        }

        private void _updater_InitUpdating(string lastversion, int updatescount)
        {
            tb_lastversion.Text = lastversion;
            tb_updateright.Text = updatescount.ToString();
        }

        private void closeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void maximizeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.bg_border.CornerRadius = new CornerRadius(0, 0, 100, 100);
            }
            else if (WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                this.bg_border.CornerRadius = new CornerRadius(0);
            }
        }
        private void minimizeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch { }
        }
    }
}
