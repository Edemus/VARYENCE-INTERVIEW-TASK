using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Http;

namespace ConsoleApplication1
{
    class Program
    {
        private List<string> m_hosts;
        private static readonly HttpClient m_client = new HttpClient();
        private static Timer timer;

        private const string URL = "https://this-is-not-validlink.com/varyence-test";
        private const int PORT_1 = 1000;
        private const int PORT_2 = 3389;
        
        // Time in millisecond.
        // 300 000 - means 5 minutes.
        private const long TIME_INTERVAL = 300000;

        public Program()
        {
            m_hosts = new List<string>();
        }

        /**
         * This method browses in all Active Directory hierarchy for collecting hosts
         * with open 1000 and 3389 ports and writes result to m_hosts list.
         */
        public void CollectNetworkHosts()
        {
            DirectoryEntry root = new DirectoryEntry("WinNT:");

            // Browse all object in Active Directory hierarchy.
            foreach (DirectoryEntry computers in root.Children)
            {
                foreach (DirectoryEntry computer in computers.Children)
                {
                    if (computer.Name != "Schema" && computer.SchemaClassName == "Computer")
                    {
                        // Check if both required ports are opened.
                        if (IsPortOpen(computer.Name, PORT_1) && IsPortOpen(computer.Name, PORT_2))
                        {
                            m_hosts.Add(computer.Name);
                        }
                    }
                }
            }
        }

        /*
         * This method invoke CollectNetworkHosts() method and returns POST request to server with
         * valid hosts.
         */
        private void ProcessCollectedHosts()
        {
            CollectNetworkHosts();

            WebRequest request = WebRequest.Create(URL);
            request.Method = "POST";
            // Create byteArray from string where HostNames separated by coma.
            byte[] byteArray = Encoding.UTF8.GetBytes(string.Join(",", m_hosts.ToArray()));
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
        }

        /*
         * This methods checks is @port opened at given @host.
         * 
         *  @param host - string, hostname
         *  @param port - int, port number
         *  @return true if port opened, othervise returns false.
         */
        private bool IsPortOpen(string host, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // Parameters: host, port, requestCallback, state
                    var result = client.BeginConnect(host, port, null, null);
                    // 1000 - Timeout in millisecond, means 1 second.
                    var success = result.AsyncWaitHandle.WaitOne(10);
                    if (!success)
                    {
                        return false;
                    }
                    client.EndConnect(result);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /*
         * This method execute on timer event.
         */
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);

            Program obj = new Program();
            obj.ProcessCollectedHosts();
        }

        public static void Main(string[] args)
        {
            timer = new Timer();
            timer.Interval = TIME_INTERVAL;

            // Assign function wich would be called every 5 minutes. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            // Start the timer
            timer.Enabled = true;

            Console.ReadLine();
        }
    }
}
