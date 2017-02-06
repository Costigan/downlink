namespace Downlink
{
    partial class RunModel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnRun = new System.Windows.Forms.Button();
            this.txtEvents = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtReport = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnScience = new System.Windows.Forms.Button();
            this.btnRails = new System.Windows.Forms.Button();
            this.numDownlink = new System.Windows.Forms.NumericUpDown();
            this.numTestPldSpeed = new System.Windows.Forms.NumericUpDown();
            this.button2 = new System.Windows.Forms.Button();
            this.btnAIM = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lbModel = new System.Windows.Forms.ListBox();
            this.cbPrintMessages = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabReports = new System.Windows.Forms.TabPage();
            this.tabPlots = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lbPlots = new System.Windows.Forms.ListBox();
            this.zed1 = new ZedGraph.ZedGraphControl();
            this.tabOther = new System.Windows.Forms.TabPage();
            this.zed2 = new ZedGraph.ZedGraphControl();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnRateVsDrops = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDownlink)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTestPldSpeed)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabReports.SuspendLayout();
            this.tabPlots.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabOther.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(12, 12);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtEvents
            // 
            this.txtEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEvents.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEvents.Location = new System.Drawing.Point(3, 16);
            this.txtEvents.Multiline = true;
            this.txtEvents.Name = "txtEvents";
            this.txtEvents.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEvents.Size = new System.Drawing.Size(258, 616);
            this.txtEvents.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtEvents);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(264, 635);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Events";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtReport);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(819, 635);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Report";
            // 
            // txtReport
            // 
            this.txtReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReport.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtReport.Location = new System.Drawing.Point(3, 16);
            this.txtReport.Multiline = true;
            this.txtReport.Name = "txtReport";
            this.txtReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtReport.Size = new System.Drawing.Size(813, 616);
            this.txtReport.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1087, 635);
            this.splitContainer1.SplitterDistance = 819;
            this.splitContainer1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.cbPrintMessages);
            this.panel1.Controls.Add(this.btnRun);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(234, 667);
            this.panel1.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnScience);
            this.groupBox3.Controls.Add(this.btnRails);
            this.groupBox3.Controls.Add(this.numDownlink);
            this.groupBox3.Controls.Add(this.numTestPldSpeed);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.btnAIM);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Location = new System.Drawing.Point(12, 312);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 217);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Separate Avionics";
            // 
            // btnScience
            // 
            this.btnScience.Location = new System.Drawing.Point(3, 96);
            this.btnScience.Name = "btnScience";
            this.btnScience.Size = new System.Drawing.Size(82, 23);
            this.btnScience.TabIndex = 12;
            this.btnScience.Text = "Science";
            this.btnScience.UseVisualStyleBackColor = true;
            this.btnScience.Click += new System.EventHandler(this.btnScience_Click);
            // 
            // btnRails
            // 
            this.btnRails.Location = new System.Drawing.Point(3, 70);
            this.btnRails.Name = "btnRails";
            this.btnRails.Size = new System.Drawing.Size(38, 23);
            this.btnRails.TabIndex = 11;
            this.btnRails.Text = "Rails";
            this.btnRails.UseVisualStyleBackColor = true;
            this.btnRails.Click += new System.EventHandler(this.btnRails_Click);
            // 
            // numDownlink
            // 
            this.numDownlink.Location = new System.Drawing.Point(134, 70);
            this.numDownlink.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numDownlink.Name = "numDownlink";
            this.numDownlink.Size = new System.Drawing.Size(60, 20);
            this.numDownlink.TabIndex = 10;
            this.numDownlink.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // numTestPldSpeed
            // 
            this.numTestPldSpeed.Location = new System.Drawing.Point(134, 96);
            this.numTestPldSpeed.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numTestPldSpeed.Name = "numTestPldSpeed";
            this.numTestPldSpeed.Size = new System.Drawing.Size(60, 20);
            this.numTestPldSpeed.TabIndex = 10;
            this.numTestPldSpeed.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 135);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(170, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "Compare Modes";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnAIM
            // 
            this.btnAIM.Location = new System.Drawing.Point(47, 70);
            this.btnAIM.Name = "btnAIM";
            this.btnAIM.Size = new System.Drawing.Size(38, 23);
            this.btnAIM.TabIndex = 8;
            this.btnAIM.Text = "AIM";
            this.btnAIM.UseVisualStyleBackColor = true;
            this.btnAIM.Click += new System.EventHandler(this.btnAIM_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(170, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Vary downlink && pld rate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lbModel);
            this.groupBox4.Location = new System.Drawing.Point(12, 64);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(216, 242);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Choose one Model";
            // 
            // lbModel
            // 
            this.lbModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbModel.FormattingEnabled = true;
            this.lbModel.Location = new System.Drawing.Point(3, 16);
            this.lbModel.Name = "lbModel";
            this.lbModel.Size = new System.Drawing.Size(210, 223);
            this.lbModel.TabIndex = 3;
            // 
            // cbPrintMessages
            // 
            this.cbPrintMessages.AutoSize = true;
            this.cbPrintMessages.Location = new System.Drawing.Point(12, 41);
            this.cbPrintMessages.Name = "cbPrintMessages";
            this.cbPrintMessages.Size = new System.Drawing.Size(98, 17);
            this.cbPrintMessages.TabIndex = 1;
            this.cbPrintMessages.Text = "Print Messages";
            this.cbPrintMessages.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabReports);
            this.tabControl1.Controls.Add(this.tabPlots);
            this.tabControl1.Controls.Add(this.tabOther);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(234, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1101, 667);
            this.tabControl1.TabIndex = 6;
            // 
            // tabReports
            // 
            this.tabReports.Controls.Add(this.splitContainer1);
            this.tabReports.Location = new System.Drawing.Point(4, 22);
            this.tabReports.Name = "tabReports";
            this.tabReports.Padding = new System.Windows.Forms.Padding(3);
            this.tabReports.Size = new System.Drawing.Size(1093, 641);
            this.tabReports.TabIndex = 0;
            this.tabReports.Text = "Reports";
            this.tabReports.UseVisualStyleBackColor = true;
            // 
            // tabPlots
            // 
            this.tabPlots.Controls.Add(this.splitContainer2);
            this.tabPlots.Location = new System.Drawing.Point(4, 22);
            this.tabPlots.Name = "tabPlots";
            this.tabPlots.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlots.Size = new System.Drawing.Size(1093, 641);
            this.tabPlots.TabIndex = 1;
            this.tabPlots.Text = "Plots";
            this.tabPlots.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lbPlots);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.zed1);
            this.splitContainer2.Size = new System.Drawing.Size(1087, 635);
            this.splitContainer2.SplitterDistance = 105;
            this.splitContainer2.TabIndex = 0;
            // 
            // lbPlots
            // 
            this.lbPlots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPlots.FormattingEnabled = true;
            this.lbPlots.Location = new System.Drawing.Point(0, 0);
            this.lbPlots.Name = "lbPlots";
            this.lbPlots.Size = new System.Drawing.Size(105, 635);
            this.lbPlots.TabIndex = 0;
            this.lbPlots.SelectedValueChanged += new System.EventHandler(this.lbPlots_SelectedValueChanged);
            // 
            // zed1
            // 
            this.zed1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zed1.Location = new System.Drawing.Point(0, 0);
            this.zed1.Name = "zed1";
            this.zed1.ScrollGrace = 0D;
            this.zed1.ScrollMaxX = 0D;
            this.zed1.ScrollMaxY = 0D;
            this.zed1.ScrollMaxY2 = 0D;
            this.zed1.ScrollMinX = 0D;
            this.zed1.ScrollMinY = 0D;
            this.zed1.ScrollMinY2 = 0D;
            this.zed1.Size = new System.Drawing.Size(978, 635);
            this.zed1.TabIndex = 0;
            this.zed1.UseExtendedPrintDialog = true;
            // 
            // tabOther
            // 
            this.tabOther.Controls.Add(this.zed2);
            this.tabOther.Controls.Add(this.splitter1);
            this.tabOther.Controls.Add(this.panel2);
            this.tabOther.Location = new System.Drawing.Point(4, 22);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(1093, 641);
            this.tabOther.TabIndex = 2;
            this.tabOther.Text = "Other Calculations";
            this.tabOther.UseVisualStyleBackColor = true;
            // 
            // zed2
            // 
            this.zed2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zed2.Location = new System.Drawing.Point(137, 0);
            this.zed2.Name = "zed2";
            this.zed2.ScrollGrace = 0D;
            this.zed2.ScrollMaxX = 0D;
            this.zed2.ScrollMaxY = 0D;
            this.zed2.ScrollMaxY2 = 0D;
            this.zed2.ScrollMinX = 0D;
            this.zed2.ScrollMinY = 0D;
            this.zed2.ScrollMinY2 = 0D;
            this.zed2.Size = new System.Drawing.Size(956, 641);
            this.zed2.TabIndex = 1;
            this.zed2.UseExtendedPrintDialog = true;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(134, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 641);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnRateVsDrops);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(134, 641);
            this.panel2.TabIndex = 2;
            // 
            // btnRateVsDrops
            // 
            this.btnRateVsDrops.Location = new System.Drawing.Point(3, 15);
            this.btnRateVsDrops.Name = "btnRateVsDrops";
            this.btnRateVsDrops.Size = new System.Drawing.Size(105, 23);
            this.btnRateVsDrops.TabIndex = 0;
            this.btnRateVsDrops.Text = "btnRateVsDrops";
            this.btnRateVsDrops.UseVisualStyleBackColor = true;
            this.btnRateVsDrops.Click += new System.EventHandler(this.btnRateVsDrops_Click);
            // 
            // RunModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1335, 667);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "RunModel";
            this.Text = "Downlink Model";
            this.Load += new System.EventHandler(this.RunModel_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numDownlink)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTestPldSpeed)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabReports.ResumeLayout(false);
            this.tabPlots.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabOther.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TextBox txtEvents;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtReport;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbPrintMessages;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabReports;
        private System.Windows.Forms.TabPage tabPlots;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox lbPlots;
        private ZedGraph.ZedGraphControl zed1;
        private System.Windows.Forms.TabPage tabOther;
        private ZedGraph.ZedGraphControl zed2;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnRateVsDrops;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListBox lbModel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnAIM;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown numTestPldSpeed;
        private System.Windows.Forms.Button btnScience;
        private System.Windows.Forms.Button btnRails;
        private System.Windows.Forms.NumericUpDown numDownlink;
    }
}

