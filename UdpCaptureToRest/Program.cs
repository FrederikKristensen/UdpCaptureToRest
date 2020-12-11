using CoronaTestRest.Model;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpCaptureToRest
{
    class Program
    {
        private const string URI = "http://coronatest.azurewebsites.net/api/CoronaTests";
        private const int Port = 7064;

        static void Main()
        {
            using (UdpClient socket = new UdpClient(new IPEndPoint(IPAddress.Loopback, Port)))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(0, 0);
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast {0}", socket.Client.LocalEndPoint);
                    byte[] datagramReceived = socket.Receive(ref remoteEndPoint);

                    string message = Encoding.ASCII.GetString(datagramReceived, 0, datagramReceived.Length);
                    Console.WriteLine("Receives {0} bytes from {1} port {2} message {3}", datagramReceived.Length,
                        remoteEndPoint.Address, remoteEndPoint.Port, message);

                    CoronaTest test = Parse(message);

                    OpretNyTest(test);
                }
            }
        }

        private static async Task OpretNyTest(CoronaTest test)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(test),
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage resp = await client.PostAsync(URI, content);
                if (resp.IsSuccessStatusCode)
                {
                    return;
                }

                throw new ArgumentException("opret fejlede");
            }

        }

        // To parse data from the IoT devices in the teachers room, Elisagårdsvej
        private static CoronaTest Parse(string response)
        {
            // Id: 20, Name: maskine 1, Temperature: 37,72396496903336, Location: sted 3, Date: 27/11, Time: 14:35
            string[] parts = response.Split(',');

            // id
            string[] ids = parts[0].Split(':');
            int id = Convert.ToInt32(ids[1].Trim());

            // name
            string[] names = parts[1].Split(':');
            string name = names[1].Trim();

            // Temperature
            string[] temps = parts[2].Split(':');
            double temp = Convert.ToDouble(temps[1].Trim());

            // Location
            string[] locas = parts[4].Split(':');
            string loca = locas[1].Trim();

            // Date
            string[] dates = parts[5].Split(':');
            string[] date = dates[1].Split(' ');
            string dat = date[1];

            // Time
            string[] times = parts[6].Split(':');
            string time = parts[1] + parts[2];

            CoronaTest test = new CoronaTest(id, name, temp, loca, dat, time);
            return test;
        }
    }
}
