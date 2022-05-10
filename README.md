# RvtLock3r

Revit .NET C# add-in to validate that certain BIM element properties have not been modified.

- [End user documentation](doc/Rvtlock3r_user_manual.md)

## Motivation

Revit does not provide any functionality to ensure that parameter values are not modified.

The add-in stores a checksum for the original read-only values of selected parameters and implements a validation function to ensure that these intended values are not modified.

The validation process can be launched in various ways:

- Initially, for testing purposes, the validation function is implemented as an external command, launched manually by the user.
- It may later be triggered automatically on opening or saving a document to notify the user that undesired tampering has taken place.
- Finally, we can use ther dynamic model updater framework DMU to detect and prevent any such tampering immediately in real-time.

That said, please note
the [caveats on model checking](#model-checker-caveat)
and [change analysis](#change-analysis-caveat).
However, they both report on modifications ater the fact.
RvtLock3r using DMU goes one step further and prevents all forbidden modifications in the first place.

Lastly, it is impossible to prevent the user from corrupting models if they really try.
A large level of trust and following best practices is required.

### Overview

We have two actors:

- The vendor shares elements and equips them with certain specific read-only properties.
- The consumer may manipulate these elements and is prohibited from modifying the protected properties.

To implement this protection, the vendor encodes the original property values in ground truth data.

The ground truth can be stored anywhere you like.
We initially implemented it as an external text file, with the disadvantage that this can be manipulated or get lost.
A better and safer solution would be to encode the ground truth in extensible storage.

We also have several options to ensure that the consumer has not modified any protected properties:

- Implement DocumentOpening and DocumentSaving events, check the ground truth in the event handlers, and cancel the operation with a useful message in case validation fails.
- Implement a DMU mechanism that prevents the consumer from performing any forbidden modifications in real time.

<!--

DocumentSaving: This event is cancellable, except when it is raised during close of the application. Check the 'Cancellable' property of event's argument to see whether it is cancellable or not. When it is cancellable, call the 'Cancel()' method of event's argument to cancel it. Your application is responsible for providing feedback to the user about the reason for the cancellation.

-->

### Model Checker Caveat

In any serious BIM environment, many rules and conventions are applied and required.
Tools such as the [Autodesk Model Checker](https://interoperability.autodesk.com/modelchecker.php) ensure that these are strictly followed and can be relied upon.
Maybe you should be using such a tool providing more coverage than RvtLock3r does?

### Change Analysis Caveat

BIM360 and ACC design collaboration provide
a [change visualization interface](https://help.autodesk.com/view/COLLAB/ENU/?guid=Design_Collab_Change_Visualization_Interface) that enables you to [find model differences by Model Properties API](https://forge.autodesk.com/blog/find-model-difference-model-properties-api).
It is based on the [Forge Model Properties API](https://forge.autodesk.com/blog/bim-360acc-model-properties-api).
Another alternative approach to this task.

## Validation

The customer add-in reads a set of [ground truth](https://en.wikipedia.org/wiki/Ground_truth) data from some [storage location](#storage). It contains a list of triples:

- `ElementId`
- `BuiltInParameter` `parameterId` or shared parameter `GUID`
- Checksum

The add-in iterates over all elements and parameters specified by these triples, reads the corresponding parameter value, calculates its checksum and validates it by comparison with the ground truth value.

Discrepancies are logged and a report is presented to the user.

The add-in does not care what kind of elements or parameters are being examined.
That worry is left up to whoever creates the ground truth file.

In the initial proof of concept, the triples are simply space separated in individual lines in a text file.

## Preparation

There are various possible approaches to prepare
the [ground truth](https://en.wikipedia.org/wiki/Ground_truth) input text file,
and they can be completely automated, more or less programmatically assisted, or fully manual.

In all three cases, the vendor must determine up front what elements and which of their parameters are to be checked.
Retrieve the corresponding parameter values, compute their checksums, and save the above-mentioned triples.

The most generic approach might be the following:

- Prompt user to select element
- Query whether to use type or instance parameters
- List all the element instance or type poarameters and their values with check boxes
- User check all paramters to define ground truth
- Repeat until happy
- Read all selected parameter values and store as ground truth with element and parameter ids

Keep in mind that many documents may be open and each one has its own ground truth.
So, we need to keep track of separate ground truth data for each open document.

## Storage

The ground truth data triples containing the data required for integrity validation needs to be stored somewhere. That could be hard-wired directly into the add-in code for a specific BIM, stored in an external text file, within the `RVT` document, or elsewhere; it may be `JSON` formatted; it may be encrypted; still to be decided.

Two options are available for storing custom data directly within the `RVT` project file: shared parameters and extensible storage.
The latter is more modern and explicitly tailored for use by applications and data that is not accessible to the end user or even Revit itself.
That seems most suitable for our purpose here.
Extensible storage can be added to any database element.
However, it interferes least with Revit operation when placed on a dedicated `DataStorage` element,
especially [in a worksharing environment](http://thebuildingcoder.typepad.com/blog/2015/02/extensible-storage-in-a-worksharing-environment.html).
Creation and population of a `DataStorage` element is demonstrated by the [named GUID storage for project identification](https://thebuildingcoder.typepad.com/blog/2016/04/named-guid-storage-for-project-identification.html) sample.

### Extensible Storage Options

Two obvious choices for storing the ground truth in extensible storage:

- Store a separate `Entity` containing ground truth for each BIM `Element` on thr `Element` itself.
  In that case, the ground truth no longer consists of triples, since the element id is already known.
- Store one single global collection of ground truth triples in a custom `DataStorage` element.

References:

- [Extensible Storage TBC topic group](https://thebuildingcoder.typepad.com/blog/about-the-author.html#5.23)
- [Add-ins in a worksharing environment](https://thebuildingcoder.typepad.com/blog/2014/10/worksharing-and-duplicating-element-geometry.html#2)
- [Extensible storage in a worksharing environment](https://thebuildingcoder.typepad.com/blog/2015/02/extensible-storage-in-a-worksharing-environment.html)
- [Named Guid storage for project identification](https://thebuildingcoder.typepad.com/blog/2016/04/named-guid-storage-for-project-identification.html)
- [Storing a dictionary &ndash; use `DataStorage`, not `ProjectInfo`](https://thebuildingcoder.typepad.com/blog/2016/11/1500-posts-devday-and-storing-a-dictionary.html#5)

## User Interface

Currently, the add-in implements two commands: `CmdGroundTruth` and `Command`.
The former is only used once to initialise the ground truth data for a given model.
The latter can be used for testing purposes.
However, it may be replaced by an automated system to launch it on opening and saving a document.
Hence, there is no great need to implement a UI.
Otherwise, maybe, a ribbon tab with buttons to launch each command might be suitable.

## Todo

- Refactor validation command `CmdValidation` into a separate method that can be
  executecd automatically from `DocumentOpened`, `DocumentSaving`, and DMU
- Refactor the entire add-in
  to [prepare for DA4R](https://thebuildingcoder.typepad.com/blog/about-the-author.html#5.55)
- Test on real-world model
- Implement detailed and user friendly log file of validation errors
- Implement event handlers for document opened and saving
- Implement automatic execution on document opened and saving
- Implement [DMU dynamic model updater](https://thebuildingcoder.typepad.com/blog/about-the-author.html#5.31) to
  prevent modification of the protected parameter values
- Implement [extensible storage](https://thebuildingcoder.typepad.com/blog/about-the-author.html#5.23) of ground truth
- Implement [end user settings](https://thebuildingcoder.typepad.com/blog/2016/09/hololens-escape-path-waypoint-json-exporter.html) to
  choose validation strategy: command / opening and closing events / DMU
- Migrate to Forge Design Automation for Revit 

## Obsolete Original Plan

Proposal:

- Read the protected parameter values
- Export the parameters and their values to a file `.txt`
- Use the SHA256 or MD5 algorithm to compute the hash of each property value and store them in the file
- Subscribe to `DocumentClosing` event
- Compare the checksums

We can use the [RevitPythonShell](https://github.com/architecture-building-systems/revitpythonshell) or `RPS` to analyse the `RVT` and export the element and parameter data of interest.
Maybe restrict to one single family, or only some types, or only some params, but that can come later.
Compute the checksum for each parameter value separately to enable reporting which element and which property has been modified, if any.
Optionally encrypt the entire file.
Use RPS only to export the list of parameters and values.

For each value, store:

- ElementId
- Parameter `Definition` ElementId
- Parameter value [AsValueString](https://www.revitapidocs.com/2022/5015755d-ee80-9d74-68d9-55effc60ed0c.htm) (not really needed)

I would implement the checksum computation and later the optional encryption in C#, not Python.

The C# add-in skeleton already implements an external app + external command.

We can implement the command to read the text file listing element and parameter ids; for each pair, open the element and its parameter, determine the value (should match the text file) and calculate the checksum. Replace the parameter value in the txt file by its checksum.
That becomes the [ground truth](https://en.wikipedia.org/wiki/Ground_truth) file that is referenced later, containing just a list of three numbers for each property to check:

- ElementId
- Parameter `Definition` ElementId
- Parameter value checksum
 
The final real-life command will read and decrypt the external ground truth file, run the code to calculate the current param value checksums, compare with the ground truth, log all discrepancies and report the result.
 
Once it works, we can trigger the validation command automatically via an event instead of manually via an external command, e.g., using the [DocumentOpening](https://www.revitapidocs.com/2022/99a0bcc4-fede-b66b-198d-a53f46ecf149.htm) and
[DocumentClosing](https://www.revitapidocs.com/2022/2f0a7a6f-ed8b-0518-c5f8-edb14b321296.htm) events.

### Obsolete Early Notes

- [Checksum](https://en.wikipedia.org/wiki/Checksum) + encryption
- [.NET MD5 Class](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-6.0)
- [forge.wpf-csharp](https://github.com/Autodesk-Forge/forge.wpf.csharp/tree/secure-dev)
- For more security and harder hacking, you can grab the values, calculate the checksum, aka signature, rotate a couple of bytes, breaking the signature, and implement the algorithm to know how to restore the proper order before decrypting, 
- Original sample model: <i>Z:/Users/jta/a/special/gypsum/test/british-gypsum-bim-a206a167-en.rvt</i>
- Revit 2022 sample: <i>/Users/jta/a/special/gypsum/test/british-gypsum-bim-a206a167-en_2022.rvt</i>

## Authors

Carol Gitonga and 
Jeremy Tammik,
[The Building Coder](http://thebuildingcoder.typepad.com),
[Forge](http://forge.autodesk.com) [Platform](https://developer.autodesk.com) Development,
[ADN](http://www.autodesk.com/adn)
[Open](http://www.autodesk.com/adnopen),
[Autodesk Inc.](http://www.autodesk.com)

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.
