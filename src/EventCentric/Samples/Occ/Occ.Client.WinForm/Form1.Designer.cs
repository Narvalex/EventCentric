namespace Occ.Client.WinForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.userNameInput = new System.Windows.Forms.TextBox();
            this.loginBtn = new System.Windows.Forms.Button();
            this.createItemBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.itemNameInput = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User";
            // 
            // userNameInput
            // 
            this.userNameInput.Location = new System.Drawing.Point(47, 23);
            this.userNameInput.Name = "userNameInput";
            this.userNameInput.Size = new System.Drawing.Size(100, 20);
            this.userNameInput.TabIndex = 1;
            this.userNameInput.Text = "OccClient1";
            // 
            // loginBtn
            // 
            this.loginBtn.Location = new System.Drawing.Point(153, 23);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(75, 23);
            this.loginBtn.TabIndex = 2;
            this.loginBtn.Text = "Login";
            this.loginBtn.UseVisualStyleBackColor = true;
            this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
            // 
            // createItemBtn
            // 
            this.createItemBtn.Location = new System.Drawing.Point(165, 12);
            this.createItemBtn.Name = "createItemBtn";
            this.createItemBtn.Size = new System.Drawing.Size(75, 23);
            this.createItemBtn.TabIndex = 3;
            this.createItemBtn.Text = "Create item";
            this.createItemBtn.UseVisualStyleBackColor = true;
            this.createItemBtn.Click += new System.EventHandler(this.createItemBtn_click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Item name";
            // 
            // itemNameInput
            // 
            this.itemNameInput.Location = new System.Drawing.Point(59, 14);
            this.itemNameInput.Name = "itemNameInput";
            this.itemNameInput.Size = new System.Drawing.Size(100, 20);
            this.itemNameInput.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.itemNameInput);
            this.panel1.Controls.Add(this.createItemBtn);
            this.panel1.Location = new System.Drawing.Point(15, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(267, 63);
            this.panel1.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 162);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.loginBtn);
            this.Controls.Add(this.userNameInput);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Occasionally Connected Client";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox userNameInput;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Button createItemBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox itemNameInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
    }
}

