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
            //this will be the storage of the ground truth file, which u will read in the validation command.
            string txtpath = rvtpath.Replace(".rte", ".lock3r");

            // Retrieve elements from database

            FilteredElementCollector wallTypes
              = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType));


            string allString = string.Empty;

            foreach (Element e in wallTypes)
            {
                //Gets a list of the ElementType Parameters
                string s = Util.GroundTruthData(e, "WallType Parameters: ");
                allString += s;
            }

            //generates a .lock3r file with  ground truth triples, with checksum which will be compared with
            //I am passing the rvt file location where the ground truth file will be stored

            WriteGroundTruthFile(txtpath, allString);

            return Result.Succeeded;
        }


        public static void WriteGroundTruthFile(string path, string s)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, s);


        }


    }
}
