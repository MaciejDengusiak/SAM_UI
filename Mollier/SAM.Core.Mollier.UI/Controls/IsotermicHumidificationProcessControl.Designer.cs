﻿namespace SAM.Core.Mollier.UI.Controls
{
    partial class IsotermicHumidificationProcessControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel_Main = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.processCalculateType_ComboBox = new System.Windows.Forms.ComboBox();
            this.MollierPointControl_Start = new SAM.Core.Mollier.UI.Controls.MollierPointControl();
            this.SuspendLayout();
            // 
            // flowLayoutPanel_Main
            // 
            this.flowLayoutPanel_Main.Location = new System.Drawing.Point(261, 55);
            this.flowLayoutPanel_Main.Name = "flowLayoutPanel_Main";
            this.flowLayoutPanel_Main.Size = new System.Drawing.Size(360, 177);
            this.flowLayoutPanel_Main.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(17, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(155, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Air Start Conditions";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(257, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 20);
            this.label1.TabIndex = 17;
            this.label1.Text = "Calculation Type";
            // 
            // processCalculateType_ComboBox
            // 
            this.processCalculateType_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processCalculateType_ComboBox.FormattingEnabled = true;
            this.processCalculateType_ComboBox.Items.AddRange(new object[] {
            "Humidity Ratio Difference",
            "Relative Humidity"});
            this.processCalculateType_ComboBox.Location = new System.Drawing.Point(261, 25);
            this.processCalculateType_ComboBox.Name = "processCalculateType_ComboBox";
            this.processCalculateType_ComboBox.Size = new System.Drawing.Size(266, 24);
            this.processCalculateType_ComboBox.TabIndex = 16;
            this.processCalculateType_ComboBox.SelectedIndexChanged += new System.EventHandler(this.processCalculateType_ComboBox_SelectedIndexChanged);
            // 
            // MollierPointControl_Start
            // 
            this.MollierPointControl_Start.Location = new System.Drawing.Point(0, 23);
            this.MollierPointControl_Start.Name = "MollierPointControl_Start";
            this.MollierPointControl_Start.Pressure = double.NaN;
            this.MollierPointControl_Start.PressureEnabled = true;
            this.MollierPointControl_Start.PressureVisible = true;
            this.MollierPointControl_Start.SelectMollierPointVisible = false;
            this.MollierPointControl_Start.Size = new System.Drawing.Size(199, 220);
            this.MollierPointControl_Start.TabIndex = 20;
            // 
            // IsotermicHumidificationProcessControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.MollierPointControl_Start);
            this.Controls.Add(this.flowLayoutPanel_Main);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.processCalculateType_ComboBox);
            this.Name = "IsotermicHumidificationProcessControl";
            this.Size = new System.Drawing.Size(645, 280);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MollierPointControl MollierPointControl_Start;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Main;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox processCalculateType_ComboBox;
    }
}
