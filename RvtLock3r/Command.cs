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

      // Access current selection

      Selection sel = uidoc.Selection;

            // Retrieve elements from database

            FilteredElementCollector col = new FilteredElementCollector(doc);

            col.OfCategory(BuiltInCategory.OST_Walls)
            //col.OfClass(typeof(WallType))
                .WhereElementIsNotElementType().ToElements();

           
            List<ChecksumData> data = new List<ChecksumData>();
            List<ChecksumData> allData = new List<ChecksumData>();

            foreach (Element e in col)
                        {
                Wall wall = (Wall)doc.GetElement(e.Id);
                WallType wallType = wall.WallType;

                // the wall type will maybe reappear many times; done like this, we need to skip wall types already processed
                //ElementType elemType = (ElementType)doc.GetElement(elemTypeId);

                //Gets a list of the ElementType Parameters
                data = ShowParameters(e, wallType);
                allData.AddRange(data);
                //allData.Add(data);
                //wholeString += s;
                //TaskDialog.Show("WallType Parameters: ", s);

            }
            //wholeString += s;

            //TaskDialog.Show("All WallType Parameters: ", wholeString);
            ExportToTextFile(allData);
            return Result.Succeeded;
    }

    public List<ChecksumData> ShowParameters(Element e, WallType wallType)
    {
      string s = string.Empty;
      List<ChecksumData> data = new List<ChecksumData>();

      foreach (Parameter param in wallType.Parameters)
      {
                if (param.IsShared)
                {
                    string name = param.Definition.Name;

          // To get the value, we need to parse the param depending on the storage type
          // see the helper function below
                  string val = ParameterToString(param);
                  s += "\r\n"+ e.Id.ToString() + " - " + name + " : " + val;
                  data.Add(new ChecksumData()
                  {
                      Element_Id = e.Id.IntegerValue,
                    ParamGuid = param.Id.IntegerValue,
                    //SharedParamValue = val,
                    Checksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val)
                  });
                } 
      }

            //export

            //ExportToTextFile(data);
            //TaskDialog.Show(header, s);
            return data;
           

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

  public class ChecksumData
  {
    public int Element_Id { get; set; } // not to confuse with ElementId class
    public int ParamGuid { get; set; } // maybe better Definition element id, or shared param GUID
    //public string SharedParamValue { get; set; }
    public string Checksum { get; set; }

  }
}
