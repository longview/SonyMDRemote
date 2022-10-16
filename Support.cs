using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SonyMDRemote
{
    public partial class SonyMDRemote
    {

        public bool ByteEquality(byte[] a1, byte[] b1)
        {
            int i;
            if (a1.Length == b1.Length)
            {
                i = 0;
                while (i < a1.Length && (a1[i] == b1[i])) //Earlier it was a1[i]!=b1[i]
                {
                    i++;
                }
                if (i == a1.Length)
                {
                    return true;
                }
            }

            return false;
        }

        bool IsBitSet(byte b, int pos)
        {
            return ((b >> pos) & 1) != 0;
        }

        private string TrimNonAscii(string value)
        {
            string pattern = "[^ -~]*";
            Regex reg_exp = new Regex(pattern);
            return reg_exp.Replace(value, "");
        }

        // our received strings are either null or FF terminated depending on where in the sequence they are
        // so we need to handle both
        private string DecodeAscii(ref byte[] buffer, int start)
        {
            int count = Array.IndexOf<byte>(buffer, 0, start) - start;
            if (count < 0) count = Array.IndexOf<byte>(buffer, 0xff, start) - start;

            if (count < 0) count = (buffer.Length - start) - 1;
            return Encoding.ASCII.GetString(buffer, start, count);
        }
    }
}
