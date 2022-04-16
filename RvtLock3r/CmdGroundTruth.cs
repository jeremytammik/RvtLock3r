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
    /// <summary>
    /// Create ground truth for given document.
    /// </summary>
    public static void CreateGroundTruthFor(Document doc)
    {
      string rvtpath = doc.PathName;
      string txtpath = rvtpath.Replace(".rte", ".lock3r");

      FilteredElementCollector wallTypes
        = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));

      string s = string.Empty;

      foreach (Element e in wallTypes)
      {
        s += Util.GroundTruthData(e);
      }

      if (File.Exists(txtpath))
      {
        File.Delete(txtpath);
      }
      File.WriteAllText(txtpath, s);
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      CreateGroundTruthFor( doc);

      string rvtpath = doc.PathName;

      return Result.Succeeded;
    }
  }
}
