﻿namespace SAM.Core.Mollier.UI.Controls
{
    partial class CoolingProcessControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.processCalculateType_ComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel_Main = new System.Windows.Forms.FlowLayoutPanel();
            this.MollierPointControl_Start = new SAM.Core.Mollier.UI.Controls.MollierPointControl();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(240, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Calculation Type";
            // 
            // processCalculateType_ComboBox
            // 
            this.processCalculateType_ComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.processCalculateType_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processCalculateType_ComboBox.FormattingEnabled = true;
            this.processCalculateType_ComboBox.Items.AddRange(new object[] {
            "Dry Bulb Temperature",
            "Enthalpy Difference",
            "Dry Bulb Temperature Difference",
            "Medium and Dry Bulb Temperature",
            "Medium and Efficiency"});
            this.processCalculateType_ComboBox.Location = new System.Drawing.Point(244, 30);
            this.processCalculateType_ComboBox.Name = "processCalculateType_ComboBox";
            this.processCalculateType_ComboBox.Size = new System.Drawing.Size(242, 24);
            this.processCalculateType_ComboBox.TabIndex = 5;
            this.processCalculateType_ComboBox.SelectedIndexChanged += new System.EventHandler(this.processCalculateType_ComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(155, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Air Start Conditions";
            // 
            // flowLayoutPanel_Main
            // 
            this.flowLayoutPanel_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel_Main.Location = new System.Drawing.Point(244, 60);
            this.flowLayoutPanel_Main.Name = "flowLayoutPanel_Main";
            this.flowLayoutPanel_Main.Size = new System.Drawing.Size(373, 290);
            this.flowLayoutPanel_Main.TabIndex = 9;
            // 
            // MollierPointControl_Start
            // 
            this.MollierPointControl_Start.Location = new System.Drawing.Point(3, 53);
            this.MollierPointControl_Start.Name = "MollierPointControl_Start";
            this.MollierPointControl_Start.Pressure = 101325D;
            this.MollierPointControl_Start.PressureEnabled = true;
            this.MollierPointControl_Start.PressureVisible = true;
            this.MollierPointControl_Start.SelectMollierPointVisible = false;
            this.MollierPointControl_Start.Size = new System.Drawing.Size(172, 220);
            this.MollierPointControl_Start.TabIndex = 10;
            // 
            // CoolingProcessControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.MollierPointControl_Start);
            this.Controls.Add(this.flowLayoutPanel_Main);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.processCalculateType_ComboBox);
            this.Name = "CoolingProcessControl";
            this.Size = new System.Drawing.Size(658, 346);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox processCalculateType_ComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Main;
        private MollierPointControl MollierPointControl_Start;
    }
}
