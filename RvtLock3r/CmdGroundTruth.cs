#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace RvtLock3r
{
  /// <summary>
  /// External command to create and store ground truth data
  /// </summary>
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
      //this will be storage of the ground truth file, which u will read in the validation command.
      string txtpath = rvtpath.Replace(".rte", ".lock3r");

      // Retrieve elements from database

      FilteredElementCollector wallTypes
        = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));

      FilteredElementCollector wallTypes2
        = new FilteredElementCollector(doc)
          .OfCategory(BuiltInCategory.OST_Walls)
          .WhereElementIsElementType();

      string allString = string.Empty;

      foreach (Element e in wallTypes)
      {
        //Gets a list of the ElementType Parameters
        string s = Util.ShowParameters(e, "WallType Parameters: ");
        allString += s;
      }

      WriteGroundTruthFile(txtpath, allString).GetAwaiter();

      return Result.Succeeded;
    }

    public static async Task WriteGroundTruthFile(string path, string s)
    {
      if (File.Exists(path))
      {
        File.Delete(path);
      }

      using (StreamWriter outputFile = File.CreateText(path))
      {
        await outputFile.WriteAsync(s);
      }
    }
  }
}
