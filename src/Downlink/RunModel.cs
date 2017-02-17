using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace Downlink
{
    public partial class RunModel : Form
    {
        public RunModel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var modelChoice = lbModel.SelectedItem;
            if (modelChoice == null)
            {
                MessageBox.Show(@"Please select a model from the list on the left");
                return;
            }
            var mtype = modelChoice.GetType();
            var model = (SharedModel)Activator.CreateInstance(mtype);
            model.PrintMessages = cbPrintMessages.Checked;
            model.PrintReport = cbPrintMessages.Checked;

            //Model model = new SingleMultiplexor { PrintMessages = cbPrintMessages.Checked, PrintReport = cbPrintMessages.Checked, TheCase = GetTheCase() };
            //var model = new OldDownlink.SimpleModel { PrintMessages = cbPrintMessages.Checked, PrintReport = cbPrintMessages.Checked, TheCase = GetTheCase() };
            model.Run();

            if (!cbPrintMessages.Checked)
                FillReport(model);
            if (cbPrintMessages.Checked)
                PrintReport(model);
        }

        private void FillReport(SharedModel m)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                foreach (var line in m.EventMessages)
                    sw.WriteLine(line);
            }
            txtEvents.Text = sb.ToString();

            using (var sw = new StringWriter())
            {
                m.Stats.Report(sw);
                btnTest2.Text = sw.ToString();
            }
        }

        private void PrintReport(SharedModel m)
        {
            m.Stats.Report(Console.Out);
        }

        private void RunModel_Load(object sender, EventArgs e)
        {
            lbModel.Items.AddRange(
                new Model[]
                {
                    new SingleMultiplexorRails(),
                    new SingleMultiplexorScience(),
                    new SeparateAvionicsRails(),
                    new SeparateAvionicsAIM(),
                    new SeparateAvionicsScience(),
                    new SeparateAvionicsRailsSingleMove(),
                    new SimpleModel100(),
                    new SimpleModel400(),
                    new SimpleModel600(),
                    new Model5a(),
                    new Model5b(),
                    new Model5c(),
                    new Model5d(),
                    new Model5e(),
                    new SeparateAvionicsScience() {DownlinkRate=110000f },
                }
                );
            lbPlots.Items.AddRange(
                new PlotAction[]
                {
                new PlotAction { Name = "Plot VC queue lengths", Action = () => PlotVCQueueLengths() },
                new PlotAction { Name = "Plot VC queue byte counts", Action = () => PlotVCQueueByteCounts() },
                new PlotAction { Name = "Plot VC queue drops", Action = () => PlotVCQueueDrops() },
                new PlotAction { Name = "Clear Graph", Action = () => zed1.GraphPane.CurveList.Clear() }
                });
        }

        protected Color[] Colors = new Color[]
        {
            Color.Black,Color.DarkRed,Color.Red,Color.Green,Color.Blue
        };

        private void PlotVCQueueLengths()
        {
            var m = Model.TheModel;
            zed1.GraphPane.CurveList.Clear();
            var states = m.StateSamples;
            if (states.Count < 1) return;
            var max = states[0].QueueLength.Length;
            for (var vc = 0;vc<max;vc++)
            {
                var points = new PointPairList();
                foreach (var s in states)
                    points.Add(s.Time, s.QueueLength[vc]);
                zed1.GraphPane.AddCurve("VC" + vc, points, HSLColor.FetchCachedHue(vc, max), SymbolType.None);
            }

            zed1.GraphPane.Title.Text = "VC Queue Lengths";
            zed1.GraphPane.XAxis.Title.Text = "Time (sec)";
            zed1.GraphPane.YAxis.Title.Text = "Queue Length";

            zed1.ZoomOutAll(zed1.GraphPane);
            zed1.GraphPane.AxisChange();
            zed1.Invalidate();
        }

        private void PlotVCQueueByteCounts()
        {
            var m = Model.TheModel;
            zed1.GraphPane.CurveList.Clear();
            var states = m.StateSamples;
            if (states.Count < 1) return;
            var max = states[0].QueueLength.Length;
            for (var vc = 0; vc < max; vc++)
            {
                var points = new PointPairList();
                foreach (var s in states)
                    points.Add(s.Time, s.QueueByteCount[vc]);
                zed1.GraphPane.AddCurve("VC" + vc, points, HSLColor.FetchCachedHue(vc, max), SymbolType.None);
            }

            zed1.GraphPane.Title.Text = "VC Queue Lengths";
            zed1.GraphPane.XAxis.Title.Text = "Time (sec)";
            zed1.GraphPane.YAxis.Title.Text = "Queue Length";

            zed1.ZoomOutAll(zed1.GraphPane);
            zed1.GraphPane.AxisChange();
            zed1.Invalidate();
        }

        private void PlotVCQueueDrops()
        {
            var m = Model.TheModel;
            zed1.GraphPane.CurveList.Clear();
            var states = m.StateSamples;
            if (states.Count < 1) return;
            var max = states[0].Drops.Length;
            for (var vc = 0; vc < max; vc++)
            {
                var points = new PointPairList();
                foreach (var s in states)
                    points.Add(s.Time, s.Drops[vc]);
                zed1.GraphPane.AddCurve("VC" + vc, points, HSLColor.FetchCachedHue(vc, max), SymbolType.None);
            }

            zed1.GraphPane.Title.Text = "VC Queue Drops since last sample";
            zed1.GraphPane.XAxis.Title.Text = "Time (sec)";
            zed1.GraphPane.YAxis.Title.Text = "Drops";

            zed1.ZoomOutAll(zed1.GraphPane);
            zed1.GraphPane.AxisChange();
            zed1.Invalidate();
        }

        private void lbPlots_SelectedValueChanged(object sender, EventArgs e)
        {
            var s = lbPlots.SelectedItem as PlotAction;
            if (s == null) return;
            s.Action();
        }

        private void btnRateVsDrops_Click(object sender, EventArgs e)
        {
            var m = new SingleMultiplexor { PrintMessages = false, PrintReport = false };
            m.Build();
            m.Start();
            zed2.GraphPane.CurveList.Clear();
            var max = m.FrameGenerator.Buffers.Count;
            var points = m.FrameGenerator.Buffers.Select(q => new PointPairList()).ToArray();

            for (var downlink_rate = 100000f; downlink_rate <= 200000f; downlink_rate += 10000f)
            {
                Console.Write(downlink_rate);
                var model = new SingleMultiplexor { PrintMessages = false, PrintReport = false, DownlinkRate = downlink_rate };
                model.Run();

                for (var i = 0; i < max; i++)
                    points[i].Add(downlink_rate, model.FrameGenerator.Buffers[i].PacketQueue.ByteDropCount);
                Console.WriteLine();
            }

            for (var i = 0; i < max; i++)
                zed2.GraphPane.AddCurve("VC" + i, points[i], HSLColor.FetchCachedHue(i, max));

            zed2.GraphPane.Title.Text = "Downlink rate vs drop byte count by VC";
            zed2.GraphPane.XAxis.Title.Text = "Downlink rate (bits/sec)";
            zed2.GraphPane.YAxis.Title.Text = "Dropped (bytes)";

            zed2.ZoomOutAll(zed2.GraphPane);
            zed2.GraphPane.AxisChange();
            zed2.Invalidate();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var m = new SeparateAvionics();
            var stats = m.Calculate(ModelCase.RailsDriving, 10000f, 30000f, 90f, 30f);
            Console.WriteLine(stats.SpeedMadeGood);
        }

        SeparateAvionics _model = new SeparateAvionics();
        private void button1_Click_2(object sender, EventArgs e)
        {
            var downlinkRates = Enumerable.Range(0, 25).Select(i => 100000f + 20000f * i).ToArray();
            //var downlinkRates = new[] { 100000f, 100000f, 100000f, 100000f };
            foreach (var d in downlinkRates)
                Console.Write("{0}\t", d);
            Console.WriteLine();

            ///var payloadRates = Enumerable.Range(0, 10).Select(i => 2 * i * 10000f).ToArray();
            var payloadRates = new[] { 30000 };
            foreach (var d in payloadRates)
                Console.Write("{0}\t", d);
            Console.WriteLine();

            //var payloadRates = new[] { 30000 };
            var table = downlinkRates.Select(r =>
                payloadRates.Select(p =>
                {
                    return _model.CachedCalculate(ModelCase.RailsDriving, r, p, 90f, 30f).SpeedMadeGood;
                    //return m.Calculate(r, p, 90f, 30f).SpeedMadeGood;
                }).ToArray()).ToArray();
            foreach (var a in table)
            {
                foreach (var v in a) Console.Write("{0}\t", v);
                Console.WriteLine();
            }
        }

        IEnumerable<float> Table(float start, float stop, float step)
        {
            for (var v = start; v <= stop; v += step)
                yield return v;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SeparateAvionics model;
            // var model = new SeparateAvionics { PrintMessages = true, PrintReport = true }; ;
            // var s1 = (ModelStats)model.CachedCalculate(ModelCase.RailsDriving, 100000f, 30000f, 90f, 30f);
            // Console.WriteLine(s1.SpeedMadeGood);

            model = new SeparateAvionicsAIM { PrintMessages = true, PrintReport = true };
            var s2 = (ModelStats)model.CachedCalculate(ModelCase.AIMDriving, 100000f, 40000f, 90f, 30f);
            Console.WriteLine(s2.SpeedMadeGood);
            PrintReport(model);

            return;

            model = new SeparateAvionics { PrintMessages = true, PrintReport = true };
            var s3 = (ModelStats)model.CachedCalculate(ModelCase.ScienceDriving, 100000f, 30000f, 90f, 30f);
            Console.WriteLine(s3.SpeedMadeGood);
            PrintReport(model);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void btnRails_Click(object sender, EventArgs e)
        {
            var downlink = ((int)numDownlink.Value) * 1000f;
            var payload = ((int)numTestPldSpeed.Value) * 1000f;
            RunTest(ModelCase.RailsDriving, downlink, payload);
        }

        private void btnScience_Click(object sender, EventArgs e)
        {
            var downlink = ((int)numDownlink.Value) * 1000f;
            var payload = ((int)numTestPldSpeed.Value) * 1000f;
            RunTest(ModelCase.ScienceDriving, downlink, payload);
        }

        private void btnAIM_Click(object sender, EventArgs e)
        {
            var downlink = ((int)numDownlink.Value) * 1000f;
            var payload = ((int)numTestPldSpeed.Value) * 1000f;
            RunTest(ModelCase.ScienceDriving, downlink, payload);
        }

        private void RunTest(ModelCase mode, float downlink, float payload)
        {
            Console.WriteLine(@"Running test mode={0} downlink={1} payload={2}", mode, downlink, payload);
            //var model = new SeparateAvionicsAIM { PrintMessages = true, PrintReport = true };
            var model = new SimpleModel100 { PrintMessages = true, PrintReport = true };
            var s2 = (ModelStats)model.Calculate(mode, downlink, payload, 90f, 30f);
            Console.WriteLine(s2.SpeedMadeGood);
            PrintReport(model);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            var model = new SeparateAvionicsAIM { PrintMessages = true, PrintReport = true };
            var rates = new float[]{10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000,100000, 120000, 140000, 160000, 180000,
                200000, 220000, 240000,260000, 280000, 300000, 320000, 340000, 360000, 380000, 400000,420000, 440000, 460000, 480000,
                500000, 520000, 540000, 560000,580000, 600000};
            var speeds = rates.Select(rate => ((ModelStats)model.CachedCalculate(ModelCase.ScienceDriving, rate, 40000f, 90f, 30f)).SpeedMadeGood).ToArray();

            /*
            var speed100 = ((ModelStats)model.CachedCalculate(ModelCase.ScienceDriving, 100000f, 40000f, 90f, 30f)).SpeedMadeGood;
            var speed200 = ((ModelStats)model.CachedCalculate(ModelCase.ScienceDriving, 200000f, 40000f, 90f, 30f)).SpeedMadeGood;
            Console.WriteLine(speed100);
            Console.WriteLine(speed200);*/

            foreach (var speed in speeds)
                Console.WriteLine(speed);
        }

        private void btnTest3_Click(object sender, EventArgs e)
        {
            var rates = new float[]{10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000,100000, 120000, 140000, 160000, 180000,
                200000, 220000, 240000,260000, 280000, 300000, 320000, 340000, 360000, 380000, 400000,420000, 440000, 460000, 480000,
                500000, 520000, 540000, 560000,580000, 600000};
            var speeds = rates.Select(rate =>
            {
                var model = new SeparateAvionicsAIM { PrintMessages = true, PrintReport = true };
                var speed = ((ModelStats)model.CachedCalculate(ModelCase.ScienceDriving, rate, 40000f, 90f, 30f)).SpeedMadeGood;
                Console.WriteLine(speed);
                return speed;
            }
            ).ToArray();
            foreach (var speed in speeds)
                Console.WriteLine(speed);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var m = new SeparateAvionics();
            foreach (var r in new float[] { 100000f, 110000f, 120000f, 130000f })
            {
                var stats = m.CachedCalculate2(ModelCase.ScienceDriving, r, 40000f, 90f, 30f) as ModelStats;
                var v = stats.PacketReportByAPID[APID.DOCWaypointImage].AvgLatency;
                Console.WriteLine(v);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var m = new Model5();
            foreach (var r in new float[] { 30000f, 25000f, 20000f, 15000f })
            {
                var smg = (m.Calculate3(ModelCase.ScienceDriving, 100000f, 90f, 30f, 30000f, r, 13800f,13800f) as ModelStats).SpeedMadeGood;
                Console.WriteLine(smg);
            }
        }
    }

    internal class PlotAction
    {
        public string Name = String.Empty;
        public Action Action;
        public override string ToString()
        {
            return Name;
        }
    }
}
