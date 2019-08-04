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
        private Config config;
        private CookieContainer cookieContainer = new CookieContainer();

        public Session(Config config)
        {
            this.config = config;
        }
        public async Task Login()
        {
            HttpWebRequest request = WebRequest.Create($"http://{config.address}/") as HttpWebRequest;
            request.CookieContainer = cookieContainer;
            WebResponse response = await request.GetResponseAsync();
            string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

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
                isDestroying = true,
                isObserverable = true
            });
            if(res["errorCategory"].ToObject<string>() != "ok")
            {
                Console.WriteLine(res);
            }
        }



        internal async Task<JToken> getApi(string api)
        {
            HttpWebRequest request = WebRequest.Create($"http://{config.address}/" + api) as HttpWebRequest;
            request.CookieContainer = cookieContainer;
            try
            {
                WebResponse response = await request.GetResponseAsync();
                string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                data = data.Substring(data.IndexOf("/*") + 2);
                data = data.Substring(0, data.LastIndexOf("*/"));
                return JToken.Parse(data);
            }catch(WebException e)
            {
                Console.WriteLine("HG659\tsession 404 on API, logging in again, " + e);
                await Login();
                await Task.Delay(1000);
                return await getApi(api);
            }
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
            try
            {
                HttpWebRequest request = WebRequest.Create($"http://{config.address}/" + api) as HttpWebRequest;
                request.CookieContainer = cookieContainer;
                request.Method = "post";
                var req = await request.GetRequestStreamAsync();
                await req.WriteAsync(Encoding.UTF8.GetBytes(JObject.FromObject(realPostData).ToString()));

                WebResponse response = await request.GetResponseAsync();
                string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                data = data.Substring(data.IndexOf("/*") + 2);
                data = data.Substring(0, data.LastIndexOf("*/"));
                return JToken.Parse(data);
            }
            catch (WebException e)
            {
                Console.WriteLine("HG659\tsession 404 on API, logging in again, " + e);
                await Login();
                await Task.Delay(1000);
                return await getApi(api);
            }
        }
    }
}
