using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using JM.Core;
using JM.Diag;

namespace JM.QingQi
{
    internal class Synerject : AbstractECU
    {
        public const int TesterID = 0xF1;
        public const int Physical = 0x11;
        public const int Functional = 0x10;
        public const int OBDServices = 0x33;
        private const byte ReturnControlToECU = 0x00;
        private const byte ReportCurrentState = 0x01;
        private const byte ResetToDefault = 0x04;
        private const byte ShortTermAdjustments = 0x07;
        private const byte LongTermAdjustments = 0x08;
        private readonly byte[] keepLink;
        private readonly byte[] startCommunication;
        private readonly byte[] startDiagnosticSession;
        private readonly byte[] stopDiagnosticSession;
        private readonly byte[] stopCommunication;
        private KWPOptions options;

        public Synerject(Core.VehicleDB Db, ICommbox commbox)
            : base(Db, commbox)
        {
            Db.CMDCatalog = "Synerject";
            Db.TCCatalog = "Synerject";
            Db.LDCatalog = "Synerject";

            keepLink = Db.GetCommand("KeepLink");
            startCommunication = Db.GetCommand("Start Communication");
            startDiagnosticSession = Db.GetCommand("Start DiagnosticSession");
            stopDiagnosticSession = Db.GetCommand("Stop DiagnosticSession");
            stopCommunication = Db.GetCommand("Stop Communication");

            DataStreamInit();
            ActiveTestInit();

            ProtocolInit();
        }

