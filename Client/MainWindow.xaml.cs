using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Таймер и счетчик для него
        static string address = "192.168.3.";
        static int port = 8000;
        static List<Addr> ports = new List<Addr>();

        public MainWindow()
        {
            InitializeComponent();
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

        private void btn_find_Click(object sender, RoutedEventArgs e)
        {
            ports.Clear();
            String addr = tb_addr.Text;
            if (addr.EndsWith(".0"))
            {
                addr = addr.Remove(tb_addr.Text.Length - 1, 1);

                for (int beginLastAddr = 1; beginLastAddr < 35; beginLastAddr++)
                {
                    String fullAddr = addr + beginLastAddr.ToString();
                    checkConnect(fullAddr, port);
                }
            }
            else
            {
                addr = tb_addr.Text;
                checkConnect(addr, port);
            }

            lv_connections.ItemsSource = null;
            lv_connections.ItemsSource = ports;
        }

        private void checkConnect(string hostUri, int portNumber)
        {
            if (!pingHost(hostUri, portNumber))
            {
                Console.WriteLine(hostUri.ToString() + " - failure");
            }
            else
            {
                Console.WriteLine(hostUri.ToString() + " - success");
                ports.Add(new Addr(hostUri));
            }
        }
    }
}
