using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Sitecore.MongoDataIntegration
{
    public static class FieldIds
    {
        public static class Product
        {
            public static ID Sku = ID.Parse("{425E8BC7-86DE-48C6-BF19-4E3AA678550A}");
            public static ID Title = ID.Parse("{C3904857-7303-4D2F-A98F-45E014945CD6}");
            public static ID Description = ID.Parse("{233692D1-A886-4485-9406-358FF2C63610}");
            public static ID Weight = ID.Parse("{BDB71C89-2A61-4659-B9A9-77195C573ADF}");
            public static ID Price = ID.Parse("{6C2D4FAE-3DE8-4C38-B6B5-73E5CB5EE529}");
            public static ID Hidden = ID.Parse("{D83A7616-69D3-4D3A-901D-E85C030AD68C}");
            public static ID ExternalId = ID.Parse("{5F9B21F4-D857-4DB7-AD04-6BA851FF3431}");
        }

        public static class BookProduct
        {
            public static ID Author = ID.Parse("{BC4464BA-2BF1-4204-B1FB-9963841C8224}");
            public static ID Genre = ID.Parse("{5C866C23-7D6F-4307-9200-08E928CE7E9F}");
            public static ID PublishDate = ID.Parse("{2AB8BFCA-7ADF-4BAB-B3D6-21F7F81893E2}");
        }

    }
}