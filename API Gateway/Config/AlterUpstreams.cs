using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace API_Gateway.Config
{
    public class AlterUpstreams
    {
        public static string AlterUpstreamSwaggerJson(HttpContext context,string swaggerJson)
        {
            var swagger = JObject.Parse(swaggerJson);
            return swagger.ToString(Formatting.Indented);
        }
    }
}
