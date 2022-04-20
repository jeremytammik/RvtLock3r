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
    /// Initialise ground truth from external text file
    /// containing triples of element id, shared parameter 
    /// guid and parameter value checksum.
    /// </summary>
    
        void InitialiseFromTextFile(string filepath)
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

         void  InitialiseFromDataStorage(Document doc)
        {
            var data = Util.GroundTruthData(doc);
            string[] lines = data.Split(',');


            foreach (string line in lines)
            {
                string[] triple = line.Split(null);
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

        /// <summary>
        /// Instantiate ground truth for given RVT document.
        /// </summary>
        public GroundTruth(Document doc)
    {
            //string path = doc.PathName;
            //path = path.Replace(".rte", ".lock3r");
            //InitialiseFromTextFile(path);
            InitialiseFromDataStorage(doc);
        }
        //public static GroundTruth GroundTruthObjectData(Document doc)
        //{
        //    //string path = doc.PathName;
        //    //path = path.Replace(".rte", ".lock3r");
        //    //InitialiseFromTextFile(path);
        //    GroundTruth t = new GroundTruth(doc);
        //    var data = t.InitialiseFromGroundTruthListList(doc);
        //    return data;
        //}

        /// <summary>
        /// Instantiate ground truth from external text file.
        /// </summary>
        public GroundTruth(string filepath)
    {
      InitialiseFromTextFile(filepath);
    }

    public ICollection<ElementId> ElementIds
    {
      get { return Keys; }
    }

    /// <summary>
    /// Validate all parameter values on the given element.
    /// Return false if validation fails.
    /// If no values are defined for this element, nothing 
    /// can fail, and true is returned.
    /// If 'errorLog' is null, terminate on first failure.
    /// Otherwise, continue and log all failures.
    /// </summary>
    public bool Validate(
      ElementId id,
      Document doc,
      Dictionary<int, List<Guid>> errorLog = null)
    {
      bool rc = true;
      if (ContainsKey(id))
      {
        Dictionary<Guid, string> ps = this[id];
        Element e = doc.GetElement(id);

        foreach (KeyValuePair<Guid, string> pair in ps)
        {
          Guid pid = pair.Key;
          Parameter p = e.get_Parameter(pid);
          string pvalue = Util.ParameterToString(p);
          string checksum = Util.ComputeChecksum(pvalue);
          rc = checksum.Equals(pair.Value);
          if (!rc)
          {
            if (null != errorLog)
            {
              int i = id.IntegerValue;
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
              break;
            }
          }
        }
      }
      if (null != errorLog)
      {
        rc = (0 == errorLog.Count);
      }
      return rc;
    }

    /// <summary>
    /// Validate all parameter values on all elements.
    /// Return false if validation fails.
    /// If 'errorLog' is null, terminate on first failure.
    /// Otherwise, continue and log all failures.
    /// </summary>
    public bool Validate(
      Document doc,
      Dictionary<int, List<Guid>> errorLog = null)
    {
      bool rc = true;
      foreach (ElementId id in ElementIds)
      {
        rc = Validate(id, doc, errorLog);

        if (null == errorLog && !rc)
        {
          break;
        }
      }
      if (null != errorLog)
      {
        rc = (0 == errorLog.Count);
      }
      return rc;
    }
  }
}
