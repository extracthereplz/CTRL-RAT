
namespace CTRL_rat_Server
{
    partial class Main
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
            this.Clients = new System.Windows.Forms.ListView();
            this.HWID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Username = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Windows_Version = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Admin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Clients
            // 
            this.Clients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.HWID,
            this.IP,
            this.Username,
            this.Windows_Version,
            this.Admin});
            this.Clients.FullRowSelect = true;
            this.Clients.GridLines = true;
            this.Clients.HideSelection = false;
            this.Clients.Location = new System.Drawing.Point(-1, 34);
            this.Clients.Name = "Clients";
            this.Clients.Size = new System.Drawing.Size(799, 416);
            this.Clients.TabIndex = 0;
            this.Clients.UseCompatibleStateImageBehavior = false;
            this.Clients.View = System.Windows.Forms.View.Details;
            this.Clients.SelectedIndexChanged += new System.EventHandler(this.Clients_SelectedIndexChanged);
            this.Clients.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Clients_MouseClick);
            // 
            // HWID
            // 
            this.HWID.Text = "HWID";
            this.HWID.Width = 156;
            // 
            // IP
            // 
            this.IP.Text = "IP";
            this.IP.Width = 119;
            // 
            // Username
            // 
            this.Username.Text = "Username";
            this.Username.Width = 178;
            // 
            // Windows_Version
            // 
            this.Windows_Version.Text = "Windows Version";
            this.Windows_Version.Width = 220;
            // 
            // Admin
            // 
            this.Admin.Text = "Admin";
            this.Admin.Width = 114;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Settings";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStrip1.Text = "Options";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(103, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Builder";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 450);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Clients);
            this.Name = "Main";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView Clients;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader HWID;
        private System.Windows.Forms.ColumnHeader Username;
        private System.Windows.Forms.ColumnHeader IP;
        private System.Windows.Forms.ColumnHeader Admin;
        private System.Windows.Forms.ColumnHeader Windows_Version;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Button button2;
    }
}

