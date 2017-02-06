using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    #region Interfaces

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

    #endregion

    #region Events

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

    public class Packet
    {
        public APID APID { get; set; }
        public int Length { get; set; }
        public float Timestamp { get; set; }
        public float Received { get; set; }
        public float Latency => Received - Timestamp;
        public override string ToString() => "<packet apid=" + APID + " timestamp=" + Timestamp + ">";
        public bool IsImagePacket => APID == APID.RoverImagePair || APID == APID.DOCProspectingImage || APID == APID.DOCWaypointImage;
    }

    /// <summary>
    /// Fragment of a packet within a frame
    /// </summary>
    public class PacketFragment : Packet
    {
        public Packet Packet { get; set; }
        public int FragmentNumber { get; set; } = 1;
        public int TotalFragments { get; set; } = 1;
        public bool IsFinal => FragmentNumber == TotalFragments;
    }

    /// <summary>
    /// Fragment of a large image packet.  These are not the same as PacketFragment's
    /// </summary>
    public class ImageFragment : Packet
    {
        public Packet Packet { get; set; }
        public int FragmentNumber { get; set; } = 1;
        public int TotalFragments { get; set; } = 1;
        public bool IsFinal => FragmentNumber == TotalFragments;
    }

    public class Frame
    {
        public static int Length = Model.RP15FrameLength;
        public static int FrameCapacity = Model.RP15FrameLength - Model.RP15FrameOverheead;
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

    public class ImagePacket  : Packet
    {
        public float RoverPosition { get; set; }
        public int SequenceNumber { get; set; }
        public bool IsLastDocWaypointImage = false;
    }
}
