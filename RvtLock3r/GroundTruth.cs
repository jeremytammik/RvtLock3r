using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RvtLock3r
{
  class GroundTruth : Dictionary<ElementId, List<Tuple<Guid, string>>>
  {
    public GroundTruth( string filepath)
    {
      // Open ile and rea
    }

    public List<ElementId> ElementIds
    {
      get { return new List this.Keys }
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

  } 
}
