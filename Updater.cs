using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BattleCityAlpha_Updater
{
    public class Updater
    {
        public static string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private UpdatesInfos _infos;

        private WebClient _client; 
        private bool _downloaded;
        private bool _error;


        public event Action<string, int> InitUpdating;
        public event Action<int> StartUpdate;
        public event Action<string> ChangeMainText;
        public event Action<int> SetProgess;
        public event Action EndUpdating;

        public Updater(UpdatesInfos infos)
        {
            _infos = infos;
            _infos.ProcessName = Path.GetFileNameWithoutExtension(_infos.ProcessName);
        }

        public void Start()
        {
            Application.Current.Dispatcher.Invoke(() => InitUpdating?.Invoke(_infos.LastVersion, _infos.Updates.Length));
            Thread updateThread = new Thread(Update) { IsBackground = true };
            updateThread.Start();
        }
        private void Update()
        {
            try
            {
                CloseBCA();
                DownloadUpdates();
                StartBCA();
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() => ChangeMainText("Erreur lors de la mise à jour"));
                Thread.Sleep(2000);
            }

            Application.Current.Dispatcher.Invoke(() => EndUpdating());
        }
        private void CloseBCA()
        {
            Process[] processes;

            string dots = "";
            for (int i = 1; i <= 3; i++)
            {
                processes = Process.GetProcessesByName(_infos.ProcessName);
                if (processes.Length == 0)
                    return;

                dots += ".";
                Application.Current.Dispatcher.Invoke(() => ChangeMainText("En attente de BCA..."));
                Thread.Sleep(1000);
            }

            processes = Process.GetProcessesByName(_infos.ProcessName);
            if (processes.Length == 0)
                return;

            foreach (Process process in processes)
                process.Kill();
        }
        private void DownloadUpdates()
        {
            _client = new WebClient { Proxy = null };
            _client.DownloadProgressChanged += _client_DownloadProgressChanged;
            _client.DownloadFileCompleted += _client_DownloadFileCompleted;
            for (int i = 0; i < _infos.Updates.Length; i++)
            {
                Application.Current.Dispatcher.Invoke(() => StartUpdate(i + 1));
                DownloadUpdate(_infos.Updates[i]);
                InstallUpdate(0);
            }
        }

        private void DownloadUpdate(string download)
        {
            _downloaded = false;
            _client.DownloadFileAsync(new Uri(download), Path.Combine(path, "update.zip"));
            while (!_downloaded)
                Thread.Sleep(10);
            if (_error)
                throw new Exception("Error when downloading.");
        }

        private void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => SetProgess(e.ProgressPercentage));
        }

        private void _client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _downloaded = true;
            if (e.Cancelled)
                _error = true;
            Application.Current.Dispatcher.Invoke(() => SetProgess(100));
        }
        private void InstallUpdate(int attempt)
        {
            ZipFile zipfile = null;
            try
            {
                zipfile = new ZipFile(File.OpenRead(Path.Combine(path, "update.zip")));
            }
            catch
            {
                if (attempt == 3)
                {
                    if (MessageBox.Show("Erreur lors de l'extractiond de la mise à jour. Voulez-vous réessayer ?", "Erreur d'installation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        InstallUpdate(0);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }

                Thread.Sleep(500);
                InstallUpdate(attempt + 1);
                return;
            }
            Application.Current.Dispatcher.Invoke(() => ChangeMainText("Installation  : "));
            for (int i = 0; i < zipfile.Count; i++)
            {
                ZipEntry entry = zipfile[i];
                if (!entry.IsFile) continue;

                double percent = (double)i / zipfile.Count;
                int percentInt = (int)(percent * 100);
                if (percentInt > 100) percentInt = 100;
                if (percentInt < 0) percentInt = 0;

                Application.Current.Dispatcher.Invoke(() => SetProgess(percentInt));

                string filename = Path.Combine(path, entry.Name == "Battle City Alpha.exe" ? _infos.ProcessName + ".exe" : entry.Name);
                string directory = Path.GetDirectoryName(filename);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                byte[] buffer = new byte[4096];
                Stream zipStream = zipfile.GetInputStream(entry);
                using (FileStream streamWriter = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
            }
            zipfile.Close();
            Application.Current.Dispatcher.Invoke(() => SetProgess(100));

            File.Delete(Path.Combine(path, "update.zip"));
        }

        private void StartBCA()
        {
            string location = Path.Combine(path, _infos.ProcessName + ".exe");
            Process.Start(location);
        }
    }
}
