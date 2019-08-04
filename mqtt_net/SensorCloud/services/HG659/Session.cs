using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SensorCloud.services.HG659
{
    public class Session
    {

        string csrf_param;
        string csrf_token;
        string cookie;
        private Config config;

        public Session(Config config)
        {
            this.config = config;
        }
        public async Task Login()
        {
            WebRequest request = WebRequest.Create($"http://{config.address}/");
            WebResponse response = await request.GetResponseAsync();
            string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

            cookie = response.Headers["Set-Cookie"];

            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(data);
            csrf_param = pageDocument.DocumentNode.SelectSingleNode("(//meta[contains(@name,'csrf_param')])").GetAttributeValue("content", "default");
            csrf_token = pageDocument.DocumentNode.SelectSingleNode("(//meta[contains(@name,'csrf_token')])").GetAttributeValue("content", "default");
            string password = Util.Sha256(config.user + Util.Base64Encode(Util.Sha256(config.password)) + csrf_param + csrf_token);
            var res = await postApi("api/system/user_login", new
            {
                UserName = config.user,
                Password = password,
                isInstance = true,
                isDestroyed = false,
                isDestroying = false,
                isObserverable = true
            });
            Console.WriteLine($"Logged in, cookie {cookie}");
        }



        internal async Task<JToken> getApi(string api)
        {
            WebRequest request = WebRequest.Create($"http://{config.address}/" + api);
            request.Headers["Cookie"] = cookie;
            WebResponse response = await request.GetResponseAsync();
            string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            data = data.Substring(data.IndexOf("/*") + 2);
            data = data.Substring(0, data.LastIndexOf("*/"));
            if (response.Headers["Set-Cookie"] != null)
                cookie = response.Headers["Set-Cookie"];
            return JToken.Parse(data);
        }

        internal async Task<JToken> postApi(string api, object postData)
        {
            object realPostData = new
            {
                csrf = new
                {
                    csrf_param = csrf_param,
                    csrf_token = csrf_token
                },
                data = postData
            };
            WebRequest request = WebRequest.Create($"http://{config.address}/" + api);
            request.Headers["Cookie"] = cookie;
            request.Method = "post";
            var req = await request.GetRequestStreamAsync();
            await req.WriteAsync(Encoding.UTF8.GetBytes(JObject.FromObject(realPostData).ToString()));

            WebResponse response = await request.GetResponseAsync();
            string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            data = data.Substring(data.IndexOf("/*") + 2);
            data = data.Substring(0, data.LastIndexOf("*/"));
            if (response.Headers["Set-Cookie"] != null)
                cookie = response.Headers["Set-Cookie"];
            return JToken.Parse(data);
        }
    }
}
