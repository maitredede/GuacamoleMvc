namespace Guacamole.Client
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.pan = new System.Windows.Forms.Panel();
            this.btnHandShake = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pan
            // 
            this.pan.BackColor = System.Drawing.Color.Black;
            this.pan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pan.Location = new System.Drawing.Point(12, 12);
            this.pan.Name = "pan";
            this.pan.Size = new System.Drawing.Size(642, 482);
            this.pan.TabIndex = 0;
            // 
            // btnHandShake
            // 
            this.btnHandShake.Location = new System.Drawing.Point(660, 12);
            this.btnHandShake.Name = "btnHandShake";
            this.btnHandShake.Size = new System.Drawing.Size(75, 23);
            this.btnHandShake.TabIndex = 1;
            this.btnHandShake.Text = "Handshake";
            this.btnHandShake.UseVisualStyleBackColor = true;
            this.btnHandShake.Click += new System.EventHandler(this.btnHandShake_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 595);
            this.Controls.Add(this.btnHandShake);
            this.Controls.Add(this.pan);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pan;
        private System.Windows.Forms.Button btnHandShake;
    }
}

