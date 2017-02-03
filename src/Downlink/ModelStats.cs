using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Downlink.Model;

namespace Downlink
{
    /// <summary>
    /// Stats for the base model class
    /// </summary>
    public class ModelStats
    {
        public string Name { get; set; }
        public int DriveCommandCount { get; set; }
        public int PacketCount { get; set; }
        public float DownlinkRate { get; set; }
        public float FinalPosition { get; set; }
        public float FinalTime { get; set; }
        public float SpeedMadeGood { get; set; }
        public long TotalFrameCount { get; set; }
        public long[] FrameCountPerVC { get; set; }
        public long[] PacketDropCountPerVC { get; set; }
        public long[] ByteDropCountPerVC { get; set; }

        public long TotalBytesReceivedInFrames { get; set; }
        public long TotalBytesReceivedInPackets { get; set; }
        public float BandwidthEfficiency { get; set; } // percent
        public long TotalBytesAvailable { get; set; } // # of bytes that could be downlinked in the time

        public PacketReport PacketReportForAllPackets { get; set; }
        public Dictionary<APID, PacketReport> PacketReportByAPID { get; set; }
        public long TotalPacketsInFlight { get; set; }
        public long TotalBytesInFlight { get; set; }
        public Dictionary<APID, int> PacketsInFlightByAPID { get; set; }
        public Dictionary<APID, int> BytesInFlightByAPID { get; set; }

        List<Packet> Packets { get; set; }

        public virtual void Report(List<string> rows)
        {
            using (var sw = new StringWriter())
            {
                Report(sw);
                rows.AddRange(Regex.Split(sw.ToString(), "\r\n|\r|\n"));
            }
        }

        public void Report(TextWriter o)
        {
            if (PacketCount < 1)
            {
                o.WriteLine(@"No packets were received.");
                return;
            }
            o.WriteLine(@"Report for {0}", Name);
            o.WriteLine(@"{0} drive commands were sent", DriveCommandCount);
            o.WriteLine(@"The rover drove {0} meters in {1} seconds", FinalPosition, FinalTime);
            o.WriteLine(@"SMG = {0} cm/sec", SpeedMadeGood.ToString("F3").PadLeft(10));
            o.WriteLine();

            o.WriteLine(@"{0} frames were received", TotalFrameCount.ToString().PadLeft(10));
            for (var i = 0; i < PacketDropCountPerVC.Length; i++)
                o.WriteLine(@"{0} VC{1} frames were received ({2}%), {3} packets were dropped entering this vc packet queue ({4} bytes)",
                    FrameCountPerVC[i].ToString().PadLeft(10),
                    i,
                    (FrameCountPerVC[i] / (float)TotalFrameCount).ToString("F2").PadLeft(6),
                    PacketDropCountPerVC[i],
                    ByteDropCountPerVC[i]);
            o.WriteLine();
            o.WriteLine(@"{0} bytes were received in all frames", TotalBytesReceivedInFrames.ToString().PadLeft(10));
            o.WriteLine(@"{0} bytes were received in all packets", TotalBytesReceivedInPackets.ToString().PadLeft(10));
            o.WriteLine(@"The bandwidth efficiency was {0}%",
                (TotalBytesReceivedInPackets / (float)TotalBytesReceivedInFrames).ToString("F2").PadLeft(6));
            o.WriteLine();
            o.WriteLine(@"Packet report for all apids:");
            PacketReportForAllPackets.Report(o);
            foreach (var apid in PacketReportByAPID.Keys)
            {
                o.WriteLine();
                o.WriteLine(@"Packet report for {0}", ((APID)apid).ToString());
                PacketReportByAPID[apid].Report(o);
            }
            o.WriteLine();
            o.WriteLine(@"Packets in flight report:");
            o.WriteLine(@"APID    Packets      Bytes");
            o.WriteLine(@"All    {0:D8}   {1:D8}", TotalPacketsInFlight, TotalBytesInFlight);
            foreach (var apid in PacketsInFlightByAPID.Keys)
                o.WriteLine(@"{0:S6}   {1:D8}   {2:D8}", ((APID)apid).ToString(), PacketsInFlightByAPID[apid], BytesInFlightByAPID[apid]);
        }

        public PacketReport GetPacketReport(IEnumerable<Packet> packets)
        {
            var lst = packets.ToList();
            return new PacketReport
            {
                Count = lst.Count,
                MinLatency = lst.SafeMin(p => p.Latency),
                AvgLatency = lst.SafeAverage(p => p.Latency),
                MaxLatency = lst.SafeMax(p => p.Latency),
                LatencyStdDev = lst.StandardDeviation(p => p.Latency)
            };
        }
    }

    public class PacketReport
    {
        public int Count { get; set; }
        public double MinLatency { get; set; }
        public double AvgLatency { get; set; }
        public double MaxLatency { get; set; }
        public double LatencyStdDev { get; set; }

        public void Report(TextWriter o)
        {
            o.WriteLine(@"Count={0}", Count);
            o.WriteLine(@"Latency (min,avg,max)={0:F3},{1:F3},{2:F3} sec", MinLatency, AvgLatency, MaxLatency);
            o.WriteLine(@"Latency StdDev={0:F3} sec", LatencyStdDev);
        }
    }
}
