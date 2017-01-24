using System;
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
            //Model model = new SingleMultiplexor { PrintMessages = cbPrintMessages.Checked, PrintReport = cbPrintMessages.Checked, TheCase = GetTheCase() };
            var model = new OldDownlink.SimpleModel { PrintMessages = cbPrintMessages.Checked, PrintReport = cbPrintMessages.Checked, TheCase = GetTheCase() };
            model.Run();

            if (!cbPrintMessages.Checked)
                FillReport(model);
        }

        private Model.ModelCase GetTheCase()
        {
            if (rbRails.Checked)
                return Model.ModelCase.Rails;
            if (rbScienceStation.Checked)
                return Model.ModelCase.ScienceStation;
            return Model.ModelCase.Rails;
        }

        private void FillReport(Model m)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                foreach (var line in m.EventMessages)
                    sw.WriteLine(line);
            }
            txtEvents.Text = sb.ToString();

            sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                foreach (var line in m.ReportMessages)
                    sw.WriteLine(line);
            }
            txtReport.Text = sb.ToString();
        }

        private void RunModel_Load(object sender, EventArgs e)
        {
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
