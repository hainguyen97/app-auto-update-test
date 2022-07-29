using System ;

namespace AutoUpdater
{
  public class RevitTool
  {
    public int id {get; set;}
    public string version { get ; set ; } = string.Empty;
    public string description { get ; set ; } = string.Empty ;
    public string url { get ; set ; } = string.Empty ;
  }
}