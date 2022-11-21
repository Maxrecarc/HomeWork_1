using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System;
using System.Collections;

namespace SocketTcpServer
{
    public class Server
    {
        const int port = 8000; // порт для приема входящих запросов
        static List<Robot> robots = new List<Robot>();

        static int maxDetails = 100;

        public Server()
        {
            Main(new string[1]);
        }

        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

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
                    data = completeTask(builder.ToString());
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

        public static byte[] completeTask(String command)
        {
            string[] str = command.Split(' ');

            switch (str[0])
            {
                case "/add":
                    {
                        robots.Add(new Robot(str[1], str[2], str[3]));
                        int details = ((int)robots.Last().type);
                        if (maxDetails < details) 
                        {
                            robots.Remove(robots.Last());
                            return Encoding.Unicode.GetBytes("Not enough details"); 
                        }
                        else
                        {
                            maxDetails -= details;
                            return Encoding.Unicode.GetBytes((robots.Count - 1).ToString());
                        }
                    }
                case "/set_func":
                    {
                        int index = Int32.Parse(str[1]);
                        Robot robot = robots[index];
                        robot.function = str[2];
                        robots.Insert(index, robot);
                        return Encoding.Unicode.GetBytes("Function changed!");
                    }
                case "/delete":
                    {
                        int index = Int32.Parse(str[1]);
                        int details = (int)robots[index].type;
                        maxDetails += details;
                        robots.Remove(robots[index]);
                        return Encoding.Unicode.GetBytes("Robot deleted");
                    }
                case "/get":
                    {
                        int index = Int32.Parse(str[1]);
                        if (index >= 0 && index <= robots.Count)
                        {
                            return Encoding.Unicode.GetBytes(robots[index].name + ": " + robots[index].function + " (" +
                                robots[index].type + ")");
                        }
                        else return Encoding.Unicode.GetBytes("Out of range");

                    }
                default:
                    Console.WriteLine("Wrong command: " + str[0]);
                    return Encoding.Unicode.GetBytes("Wrong command");
            }
        }

        public static int getRobotsCount()
        {
            return robots.Count;
        }
    }

    class Robot
    {
        public string name { get; set; }
        public string function { get; set; }
        public RobotType type { get; set; }

        RobotType convertType(string type)
        {
            switch (type.ToLower())
            {
                case "attack": return RobotType.ATTACK;
                case "defence": return RobotType.DEFENCE;
                case "heal": return RobotType.HEAL;
                default: return RobotType.DEFAULT;
            }
        }

        public Robot(string name, string function, string type)
        {
            this.name = name;
            this.function = function;
            this.type = convertType(type);
        }
    }

    enum RobotType
    {
        ATTACK = 5, DEFENCE = 10, HEAL = 20, DEFAULT = 3
    }
}