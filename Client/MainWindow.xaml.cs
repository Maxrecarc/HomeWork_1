using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.Generic;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Таймер и счетчик для него
        static List<Addr> ports = new List<Addr>();
        static ServerConnection serverConnection = new ServerConnection();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void btn_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            btn_sendMessage.IsEnabled = false;
            String message = tb_textInput.Text;
            String response = serverConnection.sendMessage((Addr)lv_connections.SelectedItem, message);

            lv_log.Items.Add("Ответ: " + response);
            btn_sendMessage.IsEnabled = true;
        }


        public void btn_find_Click(object sender, RoutedEventArgs e)
        {
            ports.Clear();
            ports = serverConnection.findServer(tb_addr.Text);

            lv_connections.ItemsSource = null;
            lv_connections.ItemsSource = ports;
        }
    }

    public class ServerConnection
    {
        static int port = 8000;

        public List<Addr> findServer(String addr)
        {
            List<Addr> addresses = new List<Addr>();
            if (addr.EndsWith(".0"))
            {
                addr = addr.Remove(addr.Length - 1, 1);

                for (int beginLastAddr = 1; beginLastAddr < 35; beginLastAddr++)
                {
                    String fullAddr = addr + beginLastAddr.ToString();
                    checkConnect(fullAddr, port);
                }
            }
            else
            {
                var res = checkConnect(addr, port);
                if (res != null)
                {
                    addresses.Add(res);
                }
            }

            return addresses;
        }

        public string sendMessage(Addr addr, string message)
        {
            try
            {
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

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                return builder.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private Addr checkConnect(string hostUri, int portNumber)
        {
            if (!pingHost(hostUri, portNumber))
            {
                Console.WriteLine(hostUri.ToString() + " - failure");
                return null;
            }
            else
            {
                Console.WriteLine(hostUri.ToString() + " - success");
                return new Addr(hostUri);
            }
        }

        private static bool pingHost(string hostUri, int portNumber)
        {
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(hostUri, portNumber, null, null);
                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(300)))
                {
                    return false;
                }

                // we have connected
                client.Close();
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }

    public class Addr
    {
        public String fullAddr { get; set; }

        public Addr(String addr)
        {
            this.fullAddr = addr;
        }
    }
}
