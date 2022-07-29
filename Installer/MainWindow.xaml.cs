using System ;
using System.Diagnostics ;
using System.IO ;
using System.IO.Compression ;
using System.Net ;
using System.Net.Http;
using System.Runtime.InteropServices ;
using System.Windows ;
using System.Windows.Input ;
using System.Windows.Interop ;
using System.Windows.Navigation ;

namespace Installer
{
    public partial class MainWindow
    {
        private readonly string _appExecuteAutoUpdate = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Autodesk\Revit\Addins\MF\AutoUpdater.exe";
        private readonly string _pathFolderDownloadTemp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MF\";
        private readonly string _pathFolderExtract = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MF\Extract";
        private readonly string _pathFolderDestination = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\";
        private const string Url = "https://localhost:7206/StaticFiles/version_20220728.zip";

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIdEnableItem, uint uEnable);

        private const uint MfGrayed = 0x00000001;
        private const uint MfEnabled = 0x00000000;
        private const uint ScClose = 0xF060;


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var hMenu = GetSystemMenu(hwnd, false);
            EnableMenuItem(hMenu, ScClose, MfGrayed);

            Button.Visibility = Visibility.Hidden;
            Cursor = Cursors.Wait;
            try
            {
                LoggerUtil.WriteLog("Start 1111111");
                //check app revit running
                var pname = Process.GetProcessesByName("revit");
                if (pname.Length != 0)
                {
                    LoggerUtil.WriteLog("Revit is running");
                    return;
                }

                var fileName = string.Format(@"{0}.zip", Guid.NewGuid());
                var outPath = _pathFolderDownloadTemp + fileName;
                if (!Directory.Exists(_pathFolderDownloadTemp))
                {
                    Directory.CreateDirectory(_pathFolderDownloadTemp);
                }

                if (Directory.Exists(_pathFolderExtract))
                {
                    Directory.Delete(_pathFolderExtract, true);
                }
                if (!Directory.Exists(_pathFolderExtract))
                {
                    Directory.CreateDirectory(_pathFolderExtract);
                }

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true ;

                var client = new HttpClient(httpClientHandler);
                var response = await client.GetAsync(Url);
                using (var fs = new FileStream(outPath,
                    FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }

                LoggerUtil.WriteLog("Download file|" + outPath);

                ZipFile.ExtractToDirectory(outPath, _pathFolderExtract);
                LoggerUtil.WriteLog("ExtractToDirectory from|" + outPath + "|to|" + _pathFolderExtract);

                CopyFolder(_pathFolderExtract, _pathFolderDestination);
                LoggerUtil.WriteLog("CopyFolder from|" + _pathFolderExtract + "|to|" + _pathFolderDestination);

                StartUp();

            }
            catch (Exception ex)
            {
                LoggerUtil.WriteLog(ex.ToString());
                EnableMenuItem(hMenu, ScClose, MfEnabled);
                return;
            }

            Cursor = Cursors.None;
            EnableMenuItem(hMenu, ScClose, MfEnabled);
            LoggerUtil.WriteLog("Completed");
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void StartUp()
        {
            const string hkcu = "HKEY_CURRENT_USER";
            const string runKey = @"SOFTWARE\\Microsoft\Windows\CurrentVersion\Run";
            Microsoft.Win32.Registry.SetValue(hkcu + "\\" + runKey, "AppMF", _appExecuteAutoUpdate);
        }


        private void CopyFolder(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
