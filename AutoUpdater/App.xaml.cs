using System ;
using System.Collections.Generic ;
using System.Diagnostics ;
using System.IO ;
using System.IO.Compression ;
using System.Net.Http;
using System.Windows ;
using Installer;
using System.Text.Json; 

namespace AutoUpdater
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    private readonly string _pathFolderDownloadTemp = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + @"\MF\" ;
    private readonly string _pathFolderExtract = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + @"\MF\Extract" ;
    private readonly string _pathFolderDestination = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) + @"\Autodesk\Revit\Addins\" ;
    private const string Url = "https://localhost:7206/current-version" ;
    public RevitTool RevitTool ;
    
    protected override async void OnStartup( StartupEventArgs e )
    {
      try {
        //check app revit running
        var pname = Process.GetProcessesByName( "revit" ) ;
        if ( pname.Length != 0 ) {
          return ;
        }

        var fileName = $@"{Guid.NewGuid()}.zip" ;
        var outPath = _pathFolderDownloadTemp + fileName ;
        if ( ! Directory.Exists( _pathFolderDownloadTemp ) ) {
          Directory.CreateDirectory( _pathFolderDownloadTemp ) ;
        }

        if ( Directory.Exists( _pathFolderExtract ) ) {
          Directory.Delete( _pathFolderExtract, true ) ;
        }

        if ( ! Directory.Exists( _pathFolderExtract ) ) {
          Directory.CreateDirectory( _pathFolderExtract ) ;
        }

        var httpClientHandler = new HttpClientHandler() ;
        httpClientHandler.ServerCertificateCustomValidationCallback = ( message, cert, chain, sslPolicyErrors ) => true ;

        var client = new HttpClient( httpClientHandler ) ;

        var httpResponse = await client.GetAsync( Url ) ;
        var json = await httpResponse.Content.ReadAsStringAsync() ;
        var revitTool = JsonSerializer.Deserialize<RevitTool>( json );
        RevitTool = revitTool ;
        if ( revitTool == null ) return ;
        var urlDownload = revitTool.url ;
        var version = revitTool.version ;
        
        var response = await client.GetAsync( urlDownload ) ;
        using ( var fs = new FileStream( outPath, FileMode.CreateNew ) ) {
          await response.Content.CopyToAsync( fs ) ;
        }

        LoggerUtil.WriteLog( "Download file|" + outPath ) ;

        ZipFile.ExtractToDirectory( outPath, _pathFolderExtract ) ;
        LoggerUtil.WriteLog( "ExtractToDirectory from|" + outPath + "|to|" + _pathFolderExtract ) ;

        CopyFolder( _pathFolderExtract, _pathFolderDestination ) ;
        LoggerUtil.WriteLog( "CopyFolder from|" + _pathFolderExtract + "|to|" + _pathFolderDestination ) ;


        WriteFile( version ) ;

        var mainWindow = new MainWindow(this) ;
        mainWindow.ShowDialog() ;
      }
      catch ( Exception ex ) {
        LoggerUtil.WriteLog( ex.ToString() ) ;
      }
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

    private void WriteFile(string version)
    {
      string[] lines = { "First line", "Second line", "Third line" };

      var docPath =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

      // Write the string array to a new file named "WriteLines.txt".
      using (var outputFile = new StreamWriter(Path.Combine(docPath, "VersionMF.txt")))
      {
        outputFile.WriteLine(version);
      }
    }
  }
}