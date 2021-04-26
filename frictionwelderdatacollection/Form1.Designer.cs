namespace FrictionWelderDataCollection
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.messageRecievedLabel = new System.Windows.Forms.Label();
            this.successCountLabel = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(210, 91);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // messageRecievedLabel
            // 
            this.messageRecievedLabel.AutoSize = true;
            this.messageRecievedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageRecievedLabel.Location = new System.Drawing.Point(16, 11);
            this.messageRecievedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.messageRecievedLabel.Name = "messageRecievedLabel";
            this.messageRecievedLabel.Size = new System.Drawing.Size(183, 20);
            this.messageRecievedLabel.TabIndex = 1;
            this.messageRecievedLabel.Text = "Messages Recieved:";
            // 
            // successCountLabel
            // 
            this.successCountLabel.AutoSize = true;
            this.successCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.successCountLabel.Location = new System.Drawing.Point(13, 55);
            this.successCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.successCountLabel.Name = "successCountLabel";
            this.successCountLabel.Size = new System.Drawing.Size(223, 20);
            this.successCountLabel.TabIndex = 2;
            this.successCountLabel.Text = "Successful Transactions:";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(20, 91);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(216, 21);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Send Last Weld To Database";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 136);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.successCountLabel);
            this.Controls.Add(this.messageRecievedLabel);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Welder Data Collection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label messageRecievedLabel;
        private System.Windows.Forms.Label successCountLabel;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}

