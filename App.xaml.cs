using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattleCityAlpha_Updater
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                MessageBox.Show("Ce logiciel ne peut-être démarré manuellement.", "Battle City Alpha", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
            }
            try
            {
                var jsonStr = Encoding.UTF8.GetString(Convert.FromBase64String(e.Args[0]));
                Updater up = new Updater(JsonConvert.DeserializeObject<UpdatesInfos>(jsonStr));
                MainWindow window = new MainWindow(up);
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Shutdown();
            }
        }
    }
}
