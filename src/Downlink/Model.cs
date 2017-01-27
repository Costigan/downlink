using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using System.Diagnostics;

namespace Downlink
{
    // Case 4
    public class Model
    {
        #region Global Variables

        public bool CaptureStates = true;

        public static float DriveStep = 4.5f;
        public static float DriveSpeed = 0.01f;
        public static float UplinkLatency = 10f + 1.3f;
        public float DownlinkLatency;

        public static float FrameLatency;
        public static float GDSLatency = 0f;  // for now

        public static float DriverDecisionTime = 90f;

        private float _DownlinkRate = 100000f;
        public float DownlinkRate
        {
            get { return _DownlinkRate; }
            set
            {
                _DownlinkRate = value;
                FrameLatency = RP15FrameLength * 8 / _DownlinkRate;
                FrameTime = RP15FrameLength * 8f / _DownlinkRate;
                DownlinkLatency = 10f + 1.3f + FrameLatency;
            }
        }

        public const int RP15FrameLength = 1115; // bytes
        public const int RP15FrameOverheead = 12;
        public static float FrameTime;  // watch out!  This is shared.

        // Input variables (expected to change)
        public int PacketQueueSize = 1000;
        public float RoverHealthBitsPerSecond = 30000f;  // was TO_DRIVE 46669.9f, updated per Howard estimate

        public const int NavPayload = (int)(2097152f * 12f / 2f / 4f / 8f); // two images

        public const float AvionicsLowDataRate = 1491f;
        public const float AvionicsNominalDataRate = 2485f;
        public const float AvionicsHighDataRate = 7456f;
        public const float AvionicsProspectingBitsPerSecond = AvionicsLowDataRate;

        public const float VMLLowDataRate = 735.3f;
        public const float VMLNominalDataRate = 2329.2f;
        public const float VMLProspectingBitsPerSecond = VMLLowDataRate;

        public const float NSSLowDataRate = 190f;
        public const float NSSNominalDataRate = 760f;
        public const float NSSHighDataRate = 800f;
        public const float NSSProspectingBitsPerSecond = NSSHighDataRate;

        public const float NIRVSSProspectingBitsPerSecond = 11552f;

        public const float PayloadWithoutDOCBitsPerSecond = AvionicsProspectingBitsPerSecond + VMLProspectingBitsPerSecond + NSSProspectingBitsPerSecond + NIRVSSProspectingBitsPerSecond;

        // The size of various payload packets, in bytes.  Note that I should be rounding up, but I'm not, for now.
        // These come from Payload Data Rates per Activity_20Jan2017.xlsx
        public const int DOCHighResNarrow = 11184923 / 8;
        public const int DOCLowContrastNarrow = 116620 / 8;
        public const int DOCLowContrastMedium = 174875 / 8;
        public const int DOCLowContrastFull = 349600 / 8;                        // One every 2 sec
        public const int DOCSingleLEDScale3 = 524400 / 8;  // one every 2.5 sec
        public const int DOCAllLEDScale3 = 524400 / 8;
        public const int DOCAllLEDScale2 = 2097264 / 8;
        public const int DOCAllLEDScale0 = 33554544 / 8;

        public float NIRVSSEvalLatency = 90f;

        public const float EmergencyStopTime = 1000000f;

        public bool PrintMessages = false;
        public bool PrintReport = false;
        public List<string> EventMessages;
        public List<string> ReportMessages;

        public List<Sample> StateSamples = new List<Sample>();

        #endregion

        #region Model State Variables

        public static Model TheModel;
        public bool IsRunning;

        public SimplePriorityQueue<Event, float> EventQueue = new SimplePriorityQueue<Event, float>();
        public float Time = 0f;

        public bool StopRequest = false;

        public const int RoverHighPriorityVC = 0;
        public const int RoverLowPriorityVC = 0;
        public const int RoverImageVC = 1;
        public const int PayloadHighPriority = 2;
        public const int PayloadHighPriorityImage = 3;
        public const int PayloadLowPriorityImage = 4;
        public const int IdleVC = 63;

        // Components in all models

        public enum ModelCase { Rails, ScienceStation }
        public ModelCase TheCase = ModelCase.Rails;

        #endregion

        public enum APID { IdlePacket, RoverHealth, RoverImagePair, PayloadGeneral, DOCProspectingImage, DOCWaypointImage, AllAPIDS }

        public Model()
        {
            DownlinkRate = _DownlinkRate;  // initialize
        }

        public string ModelName => GetType().ToString();

        public static Event First => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First : null;
        public static float FirstTime => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First.Time : float.MaxValue;

        #region Core Methods

        public List<Component> Components = new List<Component>();

