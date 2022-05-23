//
// (C) Copyright 2003-2023 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//


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

    public Result OnStartup(UIControlledApplication application)
    {
      string path = Assembly.GetExecutingAssembly().Location;
      string tabName = "Lock3r";
      string panelName = "Validation";

      //create bitmap images
      BitmapImage groundTruthImage = new BitmapImage(new Uri(_resource_path + "gtfile1.png"));
      BitmapImage validateImage = new BitmapImage(new Uri(_resource_path + "check3.png"));

      //create tab
      application.CreateRibbonTab(tabName);

      //create panel
      var lock3rPanel = application.CreateRibbonPanel(tabName, panelName);

      //create buttons

      var grdTruthButton = new PushButtonData("CmdGroundTruthButton", "Ground Truth", path, "RvtLock3r.CmdGroundTruth");
      grdTruthButton.ToolTip = "Export Ground Truth Data";
      grdTruthButton.LongDescription = "Store ground truth data of the original protected model properties in the Revit model";
      grdTruthButton.LargeImage = groundTruthImage;
      var grdTruthBtn = lock3rPanel.AddItem(grdTruthButton) as PushButton;

      var validateButton = new PushButtonData("CmdValidationButton", "Validate", path, "RvtLock3r.CmdValidation");
      validateButton.ToolTip = "Validate";
      validateButton.LongDescription = "Validate the open model with the ground truth data. Throw an exception if any protected parameter value was modified.";
      validateButton.LargeImage = validateImage;
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
          "This model is corrupted. "
          + "The original protected model properties have been modified "
          + "and the authenticity compromised. Please contact the vendor.");
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
          "Sorry, you are not allowed to modify protected model properties.");
        e.Cancel();
      }
    }

    public Result OnShutdown(UIControlledApplication a)
    {
      return Result.Succeeded;
    }
  }
}
