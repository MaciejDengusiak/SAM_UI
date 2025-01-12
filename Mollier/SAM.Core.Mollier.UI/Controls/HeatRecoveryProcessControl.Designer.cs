﻿namespace SAM.Core.Mollier.UI.Controls
{
    partial class HeatRecoveryProcessControl
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
            this.MollierPointControl_Supply = new SAM.Core.Mollier.UI.Controls.MollierPointControl();
            this.MollierPointControl_Return = new SAM.Core.Mollier.UI.Controls.MollierPointControl();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SensibleHeatRecoveryEfficiencyControl = new SAM.Core.Mollier.UI.Controls.ParameterControl();
            this.LatentHeatRecoveryEfficiencyControl = new SAM.Core.Mollier.UI.Controls.ParameterControl();
            this.SuspendLayout();
            // 
            // MollierPointControl_Supply
            // 
            this.MollierPointControl_Supply.Location = new System.Drawing.Point(0, 23);
            this.MollierPointControl_Supply.Name = "MollierPointControl_Supply";
            this.MollierPointControl_Supply.Pressure = 101325D;
            this.MollierPointControl_Supply.PressureEnabled = true;
            this.MollierPointControl_Supply.PressureVisible = true;
            this.MollierPointControl_Supply.SelectMollierPointVisible = false;
            this.MollierPointControl_Supply.Size = new System.Drawing.Size(199, 220);
            this.MollierPointControl_Supply.TabIndex = 0;
            // 
            // MollierPointControl_Return
            // 
            this.MollierPointControl_Return.Location = new System.Drawing.Point(205, 23);
            this.MollierPointControl_Return.Name = "MollierPointControl_Return";
            this.MollierPointControl_Return.Pressure = 101325D;
            this.MollierPointControl_Return.PressureEnabled = true;
            this.MollierPointControl_Return.PressureVisible = true;
            this.MollierPointControl_Return.SelectMollierPointVisible = false;
            this.MollierPointControl_Return.Size = new System.Drawing.Size(191, 220);
            this.MollierPointControl_Return.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(56, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 20);
            this.label2.TabIndex = 14;
            this.label2.Text = "Supply";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(262, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 20);
            this.label1.TabIndex = 15;
            this.label1.Text = "Return";
            // 
            // SensibleHeatRecoveryEfficiencyControl
            // 
            this.SensibleHeatRecoveryEfficiencyControl.Location = new System.Drawing.Point(402, 23);
            this.SensibleHeatRecoveryEfficiencyControl.Name = "SensibleHeatRecoveryEfficiencyControl";
            this.SensibleHeatRecoveryEfficiencyControl.ProcessParameterType = SAM.Core.Mollier.UI.ProcessParameterType.Undefined;
            this.SensibleHeatRecoveryEfficiencyControl.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.SensibleHeatRecoveryEfficiencyControl.Size = new System.Drawing.Size(200, 51);
            this.SensibleHeatRecoveryEfficiencyControl.TabIndex = 16;
            this.SensibleHeatRecoveryEfficiencyControl.UnitType = SAM.Units.UnitType.Undefined;
            this.SensibleHeatRecoveryEfficiencyControl.Value = 0D;
            // 
            // LatentHeatRecoveryEfficiencyControl
            // 
            this.LatentHeatRecoveryEfficiencyControl.Location = new System.Drawing.Point(402, 80);
            this.LatentHeatRecoveryEfficiencyControl.Name = "LatentHeatRecoveryEfficiencyControl";
            this.LatentHeatRecoveryEfficiencyControl.ProcessParameterType = SAM.Core.Mollier.UI.ProcessParameterType.Undefined;
            this.LatentHeatRecoveryEfficiencyControl.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.LatentHeatRecoveryEfficiencyControl.Size = new System.Drawing.Size(200, 51);
            this.LatentHeatRecoveryEfficiencyControl.TabIndex = 17;
            this.LatentHeatRecoveryEfficiencyControl.UnitType = SAM.Units.UnitType.Undefined;
            this.LatentHeatRecoveryEfficiencyControl.Value = 0D;
            // 
            // HeatRecoveryProcessControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.LatentHeatRecoveryEfficiencyControl);
            this.Controls.Add(this.SensibleHeatRecoveryEfficiencyControl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MollierPointControl_Return);
            this.Controls.Add(this.MollierPointControl_Supply);
            this.Name = "HeatRecoveryProcessControl";
            this.Size = new System.Drawing.Size(639, 338);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MollierPointControl MollierPointControl_Supply;
        private MollierPointControl MollierPointControl_Return;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private ParameterControl SensibleHeatRecoveryEfficiencyControl;
        private ParameterControl LatentHeatRecoveryEfficiencyControl;
    }
}
