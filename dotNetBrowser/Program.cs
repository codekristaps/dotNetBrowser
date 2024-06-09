using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Simple Console Browser");
            Console.WriteLine("======================");
            Console.WriteLine("Enter the URL you want to browse (or type 'exit' to quit):");

            string url = Console.ReadLine();

            if (url.ToLower() == "exit")
                break;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine("Invalid URL. Please enter a valid URL.");
                continue;
            }

            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(responseBody);

                string textContent = ExtractText(htmlDoc.DocumentNode);
                DisplayContent(textContent, htmlDoc, new Uri(url));

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
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

    static void DisplayContent(string textContent, HtmlDocument htmlDoc, Uri baseUrl)
    {
        var lines = textContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int totalLines = lines.Length;
        int currentLine = 0;
        const int linesPerPage = 20;
        var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        int selectedLink = -1;

        while (true)
        {
            Console.Clear();
            for (int i = currentLine; i < currentLine + linesPerPage && i < totalLines; i++)
            {
                Console.WriteLine(lines[i]);
            }

            Console.WriteLine("\n======================");
            Console.WriteLine("Navigation Menu:");
            if (links != null)
            {
                int linkCount = 0;
                foreach (var link in links)
                {
                    linkCount++;
                    Console.WriteLine($"{linkCount}. {link.InnerText} - {link.Attributes["href"].Value}");
                    if (linkCount >= 5)
                    {
                        Console.WriteLine("... and more");
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("No links found on this page.");
            }

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.UpArrow)
            {
                if (currentLine > 0)
                    currentLine--;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (currentLine + linesPerPage < totalLines)
                    currentLine++;
            }
            else if (key == ConsoleKey.Escape)
            {
                break;
            }
            else if (key >= ConsoleKey.D1 && key <= ConsoleKey.D5)
            {
                int linkIndex = (int)key - (int)ConsoleKey.D1;
                if (linkIndex < links?.Count)
                {
                    selectedLink = linkIndex;
                    break;
                }
            }
        }

        if (selectedLink != -1 && links != null)
        {
            string selectedUrl = links[selectedLink].Attributes["href"].Value;
            if (!Uri.IsWellFormedUriString(selectedUrl, UriKind.Absolute))
            {
                selectedUrl = new Uri(baseUrl, selectedUrl).ToString();
            }

            DisplayLinkContent(selectedUrl);
        }
    }

    static async void DisplayLinkContent(string url)
    {
        try
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseBody);

            string textContent = ExtractText(htmlDoc.DocumentNode);

            DisplayContent(textContent, htmlDoc, new Uri(url));
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}
