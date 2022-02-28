# RvtLock3r

Revit .NET C# add-in to validate that certain BIM element properties have not been modified.

- [Checksum](https://en.wikipedia.org/wiki/Checksum) + encryption
- [.NET MD5 Class](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-6.0)
- [forge.wpf-csharp](https://github.com/Autodesk-Forge/forge.wpf.csharp/tree/secure-dev)
- For more security and harder hacking, you can grab the values, calculate the checksum, aka signature, rotate a couple of bytes, breaking the signature, and implement the algorithm to know how to restore the proper order before decrypting, 
- Sample model: <i>Z:\Users\jta\a\special\gypsum\test\british-gypsum-bim-a206a167-en.rvt</i>
- Revit 2022 sample: <i>\Users\jta\a\special\gypsum\test\british-gypsum-bim-a206a167-en_2022.rvt</i>

## Plan

Proposal:

- Get all the shared parameters of the element with their values
- Export the shared parameters and their values to a file `.txt`
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