        public virtual void Build()
        {
        }
        public virtual void Start() { }
        public virtual void Stop() { }
        public virtual void GenerateFrame() { }

        public virtual void Loop()
        {
            while (!StopRequest && EventQueue.Count > 0)
            {
                var evt = EventQueue.Dequeue();
                Time = evt.Time;
                evt.Execute(this);

                if (Time >= EmergencyStopTime)
                {
                    Message(string.Format("Emergency stop at Time={0}", EmergencyStopTime));
                    return;
                }
            }
        }

        public virtual void Run()
        {
            if (TheModel != this && TheModel != null)
            {
                if (TheModel.IsRunning)
                    throw new Exception(@"Running two models in parallel");
            }
            TheModel = this;

            IsRunning = true;
            EventMessages = new List<string>();
            ReportMessages = new List<string>();
            Build();
            Start();
            Loop();
            Stop();
            IsRunning = false;
        }

        #endregion

        #region Event Classes

        public interface PacketReceiver
        {
            bool Receive(Packet p);
        }

        public interface FrameReceiver
        {
            void Receive(Frame f);
        }

        public interface PacketQueueOwner
        {
            // Indicates that the queue recieved a new packet (and didn't drop it)
            void WakeUp();
        }

        public interface Event
        {
            float Time { get; set; }
            void Execute(Model m);
        }

        public abstract class BaseEvent : Event
        {
            public float Time { get; set; }
            public abstract void Execute(Model m);
        }

        public class Thunk : BaseEvent
        {
            public Action Action;
            public Thunk() { }
            public Thunk(float t, Action a) { Time = t; Action = a; }
            public override void Execute(Model m) { Action(); }
        }

        // Needed?
        public class FrameEngine : BaseEvent
        {
            public float Delay;
            public override void Execute(Model m)
            {
                m.GenerateFrame();
                Time += Delay;
                m.Enqueue(this);
            }
        }

        public class PacketDelivery : BaseEvent
        {
            public PacketReceiver Receiver;
            public Packet Packet;
            public override void Execute(Model m) { Receiver.Receive(Packet); }
        }

        public class FrameDelivery : BaseEvent
        {
            public Frame Frame;
            public FrameReceiver Receiver;
            public override void Execute(Model m) { Receiver.Receive(Frame); }
        }

        public class CaptureState : BaseEvent
        {
            public float Delay;
            public SingleMultiplexor Model;
            public override void Execute(Model m)
            {
                var sample = new Sample();
                sample.Capture(Model);
                Time += Delay;
                m.Enqueue(this);
            }
        }

        #endregion

        #region Event handlers

        #endregion

        #region Classes

        public class Packet
        {
            public APID APID { get; set; }
            public int Length { get; set; }
            public float Timestamp { get; set; }
            public float Received { get; set; }
            public float Latency => Received - Timestamp;
        }

        public class PacketFragment : Packet
        {
            public Packet Packet { get; set; }
            public int FragmentNumber { get; set; } = 1;
            public int TotalFragments { get; set; } = 1;
            public bool IsFinal => FragmentNumber == TotalFragments;
        }

        public class Frame
        {
            public static int Length = RP15FrameLength;
            public static int FrameCapacity = RP15FrameLength - RP15FrameOverheead;
            public static int MasterCounter = 0;
            public int Counter = MasterCounter++;
            public int VirtualChannel = 0;
            public int Capacity = FrameCapacity;
            public bool IsFull => Capacity == 0;
            public bool IsEmpty => Capacity == FrameCapacity;
            public List<PacketFragment> Fragments = new List<PacketFragment>();
            public void Add(PacketFragment f)
            {
                Fragments.Add(f);
                Capacity -= f.Length;
                Debug.Assert(Capacity >= 0);
            }
        }

        public class Rover : Component
        {
            public FrameGenerator FrameGenerator;
            public int RoverImageVC = -1;
            public bool IsDriving = false;
            public float Position = 0f;
            public void Drive()
            {
                IsDriving = true;
                Message("Rover starts driving");
                var delta = DriveStep / DriveSpeed;
                Enqueue(TheModel.Time + delta, () => StopDriving());
            }
            public void StopDriving()
            {
                IsDriving = false;
                Message("Rover stops driving");
                Position += DriveStep;
                FrameGenerator.Buffers[RoverImageVC].Receive(new RoverImagePair { APID = APID.RoverImagePair, Length = NavPayload, Timestamp = TheModel.Time, RoverPosition = Position });

                // DOC images are triggered in ModelDocGeneration
            }
        }

