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
using System.Text;

#endregion

namespace RvtLock3r
{
  [Transaction(TransactionMode.Manual)]
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

      // Access current selection

      Selection sel = uidoc.Selection;

            // Retrieve elements from database

            FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Walls)
          .OfClass(typeof(Wall));

            // Filtered element collector is iterable

            foreach (Element e in col)
            {
                //Get the elementTypeId
                ElementId elemTypeId = e.GetTypeId();
                //Get the ElementType
                ElementType elemType = (ElementType)doc.GetElement(elemTypeId); 

                

                //Gets a list of the ElementType Parameters
                ShowParameters(e, elemType, "WallType Parameters: ");
            }

            // Modify document within a transaction

            //using (Transaction tx = new Transaction(doc))
            //{
            //  tx.Start("Transaction Name");
            //  tx.Commit();
            //}

            return Result.Succeeded;
    }
        public void ShowParameters(Element e, ElementType eType, string header)
        {
            string s = string.Empty;
            //ChecksumData checksumData = new ChecksumData();
            List<ChecksumData> data = new List<ChecksumData>();

            foreach (Parameter param in eType.Parameters)
            {
                if (param.IsShared)
                {

                    string name = param.Definition.Name;

                    // To get the value, we need to parse the param depending on the storage type
                    // see the helper function below
                    string val = ParameterToString(param);
                    s += "\r\n" + name + " : " + val;
                    data.Add(new ChecksumData()
                    {
                        ElementId = e.Id.ToString(),
                        ElementParamName = name,
                        ElementParamValue = val,
                        Checksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val)

                    });

                }
            }
            //export

            ExportToTextFile(data);
            //TaskDialog.Show(header, s);

        }

        private void ExportToTextFile(List<ChecksumData> data)
        {
            if (File.Exists(@"D:\CheckSum\checksum.txt"))
            {
                File.Delete(@"D:\CheckSum\checksum.txt");
            }
            //open file stream
            using (StreamWriter file = File.CreateText(@"D:\CheckSum\checksum.txt"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, data);
            }
        }

        private string ComputeChecksum(string s)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(s))
                ).Replace("-", String.Empty);
            }
            return hash;
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
                    val = dVal.ToString();
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

    public class ChecksumData
    {
        public string ElementId { get; set; }
        public string ElementParamName { get; set; }
        public string ElementParamValue { get; set; }
        public string Checksum { get; set; }
        
    }
}
