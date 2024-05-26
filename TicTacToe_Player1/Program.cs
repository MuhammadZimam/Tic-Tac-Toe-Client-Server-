using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe_Player1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string serverMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(serverMessage);

                if (serverMessage.Contains("Your turn:"))
                {
                    string input = Console.ReadLine();
                    byte[] data = Encoding.ASCII.GetBytes(input);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
