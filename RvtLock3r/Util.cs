using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using WinForms = System.Windows.Forms;

namespace RvtLock3r
{
  public class Util
  {
    private const string _caption = "Ground Truth Data";


    /// <summary>
    /// Return string representation of ground truth triples 
    /// to be saved on an an external file for later validation.
    /// </summary>
    public static string GroundTruthData(Document doc)
    {
      FilteredElementCollector wallTypes
          = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));
      string allData = string.Empty;

      foreach (Element e in wallTypes)
      {
        string s = string.Empty;


        foreach (Parameter param in e.Parameters)
        {
          if (param.IsShared)
          {
            string name = param.Definition.Name;

            string val = ParameterToString(param);

            string checksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val);


            if (!string.IsNullOrEmpty(val))
            {
              //s += e.Id.ToString() + " " + param.GUID + " " + checksum + "\r\n";
              s += e.Id.ToString() + " " + param.GUID + " " + checksum + ",";

            }

            Debug.Print("elementid: " + e.Id.ToString()
    + "parameter GUID: " + param.GUID
    + "Parmeter Value: " + val
    + "Checksum:" + checksum);

          }

        }
        allData += s;

      }

      return allData;

    }

    /// <summary>
    /// Generates a list of Ground Truth tripples 
    /// to be saved in in the schema for e-store
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static List<GroundTruthTriples> GroundTruthListData(Document doc)

    {
      FilteredElementCollector wallTypes
          = new FilteredElementCollector(doc)
          .OfClass(typeof(WallType));

      List<GroundTruthTriples> groundTruthData = new List<GroundTruthTriples>();

      foreach (Element e in wallTypes)
      {
        List<GroundTruthTriples> GroundTruthTriples = new List<GroundTruthTriples>();

        foreach (Parameter param in e.Parameters)
        {
          if (param.IsShared)
          {
            string name = param.Definition.Name;

            string val = ParameterToString(param);

            string checksum = string.IsNullOrEmpty(val) ? null : ComputeChecksum(val);

            if (!string.IsNullOrEmpty(val))
            {
              GroundTruthTriples gtTripples = new GroundTruthTriples();
              gtTripples.ElementId = e.Id.ToString();

              gtTripples.ParamGuid = param.GUID.ToString();
              gtTripples.Checksum = checksum;
              GroundTruthTriples.Add(gtTripples);


            }

          }


        }
        groundTruthData.AddRange(GroundTruthTriples);
      }

      return groundTruthData;
    }

    /// <summary>
    /// Return string representation of parameter value
    /// </summary>
    public static string ParameterToString(Parameter param)
    {
      string val = "none";

      if (param == null)
      {
        return val;
      }

      // Convert parameter value to string depending on its storage type

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

    /// </summary>
    /// Computes the checksum of each ElementType Parameter value
    /// </summary>
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
    /// Return a string describing the given element:
    /// .NET type name,
    /// category name,
    /// family and symbol name for a family instance,
    /// element id and element name.
    /// </summary>
    public static string ElementDescription(Element e)
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

    /// </summary>
    /// Convert error log integer keys to 
    /// ElementSet elements for error report 
    /// </summary>
    public static ElementSet GetAlteredElements(
      Document doc,
      Dictionary<int, List<Guid>> errorLog,
      ElementSet elementSet)
    {
      Element e = null;
      foreach (int i in errorLog.Keys)
      {
        ElementId eid = new ElementId(i);
        e = doc.GetElement(eid);
        elementSet.Insert(e);
      }
      return elementSet;
    }

    public static void InfoMsg(string msg)
    {
      Debug.WriteLine(msg);
      WinForms.MessageBox.Show(msg,
          _caption,
          WinForms.MessageBoxButtons.OK,
          WinForms.MessageBoxIcon.Information);
    }

    public static void ErrorMsg(string msg)
    {
      Debug.WriteLine(msg);
      WinForms.MessageBox.Show(msg,
          _caption,
          WinForms.MessageBoxButtons.OK,
          WinForms.MessageBoxIcon.Error);
    }
  }
}