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
        }
    }
}