namespace Helpers
{
    partial class DeviceManagerForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button_initialize = new System.Windows.Forms.Button();
            this.button_StartSensors = new System.Windows.Forms.Button();
            this.button_StartEffectors = new System.Windows.Forms.Button();
            this.button_PauseEffectors = new System.Windows.Forms.Button();
            this.button_EmergencyStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label_DeviceManagerAction = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_deviceManagerOverallState = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(8, 28);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(349, 266);
            this.dataGridView1.TabIndex = 0;
            // 
            // button_initialize
            // 
            this.button_initialize.Location = new System.Drawing.Point(372, 28);
            this.button_initialize.Name = "button_initialize";
            this.button_initialize.Size = new System.Drawing.Size(104, 23);
            this.button_initialize.TabIndex = 1;
            this.button_initialize.Text = "Initialize";
            this.button_initialize.UseVisualStyleBackColor = true;
            this.button_initialize.Click += new System.EventHandler(this.button_initialize_Click);
            // 
            // button_StartSensors
            // 
            this.button_StartSensors.Enabled = false;
            this.button_StartSensors.Location = new System.Drawing.Point(372, 70);
            this.button_StartSensors.Name = "button_StartSensors";
            this.button_StartSensors.Size = new System.Drawing.Size(104, 23);
            this.button_StartSensors.TabIndex = 2;
            this.button_StartSensors.Text = "Start Sensors";
            this.button_StartSensors.UseVisualStyleBackColor = true;
            this.button_StartSensors.Click += new System.EventHandler(this.button_StartSensors_Click);
            // 
            // button_StartEffectors
            // 
            this.button_StartEffectors.Enabled = false;
            this.button_StartEffectors.Location = new System.Drawing.Point(372, 119);
            this.button_StartEffectors.Name = "button_StartEffectors";
            this.button_StartEffectors.Size = new System.Drawing.Size(104, 23);
            this.button_StartEffectors.TabIndex = 3;
            this.button_StartEffectors.Text = "Start Effectors";
            this.button_StartEffectors.UseVisualStyleBackColor = true;
            this.button_StartEffectors.Click += new System.EventHandler(this.button_StartEffectors_Click);
            // 
            // button_PauseEffectors
            // 
            this.button_PauseEffectors.Enabled = false;
            this.button_PauseEffectors.Location = new System.Drawing.Point(372, 162);
            this.button_PauseEffectors.Name = "button_PauseEffectors";
            this.button_PauseEffectors.Size = new System.Drawing.Size(104, 23);
            this.button_PauseEffectors.TabIndex = 4;
            this.button_PauseEffectors.Text = "Pause Effectors";
            this.button_PauseEffectors.UseVisualStyleBackColor = true;
            this.button_PauseEffectors.Click += new System.EventHandler(this.button_PauseEffectors_Click);
            // 
            // button_EmergencyStop
            // 
            this.button_EmergencyStop.Enabled = false;
            this.button_EmergencyStop.Location = new System.Drawing.Point(372, 208);
            this.button_EmergencyStop.Name = "button_EmergencyStop";
            this.button_EmergencyStop.Size = new System.Drawing.Size(104, 23);
            this.button_EmergencyStop.TabIndex = 5;
            this.button_EmergencyStop.Text = "Emergency Stop";
            this.button_EmergencyStop.UseVisualStyleBackColor = true;
            this.button_EmergencyStop.Click += new System.EventHandler(this.button_EmergencyStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(235, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Current Device Manager action: ";
            // 
            // label_DeviceManagerAction
            // 
            this.label_DeviceManagerAction.AutoSize = true;
            this.label_DeviceManagerAction.Location = new System.Drawing.Point(403, 7);
            this.label_DeviceManagerAction.Name = "label_DeviceManagerAction";
            this.label_DeviceManagerAction.Size = new System.Drawing.Size(36, 13);
            this.label_DeviceManagerAction.TabIndex = 7;
            this.label_DeviceManagerAction.Text = "action";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Current overall state: ";
            // 
            // label_deviceManagerOverallState
            // 
            this.label_deviceManagerOverallState.AutoSize = true;
            this.label_deviceManagerOverallState.Location = new System.Drawing.Point(122, 7);
            this.label_deviceManagerOverallState.Name = "label_deviceManagerOverallState";
            this.label_deviceManagerOverallState.Size = new System.Drawing.Size(22, 13);
            this.label_deviceManagerOverallState.TabIndex = 9;
            this.label_deviceManagerOverallState.Text = "OK";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.DarkRed;
            this.button1.ForeColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(387, 249);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 39);
            this.button1.TabIndex = 10;
            this.button1.Text = "Full Start Sequence";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DeviceManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 300);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label_deviceManagerOverallState);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_DeviceManagerAction);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_EmergencyStop);
            this.Controls.Add(this.button_PauseEffectors);
            this.Controls.Add(this.button_StartEffectors);
            this.Controls.Add(this.button_StartSensors);
            this.Controls.Add(this.button_initialize);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "DeviceManagerForm";
            this.Text = "DeviceManagerForm";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button_initialize;
        private System.Windows.Forms.Button button_StartEffectors;
        private System.Windows.Forms.Button button_PauseEffectors;
        private System.Windows.Forms.Button button_EmergencyStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_DeviceManagerAction;
        private System.Windows.Forms.Button button_StartSensors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_deviceManagerOverallState;
        private System.Windows.Forms.Button button1;
    }
}