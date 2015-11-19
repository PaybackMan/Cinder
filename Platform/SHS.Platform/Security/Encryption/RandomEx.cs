using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security.Encryption
{
    public static class RandomEx
    {
        public static void GetBytes(this Random r, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)r.Next(255);
            }
        }
    }
}
