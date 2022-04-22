﻿using Autodesk.Revit.DB;
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

          
          schemaBuilder.AddSimpleField("GroundTruth", typeof(string));

          

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
      out string data,
      bool create = true)
    {
      bool rc = false;

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


            data = Util.GroundTruthData(doc);
            entity.Set("GroundTruth", data);




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

          data = entity.Get<string>("GroundTruth");

         

          rc = true;
        }
      }
      return rc;
    }
  }
}
