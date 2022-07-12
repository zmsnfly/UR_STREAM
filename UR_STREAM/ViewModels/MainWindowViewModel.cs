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
        }

        private UInt32 TotalMsgLength = 3288596480;

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
        private Socket socketClient = null;
        private Thread threadClient = null;

        private void Start()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (IP == null || Port == null)
            {
                return;
            }
            IPAddress ipAddress = IPAddress.Parse(IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, int.Parse(Port));
            try
            {
                socketClient.Connect(ipEndPoint);
            }
            catch (Exception e)
            {
                MessageBox.Show("连接失败" + e.Message);
                return;
            }
            IsRunning = true;
            threadClient = new Thread(RecieveMsg)
            {
                IsBackground = true
            };
            threadClient.Start();
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
                        X = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (32 * 8))).ToString();
                        Y = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (33 * 8))).ToString();
                        Z = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (34 * 8))).ToString();
                        RX = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (35 * 8))).ToString();
                        RY = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (36 * 8))).ToString();
                        RZ = Function.Radian2Degree(BitConverter.ToDouble(arrMsgRec, arrMsgRec.Length - 4 - (37 * 8))).ToString();
                    }
                }
            }
        }

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

        private string x;
        public string X { get => x; set => SetProperty(ref x, value); }

        private string y;
        public string Y { get => y; set => SetProperty(ref y, value); }

        private string z;
        public string Z { get => z; set => SetProperty(ref z, value); }

        private string rX;
        public string RX { get => rX; set => SetProperty(ref rX, value); }

        private string rY;
        public string RY { get => rY; set => SetProperty(ref rY, value); }

        private string rZ;
        public string RZ { get => rZ; set => SetProperty(ref rZ, value); }

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
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Disconnect(true);

            IsRunning = false;

            threadClient?.Abort();
        }
    }
}
