using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

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
            var model = new SimpleModel { PrintMessages = cbPrintMessages.Checked, PrintReport = cbPrintMessages.Checked, TheCase = GetTheCase() };
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
    }
}