        public class Driver : Component
        {
            public Rover Rover;
            public int CommandCount = 0;
            public void SendDriveCommand()
            {
                Message(@"Driver asked to send drive comomand");
                if (CommandCount++ > 10)
                {
                    Message(@"The driver is ready to send command but stopping");
                    TheModel.StopRequest = true;
                }
                TheModel.Message("Driver sends drive command");
                Enqueue(new Thunk(TheModel.Time + UplinkLatency, () => Rover.Drive()));
            }
            public void EvalImage(Packet p)
            {
                var ri = p as RoverImagePair;
                if (ri == null)
                    throw new Exception("Received a rover image packet that wasn't of the proper type");
                TheModel.Message("Driver starts image eval timestamp={0} pos={1}", ri.Timestamp, ri.RoverPosition);
                if (TheModel.TheCase == ModelCase.Rails)
                    Enqueue(new Thunk(TheModel.Time + DriverDecisionTime, () => SendDriveCommand()));
            }
        }

        public class RoverImagePair : Packet
        {
            public float RoverPosition { get; set; }
        }

        public class DOCImage : Packet
        {
            public float RoverPosition { get; set; }
            public int SequenceNumber { get; set; }
        }

        #endregion

        #region Components

        public class Component
        {
            public virtual void Build() { }
            public virtual void Start() { }
            public virtual void Stop() { }

