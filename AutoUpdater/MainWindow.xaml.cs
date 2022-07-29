using System.Windows ;

namespace AutoUpdater
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    public MainWindow(App app)
    {
      InitializeComponent() ;
      Version.Text = app.RevitTool.version ;
      Description.Text = app.RevitTool.description ;
    }
    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var desktopWorkingArea = SystemParameters.WorkArea;
      Left = desktopWorkingArea.Right - Width;
      Top = desktopWorkingArea.Bottom - Height;
    }
  }
}