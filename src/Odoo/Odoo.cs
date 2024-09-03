using RestSharp;
using Flurl;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using System;

namespace Odoo
{
    public class Credential : ICloneable
    {
        public required int uid { get; set; }
        public required string database { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string uri { get; set; }

        public object Clone()
        {
            {
                return this.MemberwiseClone();
            }
        }
    }

    public class JsonRPC
    {
        private static Credential cred { get; set; }

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
                cred.uid,
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
                jsonrpc = "2.0",
                method = "call",
                id = 1, // TODO: do random id here
                Params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = param,
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
            int offset = 0
        )
        {
            const string method = "search_read";
            Object kwargs = new
            {
                limit = limit,
                offset = offset,
                fields = fields,
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