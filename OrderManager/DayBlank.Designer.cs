namespace OrderManager
{
    partial class DayBlank
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dayNumber = new System.Windows.Forms.Label();
            this.shiftNumber = new System.Windows.Forms.Label();
            this.hourCurr = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.Controls.Add(this.dayNumber, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.shiftNumber, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.hourCurr, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(58, 29);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // dayNumber
            // 
            this.dayNumber.BackColor = System.Drawing.Color.Transparent;
            this.dayNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dayNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dayNumber.Location = new System.Drawing.Point(3, 0);
            this.dayNumber.Name = "dayNumber";
            this.dayNumber.Size = new System.Drawing.Size(25, 17);
            this.dayNumber.TabIndex = 1;
            this.dayNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // shiftNumber
            // 
            this.shiftNumber.AutoSize = true;
            this.shiftNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shiftNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.shiftNumber.Location = new System.Drawing.Point(34, 0);
            this.shiftNumber.Name = "shiftNumber";
            this.shiftNumber.Size = new System.Drawing.Size(21, 17);
            this.shiftNumber.TabIndex = 2;
            this.shiftNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // hourCurr
            // 
            this.hourCurr.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.hourCurr, 2);
            this.hourCurr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hourCurr.Location = new System.Drawing.Point(3, 17);
            this.hourCurr.Name = "hourCurr";
            this.hourCurr.Size = new System.Drawing.Size(52, 12);
            this.hourCurr.TabIndex = 3;
            this.hourCurr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DayBlank
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DayBlank";
            this.Size = new System.Drawing.Size(58, 29);
            this.Load += new System.EventHandler(this.DayBlank_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Label dayNumber;
        private System.Windows.Forms.Label shiftNumber;
        private System.Windows.Forms.Label hourCurr;
    }
}
