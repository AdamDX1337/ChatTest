using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Threading;

namespace ChatTest
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        int onlineCount = 0;
        string Username = "";
        Socket clntSock;
        bool clientOn = false;
        bool serverOn = false;
        string nameError = "Invalid Name";
        string aUser, bUser;


        private void Form1_Load(object sender, EventArgs e)
        {
            FormClosing += Form_FormClosing;
            timer1.Start();
        }
        public void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                clntSock.Close();
            }
        }

            public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void ServerSetup()
        {
            serverOn = true;
            // Constants
            string host = GetLocalIPAddress(); // Get the local host IP address
            int port = 42069;
            

            // Init
            Socket servSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            servSock.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            servSock.Listen(1);

            // Wait for client
            richTextBox2.AppendText("Server started on " + host + ":" + port + "\n");
            clntSock = servSock.Accept();
            IPEndPoint clntAddr = (IPEndPoint)clntSock.RemoteEndPoint;
            richTextBox2.AppendText("Connection started with " + clntAddr.Address + ":" + clntAddr.Port + "\n");
            string signal = "You have joined the server: " + host + ":" + port + "\n";
            clntSock.Send(Encoding.UTF8.GetBytes(signal));
            
            ChatSetup();
        }

        public void ClientSetup()
        {
            clientOn = true;
            // Init
            string hostIP = GetLocalIPAddress(); 

            // Join server
            clntSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clntSock.Connect(hostIP, 42069);
            byte[] buffer = new byte[1024];
            int bytesRead = clntSock.Receive(buffer);
            string signal = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            richTextBox2.AppendText(signal);
            ChatSetup();
        }

        public void ChatSetup()
        {
            // Start a thread to receive messages
            Thread receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        public void Receive()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = clntSock.Receive(buffer);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] splitMsg = message.Split(':');
                aUser = splitMsg[0];

                if (!listBox1.Items.Contains(aUser))
                {
                    listBox1.Items.Add(aUser);
                    
                }
                if (bUser != aUser)
                {
                    listBox1.Items.Remove(bUser);
                    bUser = aUser;
                }

                richTextBox2.AppendText(message);

                



            }
        }

        //IGNORE
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("a");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            onlineCount = listBox1.Items.Count;
            label2.Text = onlineCount.ToString();
        }

        //SET NAME
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Contains(Username))
            {
                listBox1.Items.Remove(Username);
            }
            Username = textBox1.Text;
            listBox1.Items.Add(Username);
        }

        //SEND MESSAGE
        private void button1_Click(object sender, EventArgs e)
        {
            if (serverOn || clientOn)
            {
                
                string message = richTextBox1.Text;
                if (!String.IsNullOrWhiteSpace(message))
                {
                    message = Username + ": " + message + "\n";
                    richTextBox2.AppendText(message);
                    clntSock.Send(Encoding.UTF8.GetBytes(message));
                    richTextBox1.Text = "";
                }
            }
        }

        //server
        private void button4_Click(object sender, EventArgs e)
        {
            if (clientOn || serverOn)
            {

            }
            else
            {
                if (String.IsNullOrWhiteSpace(Username))
                {
                    label4.Text = nameError;
                }
                else
                {
                    label4.Text = "No Errors";
                    if (!clientOn && !serverOn)
                        ServerSetup();
                }
            }
        }

        //client
        private void button5_Click(object sender, EventArgs e)
        {
            if (clientOn || serverOn)
            {

            }
            else {
                if (String.IsNullOrWhiteSpace(Username))
                {
                    label4.Text = nameError;
                }
                else
                {
                    label4.Text = "No Errors";
                    if (!clientOn && !serverOn)
                        ClientSetup();
                }
            }
            
            
        }
    }
}
