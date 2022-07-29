using System;
using System.Globalization ;
using System.IO;

namespace Installer
{
  public static class LoggerUtil
  {
    public static void WriteLog(string msg)
    {
      var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
      sw.WriteLine(DateTime.Now.ToString( CultureInfo.InvariantCulture ) + ": " + msg);
      sw.Flush();
      sw.Close();
    }
  }
}