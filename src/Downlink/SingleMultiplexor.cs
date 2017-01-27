using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    public class SingleMultiplexor : Model
    {
        // Input Variables

        public new Rover Rover;
        public new Driver Driver;
        public new FrameGenerator FrameGenerator;
        public new GroundSystem GroundSystem;
        public PacketGenerator RoverHighPacketGenerator, PayloadHighPacketGenerator;

        public override void Build()
        {
            // Create the components
            Rover = new Rover { Model = this, RoverImageVC = RoverImageVC };
            Driver = new Driver { Model = this, };
            GroundSystem = new GroundSystem { Model = this, };
            FrameGenerator = new FrameGenerator { Model = this, DownlinkRate = DownlinkRate };
            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadGeneral, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = Driver;
            GroundSystem.Rover = Rover;
            Driver.Rover = Rover;
            Rover.FrameGenerator = FrameGenerator;

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 1f, 1f, 1f, 1f, 1f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[RoverHighPriorityVC].PacketQueue;
            PayloadHighPacketGenerator.Receiver = FrameGenerator.Buffers[PayloadHighPriority].PacketQueue;
        }
        public override void Start()
        {
            FrameGenerator.Start();
            GroundSystem.Start();
            RoverHighPacketGenerator.Start();
            PayloadHighPacketGenerator.Start();
            Enqueue(new Thunk(Time, () => Driver.SendDriveCommand()));
            if (CaptureStates)
                Enqueue(new CaptureState { Model = this, Delay = 1f });
            Enqueue(1.5f, ModelDocGeneration);
        }

        // Runs at .2 hz while driving and 1 hz when stopped
        int _DocWaypointImageCount = 100;  // Start high so that waypoint images aren't generated for position 0
        void ModelDocGeneration()
        {
            if (Rover.IsDriving)
            {
                var p = new Packet { APID = APID.DOCProspectingImage, Length = DOCLowContrastNarrow, Timestamp = Time };
                FrameGenerator.Buffers[PayloadHighPriorityImage].Receive(p);
                //Message(@"  Sending driving doc image");
                _DocWaypointImageCount = 0;
                Enqueue(Time + 5f, ModelDocGeneration);
            }
            else
            {
                if (_DocWaypointImageCount < 9)
                {
                    var p = new DOCImage { APID = APID.DOCWaypointImage, Length = DOCAllLEDScale3, Timestamp = Time, SequenceNumber = _DocWaypointImageCount, RoverPosition = Rover.Position };
                    var vc = _DocWaypointImageCount < 4 ? PayloadHighPriorityImage : PayloadLowPriorityImage;
                    FrameGenerator.Buffers[vc].Receive(p);
                    Message(@"  Sending waypoint doc image {0} via VC{1}", _DocWaypointImageCount, vc);
                    _DocWaypointImageCount++;
                }
                Enqueue(Time + 1f, ModelDocGeneration);
            }
        }

        public override void Stop()
        {
            TheModel.Message("Stop simulation");
            if (GroundSystem.Packets.Count < 1)
            {
                Report(@"No packets were received.");
                return;
            }
            Report(@"Report for {0}", ModelName);
            Report(@"The rover drove {0} meters in {1} seconds", Rover.Position, Time);
            var smg = 100f * Rover.Position / Time;
            Report(@"SMG = {0} cm/sec", smg.ToString("F3").PadLeft(10));
            Report();

            Report(@"{0} frames were received", GroundSystem.FrameCount.ToString().PadLeft(10));
            for (var i = 0; i < FrameGenerator.Buffers.Count; i++)
                Report(@"{0} VC{1} frames were received ({2}%), {3} packets were dropped entering this vc packet queue ({4} bytes)",
                    GroundSystem.FrameCounter[i].ToString().PadLeft(10),
                    i,
                    (GroundSystem.FrameCounter[i] / (float)GroundSystem.FrameCount).ToString("F2").PadLeft(6),
                    FrameGenerator.Buffers[i].PacketQueue.DropCount,
                    FrameGenerator.Buffers[i].PacketQueue.ByteDropCount
                    );
            Report();
            Report(@"{0} bytes were received in all frames", GroundSystem.TotalBytesReceived.ToString().PadLeft(10));
            Report(@"{0} bytes were received in all packets", GroundSystem.TotalBytesInPackets.ToString().PadLeft(10));
            Report(@"The bandwidth efficiency was {0}%",
                (GroundSystem.TotalBytesInPackets / (float)GroundSystem.TotalBytesReceived).ToString("F2").PadLeft(6));
            Report();
            PacketReport(GroundSystem.Packets, "all APIDs");
            Report();
            var AllAPIDS = (int)APID.AllAPIDS;
            for (var i = 0; i <= AllAPIDS; i++)
            {
                PacketReport(GroundSystem.Packets.Where(p => p.APID == (APID)i), ((APID)i).ToString());
                Report();
            }

            Report(@"Report on packets in flight at time {0}", Time);
            for (var i = 0; i <= AllAPIDS; i++)
            {
                int packetCount, byteCount;
                PacketsInFlight((APID)i, out packetCount, out byteCount);
                Report("  APID={0} {1} {2}", ((APID)i).ToString().PadRight(20), packetCount.ToString().PadLeft(10), byteCount.ToString().PadLeft(10));
            }
        }

        public void PacketReport(IEnumerable<Packet> packets, string title)
        {
            var lst = packets.ToList();
            var packetCount = lst.Count;
            var averageLatency = lst.SafeAverage(p => p.Latency);
            var latencyStdDev = lst.StandardDeviation(p => p.Latency);
            var min = lst.SafeMin(p => p.Latency);
            var max = lst.SafeMax(p => p.Latency);
            Report(@"Packet Report for {0}", title);
            Report(@"  {0} packets were received", packetCount);
            Report(@"  latency min,avg,max = [{0:F3}, {1:F3}, {2:F3}] sec", min, averageLatency, max);
            Report(@"  latency stddev = {0:F3} sec", latencyStdDev);
        }

        private void PacketsInFlight(APID apid, out int packetCount, out int byteCount)
        {
            packetCount = 0;
            byteCount = 0;
            foreach (var vc in FrameGenerator.Buffers)
            {
                PacketsInFlight(apid, vc.Frame, ref packetCount, ref byteCount);
                PacketsInFlight(apid, vc.PacketQueue, ref packetCount, ref byteCount);
            }
            PacketsInFlight(apid, EventQueue, ref packetCount, ref byteCount);
        }

        void PacketsInFlight(APID apid, Frame f, ref int packetCount, ref int byteCount)
        {
            if (f == null) return;
            foreach (var pf in f.Fragments)
            {
                if (!MatchingAPID(apid, pf.Packet.APID))
                    continue;
                if (pf.IsFinal)
                    packetCount++;
                byteCount += pf.Length;
            }
        }

        void PacketsInFlight(APID apid, PacketQueue q, ref int packetCount, ref int byteCount)
        {
            q.PacketsInFlight(apid, ref packetCount, ref byteCount);
        }

        void PacketsInFlight(APID apid, SimplePriorityQueue<Event, float> q, ref int packetCount, ref int byteCount)
        {
            var events = new Stack<Event>();
            while (q.Count > 0)
            {
                var e = q.Dequeue();
                events.Push(e);
                if (e.Time >= Time)
                    continue;
                var fd = e as FrameDelivery;
                if (fd == null)
                    continue;
                PacketsInFlight(apid, fd.Frame, ref packetCount, ref byteCount);
            }
            while (events.Count > 0)
            {
                var e = events.Pop();
                q.Enqueue(e, e.Time);
            }
        }
    }

    public class Sample
    {
        public float Time;
        public int[] QueueLength { get; set; }
        public int[] QueueByteCount { get; set; }
        public int[] Drops { get; set; }

        public void Capture(SingleMultiplexor model)
        {
            Time = model.Time;
            QueueLength = model.FrameGenerator.Buffers.Select(q => q.PacketQueue.Count).ToArray();
            Drops = model.FrameGenerator.Buffers.Select(q => q.PacketQueue.DropCount).ToArray();
            QueueByteCount = model.FrameGenerator.Buffers.Select(q => q.PacketQueue.ByteCount).ToArray();
            model.StateSamples.Add(this);
        }
    }
}
