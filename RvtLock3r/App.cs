#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace RvtLock3r
{
  public class App : IExternalApplication
  {
    public static ParamValueValidator _paramValueValidator = null;

    //public object Session { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
      string tabName = "Lock3r";
      string panelName = "Validation";

      //creating bitimages
      BitmapImage groundTruthImage = new BitmapImage(new Uri("pack://application:,,,/RvtLock3r;component/Resources/gtfile1.png"));

      BitmapImage validateImage = new BitmapImage(new Uri("pack://application:,,,/RvtLock3r;component/Resources/check3.png"));

      //create tab
      application.CreateRibbonTab(tabName);

      //create panel
      var lock3rPanel = application.CreateRibbonPanel(tabName, panelName);

      //create buttons

      var grdTruthButton = new PushButtonData("Ground Truth Button", "Ground Truth", Assembly.GetExecutingAssembly().Location, "RvtLock3r.CmdGroundTruth");
      grdTruthButton.ToolTip = "Export Ground Truth Data";
      grdTruthButton.LongDescription = "Export ground truth triple data of the original model to an en external text file located in the same directory as the Revit model";
      grdTruthButton.LargeImage = groundTruthImage;
      //add the button1 to panel
      var grdTruthBtn = lock3rPanel.AddItem(grdTruthButton) as PushButton;

      var validateButton = new PushButtonData("My Test Button2", "Validate", Assembly.GetExecutingAssembly().Location, "RvtLock3r.CmdValidation");
      validateButton.ToolTip = "Validate";
      validateButton.LongDescription = "Validate the open model with the ground truth data. Throw an error if any protected parameter value was modified.";
      validateButton.LargeImage = validateImage;

      //add stacked buttons
      var validateBtn = lock3rPanel.AddItem(validateButton) as PushButton;
      lock3rPanel.AddSeparator();

      AddDmuCommandButtons(lock3rPanel);

      //instantiates the ParamValueValidator
      _paramValueValidator = new ParamValueValidator(application.ActiveAddInId);
      //Defines a failure Id 
      FailureDefinitionId failId = new FailureDefinitionId(new Guid("f04836cc-a698-4bec-9e02-0603d0bd8cf9"));

      //Defines failure definition text tht will be posted to the end user if the updater is not loaded
      FailureDefinition failDefError = FailureDefinition.CreateFailureDefinition(failId, FailureSeverity.Error, "Permission Denied: Sorry, you are not allowed to modify the Wall Type parameters.");
      // save ids for later reference
      _paramValueValidator.FailureId = failId;

      application.ControlledApplication.DocumentOpening += OnDocumentOpening;

      return Result.Succeeded;
    }

    private void OnDocumentOpening(object sender, DocumentOpenedEventArgs e)
    {
      Document doc = e.Document;
      
      /*
       *this code is only used for DMU approach:
       
      string path = doc.PathName;
      Debug.Assert(null != path, "expected valid document path");
      Debug.Assert(0 < path.Length, "expected valid document path");
      if((null != path) && (0 < path.Length))
      {
        GroundTruthLookup.Singleton.Add(path, new GroundTruth(doc));
      }
      */
      
      // If validtion is performed directly and only during opening and saving,
      // we can read the ground truth from this current document and validate it
      // on the spot:
      
      GroundTruth gt = new GroundTruth(doc);
      if( !gt.Validate( doc ))
      {
        // present a useful error message to the user to explain the probloem
        e.Cancel();
      }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
      // remove the event.
      return Result.Succeeded;
    }

    /// <summary>
    /// Adds toggling buttons on Lock3r ribbon tab for switching the updater On and Off
    /// </summary>
    /// <param name="panel"></param>
    private void AddDmuCommandButtons(RibbonPanel panel)
    {
      BitmapImage dmuOnImg = new BitmapImage(new Uri("pack://application:,,,/RvtLock3r;component/Resources/btn1.png"));

      BitmapImage dmuOffImg = new BitmapImage(new Uri("pack://application:,,,/RvtLock3r;component/Resources/btn2.png"));

      string path = Assembly.GetExecutingAssembly().Location;

      // create toggle buttons for radio button group 

      ToggleButtonData toggleButtonData3
        = new ToggleButtonData(
          "WallTypeDMUOff", "DMU Off", path,
          "RvtLock3r.UIDynamicModelUpdateOff");

      toggleButtonData3.LargeImage = dmuOffImg;

      ToggleButtonData toggleButtonData4
        = new ToggleButtonData(
          "WallTypeDMUOn", "DMU On", path,
          "RvtLock3r.UIDynamicModelUpdateOn");

      toggleButtonData4.LargeImage = dmuOnImg;

      // make dyn update on/off radio button group 

      RadioButtonGroupData radioBtnGroupData2 =
        new RadioButtonGroupData("ParameterUpdater");

      RadioButtonGroup radioBtnGroup2
        = panel.AddItem(radioBtnGroupData2)
          as RadioButtonGroup;

      radioBtnGroup2.AddItem(toggleButtonData3);
      radioBtnGroup2.AddItem(toggleButtonData4);
    }
  }

  /// <summary>
  /// Turns the updater OFF
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class UIDynamicModelUpdateOff : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      ParamValueValidator.updateActive = false;
      return Result.Succeeded;
    }
  }

  /// <summary>
  /// Turns the updater ON, Registers the updater and Adds Trigger to the updater
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class UIDynamicModelUpdateOn : IExternalCommand
  {
    public static ParamValueValidator _paramValueValidator = null;
    private static List<ElementId> idsToWatch = new List<ElementId>();

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      ParamValueValidator.updateActive = true;
      try
      {
        Document doc = commandData.Application.ActiveUIDocument.Document;
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        AddInId appId = commandData.Application.ActiveAddInId;
        // creating and registering the updater for the document.
        if (_paramValueValidator == null)
        {
          _paramValueValidator = App._paramValueValidator;
          _paramValueValidator.Register(doc);
        }

        string path = doc.PathName;
        string filePath = path.Replace(".rte", ".lock3r");
        GroundTruth truth = new GroundTruth(filePath);
        idsToWatch = truth.ElementIds.ToList();
        int count = truth.ElementIds.Count;

        _paramValueValidator.AddTriggerForUpdater(idsToWatch);
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return Result.Failed;
      }
      return Result.Succeeded;
    }
  }
}
