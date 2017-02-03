using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APID = Downlink.Model.APID;

namespace Downlink
{
    public class Component
    {
        private Model _Model;
        public Model Model { get { return _Model; } set
            {
                if (_Model != null)
                    _Model.Components.Remove(this);
                _Model = value;
                if (_Model != null && !_Model.Components.Contains(this))
                    _Model.Components.Add(this);
            }
        }
        public virtual void Build() { }
        public virtual void Start() { }
        public virtual void Stop() { }

        public void Message(string msg, params object[] args)
        {
            var msg1 = string.Format(msg, args);
            var fullmsg = string.Format(@"{0} {1}", Model.Time.ToString("F3").PadLeft(12), msg1);
            if (Model.PrintMessages)
                Console.WriteLine(fullmsg);
            else
                Model.EventMessages.Add(fullmsg);
        }

        public void Enqueue(Thunk t) { Model.Enqueue(t); }
        public void Enqueue(float t, Action a) { Model.Enqueue(t, a); }
        public void Enqueue(Event e) { Model.Enqueue(e); }
    }

    /// <summary>
    /// Component which generates packets of a single APID at a fixed rate
    /// Can be used to model a whole filter table if we don't care about packet size variations
    /// </summary>
    public class PacketGenerator : Component, Event
    {
        public float Time { get; set; }
        public APID APID;
        public int PacketSize;
        public float BitsPerSecond;
        public float Delay;
        public PacketReceiver Receiver;
        public float StartTimeOffset = 0f;
        public override void Start()
        {
            var bitsPerPacket = PacketSize * 8;
            var delay = bitsPerPacket / BitsPerSecond;
            var time = Model.Time + StartTimeOffset;
            Model.Enqueue(new PacketGenerator { Time = time, APID = APID, Delay = delay, PacketSize = PacketSize, Receiver = Receiver });
        }
        public void Execute(Model m)
        {
            var p = new Packet { APID = APID, Length = PacketSize, Timestamp = Time };
            Receiver.Receive(p);
            Time += Delay;
            m.Enqueue(this);
        }
    }

    /// <summary>
    /// Component that renames PacketGenerator to FilterTable
    /// </summary>
    public class FilterTable : PacketGenerator
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Bounded size packet queue; no backpressure
    /// </summary>
    public class PacketQueue : Component, PacketReceiver
    {
        public PacketQueueOwner Owner = null;
        public int DropCount = 0;
        public int ByteDropCount = 0;
        public int Size;
        public int Count => Queue.Count + Stack.Count;
        public bool IsEmpty => Count == 0;
        public int ByteCount;
        Queue<Packet> Queue = new Queue<Packet>();
        Stack<Packet> Stack = new Stack<Packet>();
        public bool Receive(Packet p)
        {
            if (Size > Queue.Count)
            {
                Queue.Enqueue(p);
                ByteCount += p.Length;
                if (Owner != null)
                    Owner.WakeUp();
            }
            else
            {
                DropCount++;
                ByteDropCount += p.Length;
            }
            return true;
        }
        public void PushBack(Packet p)
        {
            Stack.Push(p);
            ByteCount += p.Length;
        }
        public Packet Dequeue()
        {
            var p = Stack.Count > 0 ? Stack.Pop() : Queue.Count > 0 ? Queue.Dequeue() : null;
            if (p != null) ByteCount -= p.Length;
            return p;
        }
        public void PacketsInFlight(Model.APID pattern, ref int packetCount, ref int byteCount)
        {
            var a = Queue.ToArray();
            for (var i = 0; i < a.Length; i++)
            {
                if (!Downlink.Model.MatchingAPID(pattern, a[i].APID))
                    continue;
                packetCount++;
                byteCount += a[i].Length;
            }
            a = Stack.ToArray();
            for (var i = 0; i < a.Length; i++)
            {
                if (!Downlink.Model.MatchingAPID(pattern, a[i].APID))
                    continue;
                packetCount++;
                byteCount += a[i].Length;
            }
        }
    }

