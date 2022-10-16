using System;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Timers;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Таймер и счетчик для него
        static string address = "127.0.0.";
        static int port = 8000;

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                List<Addr> ports = new List<Addr>();
                //Ищем перебором открытый порт для соединения
                for (int addr = 1; addr < 55; addr++)
                {
                    String fullAddr = address + addr.ToString();
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(fullAddr), port);
                    Socket socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                        );

                    //Ping pingSender = new Ping();
                    //int timeout = 120;
                    //PingOptions options = new PingOptions();
                    //byte[] buffer = Encoding.ASCII.GetBytes("Attempt to connect");
                    //AutoResetEvent waiter = new AutoResetEvent(false);

                    //PingReply reply = pingSender.Send("127.0.0.1:8000", timeout, buffer, options);

                    //IAsyncResult asyncResult = socket.BeginConnect(
                    //    ipPoint,
                    //    new AsyncCallback(connectCallback),
                    //    socket
                    //    );

                    if (!pingHost(fullAddr, port))
                    {
                        //socket.Close();
                        Console.WriteLine(fullAddr.ToString() + " - no connect");
                    }
                    else
                    {
                        //socket.Close();
                        ports.Add(new Addr(fullAddr));
                        break;
                    }
                }

                lv_connections.ItemsSource = ports;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void connectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(ar.ToString());
            }
        }

        private void btn_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Addr addr = (Addr)lv_connections.SelectedItem;
                btn_sendMessage.IsEnabled = false;
                String message = tb_textInput.Text;
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(addr.fullAddr), port);

                // подключаемся к удаленному хосту
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                byte[] data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);

                // получаем ответ
                data = new byte[256]; // буфер для ответа
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                do
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);

                lv_log.Items.Add("Ответ: " + builder.ToString());
                btn_sendMessage.IsEnabled = true;

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private class Addr
        {
            public String fullAddr { get; set; }

            public Addr(String addr)
            {
                this.fullAddr = addr;
            }
        }

        public static bool pingHost(string hostUri, int portNumber)
        {
            try
            {
                using (var client = new TcpClient(hostUri, portNumber))
                    return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
