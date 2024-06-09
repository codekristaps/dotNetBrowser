using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter the URL you want to browse:");
        string url = Console.ReadLine();

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Console.WriteLine("Invalid URL. Please enter a valid URL.");
            return;
        }

        try
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseBody);

            var textContent = ExtractText(htmlDoc.DocumentNode);
            Console.WriteLine("\nPage Content:\n");
            Console.WriteLine(textContent);
            Console.ReadLine();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    static string ExtractText(HtmlNode node)
    {
        if (node == null)
            return string.Empty;

        if (node.NodeType == HtmlNodeType.Text)
            return node.InnerText;

        if (node.Name == "script" || node.Name == "style")
            return string.Empty;

        string result = string.Empty;

        foreach (var child in node.ChildNodes)
        {
            result += ExtractText(child);
        }

        return result;
    }
}
