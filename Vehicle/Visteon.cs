using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JM.Core;
using JM.Diag;

namespace JM.QingQi.Vehicle
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

            if (!Protocol.Config(options) ||
                !Pack.Config(options))
                throw new Exception(JM.Core.SysDB.GetText("Communication Fail"));

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

        public List<TroubleCode> ReadTroubleCode()
        {
            byte[] dtcNumberCmd = Db.GetCommand("Read DTC Number");
            byte[] readDtc = Db.GetCommand("Read DTC");

            byte[] result = Protocol.SendAndRecv(dtcNumberCmd, 0, dtcNumberCmd.Length, Pack);

            if (result == null)
                throw new IOException(Db.GetText("Read Trouble Code Fail"));

            int dtcNum = Convert.ToInt32(result[2] & 0x7F);
            if (dtcNum == 0)
            {
                throw new IOException(Db.GetText("None Trouble Code"));
            }

            result = Protocol.SendAndRecv(readDtc, 0, readDtc.Length, Pack);
            if (result == null)
                throw new IOException(Db.GetText("Read Trouble Code Fail"));

            List<byte> dtcs = new List<byte>();
            for (int i = 0; i < result.Length; i++)
            {
                if (i % 7 == 0)
                {
                    continue;
                }
                dtcs.Add(result[i]);
            }

            result = dtcs.ToArray();
            List<TroubleCode> codes = new List<TroubleCode>();
            for (int i = 0; i < dtcNum; i++)
            {
                string code = Utils.CalcStdObdTroubleCode(result, i, 2, 0);
                string content = Db.GetTroubleCode(code);
                codes.Add(new TroubleCode(code, content));
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

            var items = vec.Items;

            while (!stopReadDataStream)
            {
                foreach (var item in items)
                {
                    byte[] cmd = Db.GetCommand(item.CmdID);
                    byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                    if (recv == null)
                    {
                        throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                    }
                    item.Value = DataStreamCalc[item.ShortName](recv);
                    System.Threading.Thread.Sleep(50);
                    if (stopReadDataStream)
                        break;
                }
                //int i = vec.NextShowedIndex();
                //byte[] cmd = Db.GetCommand(vec[i].CmdID);
                //byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                //if (recv == null)
                //{
                //    throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                //}
                //// calc
                //vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
                //System.Threading.Thread.Sleep(50);
            }
        }

        public void StaticDataStream(Core.LiveDataVector vec)
        {
            var items = vec.Items;

            foreach (var item in items)
            {
                byte[] cmd = Db.GetCommand(item.CmdID);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                }
                item.Value = DataStreamCalc[item.ShortName](recv);
                System.Threading.Thread.Sleep(50);
            }

            //for (int i = 0; i < vec.ShowedCount; i++)
            //{
            //    int index = vec.NextShowedIndex();
            //    byte[] cmd = Db.GetCommand(vec[i].CmdID);
            //    byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            //    if (recv == null)
            //    {
            //        throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
            //    }
            //    // calc
            //    vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
            //    System.Threading.Thread.Sleep(50);
            //}
        }

        public void ReadFreezeFrame(Core.LiveDataVector vec)
        {
            var items = vec.Items;

            foreach (var item in items)
            {
                byte[] cmd = Db.GetCommand(item.CmdID);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                }
                item.Value = DataStreamCalc[item.ShortName](recv);
                //System.Threading.Thread.Sleep(50);
            }

            //for (int i = 0; i < vec.ShowedCount; i++)
            //{
            //    int index = vec.ShowedIndex(i);
            //    byte[] cmd = Db.GetCommand(vec[index].CmdID);
            //    byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            //    if (recv == null)
            //    {
            //        throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
            //    }
            //    // Cal
            //    vec[index].Value = DataStreamCalc[vec[index].ShortName](recv);
            //}
        }
    }
}

