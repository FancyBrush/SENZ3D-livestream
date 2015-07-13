using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace idi.livestream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Thread thread;
        //DepthPipeline depthPipe;
        ImagePipeline imagePipeline;

        UdpClient client = new UdpClient();
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000); // endpoint where server is listening
        

        public MainWindow() 
        {
            InitializeComponent();
            client.Connect(ep);

            //depthPipe = new DepthPipeline();
            //thread = new Thread(new ThreadStart(depthPipe.StartWork));
            //thread.Start();
            //depthPipe.ImageReceived += depthPipe_ImageReceived;

            imagePipeline = new ImagePipeline();
            thread = new Thread(new ThreadStart(imagePipeline.StartWork));
            thread.Start();
            imagePipeline.handler += SendImageViaUDP;
        }

        void SendImageViaUDP(object sender, UInt16[] depthImage, byte[] rgbImage)
        {
            DateTime dateTimeNow = DateTime.Now;
            byte[] dateTimeNowByte = Encoding.ASCII.GetBytes(dateTimeNow.ToString());
            client.Send(dateTimeNowByte, dateTimeNowByte.GetLength(0));

            int length = depthImage.GetLength(0)*sizeof(UInt16);
            byte[] toSend = new byte[length];
            byte[] dummy = new byte[2];

            int maxUDPlength = 65000;
            int numberOfMesg = length / maxUDPlength + 1;

            for (int i = 0; i < maxUDPlength/2; i++)
            {
                dummy = BitConverter.GetBytes(depthImage[i]);
                toSend[2*i] = dummy[0];
                toSend[2*i+1] = dummy[1];
            }
            client.Send(toSend, maxUDPlength);
            /*
                // send 3 messsages
                for (int mes = 0; mes < numberOfMesg; mes++)
                {
                    for (int i = 0; i < maxUDPlength / 2; i++)
                    {
                        dummy = BitConverter.GetBytes(depthImage[i]);
                        toSend[i] = dummy[0];
                        toSend[i + 1] = dummy[1];
                    }
                }
             */
            //client.Send(toSend, length);
        }
        
        

        
    }
}
