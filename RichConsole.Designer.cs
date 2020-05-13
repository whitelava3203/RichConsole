using System.Windows.Forms;

namespace ConsoleApp
{
    partial class ConsoleManager
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConsoleInput = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // ConsoleInput
            // 
            this.ConsoleInput.BackColor = System.Drawing.SystemColors.WindowText;
            this.ConsoleInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ConsoleInput.ForeColor = System.Drawing.Color.White;
            this.ConsoleInput.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConsoleInput.Location = new System.Drawing.Point(12, 587);
            this.ConsoleInput.Name = "ConsoleInput";
            this.ConsoleInput.Size = new System.Drawing.Size(1081, 18);
            this.ConsoleInput.TabIndex = 0;
            this.ConsoleInput.Text = "input area";
            this.ConsoleInput.TextChanged += new System.EventHandler(this.ConsoleInput_TextChanged);
            this.ConsoleInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConsoleInput_KeyPress);
            // 
            // RichConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.ClientSize = new System.Drawing.Size(1105, 617);
            this.Controls.Add(this.ConsoleInput);
            this.ForeColor = System.Drawing.Color.DarkCyan;
            this.Name = "RichConsole";
            this.Load += new System.EventHandler(this.Form_Load);
            this.Shown += new System.EventHandler(this.RichConsole_Shown);
            this.FormClosed += new FormClosedEventHandler(this.Form_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ConsoleInput;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}

