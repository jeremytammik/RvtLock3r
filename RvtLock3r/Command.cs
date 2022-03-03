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

      List<string> log = new List<string>();
      string rvtpath = doc.PathName;
      string txtpath = rvtpath.Replace(".rvt", ".lock3r");
      string[] lines = File.ReadAllLines(txtpath);
      foreach(string line in lines )
      {
        string[] triple = line.Split(null);
        ElementId eid = new ElementId(int.Parse(triple[0]));
        Guid pid = new Guid(triple[1]);
        string checksum = triple[2];
        Element e = doc.GetElement(eid);
        Parameter p = e.get_Parameter(pid);
        string pval = ParameterToString(p);
        string pchecksum = ComputeChecksum(pval);
        if( !checksum.Equals(pchecksum))
        {
          log.Add("Validation error on element/parameter '{0}' -- '{1}'",
            ElementDescription(e), p.Definition.Name);
        }
      }

      /*

      // Access current selection

      Selection sel = uidoc.Selection;

      // Retrieve elements from database

      FilteredElementCollector col
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .OfCategory(BuiltInCategory.OST_Walls)
          .OfClass(typeof(Wall)); // this collects all walls, maybe thousands

      // alternatively, filter for wall types instead of walls; then we get each type just once

      // alternative 2: filtere for the walls we want, and collect all their types first;
      // then, loop over the types, not the wall instances

      // Filtered element collector is iterable

      foreach (Element e in col)
      {
        //Get the elementTypeId
        ElementId elemTypeId = e.GetTypeId();
        //Get the ElementType
        ElementType elemType = (ElementType)doc.GetElement(elemTypeId); // the wall type will maybe reappear many times; done like this, we need to skip wall types already processed

        //Gets a list of the ElementType Parameters
        ShowParameters(elemType, "WallType Parameters: ");
      }
      */
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
