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
            //this will be storage of the ground truth file, which u will read in the validation command.
            string txtpath = rvtpath.Replace(".rte", ".lock3r");

            // Retrieve elements from database

            FilteredElementCollector col = new FilteredElementCollector(doc);

            col.OfCategory(BuiltInCategory.OST_Walls)
                //col.OfClass(typeof(WallType))
                .WhereElementIsNotElementType().ToElements();
                
            //Hi Jeremy, I tried several ways to fetch walltypes from here and i couldnt understand
            //very well the logic; when I get the wall types and lets say we have several wall elements having the same
            //wallTypes, its a bit difficult to get all wallType parameters. Maybe you cud advise me more here

            string allString = string.Empty;

            foreach (Element e in col)
            {
                Wall wall = (Wall)doc.GetElement(e.Id);
                WallType wallType = wall.WallType;

                //Gets a list of the ElementType Parameters
                string s = ShowParameters(e, wallType, "WallType Parameters: ");
                allString += s;
            }
            
            //generates a .lock3r file with  ground truth triples, with checksum which will be compared with
            //I am passing the rvt file location where the ground truth file will be stored

            WriteGroundTruthFile(txtpath, allString);

            return Result.Succeeded;
        }

        public static string ShowParameters(Element e, WallType wallType, string header)
        {
            string s = string.Empty;

            foreach (Parameter param in wallType.Parameters)
            {
                if (param.IsShared)
                {
                    string name = param.Definition.Name;

                    // To get the value, we need to parse the param depending on the storage type
                    // see the helper function below
                    string val = ParameterToString(param);
                    
                    string paramvalueChecksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val);

                    if (!string.IsNullOrEmpty(val))
                    {
                        s += e.Id.ToString() + " " + param.GUID + " " + paramvalueChecksum  + "\r\n";

                    }
                    Debug.Print("elementid: " + e.Id.ToString() + "parameter GUID: " + param.GUID + "Parmeter Value: " + val  + "Checksum:" + paramvalueChecksum);
                }
            }
            return s;
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


       
        public static string ComputeChecksum(string value)

        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        /// <summary>
        /// Helper function: return a string form of a given parameter.
        /// </summary>
        public static string ParameterToString(Parameter param)
        {
            string val = "none";

            if (param == null)
            {
                return val;
            }

            // To get to the parameter value, we need to parse it depending on its storage type

            switch (param.StorageType)
            {
                case StorageType.Double:
                    double dVal = param.AsDouble();
                    val = dVal.ToString(); // what precision? add precision control?
                    break;
                case StorageType.Integer:
                    int iVal = param.AsInteger();
                    val = iVal.ToString();
                    break;
                case StorageType.String:
                    string sVal = param.AsString();
                    val = sVal;
                    break;
                case StorageType.ElementId:
                    ElementId idVal = param.AsElementId();
                    val = idVal.IntegerValue.ToString();
                    break;
                case StorageType.None:
                    break;
            }
            return val;
        }
    }
}
