using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MongoDB.Driver;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.StringExtensions;

namespace Sitecore.MongoDataIntegration
{
    public class MongoDataProvider : DataProvider
    {
        private static volatile MongoOplogCacheClearer _cacheClearer;
        private static readonly object _lockObj = new object();

        private bool _initialized;
        private string _connectionString;
        private string _database;
        
        protected HashSet<ID> Templates = new HashSet<ID>();

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                _database = MongoUrl.Create(value).DatabaseName;
            }
        }

        public string MongoDatabase
        {
            get { return _database; }
        }

        public string Collection { get; set; }

        public string MappingCollection { get; set; }

        public void AddTemplate(string template)
        {
            if (!ID.IsID(template))
            {
                return;
            }
            Templates.Add(ID.Parse(template));
        }

        public override ItemDefinition GetItemDefinition(ID itemId, Sitecore.Data.DataProviders.CallContext context)
        {
            if (_cacheClearer == null)
            {
                lock (_lockObj)
                {
                    if (_cacheClearer == null)
                    {
                        _cacheClearer = new MongoOplogCacheClearer(this.ConnectionString, this.MongoDatabase, this.Collection, this.MappingCollection);
                        new Thread(_cacheClearer.Start).Start();
                    }
                }
            }
            if (!_initialized)
            {
                lock (_lockObj)
                {
                    if (!_initialized)
                    {
                        _cacheClearer.AddDatabase(this.Database);
                        _initialized = true;
                    }
                }
            }
            return null;
        }

        public override FieldList GetItemFields(ItemDefinition itemDefinition, VersionUri versionUri, Sitecore.Data.DataProviders.CallContext context)
        {
            var fields = new FieldList();

            if (!Templates.Contains(itemDefinition.TemplateID))
            {
                return null;
            }

            //important for this data provider to be chained after the SQL provider because of this
            var currentFields = context.CurrentResult as FieldList;
            var productCode = currentFields != null ? currentFields[FieldIds.Product.ExternalId] : null;
            if (productCode.IsNullOrEmpty())
            {
                return null;
            }

            var client = new MongoClient(ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(MongoDatabase);
            var collection = database.GetCollection(Collection);

            //look for this product in the mongo collection
            var query = new QueryDocument("id", productCode);
            var book = collection.FindOne(query);
            if (book == null)
            {
                fields.Add(FieldIds.Product.Hidden, "1");
                //TODO: return empty values for other fields
                return fields;
            }

            //map fields
            //TODO: more dynamic field mapping, via config?
            //TODO: field mapping differences by template

            //base product fields
            fields.Add(FieldIds.Product.Sku, productCode);
            fields.Add(FieldIds.Product.Title, book["title"].AsString);
            fields.Add(FieldIds.Product.Description, book["description"].AsString);
            fields.Add(FieldIds.Product.Weight, book["weight"].AsString);
            fields.Add(FieldIds.Product.Price, book["price"].AsString);

            //book fields
            fields.Add(FieldIds.BookProduct.Author, book["author"].AsString);
            fields.Add(FieldIds.BookProduct.Genre, book["genre"].AsString);
            var dateValue = DateTime.ParseExact(book["publishDate"].AsString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            fields.Add(FieldIds.BookProduct.PublishDate, Sitecore.DateUtil.ToIsoDate(dateValue));
            return fields;
        }
    }
}