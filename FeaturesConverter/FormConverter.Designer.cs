namespace FeaturesConverter
{
    partial class FormConverter
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
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.button13 = new System.Windows.Forms.Button();
            this.textBox21 = new System.Windows.Forms.TextBox();
            this.button12 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textBox28 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBoxSpeakerSet = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.textBoxUBMPhrases = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.textBoxTestPhrases = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.textBoxLearnPhrases = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBoxFeatureExtension = new System.Windows.Forms.TextBox();
            this.textBoxLists = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxUBMSpeakersNum = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSpeakerTest = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Checked = true;
            this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox6.Location = new System.Drawing.Point(295, 27);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(105, 17);
            this.checkBox6.TabIndex = 47;
            this.checkBox6.Text = "Поддиректории";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.Visible = false;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(12, 309);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(105, 22);
            this.button13.TabIndex = 46;
            this.button13.Text = "Создать списки";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // textBox21
            // 
            this.textBox21.Location = new System.Drawing.Point(12, 23);
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new System.Drawing.Size(141, 20);
            this.textBox21.TabIndex = 45;
            this.textBox21.Text = "E:\\temp\\123\\12samplesN";
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(159, 23);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(37, 23);
            this.button12.TabIndex = 44;
            this.button12.Text = "Dir";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // textBox28
            // 
            this.textBox28.Location = new System.Drawing.Point(10, 83);
            this.textBox28.Name = "textBox28";
            this.textBox28.Size = new System.Drawing.Size(61, 20);
            this.textBox28.TabIndex = 64;
            this.textBox28.Text = "0-12";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(79, 85);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(187, 17);
            this.checkBox1.TabIndex = 66;
            this.checkBox1.Text = "Используемые характеристики";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(76, 243);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(141, 13);
            this.label29.TabIndex = 99;
            this.label29.Text = "№ Дикторов для обучения";
            // 
            // textBoxSpeakerSet
            // 
            this.textBoxSpeakerSet.Location = new System.Drawing.Point(10, 243);
            this.textBoxSpeakerSet.Name = "textBoxSpeakerSet";
            this.textBoxSpeakerSet.Size = new System.Drawing.Size(47, 20);
            this.textBoxSpeakerSet.TabIndex = 98;
            this.textBoxSpeakerSet.Text = "16-25";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(76, 195);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(107, 13);
            this.label28.TabIndex = 97;
            this.label28.Text = "№ файлов для UBM";
            // 
            // textBoxUBMPhrases
            // 
            this.textBoxUBMPhrases.Location = new System.Drawing.Point(10, 195);
            this.textBoxUBMPhrases.Name = "textBoxUBMPhrases";
            this.textBoxUBMPhrases.Size = new System.Drawing.Size(47, 20);
            this.textBoxUBMPhrases.TabIndex = 96;
            this.textBoxUBMPhrases.Text = "1-50";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(76, 169);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(153, 13);
            this.label27.TabIndex = 95;
            this.label27.Text = "№ файлов для тестирования";
            // 
            // textBoxTestPhrases
            // 
            this.textBoxTestPhrases.Location = new System.Drawing.Point(10, 169);
            this.textBoxTestPhrases.Name = "textBoxTestPhrases";
            this.textBoxTestPhrases.Size = new System.Drawing.Size(47, 20);
            this.textBoxTestPhrases.TabIndex = 94;
            this.textBoxTestPhrases.Text = "41-50";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(76, 143);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(129, 13);
            this.label26.TabIndex = 93;
            this.label26.Text = "№ файлов для обучения";
            // 
            // textBoxLearnPhrases
            // 
            this.textBoxLearnPhrases.Location = new System.Drawing.Point(10, 143);
            this.textBoxLearnPhrases.Name = "textBoxLearnPhrases";
            this.textBoxLearnPhrases.Size = new System.Drawing.Size(47, 20);
            this.textBoxLearnPhrases.TabIndex = 92;
            this.textBoxLearnPhrases.Text = "1-40";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(76, 118);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(92, 13);
            this.label16.TabIndex = 91;
            this.label16.Text = "Расширение фич";
            // 
            // textBoxFeatureExtension
            // 
            this.textBoxFeatureExtension.Location = new System.Drawing.Point(10, 115);
            this.textBoxFeatureExtension.Name = "textBoxFeatureExtension";
            this.textBoxFeatureExtension.Size = new System.Drawing.Size(47, 20);
            this.textBoxFeatureExtension.TabIndex = 90;
            this.textBoxFeatureExtension.Text = ".htk";
            // 
            // textBoxLists
            // 
            this.textBoxLists.Location = new System.Drawing.Point(12, 52);
            this.textBoxLists.Name = "textBoxLists";
            this.textBoxLists.Size = new System.Drawing.Size(141, 20);
            this.textBoxLists.TabIndex = 101;
            this.textBoxLists.Text = "E:\\temp\\123\\Smile\\Lists";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(159, 52);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(37, 23);
            this.button1.TabIndex = 100;
            this.button1.Text = "Dir";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(76, 217);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 103;
            this.label1.Text = "№ Дикторов для UBM";
            // 
            // textBoxUBMSpeakersNum
            // 
            this.textBoxUBMSpeakersNum.Location = new System.Drawing.Point(10, 217);
            this.textBoxUBMSpeakersNum.Name = "textBoxUBMSpeakersNum";
            this.textBoxUBMSpeakersNum.Size = new System.Drawing.Size(47, 20);
            this.textBoxUBMSpeakersNum.TabIndex = 102;
            this.textBoxUBMSpeakersNum.Text = "1-15";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(76, 267);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 13);
            this.label2.TabIndex = 105;
            this.label2.Text = "№ Дикторов для тестирования";
            // 
            // textBoxSpeakerTest
            // 
            this.textBoxSpeakerTest.Location = new System.Drawing.Point(10, 267);
            this.textBoxSpeakerTest.Name = "textBoxSpeakerTest";
            this.textBoxSpeakerTest.Size = new System.Drawing.Size(47, 20);
            this.textBoxSpeakerTest.TabIndex = 104;
            this.textBoxSpeakerTest.Text = "16-25";
            // 
            // FormConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 354);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxSpeakerTest);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxUBMSpeakersNum);
            this.Controls.Add(this.textBoxLists);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.textBoxSpeakerSet);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.textBoxUBMPhrases);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.textBoxTestPhrases);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.textBoxLearnPhrases);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.textBoxFeatureExtension);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.textBox28);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.textBox21);
            this.Controls.Add(this.button12);
            this.Name = "FormConverter";
            this.Text = "Features converter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.TextBox textBox21;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox textBox28;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBoxSpeakerSet;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox textBoxUBMPhrases;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox textBoxTestPhrases;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox textBoxLearnPhrases;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBoxFeatureExtension;
        private System.Windows.Forms.TextBox textBoxLists;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxUBMSpeakersNum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSpeakerTest;
    }
}

