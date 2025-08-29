namespace ThirdYearLP_IP_Solver
{
    partial class MainForm
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.defaultBtn = new System.Windows.Forms.Button();
            this.clearBtn = new System.Windows.Forms.Button();
            this.resultsGridView = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.goBtn = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbxAlgorithmn = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblSuggestAlgorithmn = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGridView)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(359, 208);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(206, 240);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(78, 23);
            this.okBtn.TabIndex = 5;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // defaultBtn
            // 
            this.defaultBtn.Location = new System.Drawing.Point(12, 240);
            this.defaultBtn.Name = "defaultBtn";
            this.defaultBtn.Size = new System.Drawing.Size(75, 23);
            this.defaultBtn.TabIndex = 12;
            this.defaultBtn.Text = "Load Example";
            this.defaultBtn.UseVisualStyleBackColor = true;
            this.defaultBtn.Click += new System.EventHandler(this.defaultBtn_Click);
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(105, 240);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(75, 23);
            this.clearBtn.TabIndex = 13;
            this.clearBtn.Text = "Clear";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // resultsGridView
            // 
            this.resultsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultsGridView.Dock = System.Windows.Forms.DockStyle.Right;
            this.resultsGridView.Location = new System.Drawing.Point(459, 0);
            this.resultsGridView.Name = "resultsGridView";
            this.resultsGridView.Size = new System.Drawing.Size(746, 594);
            this.resultsGridView.TabIndex = 15;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblSuggestAlgorithmn);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmbxAlgorithmn);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.goBtn);
            this.panel1.Location = new System.Drawing.Point(12, 289);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(359, 145);
            this.panel1.TabIndex = 16;
            // 
            // goBtn
            // 
            this.goBtn.Location = new System.Drawing.Point(121, 110);
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(75, 23);
            this.goBtn.TabIndex = 12;
            this.goBtn.Text = "Solve";
            this.goBtn.UseVisualStyleBackColor = true;
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(296, 240);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 17;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(127, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Select Algorithm";
            // 
            // cmbxAlgorithmn
            // 
            this.cmbxAlgorithmn.FormattingEnabled = true;
            this.cmbxAlgorithmn.Items.AddRange(new object[] {
            "Primal Simplex",
            "Dual Simplex",
            "Branch&Bound",
            "Branch&Bound (Knapsack)",
            "Cutting Plane"});
            this.cmbxAlgorithmn.Location = new System.Drawing.Point(18, 74);
            this.cmbxAlgorithmn.Name = "cmbxAlgorithmn";
            this.cmbxAlgorithmn.Size = new System.Drawing.Size(317, 21);
            this.cmbxAlgorithmn.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Suggested algorithm:";
            // 
            // lblSuggestAlgorithmn
            // 
            this.lblSuggestAlgorithmn.AutoSize = true;
            this.lblSuggestAlgorithmn.Location = new System.Drawing.Point(229, 46);
            this.lblSuggestAlgorithmn.Name = "lblSuggestAlgorithmn";
            this.lblSuggestAlgorithmn.Size = new System.Drawing.Size(106, 13);
            this.lblSuggestAlgorithmn.TabIndex = 16;
            this.lblSuggestAlgorithmn.Text = "Suggested algorithm:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1205, 594);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.resultsGridView);
            this.Controls.Add(this.clearBtn);
            this.Controls.Add(this.defaultBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.richTextBox1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.resultsGridView)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button defaultBtn;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.DataGridView resultsGridView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbxAlgorithmn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblSuggestAlgorithmn;
    }
}