    public class PriorityPacketQueue : Component, Event, PacketQueueOwner
    {
        public PacketQueueOwner Owner = null;
        public PacketReceiver Receiver;
        public float BitRate { get; set; } = 100000f;
        public float Time { get; set; } = 0f;
        public Packet OutputPacket;
        public bool PacketInFlight => OutputPacket != null;
        public List<PacketQueue> Queues = new List<PacketQueue>();
        public void AddQueue(PacketQueue q)
        {
            Queues.Add(q);
            q.Owner = this;
        }
        public override void Start() { Model.Enqueue(this); }
        public void Execute(Model m)
        {
            if (Receiver == null || BitRate == 0f) return;
            if (OutputPacket != null)
            {
                if (OutputPacket.Received > Model.Time)
                {
                    Time = OutputPacket.Received;
                    Model.Enqueue(this);
                    return;
                }
                OutputPacket.Received = Model.Time;
                Receiver.Receive(OutputPacket);
                OutputPacket = null;
            }
            var p = NextPacket();
            if (p == null) return;

            var transmissionTime = p.Length * 8 / BitRate;
            Time += transmissionTime;
            OutputPacket = p;
            p.Received = Time;
            Model.Enqueue(this);
        }
        public Packet NextPacket()
        {
            for (int i = 0; i < Queues.Count; i++)
                if (!Queues[i].IsEmpty)
                    return Queues[i].Dequeue();
            return null;
        }
        public void WakeUp()
        {
            if (!PacketInFlight)
            {
                Time = Model.Time;
                Model.Enqueue(this);
            }
            if (Owner != null)
                Owner.WakeUp();
        }
    }

    public class VirtualChannelBuffer : Component, PacketReceiver, PacketQueueOwner
    {
        public PacketQueueOwner Owner;
        private PacketQueue _PacketQueue;
        public PacketQueue PacketQueue { get { return _PacketQueue; } set { _PacketQueue = value; _PacketQueue.Owner = this; } }
        public int VirtualChannel;
        public Frame Frame;
        public float WakeupTime { get; set; } = 0f;
        public float Timeout { get; set; } = 10f;
        public bool IsTimedOut => Model.Time > WakeupTime && ContainsData;
        public bool ContainsData => !PacketQueue.IsEmpty || (Frame != null && !Frame.IsEmpty);
        public override void Build() { PacketQueue.Build(); }
        public bool Receive(Packet p)
        {
            if (Owner != null) Owner.WakeUp();
            return PacketQueue.Receive(p);
        }
        public Frame TryToFillFrame()
        {
            if (Frame == null)
                Frame = new Frame { VirtualChannel = VirtualChannel };
            while (!Frame.IsFull)
            {
                Packet p = PacketQueue.Dequeue();
                if (p == null)
                    break;
                var f = p as PacketFragment;
                if (f != null)
                {   // use f, a PacketFragment
                    if (Frame.Capacity >= f.Length)
                    {   // can fit
                        Frame.Add(f);
                        //mhs
                        if (f.APID == APID.DOCProspectingImage)
                            Console.WriteLine(@"Writing last fragment of a DOCProspectingImage");
                    }
                    else
                    {
                        var frag1 = new PacketFragment { Packet = f.Packet, Length = Frame.Capacity, FragmentNumber = f.TotalFragments, TotalFragments = f.TotalFragments + 1 };
                        Frame.Add(frag1);
                        f.FragmentNumber = ++f.TotalFragments;
                        f.Length -= frag1.Length;
                        Debug.Assert(f.Length >= 0);
                        PacketQueue.PushBack(f);
                    }
                }
                else
                {   // use p, a Packet
                    if (Frame.Capacity >= p.Length)
                    {
                        Frame.Add(new PacketFragment { Packet = p, Length = p.Length });
                    }
                    else
                    {
                        var frag1 = new PacketFragment { Packet = p, Length = Frame.Capacity, FragmentNumber = 1, TotalFragments = 2 };
                        var frag2 = new PacketFragment { Packet = p, Length = p.Length - Frame.Capacity, FragmentNumber = 2, TotalFragments = 2 };
                        Frame.Add(frag1);
                        PacketQueue.PushBack(frag2);
                    }
                }
            }
            return Frame;
        }
        public void WakeUp() { if (Owner != null) Owner.WakeUp(); }
        public void UpdateTimeout()
        {
            WakeupTime = Model.Time + Timeout;
        }
    }

