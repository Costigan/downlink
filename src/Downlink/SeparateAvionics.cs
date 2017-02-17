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
            TheCase = ModelCase.RailsDriving;
        }

        Dictionary<Tuple<int, float, float, float, float>, dynamic> _calculationCache = new Dictionary<Tuple<int, float, float, float, float>, dynamic>();
        public dynamic CachedCalculate(ModelCase mcase, float downlinkRate, float payloadBitRate, float driverDecisionTime, float scienceDecisionTime)
        {
            var tuple = Tuple.Create((int)mcase, downlinkRate, payloadBitRate, driverDecisionTime, scienceDecisionTime);
            dynamic val;
            if (_calculationCache.TryGetValue(tuple, out val))
                return val;
            val = Calculate(mcase, downlinkRate, payloadBitRate, driverDecisionTime, scienceDecisionTime);
            _calculationCache[tuple] = val;
            return val;
        }

        public dynamic Calculate(ModelCase mcase, float downlinkRate, float payloadBitRate, float driverDecisionTime, float scienceDecisionTime)
        {
            if (!IsBuilt) Build();
            TheCase = mcase;
            DownlinkRate = downlinkRate;
            PayloadAvionics.BitRate = payloadBitRate;
            DriverDecisionTime = driverDecisionTime;
            DOCScienceDecisionTime = scienceDecisionTime;
            Reset();
            RunInternal();
            //Console.Write('.');
            return (dynamic)Stats;
        }

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
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 40000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = DefaultPacketQueueSize });  // high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = DefaultPacketQueueSize });  // doc high pri
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = DefaultPacketQueueSize });  // doc  other

            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadHealth, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

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
    public class SeparateAvionicsAIM : SeparateAvionics
    {
        public SeparateAvionicsAIM() { TheCase = ModelCase.AIMDriving; }
    }
    public class SeparateAvionicsScience : SeparateAvionics
    {
        public SeparateAvionicsScience() { TheCase = ModelCase.ScienceDriving; }
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
