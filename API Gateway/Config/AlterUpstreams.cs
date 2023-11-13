using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;

namespace API_Gateway.Config
{
    public class AlterUpstreams
    {
        public static string AlterUpstreamSwaggerJson(HttpContext context, string swaggerJson)
        {
            var swagger = JObject.Parse(swaggerJson);
            return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}
