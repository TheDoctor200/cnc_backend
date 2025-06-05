using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace CNCEmu
{
    public class WebServer
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private bool _running = false;
        private readonly Dictionary<string, Func<HttpListenerRequest, string>> _routes;

        public WebServer(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listenerThread = new Thread(ListenLoop);
            _routes = new Dictionary<string, Func<HttpListenerRequest, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "/", DefaultHandler }
                // Add more routes here as needed
            };
        }

        public void AddRoute(string path, Func<HttpListenerRequest, string> handler)
        {
            _routes[path] = handler;
        }

        public void Start()
        {
            _running = true;
            _listener.Start();
            _listenerThread.Start();
            Console.WriteLine($"[WebServer] Started at {string.Join(", ", _listener.Prefixes)}");
        }

        public void Stop()
        {
            _running = false;
            try
            {
                _listener.Stop();
            }
            catch { }
            if (Thread.CurrentThread != _listenerThread)
                _listenerThread.Join();
            Console.WriteLine("[WebServer] Stopped.");
        }

        private void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => ProcessRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WebServer] Exception: " + ex.Message);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string responseString;
            try
            {
                var path = context.Request.Url.AbsolutePath;
                if (_routes.TryGetValue(path, out var handler))
                {
                    responseString = handler(context.Request);
                }
                else
                {
                    responseString = NotFoundHandler(context.Request);
                }
            }
            catch (Exception ex)
            {
                responseString = $"<html><body>Internal Server Error: {ex.Message}</body></html>";
                context.Response.StatusCode = 500;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            try
            {
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch { }
            finally
            {
                context.Response.OutputStream.Close();
            }
            Console.WriteLine($"[WebServer] {context.Request.HttpMethod} {context.Request.Url.AbsolutePath} {context.Response.StatusCode}");
        }

        private string DefaultHandler(HttpListenerRequest req)
        {
            return "<html><body>Webserver is running!<br/>Try <a href=\"/status\">/status</a></body></html>";
        }

        private string NotFoundHandler(HttpListenerRequest req)
        {
            return "<html><body>404 Not Found</body></html>";
        }
    }
}
