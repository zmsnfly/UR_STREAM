using HandyControl.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UR_STREAM.Common;
using UR_STREAM.Models;

namespace UR_STREAM.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            Title = "UR_STREAM";
            Init();
        }

        private void Init()
        {
            IP = "192.168.122.10";
            Port = "30003";
            IsRunning = false;
            IsConnecting = false;
            CanConnect = true;
            IsRecording = false;
            URDataList = new List<URData>();
            CurrentData = new ObservableCollection<KeyValueModel>();
            KeyList = new Dictionary<string, double>();
            KeyList.Clear();
            URDataList.Clear();
        }

        public List<URData> URDataList { get; set; }

        private ObservableCollection<KeyValueModel> currentData;
        public ObservableCollection<KeyValueModel> CurrentData
        {
            get => currentData;
            set => SetProperty(ref currentData, value);
        }

        public Dictionary<string, double> KeyList { get; set; }

        private readonly UInt32 TotalMsgLength = 3288596480;

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        private Socket SocketClient = null;
        private Task TaskClient = null;
        private delegate string ConnectSocketDelegate(IPEndPoint ipep, Socket sock);

        private Task TaskWriteFile = null;

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
                UpdateCanStop();
            }
        }
        private bool stopFinished = false;
        public bool SaveFinished { get => stopFinished; set { SetProperty(ref stopFinished, value); UpdateCanStop(); } }

        private bool canStop;
        public bool CanStop
        {
            get => canStop;
            set => SetProperty(ref canStop, value);
        }
        private bool? isRecording;
        public bool? IsRecording { get => isRecording; set { SetProperty(ref isRecording, value); UpdateCanStop(); } }

        void UpdateCanStop()
        {
            CanStop = (IsRunning && !(bool)IsRecording) || SaveFinished;
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
            CanConnect = false;
            IsConnecting = true;
            Task taskConnect = new Task(TryConnect);
            taskConnect.Start();
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
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (IP == null || Port == null || !int.TryParse(Port, out int port) || port == 0)
            {
                Growl.Error("连接失败！错误信息：IP地址端口只能为非0数字");
                IsConnecting = false;
                CanConnect = true;
                return;
            }
            IPAddress ipAddress = null;
            IPEndPoint ipEndPoint = null;
            try
            {
                ipAddress = IPAddress.Parse(IP);
                ipEndPoint = new IPEndPoint(ipAddress, int.Parse(Port));
            }
            catch (Exception ex)
            {
                var exmessage = ex.Message;
                IsConnecting = false;
                CanConnect = true;
                if (exmessage.Contains("IP"))
                {
                    Growl.Error("连接失败！错误信息：IP地址错误");
                }
                else
                {
                    Growl.Error("连接失败！错误信息：端口错误");
                }
                return;
            }

            ConnectSocketDelegate connect = ConnectSocket;
            IAsyncResult asyncResult = connect.BeginInvoke(ipEndPoint, SocketClient, null, null);
            bool connectSuccess = asyncResult.AsyncWaitHandle.WaitOne(2000, false);
            if (!connectSuccess)
            {
                IsConnecting = false;
                CanConnect = true;
                Growl.Error("连接失败！错误信息：连接超时");
            }
            else
            {
                IsRunning = true;
                IsConnecting = false;
                TaskClient = new Task(RecieveMsg);
                TaskClient.Start();
            }

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

        public void Stop()
        {
            SaveFinished = false;
            try
            {
                SocketClient?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {

            }
            finally
            {
                SocketClient.Close();
            }

            IsRunning = false;
            CanConnect = true;
            Init();
        }

        private void RecieveMsg()
        {
            while (IsRunning)
            {
                byte[] arrMsgRec = new byte[1220];
                int length;
                try
                {
                    length = SocketClient.Receive(arrMsgRec);
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(Stop);
                    break;
                }

                if (length == 0)
                {
                    Application.Current.Dispatcher.Invoke(Stop);
                    break;
                }
                else
                {
                    var total_msg_length = BitConverter.ToUInt32(arrMsgRec, 0);
                    if (total_msg_length == TotalMsgLength)
                    {
                        StreamHelper streamHelper = new StreamHelper(arrMsgRec);
                        KeyList["J1"] = streamHelper.GetDeg(32);
                        KeyList["J2"] = streamHelper.GetDeg(33);
                        KeyList["J3"] = streamHelper.GetDeg(34);
                        KeyList["J4"] = streamHelper.GetDeg(35);
                        KeyList["J5"] = streamHelper.GetDeg(36);
                        KeyList["J6"] = streamHelper.GetDeg(37);
                        KeyList["X"] = streamHelper.GetNum(56);
                        KeyList["Y"] = streamHelper.GetNum(57);
                        KeyList["Z"] = streamHelper.GetNum(58);
                        KeyList["RX"] = streamHelper.GetRad(59);
                        KeyList["RY"] = streamHelper.GetRad(60);
                        KeyList["RZ"] = streamHelper.GetRad(61);

                        URData urData = new URData
                        {
                            Time = streamHelper.GetTime(),
                            KeyDic = new Dictionary<string, double>(KeyList)
                        };
                        URDataList.Add(urData);

                        Application.Current.Dispatcher.Invoke(RemoveUnusedItem);

                        foreach (var key in KeyList)
                        {
                            bool isContain = false;
                            foreach (var data in CurrentData)
                            {
                                if (data.Key.Equals(key.Key))
                                {
                                    isContain = true;
                                    data.Value = key.Value;
                                    data.Time = streamHelper.GetTime();
                                }
                            }
                            if (!isContain)
                            {
                                Application.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    CurrentData.Add(new KeyValueModel(streamHelper.GetTime(), key.Key, key.Value));
                                }));
                            }
                        }
                    }
                }

            }
        }

        private void RemoveUnusedItem()
        {
            foreach (var data in CurrentData.ToArray())
            {
                bool isContain = false;
                foreach (var key in KeyList)
                {
                    if (key.Key.Equals(data.Key))
                    {
                        isContain = true;
                    }
                }
                if (!isContain)
                {
                    CurrentData.Remove(data);
                }
            }
        }

        private bool? isConnecting;

        public bool? IsConnecting { get => isConnecting; set => SetProperty(ref isConnecting, value); }

        private bool? canConnect;

        public bool? CanConnect { get => canConnect; set => SetProperty(ref canConnect, value); }

        private RelayCommand recordCommand;

        public ICommand RecordCommand
        {
            get
            {
                if (recordCommand == null)
                {
                    recordCommand = new RelayCommand(Record);
                }

                return recordCommand;
            }
        }

        private void Record()
        {
            if ((bool)IsRecording)
            {
                SaveFinished = false;
                Growl.Info("正在记录数据");
                StartIndex = URDataList.Count;
            }
            else
            {
                EndIndex = URDataList.Count;
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "UR机器人信息（*.xlsx)|*.xlsx",
                    RestoreDirectory = true,
                    FileName = DateTime.Now.ToString("yyyy MM dd HH.mm")
                };
                if ((bool)dialog.ShowDialog())
                {
                    Growl.Info("正在保存文件");
                    string path = dialog.FileName;
                    TaskWriteFile = new Task(() => { CreateXls(path); });
                    TaskWriteFile.Start();
                }

            }


        }

        private void CreateXls(string filePath)
        {
            List<URData> TemData = new List<URData>(URDataList);
            var DataCut = TemData.Skip(StartIndex).Take(EndIndex - StartIndex).ToArray();
            string excelPath = filePath;

            FileStream fs = new FileStream(excelPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            IWorkbook workbook = new XSSFWorkbook();

            ICellStyle styleHeader = workbook.CreateCellStyle();
            styleHeader.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
            styleHeader.FillPattern = FillPattern.SolidForeground;
            styleHeader.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; ;
            IFont font = workbook.CreateFont();
            font.Color = HSSFColor.White.Index;
            font.IsBold = true;
            font.FontHeightInPoints = 16;
            styleHeader.SetFont(font);

            if (excelPath.ToLower().EndsWith(".xls"))
            {
                workbook = new HSSFWorkbook();
            }

            ISheet sheet = workbook.CreateSheet("sheet1");

            IRow rowHead = sheet.CreateRow(0);

            rowHead.CreateCell(0).SetCellValue("Time");
            rowHead.GetCell(0).CellStyle = styleHeader;
            int i = 1;
            int j = 1;
            foreach (var key in KeyList)
            {
                var cell = rowHead.CreateCell(j++);
                cell.SetCellValue(key.Key);
                cell.CellStyle = styleHeader;
            }
            foreach (var data in DataCut)
            {
                IRow row = sheet.CreateRow(i++);
                var cell = row.CreateCell(0);
                cell.SetCellValue(data.Time);
                IDataFormat dataformat = workbook.CreateDataFormat();
                ICellStyle styleTime = workbook.CreateCellStyle();
                styleTime.DataFormat = dataformat.GetFormat("hh:mm:ss.000");
                cell.CellStyle = styleTime;
                int _j = 1;
                foreach (var _data in data.KeyDic)
                {
                    row.CreateCell(_j++).SetCellValue(_data.Value);
                }
            }
            sheet.AutoSizeColumn(0);
            workbook.Write(fs);
            workbook.Close();
            Growl.Success("文件保存成功，文件路径:" + filePath);
            SaveFinished = true;
        }
    }
}