            public void Message(string msg, params object[] args)
            {
                var msg1 = string.Format(msg, args);
                var fullmsg = string.Format(@"{0} {1}", TheModel.Time.ToString("F3").PadLeft(12), msg1);
                if (TheModel.PrintMessages)
                    Console.WriteLine(fullmsg);
                else
                    TheModel.EventMessages.Add(fullmsg);
            }
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
                var time = TheModel.Time + StartTimeOffset;
                TheModel.Enqueue(new PacketGenerator { Time = time, APID = APID, Delay = delay, PacketSize = PacketSize, Receiver = Receiver });
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
            public void PacketsInFlight(APID pattern, ref int packetCount, ref int byteCount)
            {
                var a = Queue.ToArray();
                for (var i = 0; i < a.Length; i++)
                {
                    if (!MatchingAPID(pattern, a[i].APID))
                        continue;
                    packetCount++;
                    byteCount += a[i].Length;
                }
                a = Stack.ToArray();
                for (var i = 0; i < a.Length; i++)
                {
                    if (!MatchingAPID(pattern, a[i].APID))
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
            public bool PacketInFlight { get; set; } = false;
            public List<PacketQueue> Queues = new List<PacketQueue>();
            public void AddQueue(PacketQueue q)
            {
                Queues.Add(q);
                q.Owner = this;
            }
            public override void Start() { TheModel.Enqueue(this); }
            public void Execute(Model m)
            {
                var p = NextPacket();
                if (p == null)
                {
                    PacketInFlight = false;
                    return;
                }
                Receiver.Receive(p);
                var transmissionTime = p.Length * 8 / BitRate;
                Time += transmissionTime;
                PacketInFlight = true;
                TheModel.Enqueue(this);
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
                    Time = TheModel.Time;
                    TheModel.Enqueue(this);
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
            public bool IsTimedOut => TheModel.Time > WakeupTime && ContainsData;
            public bool ContainsData => !PacketQueue.IsEmpty || (Frame != null && !Frame.IsEmpty);
            public override void Build() { PacketQueue.Build(); }
            public bool Receive(Packet p) {
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
            public void UpdateTimeout() {
                WakeupTime = TheModel.Time + Timeout;
            }
        }

        public class FrameGenerator : Component, Event, PacketQueueOwner
        {
            public int FrameLength = 1115;
            public int FrameHeaderLength = 6 + 6;  // 6 bytes of header, 2 of error correction
            public float Time { get; set; } = 0f;
            public List<VirtualChannelBuffer> Buffers = new List<VirtualChannelBuffer>();
            public GroundSystem GroundSystem;
            public float FrameLatency = RP15FrameLength * 8 / 100000f;  // init value
            public float DownlinkLatency = 10f + 1.3f + RP15FrameLength * 8 / 100000f;  // init value
            public float _DownlinkRate = 100000f;
            public float DownlinkRate
            {
                get { return _DownlinkRate; }
                set
                {
                    _DownlinkRate = value;
                    FrameLatency = RP15FrameLength * 8 / _DownlinkRate;
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
                TheModel.Enqueue(this);
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

                if (frame.VirtualChannel != IdleVC)
                {
                    var buf = Buffers[frame.VirtualChannel];
                    buf.UpdateTimeout();
                    buf.Frame = null;
                }
                Enqueue(new Thunk(Time + TheModel.DownlinkLatency, () => GroundSystem.Receive(frame)));  // idle frame
                Time += FrameLatency;
                TheModel.Enqueue(this);
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
                return new Frame { VirtualChannel = IdleVC };  // Idle Frame
            }
            public void WakeUp() { }
        }

        public class GroundSystem : Component, FrameReceiver
        {
            public Driver Driver;
            public Rover Rover;
            public long TotalBytesReceived = 0L;
            public long TotalBytesInPackets = 0L;
            public long FrameCount = 0L;
            public long[] FrameCounter = new long[64]; // How should this be initialized?

            public List<Packet> Packets = new List<Packet>();

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
                TotalBytesReceived += RP15FrameLength;
                foreach (var frag in f.Fragments)
                    if (frag.IsFinal)  // Assumes no packet loss, for now
                    {
                        var packet = frag.Packet;
                        //mhs
                        //if (packet.APID == APID.DOCProspectingImage)
                        //Console.WriteLine("Ground received {0} timestamp={1}", packet.APID, packet.Timestamp);
                        TotalBytesInPackets += packet.Length;
                        packet.Received = TheModel.Time;
                        Packets.Add(packet);
                        if (packet.APID == APID.RoverImagePair)
                            Driver.EvalImage(packet);
                        if (packet.APID == APID.DOCWaypointImage)
                        {
                            var di = packet as DOCImage;
                            if (di == null) continue;
                            TheModel.Message("Received DOC Waypoint image seq={0}", di.SequenceNumber);  // Bug: This should be the last of 4, not any DOC Waypoint image
                            if (TheModel.TheCase == ModelCase.ScienceStation && di.SequenceNumber == 3 && di.RoverPosition == Rover.Position)
                            {
                                TheModel.Message("NIRVSS starts evaluating the final waypoint image");
                                Enqueue(new Thunk(TheModel.Time + TheModel.NIRVSSEvalLatency, () =>
                                 {
                                     TheModel.Message("NIRVSS allows driver to send drive command");
                                     // NIRVSS communicates with Driver with 0 latency
                                     Driver.SendDriveCommand();
                                 }
                                ));
                            }
                        }
                    }
            }
        }


        #endregion

        #region Helper Methods

        public static void Enqueue(Thunk t) { TheModel.EventQueue.Enqueue(t, t.Time); }
        public static void Enqueue(float t, Action a) { TheModel.EventQueue.Enqueue(new Thunk(t, a), t); }
        public void Enqueue(Event e) { EventQueue.Enqueue(e, e.Time); }

        public void Message(string msg, params object[] args)
        {
            var msg1 = string.Format(msg, args);
            var fullmsg = string.Format(@"{0} {1}", Time.ToString("F3").PadLeft(12), msg1);
            if (PrintMessages)
                Console.WriteLine(fullmsg);
            else
                EventMessages.Add(fullmsg);
        }

        public void Message()
        {
            if (PrintMessages)
                Console.WriteLine();
            else
                EventMessages.Add(String.Empty);
        }

        public void Report(string msg, params object[] args)
        {
            if (PrintReport)
                Console.WriteLine(msg, args);
            else
                ReportMessages.Add(string.Format(msg, args));
        }

        public void Report()
        {
            if (PrintReport)
                Console.WriteLine();
            else
                ReportMessages.Add(String.Empty);
        }

        public static bool MatchingAPID(APID pattern, APID concrete) => pattern == APID.AllAPIDS || pattern == concrete;

        #endregion
    }

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
            Rover = new Rover { RoverImageVC = RoverImageVC };
            Driver = new Driver { };
            GroundSystem = new GroundSystem { };
            FrameGenerator = new FrameGenerator { DownlinkRate = DownlinkRate };
            RoverHighPacketGenerator = new PacketGenerator { APID = APID.RoverHealth, BitsPerSecond = RoverHealthBitsPerSecond, PacketSize = 100, StartTimeOffset = 0f };
            PayloadHighPacketGenerator = new PacketGenerator { APID = APID.PayloadGeneral, BitsPerSecond = PayloadWithoutDOCBitsPerSecond, PacketSize = 100, StartTimeOffset = 0.1f };

            // Link the objects together
            FrameGenerator.GroundSystem = GroundSystem;
            GroundSystem.Driver = Driver;
            GroundSystem.Rover = Rover;
            Driver.Rover = Rover;
            Rover.FrameGenerator = FrameGenerator;

            // Wire up the packet generators through the frame generator
            //var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            var timeouts = new float[] { 1f, 1f, 1f, 1f, 1f };
            FrameGenerator.Buffers = Enumerable.Range(0, timeouts.Length).Select(i => new VirtualChannelBuffer { VirtualChannel = i, PacketQueue = new PacketQueue { Size = PacketQueueSize }, Timeout = timeouts[i], Owner = FrameGenerator }).ToList();

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
