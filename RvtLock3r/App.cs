#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace RvtLock3r
{
  class App : IExternalApplication
  {
        public static string rvtFilePath;
        public static UIControlledApplication _uiControlledApp;
    

        public Result OnStartup(UIControlledApplication application)
            { 
                _uiControlledApp = application;

                //application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(onViewActivated);
                application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(doc_opened);
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

          var validateButton = new PushButtonData("My Test Button2", "Validate", Assembly.GetExecutingAssembly().Location, "RvtLock3r.CmdCommand");
          validateButton.ToolTip = "Validate";
          validateButton.LongDescription = "Validate the open model with the ground truth data. Throw an error if any protected parameter value was modified.";
          validateButton.LargeImage = validateImage;

          //add stacked buttons
          var validateBtn = lock3rPanel.AddItem(validateButton) as PushButton;
          lock3rPanel.AddSeparator();

                AddDmuCommandButtons(lock3rPanel);
                return Result.Succeeded;
    }
        /// <summary>
        /// Gives access to the active document using the viewActivated event arguments
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        void onViewActivated(object sender, ViewActivatedEventArgs e)
        {
            View vCurrent = e.CurrentActiveView;
            Document doc = e.Document;
            rvtFilePath = doc.PathName;
            TaskDialog.Show("Revit file path", rvtFilePath);
            RegisterParamValueValidator(_uiControlledApp);

        }
        private void doc_opened(object sender, DocumentOpenedEventArgs e)
        {
            Document doc = e.Document;

            UIDocument uidoc = new UIDocument(doc);
            Application app = e.Document.Application;
            UIControlledApplication uiapp = sender as UIControlledApplication;
            rvtFilePath = doc.PathName;
            RegisterParamValueValidator(_uiControlledApp);
        }

        /// <summary>
        /// Register  the updater and add triger
        /// </summary>
        /// <param name="app"></param>
        public static void RegisterParamValueValidator(UIControlledApplication app)
    {
            //initializes the wall updater
            ParamValueValidator paramValueValidator = new ParamValueValidator(app.ActiveAddInId);
              // Register the wall updater if the updater.
              UpdaterRegistry.RegisterUpdater(paramValueValidator);

            //NOT Gets the filter of class wall type
            //ElementClassFilter filter = new ElementClassFilter(typeof(WallType));

            // Filter or elements specified in ground truth
            // Read the ground truth (file, extensible storage, ...)
            // Make a list of the element ids specified there
            // Also make a list of all the parameter guids (or ids) for each element id
            // dect element_id --> parameter id 
            // make this dictionary available in the updater (member of the updater)

            string txtpath = rvtFilePath.Replace(".rte", ".lock3r");
            int count = Util.GetGroundTruthData(txtpath).Count;
            List<ElementId> groundTruthElemIds = new List<ElementId>();
            foreach (KeyValuePair<ElementId, List<Guid>> kvp in Util.GetGroundTruthData(txtpath))
            {
                groundTruthElemIds.Add(kvp.Key);

            }
            int countIds = groundTruthElemIds.Count;
            ElementFilter filter = new ElementIdSetFilter(groundTruthElemIds);
            //ElementClassFilter filter = new ElementClassFilter(typeof(WallType));

            //creates a triger on any change type on the wallType properties
            UpdaterRegistry.AddTrigger(paramValueValidator.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            //Defines a failure Id 
            FailureDefinitionId failId = new FailureDefinitionId(new Guid("f04836cc-a698-4bec-9e02-0603d0bd8cf9"));

            //Defines failure definition text tht will be posted to the end user if the updater is not loaded
            FailureDefinition failDefError = FailureDefinition.CreateFailureDefinition(failId, FailureSeverity.Error, "Permission Denied: Sorry, you are not allowed to modify the Wall Type parameters.");
            // save ids for later reference
            paramValueValidator.FailureId = failId;


        }


    public Result OnShutdown(UIControlledApplication application)
    {
      return Result.Succeeded;
    }



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

  [Transaction(TransactionMode.ReadOnly)]
  public class UIDynamicModelUpdateOn : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      ParamValueValidator.updateActive = true;
      return Result.Succeeded;
    }
  }
}