    public class FrameGenerator : Component, Event, PacketQueueOwner
    {
        public int FrameLength = 1115;
        public int FrameHeaderLength = 6 + 6;  // 6 bytes of header, 2 of error correction
        public float Time { get; set; } = 0f;
        public List<VirtualChannelBuffer> Buffers = new List<VirtualChannelBuffer>();
        public GroundSystem GroundSystem;
        public float FrameLatency = Model.RP15FrameLength * 8 / 100000f;  // init value
        public float DownlinkLatency = 10f + 1.3f + Model.RP15FrameLength * 8 / 100000f;  // init value
        public float _DownlinkRate = 100000f;
        public float DownlinkRate
        {
            get { return _DownlinkRate; }
            set
            {
                _DownlinkRate = value;
                FrameLatency = Model.RP15FrameLength * 8 / _DownlinkRate;
                DownlinkLatency = 10f + 1.3f + FrameLatency;
            }
        }
        public override void Build()
        {
            foreach (var b in Buffers) { b.Build(); }
        }
        public override void Start()
        {
            foreach (var b in Buffers) { b.Start(); }
            Model.Enqueue(this);
        }

        //mhs
        private List<Frame> FramesSent = new List<Frame>();

        public void Execute(Model m)
        {
            var frame = GetNextFrame();
            Debug.Assert(frame != null);

            //mhs
            //if (FramesSent.Contains(frame))
            //    Console.WriteLine("here");
            //FramesSent.Add(frame);

            //mhs
            //if (frame.VirtualChannel == PayloadHighPriorityImage)
            //    Console.WriteLine("Sending PayloadHighPriorityImage frame");

            if (frame.VirtualChannel != Model.IdleVC)
            {
                var buf = Buffers[frame.VirtualChannel];
                buf.UpdateTimeout();
                buf.Frame = null;
            }
            Enqueue(new Thunk(Time + Model.DownlinkLatency, () => GroundSystem.Receive(frame)));  // idle frame
            Time += FrameLatency;
            Model.Enqueue(this);
        }
        // Always returns a frame to send
        protected Frame GetNextFrame()
        {
            Time = Time;

            // Look for the first frame that has timed out
            foreach (var buffer in Buffers)
                if (buffer.IsTimedOut)
                {
                    //Message("VC{0} timing out", buffer.VirtualChannel);
                    return buffer.TryToFillFrame();
                }

            // Look for the first full buffer
            foreach (var buffer in Buffers)
            {
                var frame = buffer.TryToFillFrame();
                if (frame.IsFull)
                    return frame;
            }

            // Look for any content
            foreach (var buffer in Buffers)
            {
                var frame = buffer.TryToFillFrame();
                if (!frame.IsEmpty)
                    return frame;
            }
            return new Frame { VirtualChannel = Model.IdleVC };  // Idle Frame
        }
        public void WakeUp() { }
    }

    public class GroundSystem : Component, FrameReceiver
    {
        public MOSTeam Driver;
        public Rover Rover;
        public long TotalBytesReceived = 0L;
        public long TotalBytesInPackets = 0L;
        public long FrameCount = 0L;
        public long[] FrameCounter = new long[64]; // How should this be initialized?

        public List<Packet> Packets = new List<Packet>();
        public List<Packet> ImagesSeen = new List<Packet>(); // all images that any part of has been received

        //mhs
        private List<Frame> FramesSeen = new List<Frame>();

        public void Receive(Frame f)
        {
            //mhs
            //if (FramesSeen.Contains(f))
            //    Console.WriteLine("here");
            //FramesSeen.Add(f);
            //Message("Received frame from VC{0}", f.VirtualChannel);

            FrameCount++;
            FrameCounter[f.VirtualChannel]++;
            TotalBytesReceived += Model.RP15FrameLength;
            foreach (var frag in f.Fragments)
                if (frag.IsFinal)  // Assumes no packet loss, for now
                {
                    var packet = frag.Packet;
                    //mhs
                    //if (packet.APID == APID.DOCProspectingImage)
                    //Console.WriteLine("Ground received {0} timestamp={1}", packet.APID, packet.Timestamp);
                    TotalBytesInPackets += packet.Length;
                    packet.Received = Model.Time;
                    Receive(packet);
                }
        }

