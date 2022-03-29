#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace RvtLock3r
{
  /// <summary>
  /// External command to validate ground truth data
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class Command : IExternalCommand
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

      string[] lines = File.ReadAllLines(txtpath);

      // Store element and parameter ids causing validation error

      Dictionary<int, List<Guid>> errorLog
        = new Dictionary<int, List<Guid>>();

      for (int j = 0; j < lines.Length - 1; j++)
      {
        string[] triple = lines[j].Split(null);
        string id = triple[0];
        int i = int.Parse(triple[0]);
        ElementId eid = new ElementId(i);
        Guid pid = new Guid(triple[1]);
        string checksum = triple[2];

        Element e = doc.GetElement(eid);

        Parameter p = e.get_Parameter(pid);

        string pval = Util.ParameterToString(p);

        string pchecksum = Util.ComputeChecksum(pval);

        if (!checksum.Equals(pchecksum))
        {
          //log.Add(string.Format(
          //  "Validation error on element/parameter '{0}' -- '{1}'",
          //  ElementDescription(e), p.Definition.Name));

          if (!errorLog.ContainsKey(i))
          {
            errorLog.Add(i, new List<Guid>());
          }
          if (!errorLog[i].Contains(pid))
          {
            errorLog[i].Add(pid);
          }
        }
      }

      int n = errorLog.Count;

      if (0 < n)
      {
        // Report errors to user by setting the
        // ElementSet `elements` and `message` return values

        message = "Model Paramaters have been Altered!";

        Util.GetAlteredElements(doc, errorLog, elements);

        return Result.Failed;
      }
      return Result.Succeeded;
    }
  }
}
