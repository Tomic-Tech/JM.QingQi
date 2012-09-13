using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using JM.Core;
using JM.Diag;

namespace JM.QingQi.Vehicle
{
    internal class Mikuni : AbstractECU
    {
        private Dictionary<int, byte[]> failureCmds;
        private Dictionary<int, DataCalcDelegate> failureCalc;
        private MikuniOptions options;

        public Mikuni(VehicleDB Db, ICommbox commbox)
            : base(Db, commbox)
        {
            Db.CMDCatalog = "Mikuni";
            Db.TCCatalog = "Mikuni";
            Db.LDCatalog = "Mikuni";

            FailureCmdsInit();

            ProtocolInit();

            DataStreamInit();
        }

        private void ProtocolInit()
        {
            Protocol = Commbox.CreateProtocol(ProtocolType.MIKUNI);

            if (Protocol == null)
            {
                throw new Exception("Not Protocol");
            }

            Pack = new MikuniPack();

            options = new MikuniOptions();
            options.Parity = MikuniParity.Even;

            if (!Protocol.Config(options))
            {
                throw new Exception(JM.Core.SysDB.GetText("Communication Fail"));
            }
        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();
            DataStreamCalc["ER"] = (recv) =>
            {
				return string.Format("{0:F0}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16)) * 500) / 256);
                //return string.Format("{0}", (Convert.ToUInt16(Encoding.ASCII.GetString(recv), 16) / 256) * 500);
            };
            DataStreamCalc["BV"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["TPS"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["MAT"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 256 - 50);
            };
            DataStreamCalc["ET"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 256 - 50);
            };
            DataStreamCalc["BP"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["MP"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["IT"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16)) * 15) / 256 - 22.5);
                //return string.Format("{0}", (Convert.ToUInt16(Encoding.ASCII.GetString(recv), 16) / 256) * 15 - 22.5);
            };
            DataStreamCalc["IPW"] = (recv) =>
            {
				return string.Format("{0:F0}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 2);
            };
            DataStreamCalc["TS"] = (recv) =>
            {
                if ((recv[0] & 0x40) != 0)
                {
                    return Db.GetText("Tilt");
                }
                else
                {
                    return Db.GetText("No Tilt");
                }
            };
            DataStreamCalc["ERF"] = (recv) =>
            {
				if ((Convert.ToUInt16(Encoding.ASCII.GetString(recv)) & 0x0001) == 1)
                {
                    return Db.GetText("Running");
                }
                else
                {
                    return Db.GetText("Stopped");
                }
            };
            DataStreamCalc["IS"] = (recv) =>
            {
                if ((recv[0] & 0x40) != 0)
                {
                    return Db.GetText("Idle");
                }
                else
                {
                    return Db.GetText("Not Idle");
                }
            };
        }

        private void FailureCmdsInit()
        {
            failureCmds = new Dictionary<int, byte[]>(15);
            failureCalc = new Dictionary<int, DataCalcDelegate>(15);

            failureCmds.Add(1, Db.GetCommand("Manifold Pressure Failure"));
            failureCalc.Add(1, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0040";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0080";
                }
                return "0000";
            });