        public void Receive(Packet p)
        {
            var f = p as ImageFragment;
            if (f == null)
            {
                Handle(p);
                return;
            }
            if (!ImagesSeen.Contains(f.Packet))
                ImagesSeen.Add(f.Packet);
            if (f.FragmentNumber == f.TotalFragments)
                Handle(f.Packet);
        }

        public void Handle(Packet packet)
        {
            packet.Received = Model.Time;
            Packets.Add(packet);
            if (packet.APID == APID.RoverImagePair)
            {
                Message(@"Received RoverImagePair timestamp={0}", packet.Timestamp);
                Driver.EvalImage(packet);
            }
            if (packet.APID == APID.DOCWaypointImage)
            {
                var di = packet as DOCImage;
                if (di == null) return;
                Model.Message("Received DOC Waypoint image seq={0}", di.SequenceNumber);  // Bug: This should be the last of 4, not any DOC Waypoint image
                Driver.EvalImage(packet);  // Driver is really MOS
            }
        }
    }


    public class Rover : Component
    {
        public PacketReceiver RoverHighPriorityReceiver;
        public PacketReceiver RoverImageReceiver;
        public PacketReceiver DOCHighPriorityReceiver;
        public PacketReceiver DOCLowPriorityReceiver;
        public int RoverImageVC = -1;
        public bool IsDriving = false;
        public float Position = 0f;
        public override void Start()
        {
            base.Start();
            Enqueue(1.5f, ModelDocGeneration);
        }
        public void Drive()
        {
            IsDriving = true;
            Message("Rover starts driving");
            var delta = Model.DriveStep / Model.DriveSpeed;
            Enqueue(Model.Time + delta, () => StopDriving());
        }
        public void StopDriving()
        {
            IsDriving = false;
            Message("Rover stops driving");
            Position += Model.DriveStep;
            EnqueueLongPacket(RoverImageReceiver, new RoverImagePair { APID = APID.RoverImagePair, Length = Model.NavPayload, Timestamp = Model.Time, RoverPosition = Position });

            // DOC images are triggered in ModelDocGeneration
        }

        // Runs at .2 hz while driving and 1 hz when stopped
        bool HaveSentDocWaypointImages = true;  // Don't send for first waypoint
        void ModelDocGeneration()
        {
            if (IsDriving)
            {
                var p = new DOCImage { APID = APID.DOCProspectingImage, Length = Model.DOCLowContrastNarrow, Timestamp = Model.Time };
                EnqueueLongPacket(DOCHighPriorityReceiver, p);
                //Message(@"  Sending driving doc image");
                HaveSentDocWaypointImages = false;
                Enqueue(Model.Time + 5f, ModelDocGeneration);
            }
            else
            {
                if (!HaveSentDocWaypointImages)
                {
                    const int CountOfDocImagesPerWaypoint = 4;
                    for (var i = 0; i < CountOfDocImagesPerWaypoint; i++)
                    {
                        var p = new DOCImage { APID = APID.DOCWaypointImage, Length = Model.DOCAllLEDScale3, Timestamp = Model.Time, SequenceNumber = i, RoverPosition = Position };
                        var time = Model.Time + i;
                        Enqueue(time, () => EnqueueLongPacket(DOCHighPriorityReceiver, p));
                        Message(@"  Sending doc waypt img {0} at {1}", i, time);
                    }
                    HaveSentDocWaypointImages = true;
                }
                Enqueue(Model.Time + 1f, ModelDocGeneration);
            }
        }
        void EnqueueLongPacket(PacketReceiver r, Packet p)
        {
            const int fragmentSize = 4096;
            if (p.Length <= fragmentSize)
            {
                r.Receive(p);
            }
            else
            {
                var count = (int)Math.Ceiling(p.Length / (double)fragmentSize);
                for (var i = 1; i <= count; i++)
                    r.Receive(new ImageFragment { APID = p.APID, Length = fragmentSize, FragmentNumber = i, TotalFragments = count, Packet = p, Timestamp = Model.Time });
            }
        }
    }

