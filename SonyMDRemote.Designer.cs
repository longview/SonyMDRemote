namespace SonyMDRemote
{
    partial class SonyMDRemote
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
            this.richTextBox_Log = new System.Windows.Forms.RichTextBox();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.comboBox1_Serial_Port = new System.Windows.Forms.ComboBox();
            this.timer1_Maintenance = new System.Windows.Forms.Timer(this.components);
            this.button_Serial_Connect = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5_Prev = new System.Windows.Forms.Button();
            this.button6_Next = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3_playstatusindicator = new System.Windows.Forms.Label();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.timer_Poll_Time = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.label7_disctitle = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.trackno = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tracktitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.write = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button15 = new System.Windows.Forms.Button();
            this.checkBox1_Autopoll = new System.Windows.Forms.CheckBox();
            this.checkBox2_Elapsed = new System.Windows.Forms.CheckBox();
            this.timer_Poll_GetInfo = new System.Windows.Forms.Timer(this.components);
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.button16 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button17 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label12_timestamp = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.linkLabel_savetracks = new System.Windows.Forms.LinkLabel();
            this.linkLabel_loadtracks = new System.Windows.Forms.LinkLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.linkLabel_PowerOn = new System.Windows.Forms.LinkLabel();
            this.linkLabel_PowerOff = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox_Log
            // 
            this.richTextBox_Log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox_Log.Location = new System.Drawing.Point(12, 417);
            this.richTextBox_Log.Name = "richTextBox_Log";
            this.richTextBox_Log.Size = new System.Drawing.Size(780, 167);
            this.richTextBox_Log.TabIndex = 0;
            this.richTextBox_Log.Text = "";
            this.richTextBox_Log.TextChanged += new System.EventHandler(this.richTextBox_Log_TextChanged);
            // 
            // serialPort1
            // 
            this.serialPort1.ReadBufferSize = 8192;
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // comboBox1_Serial_Port
            // 
            this.comboBox1_Serial_Port.FormattingEnabled = true;
            this.comboBox1_Serial_Port.Location = new System.Drawing.Point(330, 3);
            this.comboBox1_Serial_Port.Name = "comboBox1_Serial_Port";
            this.comboBox1_Serial_Port.Size = new System.Drawing.Size(57, 21);
            this.comboBox1_Serial_Port.TabIndex = 1;
            // 
            // timer1_Maintenance
            // 
            this.timer1_Maintenance.Enabled = true;
            this.timer1_Maintenance.Interval = 10;
            this.timer1_Maintenance.Tick += new System.EventHandler(this.timer1_Maintenance_Tick);
            // 
            // button_Serial_Connect
            // 
            this.button_Serial_Connect.Location = new System.Drawing.Point(393, 3);
            this.button_Serial_Connect.Name = "button_Serial_Connect";
            this.button_Serial_Connect.Size = new System.Drawing.Size(75, 23);
            this.button_Serial_Connect.TabIndex = 3;
            this.button_Serial_Connect.Text = "Connect";
            this.button_Serial_Connect.UseVisualStyleBackColor = true;
            this.button_Serial_Connect.Click += new System.EventHandler(this.button_Serial_Connect_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(3, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Remote On";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(84, 32);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Remote Off";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(99, 32);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "Play/Pause";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(165, 32);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Get Model";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5_Prev
            // 
            this.button5_Prev.Location = new System.Drawing.Point(3, 32);
            this.button5_Prev.Name = "button5_Prev";
            this.button5_Prev.Size = new System.Drawing.Size(90, 23);
            this.button5_Prev.TabIndex = 8;
            this.button5_Prev.Text = "Previous";
            this.button5_Prev.UseVisualStyleBackColor = true;
            this.button5_Prev.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6_Next
            // 
            this.button6_Next.Location = new System.Drawing.Point(195, 32);
            this.button6_Next.Name = "button6_Next";
            this.button6_Next.Size = new System.Drawing.Size(90, 23);
            this.button6_Next.TabIndex = 9;
            this.button6_Next.Text = "Next";
            this.button6_Next.UseVisualStyleBackColor = true;
            this.button6_Next.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(99, 61);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(90, 23);
            this.button7.TabIndex = 10;
            this.button7.Text = "Stop";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.AutoSize = true;
            this.button8.Location = new System.Drawing.Point(99, 3);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(90, 23);
            this.button8.TabIndex = 11;
            this.button8.Text = "Play";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Track -/-";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3_playstatusindicator
            // 
            this.label3_playstatusindicator.AutoSize = true;
            this.label3_playstatusindicator.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3_playstatusindicator.Location = new System.Drawing.Point(6, 16);
            this.label3_playstatusindicator.Name = "label3_playstatusindicator";
            this.label3_playstatusindicator.Size = new System.Drawing.Size(50, 25);
            this.label3_playstatusindicator.TabIndex = 13;
            this.label3_playstatusindicator.Text = "N/A";
            this.label3_playstatusindicator.Click += new System.EventHandler(this.label3_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(3, 61);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(89, 23);
            this.button9.TabIndex = 14;
            this.button9.Text = "Eject";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(3, 3);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(67, 23);
            this.button10.TabIndex = 15;
            this.button10.Text = "Get Info";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button11.Location = new System.Drawing.Point(3, 3);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(74, 23);
            this.button11.TabIndex = 16;
            this.button11.Text = "Update";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Track n to track n";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(80, 3);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(46, 20);
            this.numericUpDown1.TabIndex = 19;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(132, 3);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(66, 20);
            this.button13.TabIndex = 20;
            this.button13.Text = "Play";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(207, 3);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(70, 20);
            this.button14.TabIndex = 21;
            this.button14.Text = "Queue";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.label5.Size = new System.Drawing.Size(66, 19);
            this.label5.TabIndex = 22;
            this.label5.Text = "Track select";
            // 
            // timer_Poll_Time
            // 
            this.timer_Poll_Time.Interval = 10;
            this.timer_Poll_Time.Tick += new System.EventHandler(this.timer_Poll_Time_Tick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(102, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 16);
            this.label6.TabIndex = 23;
            this.label6.Text = "00:00/00:00";
            // 
            // label7_disctitle
            // 
            this.label7_disctitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7_disctitle.Font = new System.Drawing.Font("Segoe UI Black", 9.5F, System.Drawing.FontStyle.Bold);
            this.label7_disctitle.Location = new System.Drawing.Point(6, 12);
            this.label7_disctitle.Name = "label7_disctitle";
            this.label7_disctitle.Size = new System.Drawing.Size(199, 35);
            this.label7_disctitle.TabIndex = 24;
            this.label7_disctitle.Text = "Disc\r\nName\r\n";
            this.label7_disctitle.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.label7_disctitle.TextChanged += new System.EventHandler(this.label7_disctitle_TextChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.Cursor = System.Windows.Forms.Cursors.Default;
            this.label8.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(6, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(292, 50);
            this.label8.TabIndex = 25;
            this.label8.Text = "Track Title\r\nTrack Title";
            this.label8.TextChanged += new System.EventHandler(this.label8_TextChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.trackno,
            this.tracktitle,
            this.write,
            this.length});
            this.dataGridView1.Location = new System.Drawing.Point(319, 42);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(473, 354);
            this.dataGridView1.TabIndex = 26;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentDoubleClick);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_RowEnter);
            this.dataGridView1.RowHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_RowHeaderMouseDoubleClick);
            this.dataGridView1.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_RowValidated);
            // 
            // trackno
            // 
            this.trackno.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.trackno.HeaderText = "Track";
            this.trackno.Name = "trackno";
            this.trackno.ReadOnly = true;
            this.trackno.Width = 60;
            // 
            // tracktitle
            // 
            this.tracktitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.tracktitle.HeaderText = "Name";
            this.tracktitle.Name = "tracktitle";
            // 
            // write
            // 
            this.write.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.write.HeaderText = "Write";
            this.write.Name = "write";
            this.write.Width = 38;
            // 
            // length
            // 
            this.length.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.length.HeaderText = "Len";
            this.length.Name = "length";
            this.length.ReadOnly = true;
            this.length.Width = 50;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(76, 3);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(46, 23);
            this.button15.TabIndex = 15;
            this.button15.Text = "Reset";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // checkBox1_Autopoll
            // 
            this.checkBox1_Autopoll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1_Autopoll.AutoSize = true;
            this.checkBox1_Autopoll.Location = new System.Drawing.Point(84, 9);
            this.checkBox1_Autopoll.Name = "checkBox1_Autopoll";
            this.checkBox1_Autopoll.Size = new System.Drawing.Size(65, 17);
            this.checkBox1_Autopoll.TabIndex = 27;
            this.checkBox1_Autopoll.Text = "AutoPoll";
            this.checkBox1_Autopoll.UseVisualStyleBackColor = true;
            this.checkBox1_Autopoll.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2_Elapsed
            // 
            this.checkBox2_Elapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox2_Elapsed.AutoSize = true;
            this.checkBox2_Elapsed.Location = new System.Drawing.Point(165, 9);
            this.checkBox2_Elapsed.Name = "checkBox2_Elapsed";
            this.checkBox2_Elapsed.Size = new System.Drawing.Size(95, 17);
            this.checkBox2_Elapsed.TabIndex = 28;
            this.checkBox2_Elapsed.Text = "Elapsed Count";
            this.checkBox2_Elapsed.UseVisualStyleBackColor = true;
            this.checkBox2_Elapsed.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // timer_Poll_GetInfo
            // 
            this.timer_Poll_GetInfo.Interval = 1500;
            this.timer_Poll_GetInfo.Tick += new System.EventHandler(this.timer_Poll_GetInfo_Tick);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(195, 3);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.checkBox3.Size = new System.Drawing.Size(81, 22);
            this.checkBox3.TabIndex = 30;
            this.checkBox3.Text = "Auto Pause";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(214, 3);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(75, 23);
            this.button16.TabIndex = 31;
            this.button16.Text = "Write";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(128, 0);
            this.label10.Name = "label10";
            this.label10.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.label10.Size = new System.Drawing.Size(59, 19);
            this.label10.TabIndex = 32;
            this.label10.Text = "TOC Clean";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(236, 135);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 13);
            this.label11.TabIndex = 33;
            this.label11.Text = "No Repeat";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Repeat Off",
            "All Repeat",
            "1Tr Repeat"});
            this.comboBox1.Location = new System.Drawing.Point(195, 61);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(90, 21);
            this.comboBox1.TabIndex = 34;
            // 
            // button17
            // 
            this.button17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button17.Location = new System.Drawing.Point(753, 417);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(39, 23);
            this.button17.TabIndex = 35;
            this.button17.Text = "Clear";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(9, 167);
            this.progressBar1.Maximum = 1000;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(286, 19);
            this.progressBar1.TabIndex = 36;
            // 
            // label12_timestamp
            // 
            this.label12_timestamp.AutoSize = true;
            this.label12_timestamp.Location = new System.Drawing.Point(7, 151);
            this.label12_timestamp.Name = "label12_timestamp";
            this.label12_timestamp.Size = new System.Drawing.Size(60, 13);
            this.label12_timestamp.TabIndex = 37;
            this.label12_timestamp.Text = "Recorded: ";
            this.label12_timestamp.Click += new System.EventHandler(this.label12_timestamp_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.label12_timestamp);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Controls.Add(this.label3_playstatusindicator);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Location = new System.Drawing.Point(12, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(301, 192);
            this.groupBox1.TabIndex = 38;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sony";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7_disctitle);
            this.groupBox3.Location = new System.Drawing.Point(90, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(211, 50);
            this.groupBox3.TabIndex = 42;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Disc";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(0, 42);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(301, 71);
            this.groupBox4.TabIndex = 42;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Track";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Controls.Add(this.tableLayoutPanel3);
            this.groupBox2.Location = new System.Drawing.Point(12, 208);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(301, 139);
            this.groupBox2.TabIndex = 39;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Controls";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.comboBox1, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.button5_Prev, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.button6_Next, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.button7, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.button8, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.button3, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.button9, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 18);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(289, 87);
            this.tableLayoutPanel1.TabIndex = 40;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.98962F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.3391F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.29758F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.06574F));
            this.tableLayoutPanel3.Controls.Add(this.button14, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.button13, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.numericUpDown1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 109);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(289, 26);
            this.tableLayoutPanel3.TabIndex = 41;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.button11, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBox1_Autopoll, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBox2_Elapsed, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.button1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.button2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.button4, 2, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(18, 353);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(289, 58);
            this.tableLayoutPanel2.TabIndex = 40;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 6;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 83F));
            this.tableLayoutPanel4.Controls.Add(this.button10, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.button15, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label10, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.button16, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.comboBox1_Serial_Port, 4, 0);
            this.tableLayoutPanel4.Controls.Add(this.button_Serial_Connect, 5, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(319, 12);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(473, 34);
            this.tableLayoutPanel4.TabIndex = 41;
            // 
            // linkLabel_savetracks
            // 
            this.linkLabel_savetracks.AutoSize = true;
            this.linkLabel_savetracks.Location = new System.Drawing.Point(728, 399);
            this.linkLabel_savetracks.Name = "linkLabel_savetracks";
            this.linkLabel_savetracks.Size = new System.Drawing.Size(64, 13);
            this.linkLabel_savetracks.TabIndex = 42;
            this.linkLabel_savetracks.TabStop = true;
            this.linkLabel_savetracks.Text = "Save tracks";
            this.linkLabel_savetracks.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_savetracks_LinkClicked);
            // 
            // linkLabel_loadtracks
            // 
            this.linkLabel_loadtracks.AutoSize = true;
            this.linkLabel_loadtracks.Location = new System.Drawing.Point(658, 399);
            this.linkLabel_loadtracks.Name = "linkLabel_loadtracks";
            this.linkLabel_loadtracks.Size = new System.Drawing.Size(63, 13);
            this.linkLabel_loadtracks.TabIndex = 43;
            this.linkLabel_loadtracks.TabStop = true;
            this.linkLabel_loadtracks.Text = "Load tracks";
            this.linkLabel_loadtracks.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_loadtracks_LinkClicked);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "txt";
            // 
            // linkLabel_PowerOn
            // 
            this.linkLabel_PowerOn.AutoSize = true;
            this.linkLabel_PowerOn.Location = new System.Drawing.Point(316, 399);
            this.linkLabel_PowerOn.Name = "linkLabel_PowerOn";
            this.linkLabel_PowerOn.Size = new System.Drawing.Size(54, 13);
            this.linkLabel_PowerOn.TabIndex = 44;
            this.linkLabel_PowerOn.TabStop = true;
            this.linkLabel_PowerOn.Text = "Power On";
            this.linkLabel_PowerOn.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel_PowerOff
            // 
            this.linkLabel_PowerOff.AutoSize = true;
            this.linkLabel_PowerOff.Location = new System.Drawing.Point(376, 399);
            this.linkLabel_PowerOff.Name = "linkLabel_PowerOff";
            this.linkLabel_PowerOff.Size = new System.Drawing.Size(54, 13);
            this.linkLabel_PowerOff.TabIndex = 45;
            this.linkLabel_PowerOff.TabStop = true;
            this.linkLabel_PowerOff.Text = "Power Off";
            this.linkLabel_PowerOff.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // SonyMDRemote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 596);
            this.Controls.Add(this.linkLabel_PowerOff);
            this.Controls.Add(this.linkLabel_PowerOn);
            this.Controls.Add(this.linkLabel_loadtracks);
            this.Controls.Add(this.linkLabel_savetracks);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button17);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.richTextBox_Log);
            this.Controls.Add(this.tableLayoutPanel4);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(820, 635);
            this.Name = "SonyMDRemote";
            this.Text = "LA2YUA SonyMDRemote";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SonyMDRemote_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_Log;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox comboBox1_Serial_Port;
        private System.Windows.Forms.Timer timer1_Maintenance;
        private System.Windows.Forms.Button button_Serial_Connect;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5_Prev;
        private System.Windows.Forms.Button button6_Next;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3_playstatusindicator;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer_Poll_Time;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7_disctitle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.CheckBox checkBox1_Autopoll;
        private System.Windows.Forms.CheckBox checkBox2_Elapsed;
        private System.Windows.Forms.Timer timer_Poll_GetInfo;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackno;
        private System.Windows.Forms.DataGridViewTextBoxColumn tracktitle;
        private System.Windows.Forms.DataGridViewCheckBoxColumn write;
        private System.Windows.Forms.DataGridViewTextBoxColumn length;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label12_timestamp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.LinkLabel linkLabel_savetracks;
        private System.Windows.Forms.LinkLabel linkLabel_loadtracks;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.LinkLabel linkLabel_PowerOn;
        private System.Windows.Forms.LinkLabel linkLabel_PowerOff;
    }
}