        private void ProtocolInit()
        {
            Protocol = Commbox.CreateProtocol(ProtocolType.ISO14230);
            if (Protocol == null)
                throw new Exception("Not Protocol");

            options = new KWPOptions();
            options.Baudrate = 10416;
            options.SourceAddress = TesterID;
            options.TargetAddress = Physical;
            options.MsgMode = KWPMode.Mode8X;
            options.LinkMode = KWPMode.Mode8X;
            options.StartType = KWPStartType.Fast;
            options.ComLine = 7;

            Pack = new KWPPack();
            Pack.Config(options);

            options.FastCmd = Pack.Pack(startCommunication, 0, startCommunication.Length);
            if (!Protocol.Config(options))
                throw new Exception(Db.GetText("Communication Fail"));
            //Protocol.SetKeepLink(keepLink, 0, keepLink.Length, Pack);
            //Protocol.KeepLink(true);

        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();

            DataStreamCalc["TCC"] = (recv) =>
            {
                return string.Format("{0}", Convert.ToInt32(recv[6]));
            };

            DataStreamCalc["EWT"] = (recv) =>
            {
                return string.Format("{0}", (recv[11] * 256 + recv[12]) / 12);
            };

            DataStreamCalc["ES"] = (recv) =>
            {
                return string.Format("{0}", recv[45] * 256 + recv[46]);
            };

            DataStreamCalc["BP"] = (recv) =>
            {
                return string.Format("{0}", (recv[2] * 256 + recv[3]) / 10);
            };

            DataStreamCalc["IAT"] = (recv) =>
            {
                return string.Format("{0}", recv[63] - 40);
            };

            DataStreamCalc["CT"] = (recv) =>
            {
                return string.Format("{0}", recv[55] - 40);
            };

            DataStreamCalc["BV"] = (recv) =>
            {
                return string.Format("{0}", recv[69] / 16 + 4);
            };

            DataStreamCalc["ATVPSR"] = (recv) =>
            {
                return string.Format("{0}", (recv[65] * 256 + recv[66]) / 512);
            };

            DataStreamCalc["EWS"] = (recv) =>
            {
                switch (recv[54])
                {
                    case 1:
                        return Db.GetText("Stopped");
                    case 2:
                        return Db.GetText("Started");
                    case 3:
                        return Db.GetText("Idle");
                    case 4:
                        return Db.GetText("Part Load");
                    case 5:
                        return Db.GetText("Full");
                    case 6:
                        return Db.GetText("Oil Off");
                    default:
                        return "";
                }
            };

            DataStreamCalc["CLFC"] = (recv) =>
            {
                return string.Format("{0}", (recv[61] * 256 + recv[62]) / 1024 - 32);
            };

            DataStreamCalc["ATVPSZPSV"] = (recv) =>
            {
                return string.Format("{0}", ((recv[67] * 256 + recv[68]) * 5) / 1024);
            };

            DataStreamCalc["FIT"] = (recv) =>
            {
                return string.Format("{0}", (recv[59] * 256 + recv[60]) / 250);
            };

            DataStreamCalc["SAA"] = (recv) =>
            {
                return string.Format("{0}", (recv[13] * 15) / 32 - 30);
            };

            DataStreamCalc["IAMAP"] = (recv) =>
            {
                return string.Format("{0}", recv[29] * 256 + recv[30]);
            };

            DataStreamCalc["WFCOZ"] = (recv) =>
            {
                return string.Format("{0}", recv[15]);
            };

            DataStreamCalc["LSOV"] = (recv) =>
            {
                return string.Format("{0}", (recv[70] * 256 + recv[71]) * 5 / 1024);
            };

            DataStreamCalc["DTS"] = (recv) =>
            {
                return string.Format("{0}", recv[22] * 256 + 0.5);
            };

            DataStreamCalc["AFRL"] = (recv) =>
            {
                if ((recv[23] & 0x80) != 0)
                {
                    return Db.GetText("Thick");
                }
                else
                {
                    return Db.GetText("Thin");
                }
            };

            DataStreamCalc["CLC"] = (recv) =>
            {
                if ((recv[24] & 0x10) != 0)
                {
                    return Db.GetText("Yes");
                }
                else
                {
                    return Db.GetText("No");
                }
            };

            DataStreamCalc["LSH"] = (recv) =>
            {
                if ((recv[24] & 0x80) != 0)
                {
                    return Db.GetText("Yes");
                }
                else
                {
                    return Db.GetText("No");
                }
            };

            DataStreamCalc["ICR"] = (recv) =>
            {
                if ((recv[24] & 0x04) != 0)
                {
                    return Db.GetText("Yes");
                }
                else
                {
                    return Db.GetText("No");
                }
            };

            DataStreamCalc["FISA"] = (recv) =>
            {
                return string.Format("{0}", (recv[51] * 256 + recv[52]) * 15 / 32 - 180);
            };

            DataStreamCalc["AF"] = (recv) =>
            {
                return string.Format("{0}", (recv[25] * 256 + recv[26]) / 64);
            };

            DataStreamCalc["FI"] = (recv) =>
            {
                return string.Format("{0}", (recv[59] * 256 + recv[60]) / 250);
            };

            DataStreamCalc["FFHR"] = (recv) =>
            {
                return string.Format("{0}", recv[43] / 256);
            };

            DataStreamCalc["TIS"] = (recv) =>
            {
                return string.Format("{0}", (recv[49] * 256 + recv[50]) - 32768);
            };

            DataStreamCalc["MES"] = (recv) =>
            {
                return string.Format("{0}", recv[47] * 256 + recv[48]);
            };

            DataStreamCalc["FPS"] = (recv) =>
            {
                switch (recv[53])
                {
                    case 1:
                        return Db.GetText("Close");
                    case 2:
                        return Db.GetText("Open");
                    case 3:
                        return Db.GetText("The Beginning of the Pump");
                    default:
                        return "";
                }
            };

            DataStreamCalc["ICCT"] = (recv) =>
            {
                return string.Format("{0}", (recv[57] * 256 + recv[58]) / 250);
            };

            DataStreamCalc["TAC"] = (recv) =>
            {
                return string.Format("{0}", recv[64] - 40);
            };

            DataStreamCalc["OIM"] = (recv) =>
            {
                switch (recv[16])
                {
                    case 1:
                        return Db.GetText("Ban");
                    case 2:
                        return Db.GetText("Static");
                    case 3:
                        return Db.GetText("2 Stoke");
                    case 4:
                        return Db.GetText("4 Stoke");
                    case 5:
                        return Db.GetText("4 Stoke Undetermined Phase");
                    default:
                        return "";
                }
            };

            DataStreamCalc["DFCF"] = (recv) =>
            {
                if ((recv[23] & 0x20) != 0)
                {
                    return Db.GetText("Yes");
                }
                else
                {
                    return Db.GetText("No");
                }
            };

            DataStreamCalc["ATUAP"] = (recv) =>
            {
                return string.Format("{0}", recv[31] * 256 + recv[32]);
            };

            DataStreamCalc["ATAF"] = (recv) =>
            {
                return string.Format("{0}", (recv[27] * 256 + recv[28]) / 64);
            };

            DataStreamCalc["SM"] = (recv) =>
            {
                if ((recv[44] & 0x01) != 0)
                {
                    return Db.GetText("Phase");
                }
                else
                {
                    return Db.GetText("Undetermined Phase");
                }
            };

            DataStreamCalc["ISAAPICF"] = (recv) =>
            {
                return string.Format("{0}", (recv[14] * 15) / 32 - 30);
            };

            DataStreamCalc["ICPLTECDV"] = (recv) =>
            {
                return string.Format("{0}", (recv[7] * 256 + recv[8]) * 5 / 1024);
            };

            DataStreamCalc["BAFV"] = (recv) =>
            {
                return string.Format("{0}", (recv[4] * 256 + recv[5]) / 1024);
            };

            DataStreamCalc["MO"] = (recv) =>
            {
                if ((recv[23] & 0x40) != 0)
                {
                    return Db.GetText("Yes");
                }
                else
                {
                    return Db.GetText("No");
                }
            };
        }

