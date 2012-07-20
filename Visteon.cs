using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JM.Core;
using JM.Diag;

namespace JM.QingQi
{
    internal class Visteon : AbstractECU
    {
        private ISO9141Options options;

        public Visteon(VehicleDB db, ICommbox commbox)
            : base(db, commbox)
        {
            Db.CMDCatalog = "Visteon";
            Db.LDCatalog = "Visteon";
            Db.TCCatalog = "Visteon";
            ProtocolInit();
            DataStreamInit();
        }

        private void ProtocolInit()
        {
            options = new ISO9141Options();
            options.AddrCode = 0x33;
            options.ComLine = 7;
            options.Header = 0x68;
            options.LLine = true;
            options.SourceAddress = 0xF1;
            options.TargetAddress = 0x6A;

            Protocol = Commbox.CreateProtocol(ProtocolType.ISO9141_2);

            if (Protocol == null)
                throw new Exception("Not Protocol");

            Pack = new ISO9141Pack();

            if (!Protocol.Config(options))
                throw new Exception("Protocol Configuration Fail");
        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();

            DataStreamCalc["ECT"] = (recv =>
            {
                return string.Format("{0}", recv[2] - 40);
            });

            DataStreamCalc["IMAP"] = (recv =>
            {
                return string.Format("{0}", recv[2] * 3);
            });

            DataStreamCalc["ER"] = (recv =>
            {
                return string.Format("{0}", ((recv[2] << 8) + recv[3]) * 0.25);
            });

            DataStreamCalc["ITA#1"] = (recv =>
            {
                return string.Format("{0}", (recv[2] / 2 - 64));
            });

            DataStreamCalc["IAT"] = (recv =>
            {
                return string.Format("{0}", (recv[2] - 40));
            });

            DataStreamCalc["ATP"] = (recv =>
            {
                return string.Format("{0}", recv[2] * 100 / 255);
            });

            DataStreamCalc["DTC"] = (recv =>
            {
                return Core.Utils.CalcStdObdTroubleCode(recv, 0, 0, 2);
            });
        }

        public Dictionary<string, string> ReadTroubleCode()
        {
            byte[] dtcNumberCmd = Db.GetCommand("Read DTC Number");
            byte[] readDtc = Db.GetCommand("Read DTC");

            byte[] result = Protocol.SendAndRecv(dtcNumberCmd, 0, dtcNumberCmd.Length, Pack);

            if (result == null)
                throw new IOException(Db.GetText("Read Trouble Code Fail"));

            int dtcNum = Convert.ToInt32(result[2] & 0x80);
            if (dtcNum == 0)
            {
                throw new IOException(Db.GetText("None Trouble Code"));
            }

            result = Protocol.SendAndRecv(readDtc, 0, readDtc.Length, Pack);
            if (result == null)
                throw new IOException(Db.GetText("Read Trouble Code Fail"));

            Dictionary<string, string> codes = new Dictionary<string, string>();
            for (int i = 0; i < dtcNum; i++)
            {
                string code = Utils.CalcStdObdTroubleCode(result, i, 2, 1);
                string content = Db.GetTroubleCode(code);
                codes.Add(code, content);
            }
            return codes;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Clear DTC");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (result == null)
                throw new IOException(Db.GetText("Clear Trouble Code Fail"));
        }

        public void ReadDataStream(Core.LiveDataVector vec)
        {
            stopReadDataStream = false;

            vec.DeployShowedIndex();

            while (!stopReadDataStream)
            {
                int i = vec.NextShowedIndex();
                byte[] cmd = Db.GetCommand(vec[i].CmdID);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    i++;
                    throw new IOException(Db.GetText("Communication Fail"));
                }
                // calc
                vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
            }
        }

        public void ReadFreezeFrame(Core.LiveDataVector vec)
        {
            vec.DeployShowedIndex();

            for (int i = 0; i < vec.ShowedCount; i++)
            {
                int j = vec.NextShowedIndex();
                byte[] cmd = Db.GetCommand(vec[i].CmdID);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    throw new IOException(Db.GetText("Communication Fail"));
                }
                // Cal
                vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
            }
        }
    }
}

