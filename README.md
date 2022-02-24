# RvtLock3r

Validate that certain properties have not been modified.

- [Checksum](https://en.wikipedia.org/wiki/Checksum) + encryption
- [.NET MD5 Class](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-6.0)
- [forge.wpf-csharp](https://github.com/Autodesk-Forge/forge.wpf.csharp/tree/secure-dev)
- For more security and harder hacking, you can grab the values, calculate the checksum, aka signature, rotate a couple of bytes, breaking the signature, and implement the algorithm to know how to restore the proper order before decrypting, 
