using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonyMDRemote
{
    public partial class SonyMDRemote
    {
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


    }
}