    /// <summary>
    /// This models the driver and the NIRVSS camera operators
    /// </summary>
    public class MOSTeam : Component
    {
        public Rover Rover;
        public int CommandCount = 0;
        public int MaximumCommandCount = 4;
        public virtual void SendDriveCommand()
        {
            if (CommandCount >= MaximumCommandCount)
            {
                Model.StopRequest(@"Driver is ready by has exceeded max commands");
                return;
            }
            Model.Message("Driver sends drive command");
            CommandCount++;
            Enqueue(Model.Time + Model.UplinkLatency, () => Rover.Drive());
        }

        float DriverProposedCommandTime = -1f;
        bool WaitingForDriver => DriverProposedCommandTime < 0f;
        float NirvssProposedCommandTime = -1f;
        bool WaitingForNirvss => NirvssProposedCommandTime < 0f;

        public void EvalImage(Packet p)
        {
            switch (Model.TheCase)
            {
                case Model.ModelCase.Rails:
                    {
                        if (p is RoverImagePair)
                        {
                            var ri = p as RoverImagePair;
                            Model.Message("Driver starts image eval timestamp={0} pos={1}", ri.Timestamp, ri.RoverPosition);
                            // ProposedCommandTime is irrelevant in this case
                            Enqueue(Model.Time + Model.DriverDecisionTime, () => SendDriveCommand());
                        }
                        break;
                    }
                case Model.ModelCase.AIM:
                    {
                        if (p is RoverImagePair)
                        {
                            var ri = p as RoverImagePair;
                            Model.Message("Driver starts image eval timestamp={0} pos={1}", ri.Timestamp, ri.RoverPosition);
                            DriverMaybeSendCommand(Model.Time + Model.DriverDecisionTime);
                        }
                        else if (p is DOCImage)
                        {
                            var di = p as DOCImage;
                            if (di.SequenceNumber == 3)
                            {
                                Model.Message("Nirvss starts image eval timestamp={0} pos={1}", di.Timestamp, di.RoverPosition);
                                NirvssMaybeSendCommand(Model.Time + Model.DOCAIMDecisionTime);
                            }
                        }
                        break;
                    }
                case Model.ModelCase.ScienceStation:
                    {
                        if (p is RoverImagePair)
                        {
                            var ri = p as RoverImagePair;
                            Model.Message("Driver starts image eval timestamp={0} pos={1}", ri.Timestamp, ri.RoverPosition);
                            DriverMaybeSendCommand(Model.Time + Model.DriverDecisionTime);
                        }
                        else if (p is DOCImage)
                        {
                            var di = p as DOCImage;
                            if (di.SequenceNumber == 3)
                            {
                                Model.Message("Nirvss starts image eval timestamp={0} pos={1}", di.Timestamp, di.RoverPosition);
                                NirvssMaybeSendCommand(Model.Time + Model.DOCScienceDecisionTime);
                            }
                        }
                        break; ;
                    }
            }
        }

        void DriverMaybeSendCommand(float driverTime)
        {
            if (!WaitingForDriver)
                Model.StopImmediately(@"Drive received two images");
            if (WaitingForNirvss)
                DriverProposedCommandTime = driverTime;
            else
            {
                var maxtime = Math.Max(driverTime, NirvssProposedCommandTime);
                DriverProposedCommandTime = NirvssProposedCommandTime = -1f;
                Enqueue(maxtime, () => SendDriveCommand());
            }
        }

        void NirvssMaybeSendCommand(float nirvssTime)
        {
            if (!WaitingForNirvss)
                Model.StopImmediately(@"Nirvss received two triggers");
            if (WaitingForDriver)
                NirvssProposedCommandTime = nirvssTime;
            else
            {
                var maxtime = Math.Max(nirvssTime, DriverProposedCommandTime);
                DriverProposedCommandTime = NirvssProposedCommandTime = -1f;
                Enqueue(maxtime, () => SendDriveCommand());
            }
        }
    }
}
