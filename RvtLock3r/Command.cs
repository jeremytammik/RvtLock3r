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
using System.Text;

#endregion

namespace RvtLock3r
{
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
                string value = triple[3];

                Element e = doc.GetElement(eid);

                Wall wall = (Wall)doc.GetElement(e.Id);
                WallType wallType = wall.WallType;

                Parameter p = wallType.get_Parameter(pid);

                //string pval = ParameterToString(p);
                CmdGroundTruth cmdGroundTruth = new CmdGroundTruth();
                string pval = cmdGroundTruth.ParameterToString(p);


                //string pchecksum = ComputeChecksum(pval);
                string pchecksum = string.IsNullOrEmpty(pval) ? null : cmdGroundTruth.sha256_hash(pval);
                //TaskDialog.Show("Line read with values", e.Id.ToString() + " " + p.GUID + " " + pchecksum);
                //TaskDialog.Show("compare checksums", checksum + " " + " " + pchecksum);


                if (!checksum.Equals(pchecksum))
                {
                    //TaskDialog.Show("Parameter changed values", e.Id.ToString() + " " + p.GUID + " " + checksum + " " + pchecksum);
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
                else
                {
                    //TaskDialog.Show("No Parameter changed", e.Id.ToString() + " " + p.GUID);

                }
            }


            int n = errorLog.Count;

            if (0 < n)
            {
                // Report errors to user
                // Set reference return values ElementSet elements and message

                if (1 == n)
                {

                }

                return Result.Failed;
            }
            return Result.Succeeded;
        }

        public void ShowParameters(Element e, /*ElementType eType,*/ string header)
        {
            string s = string.Empty;
            List<ChecksumData> data = new List<ChecksumData>();

            foreach (Parameter param in e.Parameters)
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
                        ElementId = e.Id.IntegerValue,
                        ElementParamId = param.Id.IntegerValue,
                        ElementParamValue = val,
                        Checksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val)
                    });
                }
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

        /// <summary>
        ///     Return a string describing the given element:
        ///     .NET type name,
        ///     category name,
        ///     family and symbol name for a family instance,
        ///     element id and element name.
        /// </summary>
        public static string ElementDescription(
            Element e)
        {
            if (null == e) return "<null>";

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            var fi = e as FamilyInstance;

            var typeName = e.GetType().Name;

            var categoryName = null == e.Category
                ? string.Empty
                : $"{e.Category.Name} ";

            var familyName = null == fi
                ? string.Empty
                : $"{fi.Symbol.Family.Name} ";

            var symbolName = null == fi
                             || e.Name.Equals(fi.Symbol.Name)
                ? string.Empty
                : $"{fi.Symbol.Name} ";

            return $"{typeName} {categoryName}{familyName}{symbolName}<{e.Id.IntegerValue} {e.Name}>";
        }
    }

    public class ChecksumData
    {
        public int ElementId { get; set; } // not to confuse with ElementId class
        public int ElementParamId { get; set; } // maybe better Definition element id, or shared param GUID
        public string ElementParamValue { get; set; }
        public string Checksum { get; set; }

    }
}