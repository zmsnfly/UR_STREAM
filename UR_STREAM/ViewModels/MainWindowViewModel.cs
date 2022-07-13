using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UR_STREAM.Common;

namespace UR_STREAM.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            Title = "UR_STREAM";
            IP = "192.168.122.10";
            Port = "30003";
            IsRunning = false;
            IsConnecting = false;
            CanConnect = true;
        }

        private readonly UInt32 TotalMsgLength = 3288596480;

        private Socket socketClient = null;
        private Thread threadClient = null;
        private delegate string ConnectSocketDelegate(IPEndPoint ipep, Socket sock);

        private string port;

        public string Port { get => port; set => SetProperty(ref port, value); }

        private string iP;

        public string IP { get => iP; set => SetProperty(ref iP, value); }

        private bool isRunning;

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                SetProperty(ref isRunning, value);
                if (isRunning)
                {
                    StateColor = "Green";
                    StateText = "已连接";

                }
                else
                {
                    StateColor = "Red";
                    StateText = "已断开";
                }
            }
        }

        private string stateText;

        public string StateText
        {
            get { return stateText; }
            set { SetProperty(ref stateText, value); }
        }

        private string stateColor;

        public string StateColor
        {
            get { return stateColor; }
            set { SetProperty(ref stateColor, value); }
        }


        private string title;

        public string Title { get => title; set => SetProperty(ref title, value); }

        private string j1;
        public string J1 { get => j1; set => SetProperty(ref j1, value); }

        private string j2;
        public string J2 { get => j2; set => SetProperty(ref j2, value); }

        private string j3;
        public string J3 { get => j3; set => SetProperty(ref j3, value); }

        private string j4;
        public string J4 { get => j4; set => SetProperty(ref j4, value); }

        private string j5;
        public string J5 { get => j5; set => SetProperty(ref j5, value); }

        private string j6;
        public string J6 { get => j6; set => SetProperty(ref j6, value); }

        private RelayCommand startCommand;

        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new RelayCommand(Start);
                }

                return startCommand;
            }
        }


        private void Start()
        {
            IsConnecting = true;
            Thread threadConnect = new Thread(TryConnect);
            threadConnect.Start();
        }

        private string ConnectSocket(IPEndPoint ipep, Socket sock)
        {
            string exmessage = "";
            try
            {
                sock.Connect(ipep);
            }
            catch (Exception ex)
            {
                exmessage = ex.Message;
            }
            finally
            {
            }

            return exmessage;
        }
        private void TryConnect()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (IP == null || Port == null)
            {
                IsConnecting = false;
                return;
            }
            IPAddress ipAddress = IPAddress.Parse(IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, int.Parse(Port));
            ConnectSocketDelegate connect = ConnectSocket;
            IAsyncResult asyncResult = connect.BeginInvoke(ipEndPoint, socketClient, null, null);
            bool connectSuccess = asyncResult.AsyncWaitHandle.WaitOne(2000, false);
            if (!connectSuccess)
            {
                IsConnecting = false;
                CanConnect = true;
                MessageBox.Show(string.Format("连接失败！错误信息：{0}", "连接超时"));
            }
            else
            {
                IsRunning = true;
                threadClient = new Thread(RecieveMsg)
                {
                    IsBackground = true
                };
                threadClient.Start();
                IsConnecting = false;
            }

        }

        private RelayCommand saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(Save);
                }

                return saveCommand;
            }
        }

        private void Save()
        {

        }

        private RelayCommand stopCommand;

        public ICommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                {
                    stopCommand = new RelayCommand(Stop);
                }

                return stopCommand;
            }
        }

        private void Stop()
        {

            threadClient?.Abort();
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();
            IsRunning = false;
            CanConnect = true;
        }

        private void RecieveMsg()
        {
            while (IsRunning)
            {
                byte[] arrMsgRec = new byte[1220];
                int length;
                try
                {
                    length = socketClient.Receive(arrMsgRec);
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }

                if (length == 0)
                {
                    break;
                }
                else
                {
                    var total_msg_length = BitConverter.ToUInt32(arrMsgRec, 0);
                    if (total_msg_length == TotalMsgLength)
                    {
                        Array.Reverse(arrMsgRec);
                        J1 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (32 * 8))).ToString();
                        J2 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (33 * 8))).ToString();
                        J3 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (34 * 8))).ToString();
                        J4 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (35 * 8))).ToString();
                        J5 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (36 * 8))).ToString();
                        J6 = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (37 * 8))).ToString();
                    }
                }
            }
        }

        private bool? isConnecting;

        public bool? IsConnecting { get => isConnecting; set => SetProperty(ref isConnecting, value); }

        private bool? canConnect;

        public bool? CanConnect { get => canConnect; set => SetProperty(ref canConnect, value); }

    }
}
