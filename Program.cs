using BasicWebServer;
using BasicWebServer.DataLayer;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;

class Program
{
    const string HTML_ROUTE_DIRECTORY = "C:\\Users\\matth\\source\\repos\\BasicWebServer\\website\\html";
    const string ROUTE_DIRECTORY = "C:\\Users\\matth\\source\\repos\\BasicWebServer\\website";

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
        int port = 3000; // Port to listen on.

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

                    var requestLines = ReadRequestLines(reader);

                    HttpRequest request = HttpRequest.ReadRequestIntoClass(requestLines.ToArray());
                    
                    // process and respond
                    if (request != null)
                    {
                        ProcessRequestAndRespond(request, routes, stream);

                    }
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
    static byte[] ProcessRoute(Route[] routes, string path, List<string> accepts)
    {
        string html;
        Route route = routes.FirstOrDefault(i => i.path == path);
        // if route can't be found, return error page with 400 status code
        string filePath = route != null ? route.relativeFilePath : "errorpage.html";
        int statusCode = route != null ? 200 : 400;
        html = ReadFileToString(filePath);
        return BuildResponse(html, statusCode, accepts[0]);
    }

    static byte[] ProcessRequestAndRespond(HttpRequest request, Route[] routes, NetworkStream stream)
    {
        // check if request is for a file
        if (request.Path == "/favicon.ico")
        {
            ImageFileResponse("favicon.ico", stream);
        }

        // check if file ends with .css
        if (request.Path.EndsWith(".css") || request.Path.EndsWith(".js"))
        {
            FileResponse(request.Path, stream);
        }

        // Respond with HTML.
        byte[] response = ProcessRoute(routes, request.Path, request.Accept);
        stream.Write(response, 0, response.Length);
        return response;
    }

    static byte[] BuildResponse(string html, int httpCode, string contentType)
    {
        string response = $"HTTP/1.1 {httpCode} OK\r\n" +
                                      $"Content-Type: {contentType}; charset=UTF-8\r\n" +
                                      $"Content-Length: {html.Length}\r\n" +
                                      "\r\n" +
                                      html;
        return Encoding.UTF8.GetBytes(response);
    }

    static byte[] ImageResponse(byte[] imageData, string imageType)
    {
        string base64ImageData = Convert.ToBase64String(imageData);
        string response = $"HTTP/1.1 200 OK\r\n" +
            $"Content-Type: {imageType}\r\n" +
            $"Content-Length: {imageData.Length}\r\n" +
            $"Accept-Ranges: bytes\r\n" +
            $"Cache-Control: max-age=604800\r\n" +
            $"Last-Modified: {DateTime.UtcNow.ToString("R")}\r\n" +
            base64ImageData;
            
        return Encoding.UTF8.GetBytes(response);
    }

    static string ReadFileToString(string filePath)
    {
        string fileLine = "";
        try
        {
            string path = Path.Join(HTML_ROUTE_DIRECTORY, filePath);
            //Pass the file path and file name to the StreamReader constructor
            StreamReader sr = new StreamReader(path);
            //Read the first line of text
            string line = sr.ReadLine();
            //Continue to read until you reach end of file
            while (line != null)
            {
                //write the line to console window
                fileLine += line;
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
        return fileLine;

    }

    static byte[] ReadFileIntoByteArray(string imagePath)
    {
        // Specifying a file 
        string path = Path.Join(ROUTE_DIRECTORY, imagePath);

        // Calling the ReadAllBytes() function 
        byte[] byteArray = File.ReadAllBytes(path);
        return byteArray;
    }

    static List<string> ReadRequestLines(StreamReader reader)
    {
        // get request lines
        // Read all request lines into a List<string>
        Console.WriteLine("Reading request lines");
        Console.WriteLine("==============================");
        List<string> requestLines = new List<string>();
        string line;
        while ((line = reader.ReadLine()) != null && line != "")
        {
            Console.WriteLine($"{line}");
            requestLines.Add(line);
        }
        Console.WriteLine("END OF REQUEST LINES...");
        Console.WriteLine("==============================");
        return requestLines;
    }

    public static void ImageFileResponse(string filePath, NetworkStream stream)
    {
        byte[] imageData = ReadFileIntoByteArray(filePath);
        string header = "HTTP/1.1 200 OK\r\n" +
                                "Content-Type: image/jpeg\r\n" +
                                $"Content-Length: {imageData.Length}\r\n" +
                                "Connection: close\r\n\r\n";
        byte[] headerData = Encoding.ASCII.GetBytes(header);
        stream.Write(headerData, 0, headerData.Length);
        stream.Write(imageData, 0, imageData.Length);

        stream.Flush();

    }

    public static void FileResponse(string filePath, NetworkStream stream)
    {
        string fileExtension = filePath.Split(".").LastOrDefault();
        if (fileExtension == null)
        {
            throw new Exception("Couldn't parse file extension from file path");
        }
        byte[] fileData = ReadFileIntoByteArray(filePath);
        string header = "HTTP/1.1 200 OK\r\n" +
                                $"Content-Type: text/{fileExtension}\r\n" +
                                $"Content-Length: {fileData.Length}\r\n" +
                                "Connection: close\r\n\r\n";
        byte[] headerData = Encoding.ASCII.GetBytes(header);
        stream.Write(headerData, 0, headerData.Length);
        stream.Write(fileData, 0, fileData.Length);
        stream.Flush();
    }
} 