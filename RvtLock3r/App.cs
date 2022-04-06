#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace RvtLock3r
{
  class App : IExternalApplication
  {
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
            grdTruthButton.LongDescription = "Export Ground Truth Tripple data of the original model to an en external text file located in the same directory as the Revit model";
            grdTruthButton.LargeImage = groundTruthImage;
            //add the button1 to panel
            var grdTruthBtn = lock3rPanel.AddItem(grdTruthButton) as PushButton;

            var validateButton = new PushButtonData("My Test Button2", "Validate", Assembly.GetExecutingAssembly().Location, "RvtLock3r.CmdCommand");
            validateButton.ToolTip = "Validate";
            validateButton.LongDescription = "Validates the opened model to the Ground Truth Tripple data. Throws an error if any parameter value was modified.";
            validateButton.LargeImage = validateImage;

            //add stacked buttons
            var validateBtn = lock3rPanel.AddItem(validateButton) as PushButton;
            lock3rPanel.AddSeparator();
            RegisterWallUpdater(application);

            AddDmuCommandButtons(lock3rPanel);


            return Result.Succeeded;
    }

       /// <summary>
       /// Register  the update and add triger
       /// </summary>
       /// <param name="app"></param>
        public static void RegisterWallUpdater(UIControlledApplication app)
        {
            //initializes the wall updater
            WallUpdater wallUpdater = new WallUpdater(app.ActiveAddInId);
            // Register the wall updater if the updater.
            UpdaterRegistry.RegisterUpdater(wallUpdater);
            //Gets the filter of class wall type
            ElementClassFilter filter = new ElementClassFilter(typeof(WallType));
            //creates a triger on any change type on the wallType properties
            UpdaterRegistry.AddTrigger(wallUpdater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            //Defines a failure Id 
            FailureDefinitionId failId = new FailureDefinitionId(new Guid("65f0afd6-adce-4562-ae40-8a86b53e4002"));
            //Defines failure definitin text tht will be posted to the end user if the updater is not loaded
            FailureDefinition failDefError = FailureDefinition.CreateFailureDefinition(failId, FailureSeverity.Error, "Permission Denied: Sorry, you are not allowed to modify the Wall Type parameters.");
            // save ids for later reference
            wallUpdater.FailureId = failId;
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
    [Regeneration(RegenerationOption.Manual)]
    public class UIDynamicModelUpdateOff : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            WallUpdater.m_updateActive = false;
            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class UIDynamicModelUpdateOn : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            WallUpdater.m_updateActive = true;
            return Result.Succeeded;
        }
    }
}
