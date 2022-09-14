using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Timers;
using System.Reflection;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int port = 8080; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        Socket socket;

        //Таймер и счетчик для него
        static System.Timers.Timer timer;
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                string message = Console.ReadLine();

                // закрываем сокет
                //socket.Shutdown(SocketShutdown.Both);
                //socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (counter != 10)
            {
                byte[] data = new byte[256]; // буфер для ответа
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                bytes = socket.Receive(data, data.Length, 0);
                if (bytes == 0) return;
                    
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));

                Dispatcher.Invoke(() => lv_reseivedMessages.Items.Add(builder.ToString()));
                Dispatcher.Invoke(() => btn_sendMessage.IsEnabled = true);
                timer.Stop();
            }
            else
            {
                //lv_reseivedMessages.Items.Add("Время ожидания ответа было превышено");
                timer.Stop();
            }
            
        }

        private void btn_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            btn_sendMessage.IsEnabled = false;
            String message = tb_textInput.Text;
            byte[] data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);

            counter = 0;
            timer.Start();
        }
    }
}
