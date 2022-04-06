using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLock3r
{
  internal class ParamValueValidator : IUpdater
  {
    static AddInId m_appId;
    UpdaterId m_updaterId;
    FailureDefinitionId m_failureId = null;
    FailureDefinitionId m_warnId = null;
    public static bool m_updateActive = false;
    public Dictionary element id parameter guid

    // constructor takes the AddInId for the add-in associated with this updater
    public ParamValueValidator(AddInId id)
    {
      m_appId = id;
      m_updaterId = new UpdaterId(m_appId,
          new Guid("5b5382d3-4cc3-48db-88e8-8cefff8f0243"));
    }

    public void Execute(UpdaterData data)
    {
      if (m_updateActive == false) { return; }
      Document doc = data.GetDocument();
      Autodesk.Revit.ApplicationServices.Application app = doc.Application;
      foreach (ElementId id in data.GetModifiedElementIds())
      {
        Element e = doc.GetElement(id);

        // get all the parameter guids from the dictionary mapping element id to the ground truth guids

        foreach (string paramgin Util.GetParamNamesToLookUp(e))
        {
          //Parameter p = e.LookupParameter(paramName);
          Parameter p = e.get_Parameter(pguid);
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
      get { return m_failureId; }
      set { m_failureId = value; }
    }

    public FailureDefinitionId WarnId
    {
      get { return m_warnId; }
      set { m_warnId = value; }
    }

    public string GetAdditionalInformation()
    {
      return "Give warning and error if "
        + "wall parameters are modified";
    }

    public ChangePriority GetChangePriority()
    {
      return ChangePriority.FloorsRoofsStructuralWalls;
    }

    public UpdaterId GetUpdaterId()
    {
      return m_updaterId;
    }

    public string GetUpdaterName()
    {
      return "Wall Parameter Modification Check";
    }
  }
}
