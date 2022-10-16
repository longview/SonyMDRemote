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
        // this is mostly how to talk to the MDSes

        byte[] MDS_TX_SetRemoteOn = new byte[] { 0x10, 0x03 };
        byte[] MDS_TX_SetRemoteOff = new byte[] { 0x10, 0x04 };

        byte[] MDS_TX_ReqStatus = new byte[] { 0x20, 0x20 };
        byte[] MDS_TX_ReqDiscData = new byte[] { 0x20, 0x21 };
        byte[] MDS_TX_ReqModelName = new byte[] { 0x20, 0x22 };
        byte[] MDS_TX_ReqTrackRecordDate = new byte[] { 0x20, 0x24 }; // next byte is track number
        byte[] MDS_TX_ReqTOCData = new byte[] { 0x20, 0x44, 0x01 };
        byte[] MDS_TX_ReqTrackTime = new byte[] { 0x20, 0x45, 0x01 }; // next byte is track number
        byte[] MDS_TX_ReqDiscName = new byte[] { 0x20, 0x48, 0x01 };
        byte[] MDS_TX_ReqTrackName = new byte[] { 0x20, 0x4A }; // next byte is track number
        byte[] MDS_TX_ReqDiscAndTrackNames = new byte[] { 0x20, 0x4C, 0x01 };
        byte[] MDS_TX_ReqRemainingRecordTime = new byte[] { 0x20, 0x54, 0x01 };
        byte[] MDS_TX_ReqTrackRemainingNameSize = new byte[] { 0x20, 0x55, 0x00 }; // next byte is track number

        // 6.42 DISC NAME WRITE
        // Playback must be stopped first
        // supports ASCII values 0, 0x20-0x5A, 0x5E-0x7A
        // should wait for WRITE PACKET RECEIVED message between writes
        // final namedata byte must be 0 to effect write
        // then finally WRITE COMPLETE?
        byte[] MDS_TX_WriteDiscName1 = new byte[] { 0x20, 0x70 }; // next byte is packet number, next 16 bytes is name data
        byte[] MDS_TX_WriteDiscName2 = new byte[] { 0x20, 0x71 }; // next byte is packet number, next 16 bytes is name data, null terminated

        // 6.43 TRACK NO. NAME WRITE
        // Playback must be stopped first
        // supports ASCII values 0, 0x20-0x5A, 0x5E-0x7A
        // should wait for WRITE PACKET RECEIVED message between writes
        // final namedata byte must be 0 to effect write
        // then finally WRITE COMPLETE
        byte[] MDS_TX_WriteTrackName1 = new byte[] { 0x20, 0x72 }; // next byte is track number, next 16 bytes is name data, null terminated
        byte[] MDS_TX_WriteTrackName2 = new byte[] { 0x20, 0x73 }; // next byte is packet number, next 16 bytes is name data, null terminated

        // basic transport controls
        byte[] MDS_TX_Play = new byte[] { 0x02, 0x01 };
        byte[] MDS_TX_Stop = new byte[] { 0x02, 0x02 };
        byte[] MDS_TX_PlayPause = new byte[] { 0x02, 0x03 };
        byte[] MDS_TX_Pause = new byte[] { 0x02, 0x06 };
        byte[] MDS_TX_FFW_REW_OFF = new byte[] { 0x00 };
        byte[] MDS_TX_Rewind = new byte[] { 0x02, 0x13 };
        byte[] MDS_TX_FastForward = new byte[] { 0x02, 0x14 };
        byte[] MDS_TX_NextTrack = new byte[] { 0x02, 0x16 };
        byte[] MDS_TX_PrevTrack = new byte[] { 0x02, 0x15 };
        byte[] MDS_TX_RecordArm = new byte[] { 0x02, 0x21 };
        byte[] MDS_TX_Eject = new byte[] { 0x02, 0x40 };
        byte[] MDS_TX_AutoPauseOn = new byte[] { 0x02, 0x81 };
        byte[] MDS_TX_AutoPauseOff = new byte[] { 0x02, 0x80 };

        // from addendum
        byte[] MDS_TX_RepeatOff = new byte[] { 0x02, 0xA0 };
        byte[] MDS_TX_RepeatAll = new byte[] { 0x02, 0xA1 };
        byte[] MDS_TX_Repeat1Tr = new byte[] { 0x02, 0xA2 };

        // track management
        byte[] MDS_TX_StartPlayAtTrack = new byte[] { 0x03, 0x42, 0x01 }; // next byte is track number
        byte[] MDS_TX_PausePlayAtTrack = new byte[] { 0x03, 0x43, 0x01 }; // next byte is track number, pauses at the start of specific track

        byte[] MDS_TX_EnableElapsedTimeTransmit = new byte[] { 0x07, 0x10 };
        byte[] MDS_TX_DisableElapsedTimeTransmit = new byte[] { 0x07, 0x11 };

        // issue a null-payload if payload is even multiple of 16

        // 7.11 STATUS DATA
        enum MDS_Status_D1
        {
            STOP = 0,
            PLAY = 1,
            PAUSE = 2,
            EJECT = 3,
            REC_PLAY = 4,
            REC_PAUSE = 5,
            rehearsal = 6,
            reserved = 7
        };

        // 7.11 STATUS DATA addendum
        enum MDS_Status_D2_Repeat
        {
            REPEAT_OFF,
            ALL_REPEAT,
            TRACK_REPEAT
        };

        // 7.11 STATUS DATA
        enum MDS_Status_D3_Source
        {
            Analog = 1,
            Optical = 3,
            Coaxial = 5,
            reserved = 7
        };

        // command queue object, list of raw commands to issue and what delay should be used before the next command
        List<Tuple<byte[], int>> commandqueue = new List<Tuple<byte[], int>>();
        List<Tuple<byte[], int>> commandqueue_priority = new List<Tuple<byte[], int>>();

        private void Transmit_MDS_Write(string name, byte tracknumber = 0)
        {
            if (!serialPort1.IsOpen)
            {
                return;
            }

            timer_Poll_Time.Stop();

            List<byte> txname = new List<byte>(name.Length);
            txname.AddRange(Encoding.ASCII.GetBytes(name));
            txname.Add(0);

            bool first = true;
            int count = 0;
            // work out how many iterations are needed
            while (txname.Count > 0)
            {
                count++;
                List<byte> txdata = new List<byte>(32);
                // header, 0x7E is PC to MD, 0x6E is MD to PC
                txdata.Add(0x7E);
                // length from header to terminator, up to 32 (0x20)
                // computed at the end of the function
                txdata.Add(0x00);
                // format type, fixed
                txdata.Add(0x05);
                // category, fixed
                txdata.Add(0x47);

                // add command and sequence number if required
                if (first && tracknumber == 0)
                {
                    txdata.AddRange(MDS_TX_WriteDiscName1);
                    first = false;
                    txdata.Add((byte)count);
                }
                else if (!first && tracknumber == 0)
                {
                    txdata.AddRange(MDS_TX_WriteDiscName2);
                    txdata.Add((byte)count);
                }
                else if (first)
                {
                    txdata.AddRange(MDS_TX_WriteTrackName1);
                    first = false;
                    txdata.Add((byte)tracknumber);
                }
                else if (!first)
                {
                    txdata.AddRange(MDS_TX_WriteTrackName2);
                    txdata.Add((byte)count);
                }



                int payloadcount = 0;
                // iterate over data until we reach the limit
                foreach (char s in txname)
                {
                    byte ss = (byte)s;
                    // remove illegal values
                    if (s != 0 && (s < 0x20 || (s > 0x5A && s < 0x5E) || s > 0x7A))
                        ss = (byte)' ';

                    payloadcount++;
                    txdata.Add(ss);
                    if (payloadcount == 16)
                        break;
                }
                txname.RemoveRange(0, payloadcount);

                // terminator
                txdata.Add(0xFF);

                txdata[1] = (byte)txdata.Count;

                if (txdata.Count > 32)
                    throw new ArgumentException("Transmission packet would be too big!");
                var tup = new Tuple<byte[], int>(txdata.ToArray(), 500);
                // ignore by default if payload is already present
                commandqueue_priority.Add(tup);
            }

            timer_Poll_Time.Start();

            timer_Poll_Time.Enabled = true;
        }

        private void Transmit_MDS_Message(byte[] data, int delay = 300, byte tracknumber = 0, bool allowduplicates = false, bool priority = false, bool batch = false, bool priorityqueue = false)
        {
            if (!serialPort1.IsOpen)
            {
                //return;
                if (!Try_Open_Serial_Port())
                {
                    return;
                }
            }
            List<byte> txdata = new List<byte>(20);
            // header, 0x7E is PC to MD, 0x6E is MD to PC
            txdata.Add(0x7E);
            // length from header to terminator, up to 32 (0x20)
            // computed at the end of the function
            txdata.Add(0x00);
            // format type, fixed
            txdata.Add(0x05);
            // category, fixed
            txdata.Add(0x47);


            // data
            txdata.AddRange(data);
            if (tracknumber > 0)
                txdata.Add(tracknumber);

            // terminator
            txdata.Add(0xFF);

            txdata[1] = (byte)txdata.Count;

            if (txdata.Count > 32)
                throw new ArgumentException("Transmission packet would be too big!");

            var tup = new Tuple<byte[], int>(txdata.ToArray(), delay);
            // ignore by default if payload is already present
            // but we insert the requested command at the end of the sequence, removing the previous command
            if (!allowduplicates)
            {
                var foundcmd = commandqueue.Find(item => ByteEquality(item.Item1, tup.Item1));
                if (foundcmd != null)
                    commandqueue.Remove(foundcmd);
            }

            if (priorityqueue)
            {
                if (!priority)
                    commandqueue_priority.Add(tup);
                else
                    commandqueue_priority.Insert(0, tup);
            }
            else
            {
                if (!priority)
                    commandqueue.Add(tup);
                else
                    commandqueue.Insert(0, tup);
            }


            if (!batch)
                timer_Poll_Time.Enabled = true;
            //serialPort1.Write(txdata.ToArray(), 0, txdata.Count);

            //Start_Timeout();
        }


        private void timer_Poll_Time_Tick(object sender, EventArgs e)
        {
            Tuple<byte[], int> nextcommand;

            // do priority commands first
            if (commandqueue_priority.Count > 0)
            {
                // pop off the first item, transmit, then reset the timer with the delay
                nextcommand = commandqueue_priority[0];
                commandqueue_priority.RemoveAt(0);

                AppendLog("PC sent priority: {0}, ASCII: {1}", BitConverter.ToString(nextcommand.Item1), TrimNonAscii(System.Text.Encoding.ASCII.GetString(nextcommand.Item1)));

                serialPort1.Write(nextcommand.Item1, 0, nextcommand.Item1.Length);

                timer_Poll_Time.Interval = nextcommand.Item2;
                return;
            }

            // empty queue
            if (commandqueue == null || commandqueue.Count == 0)
            {
                timer_Poll_Time.Enabled = false;
                return;
            }
            // just spin and don't issue new command if we're getting the disc data
            else if (_inforequest && timer_Poll_GetInfo.Enabled)
            {
                timer_Poll_Time.Interval = 2000;
                return;
            }


            // pop off the first item, transmit, then reset the timer with the delay
            nextcommand = commandqueue[0];
            commandqueue.RemoveAt(0);
#if DEBUG
            AppendLog("PC sent: {0}, ASCII: {1}", BitConverter.ToString(nextcommand.Item1), TrimNonAscii(System.Text.Encoding.ASCII.GetString(nextcommand.Item1)));
#endif
            serialPort1.Write(nextcommand.Item1, 0, nextcommand.Item1.Length);

            timer_Poll_Time.Interval = nextcommand.Item2;
        }

    }
}
