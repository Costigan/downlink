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
            this.rbScienceStation = new System.Windows.Forms.RadioButton();
            this.rbRails = new System.Windows.Forms.RadioButton();
            this.cbPrintMessages = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabReports = new System.Windows.Forms.TabPage();
            this.tabPlots = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lbPlots = new System.Windows.Forms.ListBox();
            this.zed1 = new ZedGraph.ZedGraphControl();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabReports.SuspendLayout();
            this.tabPlots.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
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
            this.txtEvents.Size = new System.Drawing.Size(677, 616);
            this.txtEvents.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtEvents);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(683, 635);
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
            this.groupBox2.Size = new System.Drawing.Size(341, 635);
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
            this.txtReport.Size = new System.Drawing.Size(335, 616);
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
            this.splitContainer1.Size = new System.Drawing.Size(1028, 635);
            this.splitContainer1.SplitterDistance = 341;
            this.splitContainer1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.cbPrintMessages);
            this.panel1.Controls.Add(this.btnRun);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(134, 667);
            this.panel1.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbScienceStation);
            this.groupBox3.Controls.Add(this.rbRails);
            this.groupBox3.Location = new System.Drawing.Point(12, 64);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(116, 67);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mode";
            // 
            // rbScienceStation
            // 
            this.rbScienceStation.AutoSize = true;
            this.rbScienceStation.Location = new System.Drawing.Point(14, 41);
            this.rbScienceStation.Name = "rbScienceStation";
            this.rbScienceStation.Size = new System.Drawing.Size(100, 17);
            this.rbScienceStation.TabIndex = 0;
            this.rbScienceStation.TabStop = true;
            this.rbScienceStation.Text = "Science Station";
            this.rbScienceStation.UseVisualStyleBackColor = true;
            // 
            // rbRails
            // 
            this.rbRails.AutoSize = true;
            this.rbRails.Location = new System.Drawing.Point(14, 18);
            this.rbRails.Name = "rbRails";
            this.rbRails.Size = new System.Drawing.Size(48, 17);
            this.rbRails.TabIndex = 0;
            this.rbRails.TabStop = true;
            this.rbRails.Text = "Rails";
            this.rbRails.UseVisualStyleBackColor = true;
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
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(134, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1042, 667);
            this.tabControl1.TabIndex = 6;
            // 
            // tabReports
            // 
            this.tabReports.Controls.Add(this.splitContainer1);
            this.tabReports.Location = new System.Drawing.Point(4, 22);
            this.tabReports.Name = "tabReports";
            this.tabReports.Padding = new System.Windows.Forms.Padding(3);
            this.tabReports.Size = new System.Drawing.Size(1034, 641);
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
            this.tabPlots.Size = new System.Drawing.Size(1034, 641);
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
            this.splitContainer2.Size = new System.Drawing.Size(1028, 635);
            this.splitContainer2.SplitterDistance = 100;
            this.splitContainer2.TabIndex = 0;
            // 
            // lbPlots
            // 
            this.lbPlots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPlots.FormattingEnabled = true;
            this.lbPlots.Location = new System.Drawing.Point(0, 0);
            this.lbPlots.Name = "lbPlots";
            this.lbPlots.Size = new System.Drawing.Size(100, 635);
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
            this.zed1.Size = new System.Drawing.Size(924, 635);
            this.zed1.TabIndex = 0;
            this.zed1.UseExtendedPrintDialog = true;
            // 
            // RunModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 667);
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
            this.groupBox3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabReports.ResumeLayout(false);
            this.tabPlots.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rbScienceStation;
        private System.Windows.Forms.RadioButton rbRails;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabReports;
        private System.Windows.Forms.TabPage tabPlots;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox lbPlots;
        private ZedGraph.ZedGraphControl zed1;
    }
}

