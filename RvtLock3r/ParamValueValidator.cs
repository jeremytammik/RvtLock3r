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
  public class ParamValueValidator : IUpdater
  {
    static AddInId appId;
    static UpdaterId updaterId;
    private FailureDefinitionId failureId = null;
    public static bool updateActive = true;


    public Dictionary<ElementId, List<Guid>> paramGuid
        = new Dictionary<ElementId, List<Guid>>();
    // constructor takes the AddInId for the add-in associated with this updater
    public ParamValueValidator(AddInId id)
    {
      appId = id;
      updaterId = new UpdaterId(appId,
          new Guid("5b5382d3-4cc3-48db-88e8-8cefff8f0243"));
    }

        internal void Register(Document doc)
        {
            // Register the ParamValueValidator updater if the updater is not registered.
            if (!UpdaterRegistry.IsUpdaterRegistered(updaterId))
                UpdaterRegistry.RegisterUpdater(this, doc);
        }
        /// <summary>
        /// Adds trigger to the updater
        /// </summary>
        /// <param name="idsToWatch"></param>
        internal void AddTriggerForUpdater(List<ElementId> idsToWatch)
        {

            if (idsToWatch.Count == 0)
                return;
            ElementFilter filter = new ElementIdSetFilter(idsToWatch);
            UpdaterRegistry.AddTrigger(updaterId, filter, Element.GetChangeTypeAny());


        }


        public void Execute(UpdaterData data)
    {
      if (updateActive == false) { return; }
      Document doc = data.GetDocument();

            // from the document, retrieve its ground truth from the ground truth dictionary
            string path = doc.PathName.Replace(".rte", ".lock3r");
            GroundTruth truth = new GroundTruth(path);

      Application app = doc.Application;
      foreach (ElementId id in data.GetModifiedElementIds())
      {
        bool rc = truth.Validate(id, doc);
        {
          if (!rc)
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
