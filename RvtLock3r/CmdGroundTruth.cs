#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
#endregion

namespace RvtLock3r
{
  [Transaction(TransactionMode.ReadOnly)]
  public class CmdGroundTruth : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      string rvtpath = doc.PathName;
      string txtpath = rvtpath.Replace(".rte", ".lock3r");

      // Retrieve elements from database

      FilteredElementCollector wallTypes
        = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));

      string allString = string.Empty;

      foreach (Element e in wallTypes)
      {
        string s = Util.GroundTruthData(e);
        allString += s;
      }

      if (File.Exists(txtpath))
      {
        File.Delete(txtpath);
      }
      File.WriteAllText(txtpath, allString);

      return Result.Succeeded;
    }
  }
}
