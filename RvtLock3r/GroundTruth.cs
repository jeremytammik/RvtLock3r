using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;

namespace RvtLock3r
{
  class GroundTruth : Dictionary<ElementId, List<Tuple<Guid, string>>>
  {
    public GroundTruth( string filepath)
    {
      // Open file and rea
    }

    public List<ElementId> ElementIds
    {
      get { return new List<ElementId>(); }
    }

    public bool Validate(ElementId id, Document doc)
    {
      bool rc = false;
      if (ContainsKey(id))
      {
        Element e = doc.GetElement(id);

        foreach(Tuple<Guid, string> pair in this[id])
        {
          Parameter p = e.get_Parameter(pair.Item1);

          // compare param value with checksum in Item2

        }


      }
      return rc;
    }

    /// <summary>
    /// Read ground truth data from external text file
    /// Todo: checksum is missing!
    /// </summary>
    static Dictionary<ElementId, List<Guid>> GetGroundTruthData(string pathname)
    {
      Dictionary<ElementId, List<Guid>> elemParamaters = new Dictionary<ElementId, List<Guid>>();
      string[] lines = File.ReadAllLines(pathname);

      for (int j = 0; j < lines.Length - 1; j++)
      {
        string[] triple = lines[j].Split(null);
        string id = triple[0];
        int i = int.Parse(triple[0]);
        ElementId eid = new ElementId(i);
        Guid pid = new Guid(triple[1]);

        if (!elemParamaters.ContainsKey(eid))
        {
          elemParamaters.Add(eid, new List<Guid>());
        }
        if (!elemParamaters[eid].Contains(pid))
        {
          elemParamaters[eid].Add(pid);
        }
      }
      return elemParamaters;
    }
  } 
}
