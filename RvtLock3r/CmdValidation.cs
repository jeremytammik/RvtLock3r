//
// (C) Copyright 2003-2023 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace RvtLock3r
{
  [Transaction(TransactionMode.ReadOnly)]
  public class CmdValidation : IExternalCommand
  {
    /// <summary>
    /// Second version using GroundTruth class
    /// </summary>
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
      Document doc = uidoc.Document;
      GroundTruth truth = new GroundTruth(doc);

      // In case of validation error, store element and parameter ids

      Dictionary<int, List<Guid>> errorLog
        = new Dictionary<int, List<Guid>>();

      bool rc = truth.Validate(doc, errorLog);

      int n = errorLog.Count;

      if (0 < n)
      {
        // Report errors to user
        // Set reference return values ElementSet elements and message
        message = "Protected model paramaters have been altered!";

        Util.GetAlteredElements(doc, errorLog, elements);

        return Result.Failed;
      }
      return Result.Succeeded;
    }

    /// <summary>
    /// First naive implementation with no GroundTruth class
    /// </summary>
    public Result ExecuteFirstAttempt(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
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
        message = "Model paramaters have been altered!";

        Util.GetAlteredElements(doc, errorLog, elements);

        return Result.Failed;
      }
      return Result.Succeeded;
    }
  }

}

