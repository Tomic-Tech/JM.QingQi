using System;
using System.Collections.Generic;
using System.Text;

namespace JM.QingQi.Vehicle
{
    public static class Manager
    {
        private static Core.LiveDataVector liveDataVector;

        static Manager()
        {
            Diag.BoxFactory.Instance.Version = Diag.BoxVersion.W80;
            Diag.BoxFactory.Instance.StreamType = Diag.StreamType.SerialPort;
        }

        public static Diag.ICommbox Commbox
        {
            get { return Diag.BoxFactory.Instance.Commbox; }
        }

        public static Core.LiveDataVector LiveDataVector
        {
            get { return liveDataVector; }
            set { liveDataVector = value; }
        }

        public static string ForamtECUVersion(string hex)
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("ECU");

            //            for (int i = 0; i < hex.Length; i += 2)
            //            {
            //                string e = hex.Substring(i, 2);
            //                byte h = Convert.ToByte(e, 16);
            //                char c = Convert.ToChar(h);
            //                if (Char.IsLetterOrDigit(c))
            //                   ret.Append(c);
            //            }
            for (int i = 0; i < 6; i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                    ret.Append(c);
            }
            ret.Append("-");

            for (int i = 6; i < 14; i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                    ret.Append(c);
            }

            ret.Append("\nV");

            for (int i = 16; i < 28; i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                    ret.Append(c);
            }

            return ret.ToString();
            //return hex.ToString();
        }
    }
}
