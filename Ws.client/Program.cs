using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ws.client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("presser enter pour commencer...");
            Console.ReadLine();
            await Start();
        }

        public static async Task Start()
        {
            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUrl = new Uri("ws://localhost:5000/send");
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await client.ConnectAsync(serviceUrl, cts.Token);
                    int n = 0;
                    while(client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("Entrez votre message");
                        string mess = Console.ReadLine();
                        if (!string.IsNullOrEmpty(mess))
                        {
                            ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(mess));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cts.Token);
                            var responseBuffer = new byte[1024];
                            int offset = 0;
                            int packet = 1024;
                            while (true)
                            {
                                ArraySegment<byte> byteRecieved = new ArraySegment<byte>(responseBuffer, offset, packet);
                                WebSocketReceiveResult response = await client.ReceiveAsync(byteRecieved, cts.Token);
                                var responseMsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                                Console.WriteLine(responseMsg);
                                if (response.EndOfMessage)
                                    break;
                            }
                        }
                    }
                }
                catch (WebSocketException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadLine();
        }
    }
}
