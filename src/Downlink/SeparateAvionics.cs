using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    public class SeparateAvionics : SharedModel
    {
        public PriorityPacketQueue PayloadAvionics;

        public SeparateAvionics()
        {
            TheCase = ModelCase.Rails;
        }

        public override void Build()
        {
            // Create the components
            Rover = new Rover { Model = this, RoverImageVC = RoverImageVC };
            MOS = new MOSTeam { Model = this, };
            GroundSystem = new GroundSystem { Model = this, };

            FrameGenerator = new FrameGenerator { Model = this, DownlinkRate = DownlinkRate };
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 40000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // doc high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // doc  other

            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadGeneral, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = MOS;
            GroundSystem.Rover = Rover;
            MOS.Rover = Rover;

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 2f, 5f, 6f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[RoverHighPriorityVC].PacketQueue;

            Rover.RoverImageReceiver = FrameGenerator.Buffers[RoverImageVC];
            Rover.RoverHighPriorityReceiver = FrameGenerator.Buffers[Model.RoverHighPriorityVC];

            PayloadHighPacketGenerator.Receiver = PayloadAvionics.Queues[0];
            Rover.DOCHighPriorityReceiver = PayloadAvionics.Queues[1];
            Rover.DOCLowPriorityReceiver = PayloadAvionics.Queues[2];
            PayloadAvionics.Receiver = FrameGenerator.Buffers[PayloadHighPriority];

            //debugging
            //Rover.RoverImageReceiver = DevNull;
            //RoverHighPacketGenerator.Receiver = DevNull;
            //Rover.DOCHighPriorityReceiver = DevNull;
            //Rover.DOCLowPriorityReceiver = DevNull;
        }

        public override void Start()
        {
            base.Start();  // This starts all of the components            
            Enqueue(new Thunk(Time, () => MOS.SendDriveCommand()));
            //if (CaptureStates)
            //    Enqueue(new CaptureState { Model = this, Delay = 1f });
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void PacketsInFlight(APID apid, ref int packetCount, ref int byteCount)
        {
            base.PacketsInFlight(apid, ref packetCount, ref byteCount);
            foreach (var q in PayloadAvionics.Queues)
                PacketsInFlight(apid, q, ref packetCount, ref byteCount);
        }
    }

    public class SeparateAvionicsRails : SeparateAvionics { }
    public class SeparateAvionicsScience : SeparateAvionics
    {
        public SeparateAvionicsScience() { TheCase = ModelCase.ScienceStation; }
    }

    public class SeparateAvionicsRailsSingleMove : SeparateAvionics
    {
        public override void Build()
        {
            base.Build();
            MOS.MaximumCommandCount = 1;
        }
    }
}
