using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtLock3r
{
  internal class NamedGroundTruthStorage
  {
    const string _ground_truth_name_version = "ground_truth_identifier_v0";

    //private static DataStorage dataStorage;

    /// <summary>
    /// The extensible storage schema, 
    /// containing one single Guid field.
    /// </summary>
    public static class NamedGroundTruthSchema
    {
      public readonly static Guid SchemaGuid = new Guid(
        "{5F374308-9C59-42AE-ACC3-A77EF45EC146}");

      /// <summary>
      /// Retrieve our extensible storage schema 
      /// or optionally create a new one if it does
      /// not yet exist.
      /// </summary>
      public static Schema GetSchema(
        bool create = true)
      {
        Schema schema = Schema.Lookup(SchemaGuid);

        if (create && null == schema)
        {
          SchemaBuilder schemaBuilder =
            new SchemaBuilder(SchemaGuid);

          schemaBuilder.SetSchemaName(
            "GroundTruthStorage");

          //schemaBuilder.AddSimpleField(
          //  "Guid", typeof(Guid));

          schemaBuilder.AddSimpleField("GroundTruth", typeof(string));

          //schemaBuilder.AddSimpleField(
          //  "GroundTruth", typeof(List<>));
          //schemaBuilder.AddArrayField("GroundTruth", typeof(GroundTruthTripples));
          //schemaBuilder.AddArrayField("GroundTruth", typeof(int));

          //schemaBuilder.AddMapField("GroundTruth", typeof(ElementId), typeof(Dictionary<Guid, string>));

          schema = schemaBuilder.Finish();
        }
        return schema;
      }
    }

    /// <summary>
    /// Retrieve an existing named ground truth 
    /// in the specified Revit document or
    /// optionally create and return a new
    /// one if it does not yet exist.
    /// </summary>
    public static bool Get(
      Document doc,
      //string name,
      //out Guid guid,
      //out IDictionary<ElementId, Dictionary<Guid, string>> gtTriples,
      out string data,
      bool create = true)
    {
      bool rc = false;

      //guid = Guid.Empty;
      //gtTriples = new Dictionary<ElementId, Dictionary<Guid, string>>();
      data = string.Empty;

      // Retrieve a DataStorage element with our
      // extensible storage entity attached to it
      // and the specified element name.

      ExtensibleStorageFilter f
        = new ExtensibleStorageFilter(
          NamedGroundTruthSchema.SchemaGuid);

      DataStorage dataStorage
        = new FilteredElementCollector(doc)
          .OfClass(typeof(DataStorage))
          .WherePasses(f)
          .Where<Element>(e => _ground_truth_name_version.Equals(e.Name))
          .FirstOrDefault<Element>() as DataStorage;

      if (dataStorage == null)
      {
        if (create)
        {
          using (Transaction t = new Transaction(
            doc, "Create named ground truth extensible storage"))
          {
            t.Start();

            // Create named data storage element

            dataStorage = DataStorage.Create(doc);
            dataStorage.Name = _ground_truth_name_version;

            // Create entity to store the Guid data

            Entity entity = new Entity(
              NamedGroundTruthSchema.GetSchema());

            //entity.Set("Guid", guid = Guid.NewGuid());

            data = Util.GroundTruthData(doc);
            entity.Set("GroundTruth", data);

            //IList<GroundTruthTripples> list = Util.GroundTruthListData(doc);
            //IList<int> list = new List<int>() { 111, 222, 333 };

            //entity.Set<IList<int>>("GroundTruth", list);
            //GroundTruth gt = new GroundTruth(doc);


            //IDictionary<ElementId, Dictionary<Guid, string>> dict = GroundTruth.GroundTruthObjectData(doc);
            //entity.Set<IDictionary<ElementId, Dictionary<Guid, string>>>("GroundTruth", dict);



            // Set entity to the data storage element

            dataStorage.SetEntity(entity);

            t.Commit();

            rc = true;
          }
        }
      }
      else
      {
        // Retrieve entity from the data storage element.

        Entity entity = dataStorage.GetEntity(
          NamedGroundTruthSchema.GetSchema(false));

        Debug.Assert(entity.IsValid(),
          "expected a valid extensible storage entity");

        if (entity.IsValid())
        {
          //guid = entity.Get<Guid>("Guid");

          data = entity.Get<string>("GroundTruth");

          ////gtTriples = entity.Get<List<GroundTruthTripples>>("GroundTruth");
          //IDictionary<ElementId, Dictionary<Guid, string>> dict = entity.Get<IDictionary<ElementId, Dictionary<Guid, string>>>("GroundTruth");
          //string info = "List data:\n";
          ////IDictionary<int, string> dict = ent.Get<IDictionary<int, string>>("FieldTest7");
          //foreach (var e in dict)
          //{
          //    info += string.Format("\t{0} : {1}\n", e.Key, e.Value);
          //}
          //string path = doc.PathName;
          //string filePath = path.Replace(".rte", ".lock4r");
          //if (File.Exists(filePath))
          //{
          //    File.Delete(filePath);
          //}
          //File.WriteAllText(filePath, info);

          rc = true;
        }
      }
      return rc;
    }
  }
}
