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
        // random helper functions

        // https://stackoverflow.com/questions/9527721/resize-text-size-of-a-label-when-the-text-gets-longer-than-the-label-size
        private void scaleFont(Label lab)
        {
            SizeF extent = TextRenderer.MeasureText(lab.Text, lab.Font);

            float hRatio = (lab.Height - (lab.Padding.Left + lab.Padding.Right + lab.Padding.All)) / extent.Height;
            float wRatio = (lab.Width - (lab.Padding.Top + lab.Padding.Bottom + lab.Padding.All)) / extent.Width;
            float ratio = (hRatio < wRatio) ? hRatio : wRatio;

            // use a fudge factor since we seem to miss a bit
            float newSize = lab.Font.Size * ratio * 0.95f;

            float minsize = 12;

            // even more fudge, ideally we should store this somewhere in the object
            if (lab.Equals(label7_disctitle))
                minsize = 9.5f;


            if (newSize < minsize)
                newSize = minsize; // set a minimum and hope multi-line wrapping is enough
            lab.Font = new Font(lab.Font.FontFamily, newSize, lab.Font.Style);
        }
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
