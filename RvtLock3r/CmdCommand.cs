#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace RvtLock3r
{
    [Transaction(TransactionMode.ReadOnly)]
    public class CmdCommand : IExternalCommand
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

            // In case of validation error, store element and parameter ids

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
                // Report errors to user
                // Set reference return values ElementSet elements and message
                message = "Model Paramaters have been Altered!";

                 Util.GetAlteredElements(doc, errorLog, elements);

                return Result.Failed;
            }
            return Result.Succeeded;
        }

      
    }
}