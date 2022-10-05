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

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Таймер и счетчик для него
        static string address = "192.168.3.26";

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                List<Port> ports = new List<Port>();
                //Ищем перебором открытый порт для соединения
                for (int _port = 8000; _port < 8100; _port++)
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), _port);
                    Socket socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                        );

                    IAsyncResult asyncResult = socket.BeginConnect(
                        ipPoint,
                        new AsyncCallback(connectCallback),
                        socket
                        );

                    if (!asyncResult.AsyncWaitHandle.WaitOne(1, false))
                    {
                        socket.Close();
                    }
                    else
                    {
                        socket.Close();
                        ports.Add(new Port(_port, _port.ToString()));
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
                Port port= (Port)lv_connections.SelectedItem;
                btn_sendMessage.IsEnabled = false;
                String message = tb_textInput.Text;
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port.portNumber);

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

        private class Port
        {
            public int portNumber { get; set; }
            public string name { get; set; }

            public Port(int portNumber, string name)
            {
                this.portNumber = portNumber;
                this.name = name;
            }
        }
    }
}
