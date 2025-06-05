using System;
using System.Net;
using System.Text;
using System.Threading;

namespace CNCEmu
{
    public class WebServer
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private bool _running = false;

        public WebServer(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listenerThread = new Thread(ListenLoop);
        }

        public void Start()
        {
            _running = true;
            _listener.Start();
            _listenerThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
            _listenerThread.Join();
        }

        private void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception)
                {
                    // Log or handle error
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string responseString = "<html><body>Webserver is running!</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}
