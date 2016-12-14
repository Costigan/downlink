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

        public static float DriveStep = 4.5f;
        public static float DriveSpeed = 0.01f;
        public static float UplinkLatency = 10f + 1.3f;
        public static float DownlinkLatency = 10f + 1.3f + FrameLatency;

        public static float FrameLatency = FrameLength * 8 / DownlinkRate;
        public static float GDSLatency = 0f;  // for now

        public static float DriverDecisionTime = 90f;

        public const float RoverDownlinkRate = 60000f; // bits/sec
        public const float PayloadDownlinkRate = 40000f; // bits/sec
        public const float DownlinkRate = RoverDownlinkRate + PayloadDownlinkRate;

        public const int FrameLength = 1113; // bytes
        public const float RoverFrameTime = FrameLength * 8 / RoverDownlinkRate;
        public const float PayloadFrameTime = FrameLength * 8 / PayloadDownlinkRate;
        public const float FrameTime = FrameLength * 8f / DownlinkRate;
        public const int FrameCapacity = FrameLength - 6;  // frame header and footer

        public const float RoverHealthBitsPerSecond = 46669.9f;
        public const int NavPayload = (int)(2097152f * 12f / 2f / 4f / 8f);

        public const float AvionicsProspectingBitsPerSecond = 1491f;
        public const float VMLProspectingBitsPerSecond = 735.3f;
        public const float NSSProspectingBitsPerSecond = 800f;
        public const float NIRVSSProspectingBitsPerSecond = 11552f;
        public const float PayloadWithoutDOCBitsPerSecond = AvionicsProspectingBitsPerSecond + VMLProspectingBitsPerSecond + NSSProspectingBitsPerSecond + NIRVSSProspectingBitsPerSecond;
        public const float DOCProspectingBitsPerSecond = 0f;

        public const int DOCHighResNarrow = (((2048 * 2048 * 12) / 3) + 112) / 8;
        public const int DOCLowContrastNarrow = ((256 * 256 * 8) / 3 + 112) / 8;
        public const int DOCLowContrastMedium = ((256 * 256 * 8) / 3 + 112) / 8;
        public const int DOCLowContrastFull = ((256 * 256 * 8) / 3 + 112) / 8;  // One every 2 sec
        public const int DOCSingleLEDScale3 = ((256 * 256 * 12) / 3 + 112)/8;  // one every 2.5 sec
        public const int DOCAllLEDScale3 = ((256 * 256 * 12) / 3 + 112) / 8;
        public const int DOCAllLEDScale2 = ((512 * 512 * 12) / 3 + 112) / 8;
        public const int DOCAllLEDScale1 = ((2048 * 2048 * 12) / 3 + 112) / 8;

        public const float EmergencyStopTime = 50000000f;

        public bool PrintMessages = false;
        public bool PrintReport = false;
        public List<string> EventMessages;
        public List<string> ReportMessages;

        #endregion Global Variables

        #region Model State Variables

        public static Model TheModel = new Model();
        public SimplePriorityQueue<Event, float> EventQueue = new SimplePriorityQueue<Event, float>();
        public float Time = 0f;
        public List<Component> Components = new List<Component>();

        public bool StopRequest = false;

        public PacketQueue[] VCPacketQueue = null;
        public int RoverHighPriorityVC = 0;
        public int RoverLowPriorityVC = 0;
        public int RoverImageVC = 1;
        public int PayloadHighPriority = 2;
        public int PayloadHighPriorityImage = 3;
        public int PayloadLowPriorityImage = 4;

        public Rover TheRover;
        public Driver TheDriver;
        public GroundSystem TheGroundSystem;

        public enum ModelCase { Rails, ScienceStation }
        public ModelCase TheCase = ModelCase.Rails;

        #endregion Model State Variables

        public enum APID { IdlePacket, RoverHealth, RoverImagePair, PayloadGeneral, DOCProspectingImage, DOCWaypointImage, AllAPIDS }

        public static void Enqueue(Thunk t)
        {
            TheModel.EventQueue.Enqueue(t, t.Time);
        }

        public static void Enqueue(float t, Action a)
        {
            TheModel.EventQueue.Enqueue(new Thunk(t, a), t);
        }

        public static Event First => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First : null;
        public static float FirstTime => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First.Time : float.MaxValue;

        #region Core Methods

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
            TheModel = this;
            EventMessages = new List<string>();
            ReportMessages = new List<string>();
            Start();
            Loop();
            Stop();
        }

        #endregion Core Methods

        #region Event Classes

        public interface PacketReceiver
        {
            void Receive(Packet p);
        }

        public interface FrameReceiver
        {
            void Receive(Frame f);
        }

        public abstract class Event
        {
            public float Time;
            public abstract void Execute(Model m);
        }

        public class Thunk : Event
        {
            public Action Action;
            public Thunk() { }
            public Thunk(float t, Action a) { Time = t; Action = a; }
            public override void Execute(Model m) { Action(); }
        }

        public class PacketGenerator : Event
        {
            public APID APID;
            public int Length;
            public float Delay;
            public PacketReceiver Receiver;
            public override void Execute(Model m)
            {
                var p = new Packet { APID = APID, Length = Length, Timestamp = Time };
                Receiver.Receive(p);
                Time += Delay;
                m.Enqueue(this);
            }
        }

        public class FrameEngine : Event
        {
            public float Delay;
            public override void Execute(Model m)
            {
                m.GenerateFrame();
                Time += Delay;
                m.Enqueue(this);
            }
        }

        public class PacketDelivery : Event
        {
            public PacketReceiver Receiver;
            public Packet Packet;
            public override void Execute(Model m) { Receiver.Receive(Packet); }
        }

        public class FrameDelivery : Event
        {
            public Frame Frame;
            public FrameReceiver Receiver;
            public override void Execute(Model m) { Receiver.Receive(Frame); }
        }

        #endregion Event Classes

        #region Event handlers

        #endregion Event handlers


        #region Classes

        public class Component
        {

        }

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

        public class VirtualChannelBuffer
        {
            public PacketQueue PacketQueue;
            public int VirtualChannel;
            public Frame Frame;
            public int BypassCounter;
            public int BypassMaximum = 100;
            float _Timeout = 10f;
            public float Timeout
            {
                get { return _Timeout; }
                set
                {
                    _Timeout = value;
                    BypassMaximum = (int)Math.Ceiling(Timeout / FrameTime);
                    BypassCounter = 0;
                }
            }
            public bool IsTimedOut => Frame != null && BypassCounter > BypassMaximum;
        }

        public class Rover
        {
            public bool IsDriving = false;
            public float Position = 0f;

            public void Drive()
            {
                IsDriving = true;
                TheModel.Message("Rover starts driving");
                var delta = DriveStep / DriveSpeed;
                Model.Enqueue(Model.TheModel.Time + delta, () => StopDriving());
            }

            public void StopDriving()
            {
                IsDriving = false;
                TheModel.Message("Rover stops driving");
                Position += DriveStep;
                TheModel.VCPacketQueue[TheModel.RoverImageVC].Receive(new Packet { APID = APID.RoverImagePair, Length = NavPayload, Timestamp = TheModel.Time });

                // DOC images are triggered in ModelDocGeneration
            }
        }

        public class Driver
        {
            public int CommandCount = 0;
            public void SendDriveCommand()
            {
                if (CommandCount++ > 10)
                {
                    TheModel.Message(@"The driver is ready to send command but stopping");
                    TheModel.StopRequest = true;
                }
                TheModel.Message("Driver sends drive command");
                Enqueue(new Thunk(TheModel.Time + UplinkLatency, () => TheModel.TheRover.Drive()));
            }
            public void EvalImage(Packet p)
            {
                TheModel.Message("Driver starts evaluating image");
                if (TheModel.TheCase == ModelCase.Rails)
                    Enqueue(new Thunk(TheModel.Time + DriverDecisionTime, () => TheModel.TheDriver.SendDriveCommand()));
            }
        }

        public class GroundSystem : FrameReceiver
        {
            public long TotalBytesReceived = 0L;
            public long TotalBytesInPackets = 0L;
            public long FrameCount = 0L;
            public long[] FrameCounter = new long[5];

            public List<Packet> Packets = new List<Packet>();
            public void Receive(Frame f)
            {
                FrameCount++;
                FrameCounter[f.VirtualChannel]++;
                TotalBytesReceived += FrameLength;
                foreach (var frag in f.Fragments)
                    if (frag.IsFinal)  // Assumes no packet loss, for now
                    {
                        var packet = frag.Packet;
                        TotalBytesInPackets += packet.Length;
                        packet.Received = TheModel.Time;
                        Packets.Add(packet);
                        if (packet.APID == APID.RoverImagePair)
                            TheModel.TheDriver.EvalImage(packet);
                        if (packet.APID == APID.DOCWaypointImage)
                        {
                            var di = packet as DOCImage;
                            if (di == null) continue;
                            TheModel.Message("Received DOC Waypoint image seq={0}", di.SequenceNumber);  // Bug: This should be the last of 4, not any DOC Waypoint image
                            if (TheModel.TheCase == ModelCase.ScienceStation && di.SequenceNumber == 3 && di.RoverPosition == TheModel.TheRover.Position)
                            {
                                TheModel.Message("NIRVSS allows driver to send drive command");
                                Enqueue(new Thunk(TheModel.Time + UplinkLatency, () => TheModel.TheDriver.SendDriveCommand()));
                                //TheModel.StopRequest = true;
                            }
                        }
                    }
            }
        }

        public class PacketQueue : PacketReceiver
        {
            public int DropCount = 0;
            public int Size;
            public int Count = 0;
            Queue<Packet> Queue = new Queue<Packet>();
            Stack<Packet> Stack = new Stack<Packet>();
            public void Enqueue(Packet p)
            {
                Receive(p);
                Count++;
            }
            public void Receive(Packet p)
            {
                if (Size > Queue.Count)
                    Queue.Enqueue(p);
                else
                    DropCount++;
            }
            public void PushBack(Packet p)
            {
                Stack.Push(p);
                Count++;
            }
            public Packet Dequeue()
            {
                var p = Stack.Count > 0 ? Stack.Pop() : Queue.Count > 0 ? Queue.Dequeue() : null;
                if (p != null) Count--;
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

        public class DOCImage : Packet
        {
            public float RoverPosition { get; set; }
            public int SequenceNumber { get; set; }
        }

        #endregion Classes

        #region Helper Methods

        public void Enqueue(Event e)
        {
            EventQueue.Enqueue(e, e.Time);
        }

        public void StartPacketGenerator(PacketReceiver r, APID apid, float bitsPerSecond, int packetSize, float time)
        {
            var bitsPerPacket = packetSize * 8;
            var delay = bitsPerPacket / bitsPerSecond;
            Enqueue(new PacketGenerator { Time = time, APID = apid, Delay = delay, Length = packetSize, Receiver = r });
        }

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

        #endregion Helper Methods

        }

        public class SimpleModel : Model
    {
        List<VirtualChannelBuffer> Buffers;
        public override void Start()
        {
            Time = 0f;
            TheRover = new Rover();
            TheDriver = new Driver();
            TheGroundSystem = new GroundSystem();

            VCPacketQueue = Enumerable.Range(0, 5).Select(i => new PacketQueue { Size = 1200 }).ToArray();
            var timeouts = new float[] { 2f, 5f, 6f, 7f, 10f };
            Buffers = Enumerable.Range(0, VCPacketQueue.Length).Select(i => new VirtualChannelBuffer { VirtualChannel = i, PacketQueue = VCPacketQueue[i] }).ToList();
            for (var i = 0; i < VCPacketQueue.Length; i++)
                Buffers[i].Timeout = timeouts[i];

            // TO_DRIVE = 46669.9 bits/sec
            StartPacketGenerator(VCPacketQueue[RoverHighPriorityVC], APID.RoverHealth, RoverHealthBitsPerSecond, 100, Time);
            StartPacketGenerator(VCPacketQueue[PayloadHighPriority], APID.PayloadGeneral, PayloadWithoutDOCBitsPerSecond, 100, Time+0.1f);
            Enqueue(new Thunk(Time, () => ModelDocGeneration()));

            Enqueue(new FrameEngine { Time = Time, Delay = FrameTime });
            if (TheCase == ModelCase.Rails)
                Enqueue(new Thunk(Time, () => TheDriver.SendDriveCommand()));
        }

        // Runs at 1 Hz
        int _DocWaypointImageCount = 0;
        void ModelDocGeneration()
        {
            if (TheModel.TheRover.IsDriving)
            {
                var p = new Packet { APID = APID.DOCProspectingImage, Length = DOCLowContrastNarrow, Timestamp = Time };
                VCPacketQueue[PayloadHighPriorityImage].Receive(p);
                //Message(@"  Sending driving doc image");
                _DocWaypointImageCount = 0;
            }
            else
            {
                if (_DocWaypointImageCount < 9)
                {
                    var p = new DOCImage { APID = APID.DOCWaypointImage, Length = DOCAllLEDScale3, Timestamp = Time, SequenceNumber = _DocWaypointImageCount, RoverPosition = TheModel.TheRover.Position };
                    VCPacketQueue[_DocWaypointImageCount < 4 ? PayloadHighPriorityImage : PayloadLowPriorityImage].Receive(p);
                    Message(@"  Sending waypoint doc image {0}", _DocWaypointImageCount);
                    _DocWaypointImageCount++;
                }
            }
            Enqueue(new Thunk(TheModel.Time + 1f, () => ModelDocGeneration()));
        }

        // Debugging
        private bool _seen = false;
        private bool IsThere => Buffers[1].Frame?.Fragments?.Count == 1 && Buffers[1].Frame.Fragments[0].Packet.APID == APID.RoverImagePair;

        public override void GenerateFrame()
        {
            for (int i = 0; i < Buffers.Count; i++)
                if (Buffers[i].Frame != null)
                    Buffers[i].BypassCounter++;

            // Look for frames that have timed out
            foreach (var buffer in Buffers)
                if (buffer.IsTimedOut)
                {
                    SendFrame(buffer);
                    return;
                }

            // Look for full buffers
            foreach (var buffer in Buffers)
            {
                var frame = buffer.Frame = AssembleFrameFromVC(buffer.Frame, buffer.PacketQueue);
                if (frame.IsFull)
                {
                    SendFrame(buffer);
                    return;
                }
            }

            // Look for any content
            foreach (var buffer in Buffers)
            {
                var frame = buffer.Frame = AssembleFrameFromVC(buffer.Frame, buffer.PacketQueue);
                if (!frame.IsEmpty)
                {
                    SendFrame(buffer);
                    return;
                }
            }
            Enqueue(new Thunk(Time + DownlinkLatency, () => TheGroundSystem.Receive(new Frame())));  // idle frame
        }

        void SendFrame(VirtualChannelBuffer buf)
        {
            buf.BypassCounter = 0;
            Frame f = buf.Frame;
            f.VirtualChannel = buf.VirtualChannel;
            Enqueue(new Thunk(Time + DownlinkLatency, () => TheGroundSystem.Receive(f)));
            buf.Frame = null;
        }

        public Frame AssembleFrameFromVC(Frame frame, PacketQueue vc)
        {
            if (frame == null)
                frame = new Frame();
            while (!frame.IsFull)
            {
                Packet p = vc.Dequeue();
                if (p == null)
                    break;
                var f = p as PacketFragment;
                if (f != null)
                {   // use f, a PacketFragment
                    if (frame.Capacity >= f.Length)
                    {   // can fit
                        frame.Add(f);
                    }
                    else
                    {
                        var frag1 = new PacketFragment { Packet = f.Packet, Length = frame.Capacity, FragmentNumber = f.TotalFragments, TotalFragments = f.TotalFragments+1 };
                        frame.Add(frag1);
                        f.FragmentNumber = ++f.TotalFragments;
                        f.Length -= frag1.Length;
                        Debug.Assert(f.Length >= 0);
                        vc.PushBack(f);
                    }
                }
                else
                {   // use p, a Packet
                    if (frame.Capacity >= p.Length)
                    {
                        frame.Add(new PacketFragment { Packet = p, Length = p.Length });
                    }
                    else
                    {
                        var frag1 = new PacketFragment { Packet = p, Length = frame.Capacity, FragmentNumber = 1, TotalFragments = 2 };
                        var frag2 = new PacketFragment { Packet = p, Length = p.Length - frame.Capacity, FragmentNumber = 2, TotalFragments = 2 };
                        frame.Add(frag1);
                        vc.PushBack(frag2);
                    }
                }
            }
            return frame;
        }

        public override void Stop()
        {
            TheModel.Message("Stop simulation");
            if (TheGroundSystem.Packets.Count<1)
            {
                Report(@"No packets were received.");
                return;
            }
            Report(@"The rover drove {0} meters in {1} seconds", TheRover.Position, Time);
            var smg = 100f * TheRover.Position / Time;
            Report(@"SMG = {0} cm/sec", smg.ToString("F3").PadLeft(10));
            Report();

            Report(@"{0} frames were received", TheGroundSystem.FrameCount.ToString().PadLeft(10));
            for (var i = 0; i < TheGroundSystem.FrameCounter.Length; i++)
                Report(@"{0} VC{1} frames were received ({2}%), {3} were dropped", 
                    TheGroundSystem.FrameCounter[i].ToString().PadLeft(10),
                    i,
                    (TheGroundSystem.FrameCounter[i] / (float)TheGroundSystem.FrameCount).ToString("F2").PadLeft(6),
                    Buffers[i].PacketQueue.DropCount
                    );
            Report();
            Report(@"{0} bytes were received in all frames", TheGroundSystem.TotalBytesReceived.ToString().PadLeft(10));
            Report(@"{0} bytes were received in all packets", TheGroundSystem.TotalBytesInPackets.ToString().PadLeft(10));
            Report(@"The bandwidth efficiency was {0}%",
                (TheGroundSystem.TotalBytesInPackets / (float)TheGroundSystem.TotalBytesReceived).ToString("F2").PadLeft(6));
            Report();
            PacketReport(TheGroundSystem.Packets, "all APIDs");
            Report();
            var AllAPIDS = (int)APID.AllAPIDS;
            for (var i=0;i<=AllAPIDS;i++)
            {
                PacketReport(TheGroundSystem.Packets.Where(p => p.APID == (APID)i), ((APID)i).ToString());
                Report();
            }

            Report(@"Report on packets in flight at time {0}", Time);
            for (var i=0;i<= AllAPIDS; i++)
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
            Report(@"Packet Report for {0}", title);
            Report(@"  {0} packets were received", packetCount);
            Report(@"  The average latency was {0:F3} sec", averageLatency);
            Report(@"  The latency standard deviation was {0:F3} sec", latencyStdDev);
        }

        private void PacketsInFlight(APID apid, out int packetCount, out int byteCount)
        {
            packetCount = 0;
            byteCount = 0;
            foreach (var vc in Buffers)
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
            while (q.Count>0)
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

}
