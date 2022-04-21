#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace RvtLock3r
{
  [Transaction(TransactionMode.Manual)]
  public class CmdGroundTruth : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;
      string data;

      bool rc = NamedGroundTruthStorage.Get(doc, out data, false);

      if (rc)
      {
        Util.InfoMsg2(
          "Ground truth data already stored in model.",
          "Why are you calling this command twice?");
      }
      else
      {
        rc = NamedGroundTruthStorage.Get(doc, out data, true);

        if (rc)
        {
          Util.InfoMsg2("Success: Ground truth data stored in model.");
        }
        else
        {
          message = "Failure attempting to store ground truth data in model.";
        }
      }
      return rc ? Result.Succeeded : Result.Failed;
    }
  }
}
