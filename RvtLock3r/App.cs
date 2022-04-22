#region Namespaces
using Autodesk.Revit.ApplicationServices;
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
    const string _resource_path = "pack://application:,,,/RvtLock3r;component/Resources/";


    //public object Session { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
      string path = Assembly.GetExecutingAssembly().Location;
      string tabName = "Lock3r";
      string panelName = "Validation";

      //creating bitimages
      BitmapImage groundTruthImage = new BitmapImage(new Uri(_resource_path + "gtfile1.png"));
      BitmapImage validateImage = new BitmapImage(new Uri(_resource_path + "check3.png"));

      //create tab
      application.CreateRibbonTab(tabName);

      //create panel
      var lock3rPanel = application.CreateRibbonPanel(tabName, panelName);

      //create buttons

      var grdTruthButton = new PushButtonData("Ground Truth Button", "Ground Truth", path, "RvtLock3r.CmdGroundTruth");
      grdTruthButton.ToolTip = "Export Ground Truth Data";
      grdTruthButton.LongDescription = "Export ground truth triple data of the original model to an en external text file located in the same directory as the Revit model";
      grdTruthButton.LargeImage = groundTruthImage;
      //add the button1 to panel
      var grdTruthBtn = lock3rPanel.AddItem(grdTruthButton) as PushButton;

      var validateButton = new PushButtonData("My Test Button2", "Validate", path, "RvtLock3r.CmdValidation");
      validateButton.ToolTip = "Validate";
      validateButton.LongDescription = "Validate the open model with the ground truth data. Throw an error if any protected parameter value was modified.";
      validateButton.LargeImage = validateImage;

      //add stacked buttons
      var validateBtn = lock3rPanel.AddItem(validateButton) as PushButton;
      

      application.ControlledApplication.DocumentOpened += OnDocumentOpened;
      application.ControlledApplication.DocumentSaving += OnDocumentSaving;
      //application.ControlledApplication.DocumentOpening += OnDocumentOpening;


      return Result.Succeeded;
    }

    private void OnDocumentOpening(object sender, DocumentOpeningEventArgs e)
    {
      TaskDialog.Show("Document Opening", "My Document is Opening. lets see.");
    }

    private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
    {
      Document doc = e.Document;


      GroundTruth gt = new GroundTruth(doc);
      if (!gt.Validate(doc))
      {
        // present a useful error message to the user to explain the probloem
        TaskDialog.Show("Corrupted File!",
          "This file is corrupted. "
          + "The original vendor data has been modified "
          + "and the authenticity compromised.");
     

      }
    }
    private void OnDocumentSaving(object sender, DocumentSavingEventArgs e)
    {
      Document doc = e.Document;

      GroundTruth gt = new GroundTruth(doc);
      if (!gt.Validate(doc))
      {
        // present a useful error message to the user to explain the probloem
        TaskDialog.Show("Permission Denied!", 
          "You are not alowed to modify this parameter value.");
        e.Cancel();
      }
    }

    public Result OnShutdown(UIControlledApplication a)
    {
      // remove the event.
      return Result.Succeeded;
    }

    /// <summary>
    /// Adds toggling buttons on Lock3r ribbon tab for switching the updater On and Off
    /// </summary>
    /// <param name="panel"></param>
  
  }

 
}
