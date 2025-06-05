using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNCEmu
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start built-in webserver
            var webServer = new WebServer("http://localhost:8080/");
            webServer.AddRoute("/status", req => "<html><body>Status: OK</body></html>");
            webServer.Start();

            Application.ApplicationExit += (s, e) => webServer.Stop();

            Application.Run(new Form1());

            // No need to call webServer.Stop() here, handled by ApplicationExit
        }
    }
}
