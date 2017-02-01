using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Downlink;

namespace Downlink.tests
{
    [TestFixture]
    public class PriorityQueueTests
    {
        [Test]
        public void Test1()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildA();
            m.RunInternal();
            Assert.IsEmpty(m.Packets);
        }

        [Test]
        public void Test2()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildA();
            var p = new Packet();
            m.PayloadAvionics.Queues[0].Receive(p);
            m.RunInternal();
            Assert.AreEqual(m.Packets.Count,1);
            Assert.AreEqual(m.Packets[0], p);
            Assert.AreEqual(p.Received, 0f);
        }

        [Test]
        public void Test3()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildB();
            var p = new Packet { Length = 100 };
            m.PayloadAvionics.Queues[1].Receive(p);
            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, 1);
            Assert.AreEqual(m.Packets[0], p);
            Assert.AreEqual(p.Received, p.Length * 8 / m.PayloadAvionics.BitRate);
        }

        [Test]
        public void Test4()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildB();
            var p1 = new Packet { Length = 0 };
            var p2 = new Packet { Length = 0 };
            m.PayloadAvionics.Queues[0].Receive(p1);
            m.PayloadAvionics.Queues[1].Receive(p2);
            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, 2);
            Assert.AreEqual(m.Packets[0], p1);
            Assert.AreEqual(m.Packets[1], p2);
            Assert.AreEqual(p1.Received, 0f);
            Assert.AreEqual(p2.Received, 0f);
        }

        [Test]
        public void Test5()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildB();
            m.PayloadAvionics.BitRate = 100000f;
            var p1 = new Packet { Length = 100 };
            var p2 = new Packet { Length = 100 };
            m.PayloadAvionics.Queues[0].Receive(p1);
            m.PayloadAvionics.Queues[1].Receive(p2);
            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, 2);
            Assert.AreEqual(m.Packets[0], p1);
            Assert.AreEqual(m.Packets[1], p2);
            Assert.AreEqual(p1.Received, (p1.Length) * 8 / m.PayloadAvionics.BitRate);
            Assert.AreEqual(p2.Received, (p1.Length+p2.Length) * 8 / m.PayloadAvionics.BitRate);  // wrong
        }

        [Test]
        public void Test6()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildC();
            m.PayloadAvionics.BitRate = 100000f;
            var inpackets = Enumerable.Range(0, 100).Select(i => new Packet { Length = 100 }).ToList();
            foreach (var p in inpackets.Take(25))
                m.PayloadAvionics.Queues[0].Receive(p);
            foreach (var p in inpackets.Skip(25).Take(25))
                m.PayloadAvionics.Queues[1].Receive(p);
            foreach (var p in inpackets.Skip(50).Take(50))
                m.PayloadAvionics.Queues[2].Receive(p);
            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, 100);
            for (var i = 0; i < 100; i++)
                Assert.AreEqual(inpackets[i], m.Packets[i]);
            for (var i=0;i<100;i++)
                Assert.AreEqual(m.Packets[i].Received, Enumerable.Range(0, i + 1).Sum(i1 => m.Packets[i1].Length) * 8 / m.PayloadAvionics.BitRate, 0.001d);
        }

        [Test]
        public void Test7()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildC();
            m.PayloadAvionics.BitRate = 100000f;
            var p = Enumerable.Range(0, 3).Select(i => new Packet { Length = 100 }).ToList();

            for (var i = 0; i < p.Count; i++)
            {
                var packetIndex = i;
                var queueIndex = p.Count - i - 1;  // third queue first
                float time = i + 1;
                p[packetIndex].Timestamp = time;
                m.Enqueue(time, () => m.PayloadAvionics.Queues[queueIndex].Receive(p[packetIndex]));
            }

            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, p.Count);
            for (var i = 0; i < p.Count; i++)
                Assert.AreEqual(p[i], m.Packets[i]);
        }

        [Test]
        public void Test8()
        {
            var m = new PriorityQueueTestModel1() as PriorityQueueTestModel1;
            m.BuildC();
            m.PayloadAvionics.BitRate = 790f;  // 1 packet in just over a second
            var p = Enumerable.Range(0, 3).Select(i => new Packet { Length = 100 }).ToList();

            m.Enqueue(1f, () => m.PayloadAvionics.Queues[2].Receive(p[0]));
            m.Enqueue(1.1f, () => m.PayloadAvionics.Queues[1].Receive(p[1]));
            m.Enqueue(1.1f, () => m.PayloadAvionics.Queues[0].Receive(p[2]));

            m.RunInternal();
            Assert.AreEqual(m.Packets.Count, p.Count);
            Assert.AreEqual(p[0], m.Packets[0]);
            Assert.AreEqual(p[2], m.Packets[1]);
            Assert.AreEqual(p[1], m.Packets[2]);
        }
    }

    public class TestModel : Model, PacketReceiver
    {
        public List<Packet> Packets = new List<Packet>();
        public bool Receive(Packet p)
        {
            p.Received = Time;
            Packets.Add(p);
            return true; }
        public static List<Packet> Run<T>() where T:TestModel
        {
            var model = Activator.CreateInstance<T>();
            model.Run();
            return model.Packets;
        }
        //var packets = TestModel.Run<PriorityQueueTestModel1>();
    }

    public class PriorityQueueTestModel1 : TestModel
    {
        public PriorityPacketQueue PayloadAvionics;
        public void BuildA()
        {
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 80000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // A
            PayloadAvionics.Receiver = this;
        }
        public void BuildB()
        {
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 80000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // A
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // B
            PayloadAvionics.Receiver = this;
        }
        public void BuildC()
        {
            PayloadAvionics = new PriorityPacketQueue { Model = this, BitRate = 80000f };
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // A
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // B
            PayloadAvionics.AddQueue(new PacketQueue { Model = this, Owner = PayloadAvionics, Size = 100 });  // C
            PayloadAvionics.Receiver = this;
        }
    }
}
