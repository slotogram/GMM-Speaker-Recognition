using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;



namespace SR_GMM
{
    public partial class RecordForm : Form
    {
        WaveIn waveIn;
        WaveFileWriter writer;
        string outputFilename;

        public RecordForm()
        {
            InitializeComponent();
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Записываем данные из буфера в файл
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
            }
        }

        void waveIn_RecordingStopped(object sender, EventArgs e)
        {
           if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }

        private void getMCC(string path)
        {
            
            string shortArgs = "-e ";

            shortArgs += "-p ";
            shortArgs += 12 + " ";
            
            shortArgs += "-Z -R ";

            
            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.CurrentDirectory + "\\sfbcep.exe",
                WindowStyle = ProcessWindowStyle.Hidden

            };

            startInfo.Arguments = shortArgs + path + ".wav " + path+".mcc";
                Process.Start(startInfo).WaitForExit();
               
        }
        private SR_GMM.GMM buildModel()
        {
            Data learnData = new Data(textBox1.Text + ".mcc");

            int gmmN = 128;
            GMM g = new GMM(gmmN, learnData.dimension, learnData);
            g.Train(learnData, "asdas", textBox1.Text + ".gmm", gmmN, 0.95, 0.01, 100, 1);
            return g;
        }

        private void changelabel(int i, int time)
        {
            label3.Text = "Время: " + (i + 1) + "/" + time;
        }

        public delegate void delv(int i, int time);
        public delv myDelegate;
        private void Waiter(int time)
        {
           
            for (int i = 0; i < time; i++)
            {
                
                
                myDelegate = changelabel;
                int[] tmp = new int[2];
                tmp[0] = i;
                tmp[1] = time;
                Invoke(myDelegate,i,time);
                
                Thread.Sleep(1000);
            }
            waveIn.StopRecording();
        }

        private void Record(int time, string path)
        {
            waveIn = new WaveIn();
            waveIn.DeviceNumber = 0;
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(waveIn_RecordingStopped);
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            outputFilename = path + ".wav";
            writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
            waveIn.StartRecording();
            //Thread.Sleep(time*1000);
            //таймер на 60 секунд.
            Thread thread = new Thread(delegate()
            {
                Waiter(time);
            });
            thread.Start();
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int t = 0;
            int.TryParse(textBox2.Text, out t);
            bool rec = false;
            if (!System.IO.File.Exists(textBox1.Text + ".wav"))
            {
                Record(t, textBox1.Text);
                rec = true;
            }

            //замеряем время вычислений
           




            Thread thread = new Thread(delegate()
            {
                if (rec)  Waiter(t);
                var start = DateTime.Now;
                var stopwatch = Stopwatch.StartNew();
                // Do something
                if (!System.IO.File.Exists(textBox1.Text + ".mcc"))
                getMCC(textBox1.Text);
                GMM g = buildModel();

                
                MessageBox.Show("Обучение прошло за: "+ stopwatch.ElapsedMilliseconds.ToString()+ " мс");

            });
            thread.Start();

            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            bool rec = false;
            if (!System.IO.File.Exists(textBox4.Text + ".wav"))
            {
                Record(10, textBox4.Text);
                rec = true;
            }
            float res = 0;
            Thread thread = new Thread(delegate()
            {
                if (rec) Waiter(10);
                var stopwatch = Stopwatch.StartNew();
                if (!System.IO.File.Exists(textBox4.Text + ".mcc"))
                getMCC(textBox4.Text);
                GMM g = new GMM(textBox1.Text + ".gmm");
                res = g.Classify(textBox4.Text + ".mcc");
                MessageBox.Show("Проверка прошла за: " + stopwatch.ElapsedMilliseconds.ToString() + " мс");
                if (res < -10.9) { MessageBox.Show("Говорит не " + textBox1.Text); }
                else MessageBox.Show("Говорит "+textBox1.Text);
                System.IO.StreamWriter sw = new StreamWriter("log.txt", true);

                sw.WriteLine();
                sw.WriteLine(textBox4.Text+" "+ res.ToString());
                sw.Close();

            });
            thread.Start();

            //label2.Text = "Результат: " + res;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int t = 0;
            int.TryParse(textBox2.Text, out t);
            bool rec = false;
            if (!System.IO.File.Exists(textBox1.Text + ".wav"))
            {
                Record(t, textBox1.Text);
                rec = true;
            }
            
            //замеряем время вычислений

            Thread thread = new Thread(delegate()
            {
                if (rec)
                Waiter(t);
                var start = DateTime.Now;
                var stopwatch = Stopwatch.StartNew();
                // Do something
                if (!System.IO.File.Exists(textBox1.Text + ".mcc"))
                    getMCC(textBox1.Text);
                
                //load world model
                GMM ubm = new GMM("ubm.gmm");
                Data learnData = new Data(textBox1.Text + ".mcc");
                GMM spkr = new GMM(ubm.getNumMix(), learnData.dimension, learnData);
                spkr.Adapt(learnData, ubm, "asdas", textBox1.Text +  ".gmm", ubm.getNumMix(), 14, 0.95, 0.01, 1, 1);


                //посчитаем порог для данной записи
                float score = spkr.Classify(learnData, 1, null, ubm);
                textBoxLog.Invoke(
                (ThreadStart)delegate()
                {
                    textBoxLog.Text += "Диктор " + textBox1.Text + " = " + score.ToString() + Environment.NewLine;
                });
                

                if (System.IO.Directory.Exists("impostors"))
                {
                    List<string> impList = new List<string>();
                    impList = System.IO.Directory.GetFiles("impostors","*.mcc").ToList<string>();
                    float min = float.MaxValue, max = float.MinValue, avg = 0;
                    foreach (string s in impList)
                    {
                        Data impData = new Data(s);
                        //GMM imp = new GMM(ubm.getNumMix(), impData.dimension, impData);
                        //imp.Adapt(impData, ubm, "asdas", textBox1.Text + ".gmm", ubm.getNumMix(), 14, 0.95, 0.01, 1, 1);
                        float impScore = spkr.Classify(impData, 1, null, ubm);
                        avg += impScore;
                        if (impScore < min) min = impScore;
                        if (impScore > max) max = impScore;
                    }
                    avg /= impList.Count;
                    textBoxLog.Invoke(
                    (ThreadStart)delegate()
                    {
                        textBoxLog.Text += "Средний нарушитель " + " = " + avg.ToString() + Environment.NewLine;
                        textBoxLog.Text += "Min нарушитель " + " = " + min.ToString() + Environment.NewLine;
                        textBoxLog.Text += "Max нарушитель " + " = " + max.ToString() + Environment.NewLine;
                    });
                }
                MessageBox.Show("Обучение прошло за: " + stopwatch.ElapsedMilliseconds.ToString() + " мс");

            });
            thread.Start();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool rec = false;
            if (!System.IO.File.Exists(textBox4.Text + ".wav"))
            {
                Record(10, textBox4.Text);
                rec = true;
            }
            float res = 0;
            Thread thread = new Thread(delegate()
            {
                if (rec)
                Waiter(10);
                var stopwatch = Stopwatch.StartNew();
                if (!System.IO.File.Exists(textBox4.Text + ".mcc"))
                    getMCC(textBox4.Text);

                
                //load world model
                GMM ubm = new GMM("ubm.gmm");
                Data learnData = new Data(textBox4.Text + ".mcc");
                GMM spkr = new GMM(textBox1.Text + ".gmm");
                


                //посчитаем порог для данной записи
                float score = spkr.Classify(learnData, 1, null, ubm);
                textBoxLog.Invoke(
               (ThreadStart)delegate()
               {
                   textBoxLog.Text += "Проверка диктора " + textBox4.Text + " = " + score.ToString() + Environment.NewLine;
               });
                float thr = 0;
                float.TryParse(textBox3.Text, out thr);

                res = score;
                MessageBox.Show("Проверка прошла за: " + stopwatch.ElapsedMilliseconds.ToString() + " мс");
                if (res < thr) { MessageBox.Show("Говорит не " + textBox1.Text); }
                else MessageBox.Show("Говорит " + textBox1.Text);
                System.IO.StreamWriter sw = new StreamWriter("log.txt", true);

                sw.WriteLine();
                sw.WriteLine(textBox4.Text + " " + res.ToString());
                sw.Close();

            });
            thread.Start();

        }
    }
}