            failureCmds.Add(2, Db.GetCommand("O2 Sensor Failure"));
            failureCalc.Add(2, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0140";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0180";
                }
                return "0000";
            });

            failureCmds.Add(3, Db.GetCommand("TPS Sensor Failure"));
            failureCalc.Add(3, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0240";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0280";
                }
                return "0000";
            });

            failureCmds.Add(4, Db.GetCommand("Sensor Source Failure"));
            failureCalc.Add(4, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0340";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0380";
                }
                return "0000";
            });

            failureCmds.Add(5, Db.GetCommand("Battery Voltage Failure"));
            failureCalc.Add(5, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0540";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0580";
                }
                return "0000";
            });

            failureCmds.Add(
                6,
                Db.GetCommand("Engine Temperature Sensor Failure")
            );
            failureCalc.Add(6, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0640";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0680";
                }
                return "0000";
            });

            failureCmds.Add(7, Db.GetCommand("Manifold Temperature Failure"));
            failureCalc.Add(7, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0740";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0780";
                }
                return "0000";
            });

            failureCmds.Add(8, Db.GetCommand("Tilt Sensor Failure"));
            failureCalc.Add(8, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0840";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0880";
                }
                return "0000";
            });

            failureCmds.Add(9, Db.GetCommand("DCP Failure"));
            failureCalc.Add(9, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2040";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2080";
                }
                return "0000";
            });

            failureCmds.Add(10, Db.GetCommand("Ignition Coil Failure"));
            failureCalc.Add(10, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2140";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2180";
                }
                return "0000";
            });

            failureCmds.Add(11, Db.GetCommand("O2 Heater Failure"));
            failureCalc.Add(11, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2240";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2280";
                }
                return "0000";
            });

            failureCmds.Add(12, Db.GetCommand("EEPROM Failure"));
            failureCalc.Add(12, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Write
                {
                    return "4040";
                }
                else if ((data & 0xE000) != 0) // Read
                {
                    return "4080";
                }
                return "0000";
            });

            failureCmds.Add(13, Db.GetCommand("Air Valve Failure"));
            failureCalc.Add(13, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2340";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2380";
                }
                return "0000";
            });

            failureCmds.Add(14, Db.GetCommand("SAV Failure"));
            failureCalc.Add(14, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2440";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2480";
                }
                return "0000";
            });

            failureCmds.Add(15, Db.GetCommand("CPS Failure"));
            failureCalc.Add(15, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0xE000) != 0)
                    return "0940";
                return "0000";
            });
        }

        public List<TroubleCode> ReadCurrentTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Synthetic Failure");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (result == null)
            {
                throw new IOException(Db.GetText("Read Trouble Code Fail"));
            }

            if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
            {
                List<TroubleCode> ret = new List<TroubleCode>();
                for (int i = 1; i <= 15; i++)
                {
                    result = Protocol.SendAndRecv(failureCmds[i], 0, failureCmds[i].Length, Pack);
                    if (result == null)
                    {
                        throw new IOException(Db.GetText("Read Trouble Code Fail"));
                    }

                    if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
                    {
                        string code = failureCalc[i](result);
                        string content = Db.GetTroubleCode(code);
                        ret.Add(new TroubleCode(code, content));
                    }
                }
                return ret;
            }
            throw new IOException(Db.GetText("None Trouble Code"));
        }

        public List<TroubleCode> ReadHistoryTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Failure History Pointer");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null)
            {
                throw new IOException(Db.GetText("Read Trouble Code Fail"));
            }

            List<TroubleCode> ret = new List<TroubleCode>();
            int pointer = Convert.ToInt32(Encoding.ASCII.GetString(result));

            for (int i = 0; i < 16; i++)
            {
                string name;
                int temp = pointer - i - 1;
                if (temp >= 0)
                {
                    name = string.Format("Failure History Buffer{0}", temp);
                }
                else
                {
                    name = string.Format("Failure History Buffer{0}", pointer + 15 - i);
                }
                cmd = Db.GetCommand(name);
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null)
                {
                    throw new IOException(Db.GetText("Read Trouble Code Fail"));
                }
                if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
                {
                    string code = Encoding.ASCII.GetString(result);
                    string content = Db.GetTroubleCode(code);
                    ret.Add(new TroubleCode(code, content));
                }
            }
            if (ret.Count == 0)
            {
                throw new IOException(Db.GetText("None Trouble Code"));
            }
            return ret;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Db.GetCommand("Failure History Clear");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Db.GetText("Clear Trouble Code Fail"));
            }

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            cmd = Db.GetCommand("Failure History Pointer");
            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
            {
                throw new IOException(Db.GetText("Clear Trouble Code Fail"));
            }

            for (int i = 0; i < 16; i++)
            {
                string name = string.Format("Failure History Buffer{0}", i);
                cmd = Db.GetCommand(name);
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
                {
                    throw new IOException(Db.GetText("Clear Trouble Code Fail"));
                }
            }
        }

        public string GetECUVersion ()
		{
			byte[] cmd = Db.GetCommand ("Read ECU Version 1");
			byte[] result = Protocol.SendAndRecv (cmd, 0, cmd.Length, Pack);
			if (result == null) {
				throw new IOException (Db.GetText ("Read ECU Version Fail"));
			}

			cmd = Db.GetCommand ("Read ECU Version 2");
			Array.Copy (result, 0, cmd, 2, 4);
			result = Protocol.SendAndRecv (cmd, 0, cmd.Length, Pack);
			if (result == null) {
				throw new IOException (Db.GetText ("Read ECU Version Fail"));
			}

			string hex = Encoding.ASCII.GetString (result);
			StringBuilder ret = new StringBuilder ();
			ret.Append ("ECU");

//            for (int i = 0; i < hex.Length; i += 2)
//            {
//                string e = hex.Substring(i, 2);
//                byte h = Convert.ToByte(e, 16);
//                char c = Convert.ToChar(h);
//                if (Char.IsLetterOrDigit(c))
//                   ret.Append(c);
//            }
			for (int i = 0; i < 6; i += 2) {
				string e = hex.Substring (i, 2);
				byte h = Convert.ToByte (e, 16);
				char c = Convert.ToChar (h);
				if (Char.IsLetterOrDigit (c))
					ret.Append (c);
			}
			ret.Append ("-");

			for (int i = 6; i < 14; i += 2) {
				string e = hex.Substring (i, 2);
				byte h = Convert.ToByte (e, 16);
				char c = Convert.ToChar (h);
				if (Char.IsLetterOrDigit (c))
					ret.Append (c);
			}

			ret.Append ("\nV");

			for (int i = 16; i < 28; i += 2)
			{
				string e = hex.Substring(i, 2);
				byte h = Convert.ToByte (e, 16);
				char c = Convert.ToChar (h);
				if (Char.IsLetterOrDigit(c))
					ret.Append (c);
			}

            return ret.ToString();
            //return hex.ToString();
        }

        public void TPSIdleSetting()
        {
            byte[] cmd = Db.GetCommand("Engine Revolutions");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null)
            {
                throw new IOException(Db.GetText("Read Engine RPM Fail"));
            }

            if (result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
            {
                throw new IOException(Db.GetText("Engine RPM Not Zero"));
            }

            cmd = Db.GetCommand("TPS Idle Adjustment");
            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Db.GetText("TPS Idle Setting Fail"));
            }
        }

        public void LongTermLearnValueZoneInitialization()
        {
            byte[] cmd = Db.GetCommand("Long Term Learn Value Zone Initialization");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Db.GetText("Long Term Learn Value Zone Initialization Fail"));
            }

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

            for (int i = 1; i < 11; i++)
            {
                string name = string.Format("Long Term Learn Value Zone_{0}", i);
                cmd = Db.GetCommand(name);
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '8' || result[3] != '0')
                    throw new IOException(Db.GetText("Long Term Learn Value Zone Initialization Fail"));
            }

        }

        public void ISCLearnValueInitialize()
        {
            byte[] cmd = Db.GetCommand("ISC Learn Value Initialization");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Db.GetText("ISC Learn Value Initialization Fail"));
            }
        }

        public void ReadDataStream(Core.LiveDataVector vec)
        {
            stopReadDataStream = false;

            var items = vec.Items;
            //int i = 0;

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
                    // calc
                    item.Value = DataStreamCalc[item.ShortName](recv);
                    if (stopReadDataStream)
                        break;
                }
                //byte[] cmd = Db.GetCommand(items[i].CmdID);
                //byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                //if (recv == null)
                //{
                //    throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                //}
                //// calc
                //items[i].Value = DataStreamCalc[vec[i].ShortName](recv);
                //i++;
                //if (i == items.Count)
                //{
                //    i = 0;
                //}
            }
        }

        public void StaticDataStream(Core.LiveDataVector vec)
        {
            vec.DeployShowedIndex();

            for (int i = 0; i < vec.ShowedCount; i++)
            {
                int index = vec.ShowedIndex(i);
                byte[] cmd = Db.GetCommand(vec[index].CmdID);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    throw new IOException(JM.Core.SysDB.GetText("Communication Fail"));
                }
                // calc
                vec[index].Value = DataStreamCalc[vec[index].ShortName](recv);
            }
        }
    }
}
