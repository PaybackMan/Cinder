
using System;
namespace Orient.Client
{
    public class ORID : IConvertible
    {
        static readonly char[] colon = new char[] {':'};
        public short ClusterId { get; set; }
        public long ClusterPosition { get; set; }
        public string RID 
        {
            get
            {
                return string.Format("#{0}:{1}",  ClusterId , ClusterPosition);
            }

            set
            {
                int offset = 1;
                ClusterId = (short)FastParse(value, ref offset);
                offset += 1;
                ClusterPosition = FastParse(value, ref offset);

                //ClusterId = short.Parse(split[0].Substring(1));
                //ClusterPosition = long.Parse(split[1]);
            } 
        }

        long FastParse(string s, ref int offset)
        {
            long result = 0;
            short multiplier = 1;
            if (s[offset] == '-')
            {
                offset++;
                multiplier = -1;
            }

            while (offset < s.Length)
            {
                int iVal = s[offset] - '0';
                if (iVal < 0 || iVal > 9)
                    break;
                result = result * 10 + iVal;
                offset++;
            }

            return (result * multiplier);
        }

        public ORID()
        {

        }

        public ORID(ORID other)
        {
            ClusterId = other.ClusterId;
            ClusterPosition = other.ClusterPosition;
        }

        public ORID(short clusterId, long clusterPosition)
        {
            ClusterId = clusterId;
            ClusterPosition = clusterPosition;
        }

        public ORID(string orid)
        {
            RID = orid;
        }

        public ORID(string source, int offset)
        {
            if (source[offset] == '#')
                offset++;
            ClusterId = (short)FastParse(source, ref offset);
            offset += 1;
            ClusterPosition = FastParse(source, ref offset);
        }

        public override string ToString()
        {
            return RID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // if parameter cannot be cast to ORID return false.
            ORID orid = obj as ORID;
            
            if (orid == null)
            {
                return false;
            }

            return ClusterId == orid.ClusterId && ClusterPosition == orid.ClusterPosition;
        }

        public override int GetHashCode()
        {
            return (ClusterId * 17) ^ ClusterPosition.GetHashCode();
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType.Equals(typeof(string)))
                return ToString(provider);
            else throw new NotSupportedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public static implicit operator string(ORID orid)
        {
            return orid.ToString();
        }

        public static implicit operator ORID (string orid)
        {
            return new ORID(orid);
        }
    }
}
