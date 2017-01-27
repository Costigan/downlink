using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    public class SeparateAvionics : Model
    {
        public Rover Rover;
        public Driver Driver;
        public PriorityPacketQueue PayloadAvionics;
        public FrameGenerator FrameGenerator;
        public GroundSystem GroundSystem;
        public PacketGenerator RoverHighPacketGenerator, PayloadHighPacketGenerator;

        public SeparateAvionics()
        {
            TheCase = ModelCase.Rails;
        }

        public override void Build()
        {
            // Create the components
            Rover = new Rover { Model = this, RoverImageVC = RoverImageVC };
            Driver = new Driver { Model = this, };
            GroundSystem = new GroundSystem { Model = this, };

            FrameGenerator = new FrameGenerator { Model = this, DownlinkRate = DownlinkRate };
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 80000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // doc high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // doc  other

            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadGeneral, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = Driver;
            GroundSystem.Rover = Rover;
            Driver.Rover = Rover;

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 1f, 1f, 1f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[RoverHighPriorityVC].PacketQueue;
 
            Rover.RoverImageReceiver = FrameGenerator.Buffers[RoverImageVC];
            Rover.RoverHighPriorityReceiver = FrameGenerator.Buffers[Model.RoverHighPriorityVC];

            PayloadHighPacketGenerator.Receiver = PayloadAvionics.Queues[0];
            Rover.DOCHighPriorityReceiver = PayloadAvionics.Queues[1];
            Rover.DOCLowPriorityReceiver = PayloadAvionics.Queues[2];
            PayloadAvionics.Receiver = FrameGenerator.Buffers[PayloadHighPriority];
        }

        public override void Start()
        {
            base.Start();  // This starts all of the components            
            Enqueue(new Thunk(Time, () => Driver.SendDriveCommand()));
            //if (CaptureStates)
            //    Enqueue(new CaptureState { Model = this, Delay = 1f });
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

    public class SeparateAvionicsRails : SeparateAvionics { }
    public class SeparateAvionicsScience : SeparateAvionics
    {
        public SeparateAvionicsScience() { TheCase = ModelCase.ScienceStation; }
    }
}
