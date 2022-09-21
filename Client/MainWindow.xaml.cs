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

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Таймер и счетчик для него
        static System.Timers.Timer timer;
        static int counter = 0;

        public MainWindow()
        {
            InitializeComponent();

            timer = new System.Timers.Timer(1000);
            //timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            List<PortInfo> pi = MainWindow.GetOpenPort();
            lv_connections.ItemsSource = pi;
        }

        private static List<PortInfo> GetOpenPort()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            return tcpEndPoints
                .Where(selected => !selected.Address.ToString().Equals("::") && !selected.Address.ToString().Equals("0.0.0.0"))
                .Select(passed => 
                {
                    return new PortInfo(
                        portNumber: passed.Port,
                        address: passed.Address.ToString(),
                        local: String.Format("{0}:{1}", passed.Address, passed.Port));
                }).ToList();
        }

        //private void OnTimedEvent(object sender, ElapsedEventArgs e)
        //{
        //    if (counter != 10)
        //    {
        //        byte[] data = new byte[256]; // буфер для ответа
        //        StringBuilder builder = new StringBuilder();
        //        int bytes = 0; // количество полученных байт

        //        bytes = socket.Receive(data, data.Length, 0);
        //        if (bytes == 0) return;
                    
        //        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));

        //        Dispatcher.Invoke(() => lv_reseivedMessages.Items.Add(builder.ToString()));
        //        Dispatcher.Invoke(() => btn_sendMessage.IsEnabled = true);
        //        timer.Stop();

        //        // закрываем сокет
        //        socket.Shutdown(SocketShutdown.Both);
        //        socket.Close();
        //    }
        //    else
        //    {
        //        lv_reseivedMessages.Items.Add("Время ожидания ответа было превышено");
        //        timer.Stop();
        //    }
            
        //}

        private void btn_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PortInfo connection = (PortInfo)lv_connections.SelectedItem;
                string address = connection.address;
                int port = connection.portNumber;

                btn_sendMessage.IsEnabled = false;
                lv_connections.IsEnabled = false;
                String message = tb_textInput.Text;

                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

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

                lv_reseivedMessages.Items.Add(builder.ToString());
                btn_sendMessage.IsEnabled = true;
                lv_connections.IsEnabled = true;

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class PortInfo
    {
        public int portNumber { get; set; }
        public string local { get; set; }
        public string address { get; set; }

        public PortInfo(int portNumber, string local, string address)
        {
            this.portNumber = portNumber;
            this.address = address;
            this.local = local;
        }
    }
}
