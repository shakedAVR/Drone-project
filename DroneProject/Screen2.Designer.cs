namespace DroneProject
{
    partial class Screen2
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
            this.richTextBox_DataSwitch = new System.Windows.Forms.RichTextBox();
            this.button_Replace = new System.Windows.Forms.Button();
            this.button_Delete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox_DataSwitch
            // 
            this.richTextBox_DataSwitch.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.richTextBox_DataSwitch.Location = new System.Drawing.Point(56, 12);
            this.richTextBox_DataSwitch.Name = "richTextBox_DataSwitch";
            this.richTextBox_DataSwitch.Size = new System.Drawing.Size(716, 321);
            this.richTextBox_DataSwitch.TabIndex = 0;
            this.richTextBox_DataSwitch.Text = "";
            // 
            // button_Replace
            // 
            this.button_Replace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.button_Replace.Location = new System.Drawing.Point(69, 371);
            this.button_Replace.Name = "button_Replace";
            this.button_Replace.Size = new System.Drawing.Size(336, 106);
            this.button_Replace.TabIndex = 1;
            this.button_Replace.Text = "Replace";
            this.button_Replace.UseVisualStyleBackColor = false;
            this.button_Replace.Click += new System.EventHandler(this.button_Replace_Click);
            // 
            // button_Delete
            // 
            this.button_Delete.BackColor = System.Drawing.Color.Lime;
            this.button_Delete.Location = new System.Drawing.Point(481, 371);
            this.button_Delete.Name = "button_Delete";
            this.button_Delete.Size = new System.Drawing.Size(304, 106);
            this.button_Delete.TabIndex = 3;
            this.button_Delete.Text = "Delete";
            this.button_Delete.UseVisualStyleBackColor = false;
            this.button_Delete.Click += new System.EventHandler(this.button_Delete_Click);
            // 
            // Screen2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Highlight;
            this.ClientSize = new System.Drawing.Size(838, 511);
            this.Controls.Add(this.button_Delete);
            this.Controls.Add(this.button_Replace);
            this.Controls.Add(this.richTextBox_DataSwitch);
            this.Name = "Screen2";
            this.Text = "Screen2";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_DataSwitch;
        private System.Windows.Forms.Button button_Replace;
        private System.Windows.Forms.Button button_Delete;
    }
}