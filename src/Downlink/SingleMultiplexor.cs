using Priority_Queue;
using System.Collections.Generic;
using System.Linq;

namespace Downlink
{
    public class SingleMultiplexor : Model
    {
        // Input Variables

        public Rover Rover;
        public Driver Driver;
        public FrameGenerator FrameGenerator;
        public GroundSystem GroundSystem;
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

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 1f, 1f, 1f, 1f, 1f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[RoverHighPriorityVC].PacketQueue;
            PayloadHighPacketGenerator.Receiver = FrameGenerator.Buffers[PayloadHighPriority].PacketQueue;

            Rover.RoverImageReceiver = FrameGenerator.Buffers[RoverImageVC];
            Rover.RoverHighPriorityReceiver = FrameGenerator.Buffers[Model.RoverHighPriorityVC];
            Rover.DOCHighPriorityReceiver = FrameGenerator.Buffers[Model.PayloadHighPriorityImage];
            Rover.DOCLowPriorityReceiver = FrameGenerator.Buffers[Model.PayloadLowPriorityImage];
        }

        public override void Start()
        {
            base.Start();  // This starts all of the components            
            Enqueue(new Thunk(Time, () => Driver.SendDriveCommand()));
            if (CaptureStates)
                Enqueue(new CaptureState { Model = this, Delay = 1f });
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

        public override void PacketsInFlight(APID apid, out int packetCount, out int byteCount)
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
    }

    public class SingleMultiplexorRails : SingleMultiplexor
    {
        public SingleMultiplexorRails() { TheCase = ModelCase.Rails; }
    }

    public class SingleMultiplexorScience : SingleMultiplexor
    {
        public SingleMultiplexorScience() { TheCase = ModelCase.ScienceStation; }
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
