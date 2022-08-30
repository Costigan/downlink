using System;
using System.Collections.Generic;
using System.Linq;

namespace Downlink
{
    //  This is Model4 with the ability to change the payload and drive rates
    public class Model5 : SharedModel
    {
        public float TO_DRIVE_moving
        {
            get
            {
                return RoverHighPacketGenerator.BitsPerSecondMoving;
            }
            set
            {
                RoverHighPacketGenerator.BitsPerSecondMoving = value;
            }
        }
        public float TO_DRIVE_stopped { get { return RoverHighPacketGenerator.BitsPerSecondStopped; } set { RoverHighPacketGenerator.BitsPerSecondStopped = value; } }
        public float TO_PAYLOAD_moving { get { return PayloadHighPacketGenerator.BitsPerSecondMoving; } set { PayloadHighPacketGenerator.BitsPerSecondMoving = value; } }
        public float TO_PAYLOAD_stopped { get { return PayloadHighPacketGenerator.BitsPerSecondStopped; } set { PayloadHighPacketGenerator.BitsPerSecondStopped = value; } }
        public float[] FrameTimeouts = new float[] { float.MaxValue, 1f, 1f, 1f };
        public new PacketGenerator2 RoverHighPacketGenerator = new PacketGenerator2(), PayloadHighPacketGenerator = new PacketGenerator2();

        Dictionary<Tuple<int, float, float, float, float, float, float, Tuple<float, float>>, dynamic> _cache = new Dictionary<Tuple<int, float, float, float, float, float, float, Tuple<float, float>>, dynamic>();
        public dynamic CachedCalculate3(
            ModelCase mcase,
            float downlinkRate,
            float driverDecisionTime,
            float scienceDecisionTime,
            float to_drive_moving,
            float to_drive_stopped,
            float to_payload_moving,
            float to_payload_stopped,
            float nav_compression = 4f
            )
        {
            var tuple = CreateForCache((int)mcase, downlinkRate, driverDecisionTime, scienceDecisionTime, to_drive_moving, to_drive_stopped, to_payload_moving, to_payload_stopped, nav_compression);
            dynamic val;
            if (_cache.TryGetValue(tuple, out val))
                return val;

            var m = Activator.CreateInstance(GetType()) as Model5;
            m.Build();
            m.TheCase = mcase;
            m.DownlinkRate = downlinkRate;
            m.DriverDecisionTime = driverDecisionTime;
            m.DOCScienceDecisionTime = scienceDecisionTime;
            m.TO_DRIVE_moving = to_drive_moving;
            m.TO_DRIVE_stopped = to_drive_stopped;
            m.TO_PAYLOAD_moving = to_payload_moving;
            m.TO_PAYLOAD_stopped = to_payload_stopped;
            m.NavCompression = nav_compression;
            m.RunInternal();
            //Console.Write('.');
            val= (dynamic)m.Stats;

            _cache[tuple] = val;
            return val;
        }

        Tuple<int, float, float, float, float, float, float, Tuple<float, float>> CreateForCache(int a, float b, float c, float d, float e, float f, float g, float h, float i)
            => new Tuple<int, float, float, float, float, float, float, Tuple<float, float>>(a, b, c, d, e, f, g, new Tuple<float, float>(h, i));

        public dynamic Calculate3(
            ModelCase mcase,
            float downlinkRate,
            float driverDecisionTime,
            float scienceDecisionTime,
            float to_drive_moving,
            float to_drive_stopped,
            float to_payload_moving,
            float to_payload_stopped
            )
        {
            var m = Activator.CreateInstance(GetType()) as Model5;
            m.Build();
            m.TheCase = mcase;
            m.DownlinkRate = downlinkRate;
            m.DriverDecisionTime = driverDecisionTime;
            m.DOCScienceDecisionTime = scienceDecisionTime;
            m.TO_DRIVE_moving = to_drive_moving;
            m.TO_DRIVE_stopped = to_drive_stopped;
            m.TO_PAYLOAD_moving = to_payload_moving;
            m.TO_PAYLOAD_stopped = to_payload_stopped;
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

            RoverHighPacketGenerator = new PacketGenerator2 { Model = this, Rover = Rover, APID = APID.RoverHealth, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator2 { Model = this, Rover = Rover, APID = APID.PayloadHealth, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = MOS;
            GroundSystem.Rover = Rover;
            MOS.Rover = Rover;

            // Wire up the packet generators through the frame generator
            var timeouts = FrameTimeouts;
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { Model = this, VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

            RoverHighPacketGenerator.Receiver = FrameGenerator.Buffers[1].PacketQueue;
            Rover.RoverImageReceiver = FrameGenerator.Buffers[3];

            PayloadHighPacketGenerator.Receiver = FrameGenerator.Buffers[1];
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
    public class Model5a : Model5 { }

    public class Model5b : Model5
    {
        public Model5b()
        {
            FrameTimeouts = new float[] { float.MaxValue, 3f, 3f, 3f };
        }
    }

    public class Model5c : Model5
    {
        public Model5c()
        {
            FrameTimeouts = new float[] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };
        }
    }

    public class Model5d : Model5
    {
        public Model5d()
        {
            TO_DRIVE_moving = 30000f;
            TO_DRIVE_stopped = TO_DRIVE_moving;
            TO_PAYLOAD_moving = PayloadWithoutDOCBitsPerSecond;
            TO_PAYLOAD_stopped = TO_PAYLOAD_moving;
            FrameTimeouts = new float[] { float.MaxValue, 1f, 1f, 1f };
        }
    }

    public class Model5e : Model5
    {
        public Model5e()
        {
            TO_DRIVE_moving = 30000f;
            TO_DRIVE_stopped = 20000f;
            TO_PAYLOAD_moving = PayloadWithoutDOCBitsPerSecond;
            TO_PAYLOAD_stopped = PayloadWithoutDOCBitsPerSecond / 2f;
            FrameTimeouts = new float[] { float.MaxValue, 1f, 1f, 1f };
        }
    }
}
