using System.Linq;

namespace Downlink
{
    public class SingleMultiplexor : SharedModel
    {
        // Input Variables

        public override void Build()
        {
            base.Build();
            // Create the components
            Rover = new Rover { Model = this, RoverImageVC = RoverImageVC };
            MOS = new MOSTeam { Model = this, };
            GroundSystem = new GroundSystem { Model = this, };
            FrameGenerator = new FrameGenerator { Model = this };
            RoverHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { Model = this, APID = APID.PayloadHealth, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = MOS;
            GroundSystem.Rover = Rover;
            MOS.Rover = Rover;

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
            Enqueue(new Thunk(Time, () => MOS.SendDriveCommand()));
            if (CaptureStates)
                Enqueue(new CaptureState { Model = this, Delay = 1f });
        }

        public override void Stop()
        {
            base.Stop();
        }
    }

    public class SingleMultiplexorRails : SingleMultiplexor
    {
        public SingleMultiplexorRails() { TheCase = ModelCase.RailsDriving; }
    }

    public class SingleMultiplexorScience : SingleMultiplexor
    {
        public SingleMultiplexorScience() { TheCase = ModelCase.ScienceDriving; }
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
