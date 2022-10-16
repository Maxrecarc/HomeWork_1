using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System;

namespace SocketTcpServer
{
    class Server
    {
        static int port = 8000; // порт для приема входящих запросов
        static List<Dictionary<String, String>> robots;

        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("192.168.1.237"), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(1000);

                Console.WriteLine("Сервер запущен");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);


                    // отправляем ответ
                    data = Encoding.Unicode.GetBytes(builder.ToString());
                    Console.WriteLine("Запрос получен: " + builder.ToString());
                    handler.Send(data);
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private byte[] completeTask(String command)
        {
            String[] str = command.Split(' ');

            if (str[0] == "/")
            {
                switch (str[0])
                {
                    case "/add":
                        Dictionary<String, String> dict = new Dictionary<String, String>();
                        dict.Add(str[1], str[2]);
                        robots.Add(dict);
                        break;
                    case "/set_name":
                        //robots
                        //robots.Insert()
                        break;

                }
            }
            return null;
        }

    }
}