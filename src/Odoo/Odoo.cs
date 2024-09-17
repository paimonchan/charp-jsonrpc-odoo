using RestSharp;
using Flurl;
using System.Text.Json.Nodes;

namespace Odoo
{
    public class Credential : ICloneable
    {
        public required int userid      { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string database { get; set; }
        public required string uri      { get; set; }

        public object Clone()
        {
            {
                return this.MemberwiseClone();
            }
        }
    }

    public class JsonRPC
    {
        private static Credential cred { get; set; } = new Credential
        {
            userid              = 0,
            username            = "",
            password            = "",
            database            = "",
            uri                 = "",
        };

        private static Object[] ConstructParams(string method, string model, Object[]? args = null, Object? kwargs = null)
        {
            if (kwargs == null)
            {
                kwargs = new { };
            }
            if (args == null)
            {
                args = new Object[] { };
            }

            Object[] param = new Object[]
            {
                cred.database,
                cred.userid,
                cred.password,
                model,
                method,
                args,
                kwargs,
            };

            return param;
        }

        private static Object ConstructBody(string method, string model, Object[]? args, Object? kwargs = null)
        {
            Object[] param = ConstructParams(method, model, args, kwargs);

            var body = new
            {
                jsonrpc         = "2.0",
                method          = "call",
                id              = 1, // TODO: do random id here
                Params          = new
                {
                    service     = "object",
                    method      = "execute_kw",
                    args        = param,
                }
            };

            return body;
        }

        private static JsonNode? Post(string method, string model, Object[]? args = null, Object? kwargs = null)
        {
            string url = Url.Combine(cred.uri, "jsonrpc");
            var body = ConstructBody(method, model, args, kwargs);

            var client = new RestClient(url);
            var request = new RestRequest();

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(body);

            var response = client.Post(request);
            if (response != null && response.Content != null)
            {
                var content = response.Content;
                var resString = content.ToString();
                var res = System.Text.Json.JsonSerializer.Deserialize<JsonNode>(resString);
                return res;
            }

            return null;
        }

        public static void Configure(Credential incomingCred)
        {
            cred = (Credential) incomingCred.Clone();
        }

        public static JsonNode? Read(
            string model,
            Object[]? domain = null,
            String[]? fields = null,
            int limit = 0,
            int offset = 0,
            Object? context = null
        )
        {
            const string method = "search_read";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                limit           = limit,
                offset          = offset,
                fields          = fields,
                context         = context,
            };

            Object[]? args = null;
            if (domain != null)
            {
                args = new object[] { domain };
            }

            return Post(method, model, args, kwargs);
        }

        public static JsonNode? Create(
            string model,
            Object[] vals,
            Object? context = null
        )
        {
            const string method = "create";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                context         = context
            };
            Object[] args = new Object[] { vals };

            return Post(method, model, args, kwargs);
        }

        public static JsonNode? Update(
            string model,
            int[] recordIds,
            Object val,
            Object? context = null
        )
        {
            const string method = "write";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                context = context
            };
            Object[] args = new Object[] { recordIds, val };

            return Post(method, model, args, kwargs);
        }

        public static JsonNode? Delete(
            string model,
            int[] recordIds,
            Object? context = null
        )
        {
            const string method = "unlink";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                context = context
            };
            Object[] args = new Object[] { recordIds };

            return Post(method, model, args, kwargs);
        }

        public static JsonNode? Model()
        {
            const string model = "ir.model";

            string[] fields = new string[] { 
                "id",
                "name",
                "model"
            };
            Object[] domain = new object[] {
                new Object[] {"transient", "=", false}

            };

            return Read(model, domain, fields);
        }

        public static JsonNode? Field(string modelRef)
        {
            const string model = "ir.model.fields";

            string[] fields = new string[] {
                "id",
                "name",
                "model",
                "ttype",
                "selection_ids",
                "relation_table",
            };
            Object[] domain = new object[] {
                new Object[] {"model", "=", modelRef}
            };

            var res = Read(model, domain, fields).AsObject();

            var result = res["result"];
            foreach (var field in result.AsArray())
            {
                if (field["selection_ids"].AsArray().Count() == 0)
                {
                    continue;
                }
                var fieldIds = new List<int>() { (int)field["id"] };
                var selections = Selection(fieldIds);
                field["selection_ids"] = selections["result"].DeepClone();
            }

            return result;
        }

        public static JsonNode? Selection(List<int> fieldIds)
        {
            const string model = "ir.model.fields.selection";

            string[] fields = new string[] {
                "id",
                "name",
                "sequence",
                "value",
            };
            Object[] domain = new object[] {
                new Object[] {"field_id", "in", fieldIds}
            };

            return Read(model, domain, fields);
        }

        public static JsonNode? Count(
            string model,
            Object[]? domain = null,
            Object? context = null
        )
        {
            const string method = "search_count";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                context = context,
            };

            Object[]? args = null;
            if (domain != null)
            {
                args = new object[] { domain };
            }

            return Post(method, model, args, kwargs);
        }

        public static JsonNode? Group(
            string model,
            Object[]? domain = null,
            String[] groupby = null,
            String[] fields = null,
            int limit = 0,
            int offset = 0,
            bool lazy = true,
            Object? context = null
        )
        {
            const string method = "read_group";

            if (context == null)
            {
                context = new { };
            }

            Object kwargs = new
            {
                lazy = lazy,
                limit = limit,
                offset = offset,
                fields = fields,
                groupby = groupby,
                context = context,
            };

            Object[]? args = null;
            if (domain != null)
            {
                args = new object[] { domain };
            }

            return Post(method, model, args, kwargs);
        }
    }
}