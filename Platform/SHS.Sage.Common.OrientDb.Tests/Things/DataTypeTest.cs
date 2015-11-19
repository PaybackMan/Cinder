using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Things
{
    public class DataTypeTest : IThing
    {
        public string Id { get; set; }

        // Native Orient Types
        public bool Boolean { get; set; }
        public byte[] ByteArray { get; set; }
        public int[] IntArray { get; set; }
        public string[] StringArray { get; set; }
        public DateTime DateTime { get; set; }
        public Decimal Decimal { get; set; }
        public Double Double { get; set; }
        public float Float { get; set; }
        public int Integer { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
        public string String { get; set; }


        // Translated .Net Types
        public byte Byte { get; set; } // binary
        public sbyte SByte { get; set; } // short
        public uint UInt { get; set; } // long
        public ushort UShort { get; set; } // int

    }
}