        private void ActiveTestInit()
        {
            ActiveTests = new Dictionary<string, ActiveTest>();
            ActiveTests[Db.GetText("Injector")] = (on) =>
            {
                byte[] cmd = Db.GetCommand("Activate Injector");

                if (!on)
                {
                    cmd[3] = 0x00;
                }
                else
                {
                    cmd[3] = 0x01;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Db.GetText("Injector Off Test Finish");
                    return Db.GetText("Injector On Test Finish");
                }
                throw new IOException(Db.GetText("Active Test Fail"));
            };

            ActiveTests[Db.GetText("Ignition Coil")] = (on) =>
            {
                byte[] cmd = Db.GetCommand("Activate Ignition Coil");

                if (!on)
                {
                    cmd[3] = 0x00;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Db.GetText("Ignition Coil Off Test Finish");
                    return Db.GetText("Ignition Coil On Test Finish");
                }
                throw new IOException(Db.GetText("Active Test Fail"));
            };

            ActiveTests[Db.GetText("Fuel Pump")] = (on) =>
            {
                byte[] cmd = Db.GetCommand("Activate The Fuel Pump");

                if (!on)
                {
                    cmd[3] = 0x00;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Db.GetText("Fuel Off Test Finish");
                    return Db.GetText("Fuel On Test Finish");
                }
                throw new IOException(Db.GetText("Active Test Fail"));
            };
        }

        public Dictionary<string, string> ReadTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Read DTC By Status");
            byte[] result = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);
            if (result == null || result[0] != 0x50)
            {
                throw new IOException(Db.GetText("Read Trouble Code Fail"));
            }

            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);

            if (result == null || result[0] != 0x58)
            {
                throw new IOException(Db.GetText("Read Trouble Code Fail"));
            }

            int dtcNum = Convert.ToInt32(result[1]);

            Dictionary<string, string> tcs = new Dictionary<string, string>();

            if (dtcNum == 0)
            {
                throw new IOException(Db.GetText("None Trouble Code"));
            }

            for (int i = 0; i < dtcNum; i++)
            {
                string code = Utils.CalcStdObdTroubleCode(result, i, 3, 2);
                string content = Db.GetTroubleCode(code);
                tcs.Add(code, content);
            }

            return tcs;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Clear Trouble Code1");
            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (recv == null || recv[0] != 0x50)
            {
                throw new IOException(Db.GetText("Clear Trouble Code Fail"));
            }

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);


            if (recv == null || recv[0] != 0x54)
            {
                throw new IOException(Db.GetText("Clear Trouble Code Fail"));
            }

        }

        public void ReadDataStream(Core.LiveDataVector vec)
        {
            byte[] cmd = Db.GetCommand("Read Data By Local Identifier1");
            stopReadDataStream = false;

            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);
            if (recv == null || recv[0] != 0x50)
                throw new IOException(Db.GetText("Communication Fail"));

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (recv == null)
            {
                Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
                Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
                throw new IOException(Db.GetText("Communication Fail"));
            }
            Task task = Task.Factory.StartNew(() =>
            {
                while (!stopReadDataStream)
                {
                    Thread.Sleep(50);
                    Thread.Yield();
                    recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                }
            });
            while (!stopReadDataStream)
            {
                int i = vec.NextShowedIndex();
                vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
                Thread.Sleep(10);
                Thread.Yield();
                //byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                //if (recv == null)
                //    throw new IOException(Db.GetText("Communication Fail"));
                //for (int i = 0; i < vec.ShowedCount; i++)
                //{
                //    int j = vec.NextShowedIndex();
                //    vec[j].Value = DataStreamCalc[vec[j].ShortName](recv);
                //}
                //int i = vec.NextShowedIndex();
                //if (recv == null)
                //{
                //    continue;
                //}
                //// calc
                //vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
            }
            task.Wait();
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
        }

        public string Active(string mode, bool on)
        {
            byte[] buff = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (buff == null || buff[0] != 0x50)
                throw new IOException(Db.GetText("Active Test Fail"));

            string ret = ActiveTests[mode](on);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
            return ret;
        }

        public string ReadECUVersion()
        {
            byte[] cmd = Db.GetCommand("Version");

            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (recv == null || recv[0] != 0x50)
                throw new IOException(Db.GetText("Read ECU Version Fail"));

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);

            if (recv == null || recv[0] != 0x61)
                throw new IOException(Db.GetText("Read ECU Version Fail"));

            recv[0] = 0x20;
            recv[1] = 0x20;
            return Encoding.ASCII.GetString(recv);
        }
    }
}
