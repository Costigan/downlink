using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using System.Diagnostics;

namespace Downlink
{
    // Case 4
    public abstract class Model
    {
        #region Global Variables

        public enum APID { IdlePacket, RoverHealth, RoverImagePair, PayloadGeneral, DOCProspectingImage, DOCWaypointImage, AllAPIDS }
        public const int RP15FrameLength = 1115; // bytes
        public const int RP15FrameOverheead = 12;

        public bool CaptureStates = true;

        public float DriveStep = 4.5f;
        public float DriveSpeed = 0.01f;
        public float UplinkLatency = 10f + 1.3f;
        public float DownlinkLatency;

        public float FrameLatency;
        public float GDSLatency = 0f;  // for now

        public float DriverDecisionTime = 90f;

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

        public Model()
        {
            DownlinkRate = _DownlinkRate;  // initialize
        }

        public string ModelName => GetType().ToString();
        public override string ToString() => ModelName;

        public static Event First => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First : null;
        public static float FirstTime => TheModel.EventQueue.Count > 0 ? TheModel.EventQueue.First.Time : float.MaxValue;

        #region Core Methods

        public List<Component> Components = new List<Component>();

        public virtual void Build()        {        }
        public virtual void Start()
        {
            foreach (var c in Components) c.Start();
        }
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
            Build();
            RunInternal();
        }

        public virtual void RunInternal()
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
            Start();
            Loop();
            Stop();
            IsRunning = false;
        }

        #endregion

        #region Helper Methods

        public void Enqueue(Thunk t) { EventQueue.Enqueue(t, t.Time); }
        public void Enqueue(float t, Action a) { EventQueue.Enqueue(new Thunk(t, a), t); }
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

        public virtual void PacketsInFlight(APID apid, out int packetCount, out int byteCount)
        {
            packetCount = byteCount = 0;
        }

        public virtual void PacketReport(IEnumerable<Packet> packets, string title)
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

        public void PacketsInFlight(APID apid, Frame f, ref int packetCount, ref int byteCount)
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

        public void PacketsInFlight(APID apid, PacketQueue q, ref int packetCount, ref int byteCount)
        {
            q.PacketsInFlight(apid, ref packetCount, ref byteCount);
        }

        public void PacketsInFlight(APID apid, SimplePriorityQueue<Event, float> q, ref int packetCount, ref int byteCount)
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

        #endregion

    }
}
