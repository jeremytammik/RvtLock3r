using System.Collections.Generic;

namespace RvtLock3r
{
  /// <summary>
  /// Manage ground truth for all open documents
  /// </summary>
  class GroundTruthLookup : Dictionary<string,GroundTruth>
  {
    private static GroundTruthLookup _instance = new GroundTruthLookup();

    /// <summary>
    /// Hidden private singleton constructor
    /// </summary>
    private GroundTruthLookup() { }

    /// <summary>
    /// Singleton public access
    /// </summary>
    public static GroundTruthLookup Singleton
    {
      get
      {
        return _instance;
      }
    }
  }
}
