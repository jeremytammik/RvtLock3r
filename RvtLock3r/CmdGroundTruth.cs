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
  [Transaction(TransactionMode.Manual)]
  public class CmdGroundTruth : IExternalCommand
  {
    ///// <summary>
    ///// Create ground truth for given document.
    ///// </summary>
    //public static void CreateGroundTruthFor(Document doc)
    //{
    //        var data = Util.GroundTruthListData(doc);


    // }

    //    /// <summary>
    //    /// This method is purely for my testing, attempting to read the data that I have written to the e-store, will be deleting it soon
    //    /// </summary>
    //    /// <param name="doc"></param>
    //    public static void ReadGroundTruthFor(Document doc)
    //    {


    //        FilteredElementCollector wallTypes
    //          = new FilteredElementCollector(doc)
    //            .OfClass(typeof(WallType));



    //        WallType wallType = null;


    //        foreach (Element e in wallTypes)
    //        {
    //            wallType = e as WallType;

    //            Util.ExtractDataFromExternalStorage(e);

    //        }

    //    }

    public Result Execute(
  ExternalCommandData commandData,
  ref string message,
  ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      Result rslt = Result.Failed;

      //CreateGroundTruthFor(doc);

      string description = "Ground Truth Data set successfully";

      //Guid named_guid;
      //IDictionary < ElementId, Dictionary<Guid, string> > tr;

      bool rc = NamedGroundTruthStorage.Get(doc, out string gtStringdata, false);

      if (rc)
      {
        Util.InfoMsg(string.Format(
          "This document already has a project "
          + "identifier: {0} = {1}",
          gtStringdata));
        Util.InfoMsg(string.Format(
              "Created a new project identifier "
              + "for this document: {0} = {1}",
               description));

        rslt = Result.Succeeded;
      }
      else
      {
        rc = NamedGroundTruthStorage.Get(doc,
          out gtStringdata, true);

        if (rc)
        {
          Util.InfoMsg(string.Format(
            "Created a new project identifier "
            + "for this document: {0} = {1}",
             description));
          Util.InfoMsg(string.Format(
            "Created a new project identifier "
            + "for this document: {0} = {1}",
             gtStringdata));


          rslt = Result.Succeeded;
        }
        else
        {
          Util.ErrorMsg("Something went wrong");
        }
      }
      return rslt;
      //Transaction trans = new Transaction(doc);
      //trans.Start("GroundTruth");

      //CreateGroundTruthFor( doc);
      //trans.Commit();

      //ReadGroundTruthFor(doc);

      //string rvtpath = doc.PathName;

      //return Result.Succeeded;
    }
  }
}
