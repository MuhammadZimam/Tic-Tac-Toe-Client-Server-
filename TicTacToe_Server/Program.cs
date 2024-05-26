using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TicTacToe_Server
{
    internal class Program
    {
        static List<TcpClient> clients=new List<TcpClient>();
        public static TcpListener server;
        static char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        static char currentPlayer = 'X';
        static bool gameOver = false;
        static readonly int[][] winningCombinations = {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, 
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, 
        new[] {0, 4, 8}, new[] {2, 4, 6}  
    };


        static async Task Main(string[] args)
        {
            IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
            int port = 5000;
            server = new TcpListener(ipaddress, port);
            server.Start();
            Console.WriteLine("Server Started");
            await AcceptClientsAsync();
            Console.ReadKey();
        }

        static async Task AcceptClientsAsync()
        {
            while (clients.Count < 2)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                char player;

                if (clients.Count == 1)
                {
                    player = 'X';
                }
                else
                {
                    player = 'O';
                }
                
                SendMessage(client, $"Connected as Player {player}\n");
                //Console.WriteLine( client, $"Connected as Player {player}\n");

                if (clients.Count == 2)
                {
                    BroadcastMessage("Game starts!\n");
                    Console.WriteLine("Game Starts!\n");
                    BroadcastBoard();
                }
                Thread clientThread = new Thread(() => HandleClient(client, player));
                clientThread.Start();
            }
        }

        
        static async void HandleClient(TcpClient client, char player) 
        {
            NetworkStream stream = client.GetStream();
            while (!gameOver)
            {
                if (currentPlayer == player)
                {
                    //BroadcastBoard();
                    SendMessage(client, "Your turn: Enter position number (1-9):\n");
                    
                    string moveStr = ReadMessage(stream);
                    if (int.TryParse(moveStr, out int move) && move >= 1 && move <= 9 && board[move - 1] == ' ')
                    {
                        board[move - 1] = player;
                        if (CheckWinner() is char winner)
                        {
                            gameOver = true;
                            BroadcastBoard();
                            string message;

                            if (winner == ' ')
                            {
                                message = "It's a draw!\n";
                                 Console.WriteLine("Its a draw");
                            }
                            else
                            {
                                message = $"Player {winner} wins!\n";
                                Console.WriteLine("Winner: "+winner);
                            }

                            BroadcastMessage(message);
                            
                        }
                        else
                        {
                            currentPlayer = currentPlayer == 'X' ? 'O' : 'X';
                            BroadcastBoard();
                        }
                    }
                    else
                    {
                        SendMessage(client, "Invalid move. Try again.\n");
                    }
                }
                else
                {
                    SendMessage(client, "Waiting for the other player...\n");
                    await Task.Delay(10000);
                }
            }
        }
        static void SendMessage(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }
        static void BroadcastMessage(string message)
        {
            foreach (var client in clients)
            {
                SendMessage(client, message);
            }
        }
        static char? CheckWinner()
        {
            foreach (var combo in winningCombinations)
            {
                if (board[combo[0]] == board[combo[1]] && board[combo[1]] == board[combo[2]] && board[combo[0]] != ' ')
                {
                    return board[combo[0]];
                }
            }

            if (Array.IndexOf(board, ' ') == -1)
            {
                return ' '; 
            }

            return null;
        }
        static void BroadcastBoard()
        {
            string boardStr = $" {board[0]} | {board[1]} | {board[2]}\n" +
                              "-----------\n" +
                              $" {board[3]} | {board[4]} | {board[5]}\n" +
                              "-----------\n" +
                              $" {board[6]} | {board[7]} | {board[8]}\n";
            Console.WriteLine(boardStr);

            BroadcastMessage(boardStr);
        }
        static string ReadMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        }

    }
}
