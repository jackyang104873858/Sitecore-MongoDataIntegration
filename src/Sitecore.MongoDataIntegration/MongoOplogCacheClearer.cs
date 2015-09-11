using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Diagnostics;

namespace Sitecore.MongoDataIntegration
{
    public class MongoOplogCacheClearer
    {
        private const string LOCAL = "local";
        private const string OPLOG = "oplog.$main"; //could be different depending on replica configuration

        private readonly IList<Database> _databases;
        private readonly string _connectionString;
        private readonly string _mongoDatabase;
        private readonly string _mongoCollection;
        private readonly string _mappingCollection;

        public MongoOplogCacheClearer(string connectionString, string mongoDatabase, string mongoCollection, string mappingCollection)
        {
            _connectionString = connectionString;
            _mongoDatabase = mongoDatabase;
            _mongoCollection = mongoCollection;
            _mappingCollection = mappingCollection;
            _databases = new List<Database>();
        }

        public void AddDatabase(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            Sitecore.Diagnostics.Log.Info(string.Format("MongoOplogCacheClearer: Monitoring {0}", database.Name), this);
            _databases.Add(database);
        }

        public void Start()
        {
            try
            {
                Sitecore.Diagnostics.Log.Info("MongoOplogCacheClearer: Starting", this);
                var client = new MongoClient(_connectionString);
                var server = client.GetServer();
                var mappingDatabase = server.GetDatabase(_mongoDatabase);
                var mappingCollection = mappingDatabase.GetCollection(_mappingCollection);

                //run a query against the oplog with a tailable cursor, which will block until new records are created
                var mongoDatabase = server.GetDatabase(LOCAL);
                var opLog = mongoDatabase.GetCollection(OPLOG);
                var queryCollection = string.Format("{0}.{1}", _mongoDatabase, _mongoCollection);
                var queryDoc = new QueryDocument("ns", queryCollection);
                var query = opLog.Find(queryDoc)
                                .SetFlags(QueryFlags.AwaitData | QueryFlags.NoCursorTimeout | QueryFlags.TailableCursor);
                var cursor = new MongoCursorEnumerator<BsonDocument>(query);
                while (true)
                {
                    if (cursor.MoveNext())
                    {
                        var document = cursor.Current;
                        if (document["op"].AsString == "d")
                        {
                            //get the deleted document
                            document = document["o"].AsBsonDocument;

                            //TODO: on delete, delete from the mapping collection as well?
                        }
                        else if (document["op"].AsString == "u")
                        {
                            //get the updated document
                            document = document["o2"].AsBsonDocument;
                        }
                        else
                        {
                            continue;
                        }
                        var objectId = document["_id"].AsObjectId;

                        //look in our mapping table for the associated sitecore item id
                        var mapping = mappingCollection.FindOne(new QueryDocument("mongoId", objectId.ToString()));
                        if (mapping == null)
                        {
                            continue;
                        }
                        var id = Sitecore.Data.ShortID.Parse(mapping["sitecoreId"].AsString).ToID();

                        //clear caches for all configured databases, simulate item save
                        foreach (var database in _databases)
                        {
                            var item = database.GetItem(id);
                            if (item == null)
                            {
                                continue;
                            }
                            Sitecore.Diagnostics.Log.Info(
                                string.Format("MongoOplogCacheClearer: Product {0}://{1} updated", database.Name, id), this);
                            database.Caches.ItemCache.RemoveItem(id);
                            database.Caches.DataCache.RemoveItemInformation(id);
                            database.Engines.DataEngine.RaiseSavedItem(item, true);
                            var args = new ItemSavedEventArgs(item);
                            Sitecore.Events.Event.RaiseItemSaved(this, args);
                            //TODO: Something to cause reindex in web, new indexing strategy?
                            //TODO: Clear HTML cache?
                        }
                    }
                    else if (cursor.IsDead)
                    {
                        //TODO: restart mechanism?
                        Sitecore.Diagnostics.Log.Info("MongoOplogCacheClearer: Dead", this);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Sitecore.Diagnostics.Log.Error("Exception starting MongoDb oplog monitoring", e, this);
            }
        }
    }
}