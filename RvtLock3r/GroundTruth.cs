using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;

namespace RvtLock3r
{
  /// <summary>
  /// Manage ground truth for one given model
  /// </summary>
  class GroundTruth : Dictionary<ElementId, Dictionary<Guid, string>>
  {
    /// <summary>
    /// Instantiate ground truth from external text file
    /// containing triples of element id, shared parameter 
    /// guid and parameter value checksum
    /// </summary>
    public GroundTruth(string filepath)
    {
      string[] lines = File.ReadAllLines(filepath);

      for (int j = 0; j < lines.Length - 1; j++)
      {
        string[] triple = lines[j].Split(null);
        string id = triple[0];
        int i = int.Parse(triple[0]);
        ElementId eid = new ElementId(i);
        Guid pid = new Guid(triple[1]);

        if (!ContainsKey(eid))
        {
          Add(eid, new Dictionary<Guid, string>());
        }
        if (!this[eid].ContainsKey(pid))
        {
          this[eid].Add(pid, triple[2]);
        }
      }

    }

    public ICollection<ElementId> ElementIds
    {
      get { return Keys; }
    }

    public bool Validate(ElementId id, Document doc)
    {
      bool rc = true;
      if (ContainsKey(id))
      {
        Dictionary<Guid, string> ps = this[id];
        Element e = doc.GetElement(id);

        foreach (KeyValuePair<Guid, string> pair in ps)
        {
          Parameter p = e.get_Parameter(pair.Key);
          string pvalue = Util.ParameterToString(p);
          string checksum = Util.ComputeChecksum(pvalue);
          rc = checksum.Equals(pair.Value);
          if(!rc) { break; }
        }
      }
      return rc;
    }
  }
}
