# RvtLock3r End User Help Documentation

[online](https://myshare.autodesk.com/:w:/g/personal/mikako_harada_autodesk_com/EXbZYeXRuZ9Kr_E5RH7u-h0B70L7kd2dDIjXjmaKM-7p8g?e=X5Bqp9)

Currently, Revit does not provide any built-in functionality to prevent the user from modifying parameter values.
RvtLock3r is a Revit add-in that you can use to verify whether parameter values have been changed. 

There are two types of end users for RvtLock3r: 

- Vendor: the provider of a set of elements with protected properties 
- Consumer: the consumer of these elements to create a model 

## Vendor 

The vendor defines the BIM elements properties they want to protect, also known as _ground truth_.

They make use of two external commands in the Revit ribbon tab _Lock3r_ in the panel _Validation_:  

- Ground Truth: this command generates the ground truth data and saves it in the Revit BIM. 
- Validate: this command checks if any protected properties have been modified.

The validation command runs successfully and completes silently with no error if the model is in its original state and no protected parameter values have been modified.
In case any of the protected properties were modified, it returns an error code with a message and a list of element ids of the modified elements:

img

The vendor can manually validate a given model to check if any prohibited property has been modified using the 


## Consumer 

The consumer makes use of the elements equipped with the veendor's read-only properties.
They can interact with the model however they wish.
The interaction is based on trust that they will not interfere with prohibited properties.
The consumer will see a warning message from RvtLock3r in the following two scenarios:  

- The consumer opens a model containing modified properties 
- The consumer tried to save a model containing modified properties 

Case 1.  In case the consumer opens a model containing modified properties from whichever source, an informational dialog box is provided to inform the user that the protected properties have been tampered with. They may choose to proceed with the tampered model or contact the vendor for the original data.

Case 2.  During interaction with the model, the consumer may intentionally or unintentionally modify a protected property. When they try to save, an informational dialog box is provided informing the user that the property is read-onlyread-only, and they are not allowed to modify it.

In case the consumer decides to close Revit with open unsaved modifications, an optional dialog box is provided for the consumer’s decision to save or not save the model.

If the consumer chooses `Yes`, the model is saved; if `No`, it remains unchanged.
