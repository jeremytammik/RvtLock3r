using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLock3r
{
  internal class ParamValueValidator : IUpdater
  {
    static AddInId appId;
    UpdaterId updaterId;
    private FailureDefinitionId failureId = null;
    public static bool updateActive = false;


    public Dictionary<ElementId, List<Guid>> paramGuid
        = new Dictionary<ElementId, List<Guid>>();
    // constructor takes the AddInId for the add-in associated with this updater
    public ParamValueValidator(AddInId id)
    {
      appId = id;
      updaterId = new UpdaterId(appId,
          new Guid("5b5382d3-4cc3-48db-88e8-8cefff8f0243"));
    }


    public void Execute(UpdaterData data)
    {
      if (updateActive == false) { return; }
      Document doc = data.GetDocument();

      // from the document, retrieve its ground truth from the ground truth dictionary

      GroundTruth truth = null;

      Application app = doc.Application;
      foreach (ElementId id in data.GetModifiedElementIds())
      {
        Element e = doc.GetElement(id);
        truth.Validate(id, doc);

        string rvtpath = doc.PathName;
        string txtpath = rvtpath.Replace(".rte", ".lock3r");

        // 
        // Two problems calling GetGroundTruthData
        // at this point:
        // 1. why inside the foreach elementd loop?
        // that means, you re-read the file for every element.
        // why? the file is constant and nothing will change!
        // 2. much worse: why in updater execute?
        // the file should be read only one single time,
        // when the document is opened.
        //

        //int count = Util.GetGroundTruthData(txtpath).Count;

        // get all the parameter guids from the dictionary mapping element id to the ground truth guids
        List<Guid> groundTruthParamGuids = new List<Guid>();
        //foreach (KeyValuePair<ElementId, List<Guid>> kvp in Util.GetGroundTruthData(txtpath))
        //{
        //  groundTruthParamGuids.AddRange(kvp.Value);

        //}
        int i = groundTruthParamGuids.Count;

        foreach (Guid paramGuid in groundTruthParamGuids)
        {
          //Parameter p = e.LookupParameter(paramName);
          Parameter p = e.get_Parameter(paramGuid);
          if (p != null)
          {

            FailureMessage failMessage = new FailureMessage(FailureId);
            failMessage.SetFailingElement(id);
            doc.PostFailure(failMessage);

          }
        }


      }
    }

    public FailureDefinitionId FailureId
    {
      get
      {
        return failureId;
      }

      set
      {
        failureId = value;
      }


    }



    public string GetAdditionalInformation()
    {
      return "Give warning and error if wall parameters are modified";
    }

    public ChangePriority GetChangePriority()
    {
      return ChangePriority.FloorsRoofsStructuralWalls;
    }

    public UpdaterId GetUpdaterId()
    {
      return updaterId;
    }

    public string GetUpdaterName()
    {
      return "Parameter Value Validator";
    }
  }
}
