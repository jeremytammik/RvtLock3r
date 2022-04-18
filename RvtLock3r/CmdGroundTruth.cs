#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;
#endregion

namespace RvtLock3r
{
  [Transaction(TransactionMode.Manual)]
  public class CmdGroundTruth : IExternalCommand
  {
    /// <summary>
    /// Create ground truth for given document.
    /// </summary>
    public static void CreateGroundTruthFor(Document doc)
    {
      //string rvtpath = doc.PathName;
      //string txtpath = rvtpath.Replace(".rvt", ".lock3r");

      FilteredElementCollector wallTypes
        = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));


            //string s = string.Empty;
           
            foreach (Element e in wallTypes)
            {
                //s += Util.GroundTruthData(e);
                Util.GroundTruthSchemaEntity(e);
               
            }


            //if (File.Exists(txtpath))
            //{
            //    File.Delete(txtpath);
            //}
            //File.WriteAllText(txtpath, s);
        }

        /// <summary>
        /// This method is purely for my testing, attempting to read the data that I have written to the e-store, will be deleting it soon
        /// </summary>
        /// <param name="doc"></param>
        public static void ReadGroundTruthFor(Document doc)
        {
            

            FilteredElementCollector wallTypes
              = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType));

       

            WallType wallType = null;
         

            foreach (Element e in wallTypes)
            {
                wallType = e as WallType;

                Util.ExtractDataFromExternalStorage(e);

            }

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
            Transaction trans = new Transaction(doc);
            trans.Start("GroundTruth");

      CreateGroundTruthFor( doc);
            trans.Commit();

            ReadGroundTruthFor(doc);

            //string rvtpath = doc.PathName;

            return Result.Succeeded;
    }
  }
}
