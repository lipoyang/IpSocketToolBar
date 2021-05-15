using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpSocketToolBar
{
    public class PacketPayload
    {

        public byte[] Data;

        public PacketPayload(byte[] data)
        {
            this.Data = data;
        }

        public PacketPayload(string stringData)
        {
            this.Data = Encoding.ASCII.GetBytes(stringData);
        }
    }
}
