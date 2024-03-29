using BasicWebServer;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

class Program
{
    const string htmlRouteDirectory = "C:\\Users\\matth\\source\\repos\\BasicWebServer\\website\\html";

    const string errorPageHtml = "<html><body><h1>404 Not Found</h1></body></html>";
    const string indexPageHtml = "<html><body><h1>Hello world</h1><p>This is the index page</p></body></html>";
    const string infoPageHtml = "<html><body><h1>Info Page</h1><p>This page will contain some info</p></body></html>";

    static void Main(string[] args)
    {
        Route[] routes = {
            new Route("/", "index.html"),
            new Route("/info", "infopage.html"),
        };
        // Define the IP Address and port to listen on.
        // IPAddress.Loopback is localhost. You might need to use a different IP address if connecting from other devices.
        IPAddress ipAddress = IPAddress.Loopback;
        int port = 8080; // Port to listen on.

        // Create and start TcpListener.
        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();
        Console.WriteLine($"Listening on {ipAddress}:{port}...");

        try
        {
            while(true)
            {
                // Accept TCP client
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                // read request stream from client
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
                {
                    Console.WriteLine($"Connected client");

                    // get request lines
                    // Read all request lines into a List<string>
                    List<string> requestLines = new List<string>();
                    string line;
                    while ((line = reader.ReadLine()) != null && line != "")
                    {
                        requestLines.Add(line);
                    }
                    // process and respond


                    string response = ProcessRequestAndRespond(requestLines, routes);
                                       
                    // convert response to bytes
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    // write response to stream
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    // close connetion
                    client.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            listener.Stop();
        }

        
    }
    static string GetHTMLByRoute(Route[] routes, string path)
    {
        /*switch(route)
        {
            case "/":
                return indexPageHtml;
            case "/info":
                return infoPageHtml;
            default:
                return errorPageHtml;
        }*/
        Route route = routes.FirstOrDefault(i => i.path == path);
        if (route == null)
        {
            return ReadHTMLDoc("errorpage.html");
        }
        else
        {
            return ReadHTMLDoc(route.relativeFilePath);
        }
    }

    static string ProcessRequestAndRespond(List<string> requestLines, Route[] routes)
    {
        string html;
        string response;
        if (requestLines.Count == 0)
        {
            html = "<html><body><h1>400 Bad Request</h1></body></html>";

            response = "HTTP/1.1 200 OK\r\n" +
                                      "Content-Type: text/html; charset=UTF-8\r\n" +
                                      $"Content-Length: {html.Length}\r\n" +
                                      "\r\n" +
                                      html;
            return response;
        }

        // get request type and path
        
        Console.WriteLine($"Request: {requestLines[0]}");
        // Extract the path from the request line
        string[] requestParts = requestLines[0]?.Split(' ') ?? new string[0];
        string path = requestParts.Length > 1 ? requestParts[1] : "/";
        // Respond with HTML.
        html = GetHTMLByRoute(routes, path);

        response = "HTTP/1.1 200 OK\r\n" +
                                      "Content-Type: text/html; charset=UTF-8\r\n" +
                                      $"Content-Length: {html.Length}\r\n" +
                                      "\r\n" +
                                      html;
        return response;
    }

    static string ReadHTMLDoc(string filePath)
    {
        string htmlString = "";
        try
        {
            string path = Path.Join(htmlRouteDirectory, filePath);
            //Pass the file path and file name to the StreamReader constructor
            StreamReader sr = new StreamReader(path);
            //Read the first line of text
            string line = sr.ReadLine();
            //Continue to read until you reach end of file
            while (line != null)
            {
                //write the line to console window
                htmlString += line;
                //Read the next line
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }
        return htmlString;

    }
}