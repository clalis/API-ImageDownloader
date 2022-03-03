// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace ImageDownloader
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static IEnumerable<XElement> posts = new List<XElement>();

        static void ShowResult(string result)
        {
            XElement results = XElement.Parse(result);
            Console.WriteLine($"{results.Attribute("count")} {results.Attribute("offset")}");
            posts =
             from fileUrl in results.Elements()
             select fileUrl;
            foreach (var post in posts)
            {
                Console.WriteLine($"{post.Attribute("file_url").Value}");
            }
        }

        static async Task<string> GetResultAsync(string path)
        {
            string result = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        static async Task DownloadAsync()
        {
            foreach (var post in posts)
            {
                var requestUri = new Uri(post.Attribute("file_url").Value);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var fileStream = new FileStream($"./out/{post.Attribute("id").Value}{Path.GetExtension(requestUri.ToString())}", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            Console.WriteLine("All done.");
        }

        static void Main()
        {
            Console.WriteLine("Hello, World!");
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://api.rule34.xxx/index.php");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                string result;
                var query = "?page=dapi&s=post&q=index&limit=100&pid=2&tags=futanari+size_difference+stomach_bulge+sort%3aupdated%3aasc";

                result = await GetResultAsync(query);
                ShowResult(result);

                DownloadAsync();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
