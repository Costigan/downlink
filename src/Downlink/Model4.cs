using System;
using System.Collections.Generic;
using System.Linq;

namespace Downlink
{
    // This one corresponds to Howard's description of the VCs
    public class Model4 : SharedModel
    {
        public PacketGenerator NirvssPacketGenerator;
        public float[] FrameTimeouts = new float[] { 2f, 5f, 10f, 15f };

        Dictionary<Tuple<int, float, float, float, float>, dynamic> _calculationCache2 = new Dictionary<Tuple<int, float, float, float, float>, dynamic>();
        public dynamic CachedCalculate2(ModelCase mcase, float downlinkRate, float payloadBitRate, float driverDecisionTime, float scienceDecisionTime)
        {
            var tuple = Tuple.Create((int)mcase, downlinkRate, payloadBitRate, driverDecisionTime, scienceDecisionTime);
            dynamic val;
            if (_calculationCache2.TryGetValue(tuple, out val))
                return val;
            val = Calculate2(mcase, downlinkRate, payloadBitRate, driverDecisionTime, scienceDecisionTime);
            _calculationCache2[tuple] = val;
            return val;
        }

        public dynamic Calculate2(ModelCase mcase, float downlinkRate, float payloadBitRate, float driverDecisionTime, float scienceDecisionTime)
        {
            var m = Activator.CreateInstance(GetType()) as SeparateAvionics;
            m.Build();
            m.TheCase = mcase;
            m.DownlinkRate = downlinkRate;
            m.PayloadAvionics.BitRate = payloadBitRate;
            m.DriverDecisionTime = driverDecisionTime;
            m.DOCScienceDecisionTime = scienceDecisionTime;
            m.RunInternal();
            //Console.Write('.');
            return (dynamic)m.Stats;
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
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadHealth, BitsPerSecond = PayloadWithoutDOCBitsPerSecond - NIRVSSProspectingBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };
            NirvssPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadHealth, BitsPerSecond = NIRVSSProspectingBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = MOS;
            GroundSystem.Rover = Rover;
            MOS.Rover = Rover;

            // Wire up the packet generators through the frame generator
            var timeouts = FrameTimeouts;
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[0].PacketQueue;
            NirvssPacketGenerator.Receiver = FrameGenerator.Buffers[0].PacketQueue;
            Rover.RoverImageReceiver = FrameGenerator.Buffers[1];

            PayloadHighPacketGenerator.Receiver = FrameGenerator.Buffers[2];
            Rover.DOCHighPriorityReceiver = FrameGenerator.Buffers[3];
            Rover.DOCLowPriorityReceiver = FrameGenerator.Buffers[3];

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

    }
    public class Model4a : Model4 { }

    public class Model4b : Model4
    {
        public Model4b()
        {
            FrameTimeouts = new float[] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };
        }
    }

    public class Model4c : Model4
    {
        public Model4c()
        {
            FrameTimeouts = new float[] { 2f, 5f, 6f, 6f }; ;
        }
    }

    public class Model4d : Model4
    {
        public Model4d()
        {
            FrameTimeouts = new float[] { 1f, 1f, 100f, 100f }; ;
        }
    }
}
