using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    public class SimpleModel100 : SharedModel
    {
        public dynamic Calculate(ModelCase mcase, float downlinkRate, float payloadBitRate, float driverDecisionTime, float scienceDecisionTime)
        {
            if (!IsBuilt) Build();
            TheCase = mcase;
            DownlinkRate = downlinkRate;
            DriverDecisionTime = driverDecisionTime;
            Reset();
            RunInternal();
            //Console.Write('.');
            return (dynamic)Stats;
        }

        public override void Build()
        {
            base.Build();
            // Create the components
            Rover = new Rover { Model = this, RoverImageVC = RoverImageVC };
            MOS = new MOSTeam { Model = this, };
            GroundSystem = new GroundSystem { Model = this, };

            FrameGenerator = new FrameGenerator { Model = this };

            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = MOS;
            GroundSystem.Rover = Rover;
            MOS.Rover = Rover;

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 1f, 1f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[RoverHighPriorityVC].PacketQueue;
            Rover.RoverImageReceiver = FrameGenerator.Buffers[RoverImageVC];

            Rover.DOCHighPriorityReceiver = new NullReceiver();
            Rover.DOCLowPriorityReceiver = new NullReceiver();
        }

        public override void Start()
        {
            base.Start();  // This starts all of the components            
            Enqueue(new Thunk(Time, () => MOS.SendDriveCommand()));
        }
    }

    public class SimpleModel400 : SimpleModel100
    {
        public SimpleModel400() { DownlinkRate = 400000f; }
    }

    public class SimpleModel600 : SimpleModel100
    {
        public SimpleModel600() { DownlinkRate = 600000f; }
    }

}
