using System;
using System.Collections.Generic;
using System.Text;

namespace JM.QingQi.Vehicle
{
    internal class ResourceManager
    {
        private static ResourceManager instance;

        static ResourceManager()
        {
            instance = new ResourceManager();
        }

        public static ResourceManager Instance
        {
            get { return instance; }
        }

        private Core.LiveDataVector liveDataVector;

        private ResourceManager()
        {
            Diag.BoxFactory.Instance.Version = Diag.BoxVersion.W80;
            Diag.BoxFactory.Instance.StreamType = Diag.StreamType.SerialPort;
        }

        public Diag.ICommbox Commbox
        {
            get { return Diag.BoxFactory.Instance.Commbox; }
        }

        public Core.LiveDataVector LiveDataVector
        {
            get { return liveDataVector; }
            set { liveDataVector = value; }
        }
    }
}
