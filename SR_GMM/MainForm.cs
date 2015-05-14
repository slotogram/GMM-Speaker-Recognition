using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SR_GMM
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        //Test method for creating and reading MFC for test audio file
        private void button1_Click(object sender, EventArgs e)
        {
            string dir = System.Environment.CurrentDirectory;
            var startInfo = new ProcessStartInfo
            {
                FileName =  dir + "\\sfbcep.exe",
                Arguments = "-e -D "+dir + "\\12.wav out.txt",
                WindowStyle = ProcessWindowStyle.Hidden

            };
            Process.Start(startInfo);

            Data mcc = new Data(dir + "\\out.txt");
            GMM g = new GMM(24, mcc.dimension, mcc);
            g.Train(mcc, "asdas", "model.txt", 24, 0.95, 0.01, 100, 1);

            Data mcc1 = new Data(dir + "\\out.txt");
            this.Text = g.Classify((dir + "\\out.txt")).ToString();

            //MCC tmp = new MCC(dir + "\\out.txt");
                        

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            
        }

        //Подумать насчет нормализации и удаления статической составляющей
        private void button3_Click(object sender, EventArgs e)
        {
            int n = 0;
            string[] s;
            if (checkBox4.Checked) 
                s = System.IO.Directory.GetFiles(textBox1.Text, "*.wav", SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(textBox1.Text, "*.wav");

                       
            int.TryParse( textBox2.Text, out n);
            if (n > 0 && n < 24)
            {
                string dir = textBox1.Text + "\\" + textBox2.Text + "samples";
                string shortArgs = "-e ";

                shortArgs += "-p ";
                shortArgs += n + " ";
                if (radioButton2.Checked) { shortArgs += "-D "; dir += "1d"; }
                if (radioButton3.Checked) { shortArgs += "-D -A "; dir += "2d"; }
                if (checkBox2.Checked) { shortArgs += "-Z -R "; dir += "N"; }

                if (checkBox5.Checked) Directory.CreateDirectory(dir);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\sfbcep.exe",
                    WindowStyle = ProcessWindowStyle.Hidden

                };
                foreach (string s1 in s)
                {
                    if (checkBox5.Checked)
                    startInfo.Arguments =      shortArgs + "\"" + s1 + "\" \"" + dir + "\\" + Path.GetFileNameWithoutExtension(s1) + ".mcc\"";
                    else startInfo.Arguments = shortArgs + "\"" + s1 + "\" \"" + Path.GetDirectoryName(s1) + "\\" + Path.GetFileNameWithoutExtension(s1) + ".mcc\"";
                    Process.Start(startInfo).WaitForExit();
                    //порог делаем

                }

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int n = 0;

            int gmmN = 0;
            int.TryParse(textBox6.Text, out gmmN);

            
            bool oneSpeaker = false;
            if (checkBox1.Checked)
            {
                int.TryParse(textBox4.Text, out n);
                oneSpeaker = true;
            }
            //Парсим текстбокс с данными о файлах
            string[] numbers = textBox5.Text.Replace(',', ' ').Split(' ');
            List<int> inum = new List<int>();
            foreach (string s1 in numbers)
            {
                if (!s1.Contains(' '))
                {
                    if (s1.Contains('-'))
                    {
                        int i1 = 0, i2 = 0;
                        int.TryParse(s1.Split('-')[0], out i1);
                        int.TryParse(s1.Split('-')[1], out i2);
                        if (i1 != 0 && i2 != 0)
                            for (int i = i1; i <= i2; i++) inum.Add(i);
                    }
                    else
                    {
                        int i = 0;
                        int.TryParse(s1, out i);
                        if (i != 0) inum.Add(i);
                    }
                }
            }
            //Дальше создать единый Data для нескольких файлов
            // и по очереди на каждого диктора создать модель
            string[] s;
            if (oneSpeaker)
            {
                s = System.IO.Directory.GetFiles(textBox3.Text, n.ToString() + " *.mcc");
                List<string> lst = new List<string>();
                foreach (string s1 in s)
                {
                    int j = 0;
                    int.TryParse(s1.Split('(')[1].Split(')')[0], out j);

                    if (inum.Contains(j)) lst.Add(s1);

                }
                //AddData and run GMM creator
                Data mcc = new Data(lst);
                GMM g = new GMM(gmmN, mcc.dimension, mcc);
                g.Train(mcc, "asdas", textBox3.Text + "\\" +n.ToString() + ".gmm", gmmN, 0.95, 0.01, 100, 1);
            }
            else
            {
                List<int> speakerList = new List<int>();


                //Пока что нам надо только 50 дикторов
                for (int i = 1; i <= 50; i++) speakerList.Add(i);
                //for (int i = 157; i < 160; i++) speakerList.Add(i);

                for (int i = 0; i < speakerList.Count; i++)
                {
                    s = System.IO.Directory.GetFiles(textBox3.Text, speakerList[i].ToString() + " *");
                    List<string> lst = new List<string>();
                    foreach (string s1 in s)
                    {
                        int j = 0;
                        int.TryParse(s1.Split('(')[1].Split(')')[0], out j);

                        if (inum.Contains(j)) lst.Add(s1);
                    }
                    //AddData and run GMM creator
                    Data mcc = new Data(lst);
                    GMM g = new GMM(gmmN, mcc.dimension, mcc);
                    g.Train(mcc, "asdas", textBox3.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 0.95, 0.01, 100, 1);
                }
                
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox3.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox9.Text = folderBrowserDialog1.SelectedPath;
        
        }
        private void sortGmmList(ref string[] s)
        {
            int[] val = new int[s.Length]; string buf; int ibuf; int min;
            for (int i=0; i< s.Length; i++)
            {
                
                int.TryParse(Path.GetFileNameWithoutExtension(s[i]), out val[i]);

            }


            for (int i = 0; i < s.Length; i++)
            {
                min = i;
                for (int j = i+1; j < s.Length; j++)
                {
                    if (val[j]<val[min]) min = j; 
                }
                if (i != min)
                {
                    ibuf = val[i];
                    val[i] = val[min];
                    val[min] = ibuf;

                    buf = s[i];
                    s[i] = s[min];
                    s[min] = buf;
                }
            }
                
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            //Парсим текстбокс с данными о файлах
            string[] numbers = textBox7.Text.Replace(',', ' ').Split(' ');
            List<int> inum = new List<int>();
            foreach (string s1 in numbers)
            {
                if (!s1.Contains(' '))
                {
                    if (s1.Contains('-'))
                    {
                        int i1 = 0, i2 = 0;
                        int.TryParse(s1.Split('-')[0], out i1);
                        int.TryParse(s1.Split('-')[1], out i2);
                        if (i1 != 0 && i2 != 0)
                            for (int i = i1; i <= i2; i++) inum.Add(i);
                    }
                    else
                    {
                        int i = 0;
                        int.TryParse(s1, out i);
                        if (i != 0) inum.Add(i);
                    }
                }
            }

            //Загружаем модели и проводим классификацию

            string[] s = System.IO.Directory.GetFiles(textBox9.Text, "*.gmm");
            //sort

            sortGmmList(ref s);

            List<GMM> gmmList = new List<GMM>();
            foreach (string s1 in s)
            {
                gmmList.Add(new GMM(s1));
            }

            //Берем список файлов, которые нужно классифицировать

            List<string> testList = new List<string>();

            for (int j = 1; j <= s.Length; j++)
            {
                foreach (int i in inum)
                {

                    //testList=testList.Concat(System.IO.Directory.GetFiles(textBox3.Text, inum[i].ToString() + " *").ToList()).ToList();
                    //testList.AddRange(System.IO.Directory.GetFiles(textBox9.Text,j.ToString+ " (" + i.ToString() + ")*"));
                    testList.Add(textBox9.Text + "\\" + j + " (" + i + ").mcc");
                }
            }

            float[] res = new float[gmmList.Count];
            int[] rSp = new int[gmmList.Count];
            int[] errSp = new int[gmmList.Count];
            int[] falseAlarm = new int[gmmList.Count];
            int right = 0, error = 0;
            int total = 0;
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory +"\\" + textBox8.Text +  ".txt");
            
                
            for (int j = 0; j < testList.Count; j++)
            {

                for (int i = 0; i < gmmList.Count; i++)
                {
                    res[i] = gmmList[i].Classify(testList[j]);
                    //s содержит путь к gmm
                }

                float max = -(float.MaxValue - 1);
                int numb = 0;
                for (int i = 0; i < gmmList.Count; i++)
                    if (res[i] > max) { max = res[i]; numb = i; }
                //вывести инфу
                
                if (Path.GetFileNameWithoutExtension(s[numb]) == Path.GetFileNameWithoutExtension(testList[j]).Split(' ')[0])
                {
                    //fs.WriteLine("Распознано верно - " + Path.GetFileNameWithoutExtension(testList[j]));
                    //int.TryParse(Path.GetFileNameWithoutExtension(s[numb]).Split('.')[0], out numb);
                    right++;
                    rSp[numb]++;
                }
                else
                {
                    //fs.WriteLine("Распознано неверно - " + Path.GetFileNameWithoutExtension(testList[j]));
                    error++;
                    falseAlarm[numb]++;
                    int.TryParse(Path.GetFileNameWithoutExtension(testList[j]).Split(' ')[0], out numb);
                    if (numb > 100) numb -= 73; else numb--;
                   
                    errSp[numb]++;
                }
                 
                
            }
            total = right+error;

            for (int i = 0; i < gmmList.Count; i++)
            {
                fs.WriteLine("Диктор - " +i+ " распознано " + rSp[i] + " ; процентов - " + (rSp[i]*100/(errSp[i]+rSp[i]))+ " ложных срабатываний - " + falseAlarm[i]);
            }

            fs.WriteLine("Всего распознано верно - " + right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100*right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100*error / total)).ToString());
            fs.Close();
        }

        private float SumLen(float[] len,bool[] used)
        {
            float res = 0;
            for (int i = 0; i < len.Length; i++)
                if (used[i]) res += len[i];
            return res;
        }

        public float GetSeconds(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            int dimension = (int)((ushort)(Hb * byte.MaxValue + Lb));

            long n = (long)(len / (dimension * 4));

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);
            stream.Read(buf, 0, 4);

            float fr_rate = BitConverter.ToSingle(buf, 0);
            stream.Close();
            return n / fr_rate;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            /* Что тут делается:
             * 1. Глобальный цикл на число кросс-валидаций
             * 2. В каждом цикле данные разбиваются на участки x и y длительностью, x-обучающая, y - тестовая
             * 3. Проводится обучение GMM модели на x данных
             * 4. Проводится тестирование на Y данных
             * 5. Подводиться итог данного цикла
             * 6. Подводится итог всех цикллов
             */
            
            //загоняем данные о коэфф-ах в память
           

            //Узнать длительность каждого файла в кадрах
            //Составить список файлов, которые не должны участвовать в тестировании
            //
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + textBox10.Text + ".txt");
            int right = 0, error = 0, Right = 0, Error = 0;
            int total = 0;
            int CVCount = 0;
            int.TryParse(textBox14.Text, out CVCount);

            List<int> speakerList = new List<int>();
            for (int i = 1; i <= 50; i++) speakerList.Add(i);
            List<string>[] learnList = new List<string>[50];
            int learnLen, testLen;
            int.TryParse(textBox11.Text, out learnLen);
            int.TryParse(textBox13.Text, out testLen);
           
            //прогнать цикл по спикерам
            //выбрать тестовые данные
            //создать модель из тестовых данных

            for (int ii = 0; ii < CVCount; ii++)
            {
                right = 0; error = 0; total = 0;
                List<GMM> gmmList = new List<GMM>();
                for (int i = 0; i < speakerList.Count; i++)
                {
                    learnList[i] = new List<string>();
                    string[] s = System.IO.Directory.GetFiles(textBox12.Text, (i + 1) + " (*.mcc");
                    Random r = new Random(DateTime.Now.Millisecond);
                    bool[] used = new bool[s.Length];
                    float[] len = new float[s.Length];

                    for (int k = 0; k < s.Length; k++)
                    {
                        len[k] = GetSeconds(s[k]);
                    }

                    while (SumLen(len, used) < learnLen)
                    {
                        int j = r.Next(50);
                        if (!used[j])
                        {
                            used[j] = true;
                            learnList[i].Add(s[j]);
                        }
                    }
                    //сформировать Data из обучающих данных и создать модель

                    Data learnData = new Data(learnList[i], learnLen);

                    int gmmN = 0;
                    int.TryParse(textBox15.Text, out gmmN);

                    GMM g = new GMM(gmmN, learnData.dimension, learnData);
                    g.Train(learnData, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 0.95, 0.01, 100, 1);
                    gmmList.Add(g);
                }
                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count];
                int[] errSp = new int[gmmList.Count];
                int[] falseAlarm = new int[gmmList.Count];

                for (int i = 0; i < speakerList.Count; i++)
                {
                    string[] s = System.IO.Directory.GetFiles(textBox12.Text, (i + 1) + " (*.mcc");
                    Random r = new Random(DateTime.Now.Millisecond);
                    bool[] used = new bool[s.Length];
                    float[] len = new float[s.Length];

                    for (int k = 0; k < s.Length; k++)
                    {
                        len[k] = GetSeconds(s[k]);
                    }
                    //объединить все кроме обучающей выборки и разбить на Data по testLen секунд

                    List<string> DirList = s.ToList<string>();
                    foreach (string str in learnList[i])
                        DirList.Remove(str);

                    //перемешать список
                    int n = DirList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        string tmp = DirList[k];
                        DirList[k] = DirList[n];
                        DirList[n] = tmp;

                    }
                    List<Data> AllTestData = Data.JoinDataWLen(DirList, testLen);


                    float[] res = new float[gmmList.Count];


                    foreach (Data d in AllTestData)
                    {
                        //проводим классификацию каждого тестового случая по всем моделям и проверяем результат
                        float max = -(float.MaxValue - 1);
                        int numb = 0;
                        
                        for (int j = 0; j < gmmList.Count; j++)
                        {
                            res[j] = gmmList[j].Classify(d);
                            if (res[j] > max) { max = res[j]; numb = j; }
                        }

                        //вывести инфу

                        if (numb == i)
                        {
                            right++;
                            rSp[numb]++;
                        }
                        else
                        {
                            error++;
                            falseAlarm[numb]++;
                            errSp[i]++;
                        }

                    }
                }
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (errSp[i] + rSp[i])) + " ложных срабатываний - " + falseAlarm[i]);
                }

                fs.WriteLine("Всего распознано верно - " + right.ToString());
                fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * right / total)).ToString());

                fs.WriteLine("Всего распознано неверно - " + error.ToString());
                fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * error / total)).ToString());
                fs.Flush();

                Right += right;
                Error += error;
            }
            total = Right + Error;
            fs.WriteLine("-------------------------------------------------------------");
            fs.WriteLine("Всего распознано верно - " + Right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * Right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + Error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * Error / total)).ToString());
            fs.WriteLine("Всего - " + total.ToString());
            fs.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox12.Text = folderBrowserDialog1.SelectedPath;
       
        }

        private void button10_Click(object sender, EventArgs e)
        {
            /* Что тут делается:
             * 1. Глобальный цикл на число кросс-валидаций
             * 2. В каждом цикле данные разбиваются на участки x и y длительностью, x-обучающая, y - тестовая
             * 3. Проводится обучение GMM модели на x данных
             * 4. Проводится тестирование на Y данных
             * 5. Подводиться итог данного цикла
             * 6. Подводится итог всех цикллов
             */

            //загоняем данные о коэфф-ах в память


            //Узнать длительность каждого файла в кадрах
            //Составить список файлов, которые не должны участвовать в тестировании
            //
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + textBox10.Text + ".txt");
            int right = 0, error = 0, Right = 0, Error = 0;
            int total = 0;
            int CVCount = 0;
            float thrshSVM = 0;
            float.TryParse(textBox18.Text, out thrshSVM);
            int.TryParse(textBox14.Text, out CVCount);

            List<int> speakerList = new List<int>();
            for (int i = 1; i <= 50; i++) speakerList.Add(i);
            List<string>[] learnList = new List<string>[50];
            int learnLen, testLen;
            int.TryParse(textBox11.Text, out learnLen);
            int.TryParse(textBox13.Text, out testLen);

            //прогнать цикл по спикерам
            //выбрать тестовые данные
            //создать модель из тестовых данных

            for (int ii = 0; ii < CVCount; ii++)
            {
                right = 0; error = 0; total = 0;
                List<GMM> gmmList = new List<GMM>();
                for (int i = 0; i < speakerList.Count; i++)
                {
                    learnList[i] = new List<string>();
                    string[] s = System.IO.Directory.GetFiles(textBox12.Text, (i + 1) + " (*.mcc");
                    Random r = new Random(DateTime.Now.Millisecond);
                    bool[] used = new bool[s.Length];
                    float[] len = new float[s.Length];

                    for (int k = 0; k < s.Length; k++)
                    {
                        len[k] = GetSeconds(s[k]);
                    }

                    while (SumLen(len, used) < learnLen)
                    {
                        int j = r.Next(50);
                        if (!used[j])
                        {
                            used[j] = true;
                            learnList[i].Add(s[j]);
                        }
                    }
                    //сформировать Data из обучающих данных и создать модель

                    Data learnData = new Data(learnList[i], learnLen);

                    int gmmN = 0;
                    int.TryParse(textBox15.Text, out gmmN);

                    GMM g = new GMM(gmmN, learnData.dimension, learnData);
                    g.Train(learnData, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 0.95, 0.01, 100, 1);
                    gmmList.Add(g);
                }
                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count];
                int[] errSp = new int[gmmList.Count];
                int[] falseAlarm = new int[gmmList.Count];

                for (int i = 0; i < speakerList.Count; i++)
                {
                    string[] s = System.IO.Directory.GetFiles(textBox12.Text, (i + 1) + " (*.mcc");
                    Random r = new Random(DateTime.Now.Millisecond);
                    bool[] used = new bool[s.Length];
                    float[] len = new float[s.Length];

                    for (int k = 0; k < s.Length; k++)
                    {
                        len[k] = GetSeconds(s[k]);
                    }
                    //объединить все кроме обучающей выборки и разбить на Data по testLen секунд

                    List<string> DirList = s.ToList<string>();
                    foreach (string str in learnList[i])
                        DirList.Remove(str);

                    //перемешать список
                    int n = DirList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        string tmp = DirList[k];
                        DirList[k] = DirList[n];
                        DirList[n] = tmp;

                    }
                    List<Data> AllTestData = Data.JoinDataWLen(DirList, testLen);


                    float[] res = new float[gmmList.Count];


                    foreach (Data d in AllTestData)
                    {
                        //проводим классификацию каждого тестового случая по всем моделям и проверяем результат
                        float max = -(float.MaxValue - 1);
                        float max2 = -(float.MaxValue - 2);
                        int numb = 0, numb2 = 0;

                        for (int j = 0; j < gmmList.Count; j++)
                        {
                            res[j] = gmmList[j].Classify(d);
                            if (res[j] > max) { max = res[j]; numb = j; }
                            else { if (res[j] > max2 && j != numb) { max2 = res[j]; numb2 = j; } }

                        }
                        if (max - max2 < thrshSVM)
                        {
                            //svm-классификация. сохраняем образец в формате
                            StreamWriter stream = new StreamWriter(Environment.CurrentDirectory+"\\tmp", false);
                            Data.writeMFCC_to_SVM_GMM(stream, d);
                            stream.Close();

                            //выполняем scale
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true

                            };

                            if (checkBox3.Checked)
                            {
                                
                                startInfo.UseShellExecute = false;

                                startInfo.Arguments = "/c " + Environment.CurrentDirectory + "\\svm-scale.exe" + " -r " + Environment.CurrentDirectory + "\\range1 " + Environment.CurrentDirectory + "\\tmp" + " > " + Environment.CurrentDirectory + "\\tmp" + ".s";

                                var proc = Process.Start(startInfo);

                                proc.WaitForExit();

                            }
                            //predict
                            startInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true

                            };
                            startInfo.UseShellExecute = false;

                            if (numb > numb2)
                            {
                                startInfo.Arguments = "/c " + Environment.CurrentDirectory + "\\svm-predict.exe " + Environment.CurrentDirectory + "\\tmp.s " + textBox19.Text + "\\svm_train\\" + (numb2 + 1) + '-' + (numb + 1) + ".trs.model "
                                + Environment.CurrentDirectory + "\\svm_outp";
                            }
                            else
                            {
                                startInfo.Arguments = "/c " + Environment.CurrentDirectory + "\\svm-predict.exe " + Environment.CurrentDirectory + "\\tmp.s " + textBox19.Text + "\\svm_train\\" + (numb + 1) + '-' + (numb2 + 1) + ".trs.model "
                                + Environment.CurrentDirectory + "\\svm_outp";
                            }


                            var proc2 = Process.Start(startInfo);

                            proc2.WaitForExit();

                            //считать результат
                            StreamReader streamR = new StreamReader(Environment.CurrentDirectory + "\\svm_outp");
                            int maxResult = 0, firstResult = 0;
                            while (!streamR.EndOfStream)
                            {
                                maxResult++;
                                if (streamR.ReadLine() == "1") firstResult++;
                            }

                            if (firstResult < maxResult / 2) numb = numb2;

                            streamR.Close();
                        }
                        //вывести инфу

                        if (numb == i)
                        {
                            right++;
                            rSp[numb]++;
                        }
                        else
                        {
                            error++;
                            falseAlarm[numb]++;
                            errSp[i]++;
                        }

                    }
                }
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (errSp[i] + rSp[i])) + " ложных срабатываний - " + falseAlarm[i]);
                }

                fs.WriteLine("Всего распознано верно - " + right.ToString());
                fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * right / total)).ToString());

                fs.WriteLine("Всего распознано неверно - " + error.ToString());
                fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * error / total)).ToString());
                fs.Flush();

                Right += right;
                Error += error;
            }
            total = Right + Error;
            fs.WriteLine("-------------------------------------------------------------");
            fs.WriteLine("Всего распознано верно - " + Right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * Right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + Error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * Error / total)).ToString());
            fs.WriteLine("Всего - " + total.ToString());
            fs.Close();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox19.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox21.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            float thr = 0;
            float.TryParse(textBox20.Text, out thr);
            string[] s;
            if (checkBox6.Checked) s = System.IO.Directory.GetFiles(textBox21.Text, "*.mcc",SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(textBox21.Text, "*.mcc");

            //загружаем Data;
            //записываем без пауз

            foreach (string s1 in s)
            {
                Data dt = new Data(s1, false);

                dt.Save(s1, thr);


            }
        }

        // ТЕст с чистой КВ
        private void button14_Click(object sender, EventArgs e)
        {
            /* Что тут делается:
             * 1. Глобальный цикл на число кросс-валидаций
             * 2. В каждом цикле данные разбиваются на участки x и y длительностью, x-обучающая, y - тестовая; x -10 сегментов
             * 3. Проводится обучение GMM модели на x данных
             * 4. Проводится тестирование на Y данных
             * 5. Подводиться итог данного цикла
             * 6. Подводится итог всех цикллов
             */

            //загоняем данные о коэфф-ах в память


            //Узнать длительность каждого файла в кадрах
            //Составить список файлов, которые не должны участвовать в тестировании
            //
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + textBox10.Text + ".txt");
            int right = 0, error = 0, Right = 0, Error = 0;
            int total = 0;
            int CVCount = 4;
            
            List<int> speakerList = new List<int>();
            for (int i = 1; i <= 50; i++) speakerList.Add(i);
            List<string>[] learnList = new List<string>[50];
            int learnLen = int.MaxValue, testLen;
            //int.TryParse(textBox11.Text, out learnLen);
            int.TryParse(textBox13.Text, out testLen);

            //прогнать цикл по спикерам
            //выбрать тестовые данные
            //создать модель из тестовых данных

            for (int ii = 0; ii < CVCount; ii++)
            {
                right = 0; error = 0; total = 0;
                List<GMM> gmmList = new List<GMM>();
                for (int i = 0; i < speakerList.Count; i++)
                {
                    learnList[i] = new List<string>();
                    //Создание списка обучающих сегментов
                    for (int k = 1; k <= 20; k++)
                    {
                        learnList[i].Add(textBox12.Text + "\\" + (i + 1) + " (" + (k+ii*10) + ").mcc");
                    }
                    
                    //сформировать Data из обучающих данных и создать модель

                    Data learnData = new Data(learnList[i], learnLen);

                    int gmmN = 0;
                    int.TryParse(textBox15.Text, out gmmN);

                    GMM g = new GMM(gmmN, learnData.dimension, learnData);
                    g.Train(learnData, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 0.95, 0.01, 100, 1);
                    gmmList.Add(g);
                }
                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count];
                int[] errSp = new int[gmmList.Count];
                int[] falseAlarm = new int[gmmList.Count];

                for (int i = 0; i < speakerList.Count; i++)
                {
                    string[] s = System.IO.Directory.GetFiles(textBox12.Text, (i + 1) + " (*.mcc");
                    Random r = new Random(DateTime.Now.Millisecond);
                    bool[] used = new bool[s.Length];
                    float[] len = new float[s.Length];

                    for (int k = 0; k < s.Length; k++)
                    {
                        len[k] = GetSeconds(s[k]);
                    }
                    //объединить все кроме обучающей выборки и разбить на Data по testLen секунд

                    List<string> DirList = s.ToList<string>();
                    foreach (string str in learnList[i])
                        DirList.Remove(str);

                    //перемешать список
                    int n = DirList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        string tmp = DirList[k];
                        DirList[k] = DirList[n];
                        DirList[n] = tmp;

                    }

                    // подменяем директории на те, где не обрезаны фильтром энергии
                    for ( int i1=0; i1<DirList.Count; i1++)
                    {

                        DirList[i1] = textBox22.Text + "\\" + System.IO.Path.GetFileName(DirList[i1]);
                    }
                    List<Data> AllTestData = Data.JoinDataWLen(DirList, testLen);


                    float[] res = new float[gmmList.Count];


                    foreach (Data d in AllTestData)
                    {
                        //проводим классификацию каждого тестового случая по всем моделям и проверяем результат
                        float max = -(float.MaxValue - 1);
                        int numb = 0;
                        float E = 0;
                        float.TryParse(textBox23.Text, out E);
                        d.deleteLowEnergySamples(E);
                        for (int j = 0; j < gmmList.Count; j++)
                        {
                            res[j] = gmmList[j].Classify(d);
                            if (res[j] > max) { max = res[j]; numb = j; }
                        }

                        //вывести инфу

                        if (numb == i)
                        {
                            right++;
                            rSp[numb]++;
                        }
                        else
                        {
                            error++;
                            falseAlarm[numb]++;
                            errSp[i]++;
                        }

                    }
                }
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (errSp[i] + rSp[i])) + " ложных срабатываний - " + falseAlarm[i]);
                }

                fs.WriteLine("Всего распознано верно - " + right.ToString());
                fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * right / total)).ToString());

                fs.WriteLine("Всего распознано неверно - " + error.ToString());
                fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * error / total)).ToString());
                fs.Flush();

                Right += right;
                Error += error;
            }
            total = Right + Error;
            fs.WriteLine("-------------------------------------------------------------");
            fs.WriteLine("Всего распознано верно - " + Right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * Right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + Error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * Error / total)).ToString());
            fs.WriteLine("Всего - " + total.ToString());
            fs.Close();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            
        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            /* Что тут делается:
             * 1. Глобальный цикл на число кросс-валидаций
             * 2. В каждом цикле данные разбиваются на участки x и y длительностью, x-обучающая, y - тестовая; x -20 сегментов
             * 3. Проводится обучение UBM-GMM модели на x данных
             * 3.2. Проводится MAP адаптация моделей дикторов
             * 4. Проводится тестирование на Y данных
             * 5. Подводиться итог данного цикла
             * 6. Подводится итог всех цикллов
             */

            //загоняем данные о коэфф-ах в память


            //Узнать длительность каждого файла в кадрах
            //Составить список файлов, которые не должны участвовать в тестировании
            //
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + textBox10.Text + ".txt");
            int right = 0, error = 0, Right = 0, Error = 0;
            int total = 0;
            int CVCount = 1; //заглушка
            float thrsh = 0;
            float.TryParse(textBox18.Text, out thrsh);

            List<int> speakerList = new List<int>();
            for (int i = 1; i <= 50; i++) speakerList.Add(i);
            List<string>[] learnList = new List<string>[50];
            int learnLen = int.MaxValue, testLen;
            //int.TryParse(textBox11.Text, out learnLen);
            int.TryParse(textBox13.Text, out testLen);
            int iter_num = 1;
            int.TryParse(textBox30.Text, out iter_num);
            int alpha = 14;
            int.TryParse(textBox31.Text, out alpha);

            bool cutMFT = false, delete_mft = checkBox10.Checked; int mft_num = 0;
            if (checkBox9.Checked) { cutMFT = true; int.TryParse(textBox29.Text, out mft_num); }

            //Парсим текстбокс с номерами характеристик
            string[] numbers = textBox28.Text.Replace(',', ' ').Split(' ');
            List<int> feat_num = new List<int>();
            foreach (string s1 in numbers)
            {
                if (!s1.Contains(' '))
                {
                    if (s1.Contains('-'))
                    {
                        int i1 = 0, i2 = 0;
                        int.TryParse(s1.Split('-')[0], out i1);
                        int.TryParse(s1.Split('-')[1], out i2);
                        if (i2 != 0)
                            for (int i = i1; i <= i2; i++) feat_num.Add(i);
                    }
                    else
                    {
                        int i = 0;
                        int.TryParse(s1, out i);
                        if (i != 0) feat_num.Add(i);
                        if (s1 == "0") feat_num.Add(0);
                    }
                }
            }
            if (feat_num.Count == 0) feat_num = null;
            else feat_num.Add(0);
            
            //прогнать цикл по спикерам
            //выбрать тестовые данные
            //создать модель из тестовых данных

            for (int ii = 0; ii < CVCount; ii++)
            {

                right = 0; error = 0; total = 0;
                List<GMM> gmmList = new List<GMM>();
                learnList[0] = new List<string>();
                GMM ubm = null;
                Data learnData = null;
                int gmmN = 0;
                int.TryParse(textBox15.Text, out gmmN);
                //проверить, есть ли gmm?
                if (File.Exists(textBox12.Text + "\\" + "ubm.gmm"))
                {
                    ubm = new GMM(textBox12.Text + "\\" + "ubm.gmm", feat_num);
                }
                else
                {
                    for (int i = 0; i < speakerList.Count; i++)
                    {

                        //Создание списка обучающих сегментов
                        for (int k = 1; k <= 20; k++)
                        {
                            learnList[0].Add(textBox12.Text + "\\" + (i + 1) + " (" + k + ").mcc");
                        }

                    }
                    
                    //сформировать Data из обучающих данных и создать модель UBM
                    learnData = new Data(learnList[0], learnLen,feat_num);

                    if (cutMFT) learnData.CutMftSamples(mft_num, delete_mft);
                    

                    ubm = new GMM(gmmN, learnData.dimension, learnData);
                    ubm.Train(learnData, "asdas", textBox12.Text + "\\" + "ubm.gmm", gmmN, 0.95, 0.01, 100, 1);
                }
                //адаптировать модели дикторов
                for (int i = 0; i < speakerList.Count; i++)
                {
                    learnList[i] = new List<string>();
                    //Создание списка обучающих сегментов
                    for (int k = 21; k <= 30; k++)
                    {
                        learnList[i].Add(textBox12.Text + "\\" + (i + 1) + " (" + k + ").mcc");
                    }

                    learnData = new Data(learnList[i], learnLen,feat_num);
                    if (cutMFT) learnData.CutMftSamples(mft_num,delete_mft);
                    GMM spkr = new GMM(ubm,true);
                    spkr.Adapt(learnData, ubm, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN,alpha, 0.95, 0.01,iter_num, 1);
                    gmmList.Add(spkr);
                }

                //взять фразы по 10 секунд, вычислить счет и записать в файлик
                //усреднить счет легальный и злоумышленника
                StreamWriter fs2 = new StreamWriter(Environment.CurrentDirectory + "\\Thr_log.txt",false);
                Random r = new Random(DateTime.Now.Millisecond);
                List<float> trueList = new List<float>();
                List<float> falseList = new List<float>();
                float[] thr = new float[speakerList.Count];

                for (int i = 1; i <= speakerList.Count; i++)
                {
                    //берем несколько записей i-го диктора
                    //пока возьму 3 первых из ubm
                    fs2.WriteLine("Диктор "+i+" результаты его отрезков");
                    fs2.WriteLine();

                    //схема с отдельными файлами
                    /*for (int j = 1; j <= 10; j++)
                    {
                        Data d = new Data(textBox12.Text + "\\" + i + " (" + j + ").mcc");
                        trueList.Add(gmmList[i-1].Classify(d, 1, null, ubm));
                        fs2.WriteLine(trueList.Last());
                    }*/

                    //схема с 10 секундами
                    //true speaker
                    List<Data> dList = new List<Data>();
                    List<string> dirList = new List<string>();
                    float min = float.MaxValue;
                    float max2 = float.MinValue;
                    float avg = 0;
                    float avg2 = 0;
                    trueList.Clear();
                    falseList.Clear();

                    for (int j = 1; j <= 15; j++)
                    {
                        dirList.Add(textBox12.Text + "\\" + i + " (" + j + ").mcc");
                    }
                    dList.AddRange(Data.JoinDataWLen(dirList, testLen,feat_num));

                    foreach (Data d in dList)
                    {
                        if (cutMFT) d.CutMftSamples(mft_num,delete_mft);
                        trueList.Add(gmmList[i - 1].Classify(d, 1, null, ubm));
                        if (trueList.Last() < min) min = trueList.Last();
                        avg += trueList.Last();
                        fs2.WriteLine(trueList.Last());
                    }
                    //thr[i - 1] = min;
                    //thr[i - 1] = avg/trueList.Count;
                    //thr[i - 1] = avg/trueList.Count - ((avg/trueList.Count) - min)/2;
                    fs2.WriteLine();
                    fs2.WriteLine("Диктор " + i + " результаты чужих отрезков");

                    /*
                    for (int k = 1; k <= speakerList.Count; k++)
                    {
                        if (k != i)
                        {

                            for (int j = 1; j <= 3; j++)
                            {
                                Data d = new Data(textBox12.Text + "\\" + k + " (" + j + ").mcc");
                                falseList.Add(gmmList[i - 1].Classify(d, 1, null, ubm));
                                fs2.WriteLine(falseList.Last());
                            }
                        }
                    }
                     */

                    //переделал на схему с 10 секундами

                    for (int k = 1; k <= speakerList.Count; k++)
                    {
                        if (k != i)
                        {
                            dirList.Clear();
                            for (int j = 1; j <= 5; j++)
                            {
                                dirList.Add(textBox12.Text + "\\" + k + " (" + j + ").mcc");
                            }
                            dList.Clear();
                            dList.AddRange(Data.JoinDataWLen(dirList, testLen,feat_num));

                            foreach (Data d in dList)
                            {
                                if (cutMFT) d.CutMftSamples(mft_num,delete_mft);
                                falseList.Add(gmmList[i - 1].Classify(d, 1, null, ubm));
                                if (falseList.Last() > max2) max2 = falseList.Last();
                                avg2 += falseList.Last();
                                fs2.WriteLine(falseList.Last());
                            }
                            
                        }
                    }

                    thr[i - 1] = ((avg / trueList.Count - ((avg / trueList.Count) - min) / 2) + (avg2 / falseList.Count + (max2 - (avg2 / falseList.Count)) / 2))/2;


                    fs2.WriteLine("------------------------------------------");
                    fs2.WriteLine();
                }

                fs2.Close();

                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count+1];
                int[] rejected = new int[gmmList.Count+1];
                int[] errSp = new int[gmmList.Count+1]; //1
                int[] falseAlarm = new int[gmmList.Count+1]; //2
                

                //вывести параметры теста
                fs.WriteLine("Путь с семплами: "+textBox12.Text);
                fs.WriteLine("Компонент GMM: " + textBox15.Text);
                fs.WriteLine("Длина тестовых данных (сек): " + textBox13.Text);
                fs.WriteLine("Используемые характеристики: " + textBox28.Text);
                fs.WriteLine("Удалять семплы без тона: " + checkBox9.Checked);
                fs.WriteLine("Удалять характеристики после mft: " + checkBox10.Checked);
                fs.WriteLine("Циклов адаптации UBM: " + textBox30.Text);
                fs.WriteLine("Параметр адаптации UBM alpha: " + textBox31.Text);

                fs.WriteLine("-------------------------------------------------------------");

                //создаем список обучающих сегментов
                
                List<string> DirList;
                List<Data> AllTestData = new List<Data>();

                for (int l = 0; l < speakerList.Count; l++)
                {
                    DirList = new List<string>();
                    //Создание списка обучающих сегментов
                    for (int k = 31; k <= 50; k++)
                    {
                        DirList.Add(textBox12.Text + "\\" + (l + 1) + " (" + k + ").mcc");
                    }

                    // не надо перемешать список!
                    /*int n = DirList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        string tmp = DirList[k];
                        DirList[k] = DirList[n];
                        DirList[n] = tmp;
                    }*/
                    AllTestData.AddRange(Data.JoinDataWLen(DirList, testLen,feat_num));
                }
                    //объединить все кроме обучающей выборки и разбить на Data по testLen секунд

                foreach (Data d in AllTestData)
                if (cutMFT) d.CutMftSamples(mft_num,delete_mft);

                    for (int i = 0; i < speakerList.Count; i++)
                    {
                        //bool[] used = new bool[s.Length];

                        float[] res = new float[gmmList.Count];

                        foreach (Data d in AllTestData)
                        {
                            //проводим классификацию каждого тестового случая по всем моделям и проверяем результат
                            //float max = -(float.MaxValue - 1);

                            /*float E = 0;
                            float.TryParse(textBox23.Text, out E);
                            d.deleteLowEnergySamples(E);*/
                            
                            if (gmmList[i].Classify(d, 1, null, ubm) > thr[i])
                            //if (gmmList[i].Classify(d, 1, null) - (ubm.Classify(d, 1, null)) > thrsh)
                            {
                                if (d.spkrID == i + 1)
                                {
                                    right++;
                                    rSp[d.spkrID - 1]++;
                                }
                                else
                                {
                                    error++;
                                    errSp[i]++;
                                }
                            }
                            else
                            {
                                if (d.spkrID == i + 1)
                                {
                                    error++;
                                    falseAlarm[i]++;
                                    
                                }
                                else
                                {
                                    right++;
                                    rejected[d.spkrID - 1]++;
                                }                               
                                
                            }


                        }

                    }
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (falseAlarm[i] + rSp[i])) + " ош 1 рода - " + errSp[i] + "; ош 2 рода - " + falseAlarm[i] + "; отвергнуто - " + rejected[i] + " ; процентов - " + (rejected[i] * 100 / (rejected[i] + errSp[i])));
                    rSp[gmmList.Count ] += rSp[i];
                    errSp[gmmList.Count ] += errSp[i];
                    falseAlarm[gmmList.Count ] += falseAlarm[i];
                    rejected[gmmList.Count ] += rejected[i];
                }

                fs.WriteLine("Ошибка 1 рода - " + ((float)errSp[gmmList.Count] / (errSp[gmmList.Count] + rejected[gmmList.Count])).ToString("0.0000"));
                fs.WriteLine("Ошибка 2 рода - " + ((float)falseAlarm[gmmList.Count] / (falseAlarm[gmmList.Count] + rSp[gmmList.Count])).ToString("0.0000"));

                fs.WriteLine("Всего распознано верно - " + right.ToString());
                fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * right / total)).ToString());

                fs.WriteLine("Всего распознано неверно - " + error.ToString());
                fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * error / total)).ToString());
                fs.Flush();

                Right += right;
                Error += error;

            }
            total = Right + Error;
            fs.WriteLine("-------------------------------------------------------------");
            fs.WriteLine("Всего распознано верно - " + Right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * Right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + Error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * Error / total)).ToString());
            fs.WriteLine("Всего - " + total.ToString());
            fs.WriteLine("-------------------------------------------------------------");
                    
            
            fs.Close();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string[] s = System.IO.Directory.GetFiles(textBox21.Text, "*.mcc");

            //загружаем Data;

            Data.SaveCSV(s,textBox24.Text);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox25.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string gmmName = textBox26.Text;
            string path = textBox25.Text;
            int gmmN = 0;
            int.TryParse(textBox27.Text, out gmmN);
                                   
            //создаем единый дата из всех mfcc
            List<string> list = System.IO.Directory.GetFiles(path, "*.mcc",SearchOption.AllDirectories).ToList<string>();

            //обучаем gmm-ubm
            Data learnData = new Data(list);
            GMM ubm = new GMM(gmmN, learnData.dimension, learnData);
            ubm.Train(learnData, "asdas", path + "\\" + gmmName, gmmN, 0.95, 0.01, 100, 1);
        }

        /// <summary>
        /// Создает ЧОТ для всех wav в папке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button19_Click(object sender, EventArgs e)
        {
            int n = 0;
            string[] s;
            if (checkBox4.Checked)
                s = System.IO.Directory.GetFiles(textBox1.Text, "*.wav", SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(textBox1.Text, "*.wav");


            int.TryParse(textBox2.Text, out n);
            string dir = textBox1.Text;
                string shortArgs = " ";

            if (checkBox5.Checked) { dir+= "\\mft"; }
            if (n != 0) { shortArgs += "-X " + n + " "; dir += n + "X"; }
                if (radioButton2.Checked) { shortArgs += "-d "; dir += "1d"; }
                if (radioButton3.Checked) { shortArgs += "-dd "; dir += "2d"; }
                if (checkBox2.Checked) { shortArgs += "-n "; dir += "N"; }
                if (checkBox5.Checked) { Directory.CreateDirectory(dir); }

                
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\test.exe",
                    WindowStyle = ProcessWindowStyle.Hidden

                };
                foreach (string s1 in s)
                {
                    if (checkBox5.Checked)
                        startInfo.Arguments = "\"" + s1 + "\" \"" + dir + "\\" + Path.GetFileNameWithoutExtension(s1) + ".mft\"" + shortArgs;
                    else startInfo.Arguments = "\"" + s1 + "\" \"" + Path.GetDirectoryName(s1) + "\\" + Path.GetFileNameWithoutExtension(s1) + ".mft\"" + shortArgs;
                    Process.Start(startInfo).WaitForExit();
                  

                }

            
        }

        /// <summary>
        /// Объединяем характеристики в единый mfcc файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button20_Click(object sender, EventArgs e)
        {
            string dir = textBox1.Text;

            string[] s;
            if (checkBox4.Checked) s = System.IO.Directory.GetFiles(textBox1.Text, "*.mcc", SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(textBox1.Text, "*.mcc");

            //загружаем Data;
            //записываем без пауз

            foreach (string s1 in s)
            {
                Data d1 = new Data(s1, false,false);
                string s2 = Path.GetDirectoryName(s1)+"\\"+ Path.GetFileNameWithoutExtension(s1)+".mft";
                Data d2 = new Data(true, s2, false, false);
                d1.combine_mfcc_mft(d2, s1, s2);
                
                d1.Save(s1);


            }

        }

        private void button21_Click(object sender, EventArgs e)
        {
            /* Что тут делается:
             * 1. Глобальный цикл на число кросс-валидаций
             * 2. В каждом цикле данные разбиваются на участки x и y длительностью, x-обучающая, y - тестовая; x -20 сегментов
             * 3. Проводится обучение UBM-GMM модели на x данных
             * 3.2. Проводится MAP адаптация моделей дикторов
             * 4. Проводится тестирование на Y данных
             * 5. Подводиться итог данного цикла
             * 6. Подводится итог всех цикллов
             */

            //загоняем данные о коэфф-ах в память


            //Узнать длительность каждого файла в кадрах
            //Составить список файлов, которые не должны участвовать в тестировании
            //
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + textBox10.Text + ".txt");
            int right = 0, error = 0, Right = 0, Error = 0;
            int total = 0;
            int CVCount = 1; //заглушка
            float thrsh = 0;
            float.TryParse(textBox18.Text, out thrsh);

            List<int> speakerList = new List<int>();
            for (int i = 1; i <= 50; i++) speakerList.Add(i);
            List<string>[] learnList = new List<string>[50];
            int learnLen = int.MaxValue, testLen;
            //int.TryParse(textBox11.Text, out learnLen);
            int.TryParse(textBox13.Text, out testLen);
            int iter_num = 1;
            int.TryParse(textBox30.Text, out iter_num);
            int alpha = 14;
            int.TryParse(textBox31.Text, out alpha);

            bool cutMFT = false, delete_mft = checkBox10.Checked; int mft_num = 0;
            if (checkBox9.Checked) { cutMFT = true; int.TryParse(textBox29.Text, out mft_num); }

            //Парсим текстбокс с номерами характеристик
            string[] numbers = textBox28.Text.Replace(',', ' ').Split(' ');
            List<int> feat_num = new List<int>();
            foreach (string s1 in numbers)
            {
                if (!s1.Contains(' '))
                {
                    if (s1.Contains('-'))
                    {
                        int i1 = 0, i2 = 0;
                        int.TryParse(s1.Split('-')[0], out i1);
                        int.TryParse(s1.Split('-')[1], out i2);
                        if (i2 != 0)
                            for (int i = i1; i <= i2; i++) feat_num.Add(i);
                    }
                    else
                    {
                        int i = 0;
                        int.TryParse(s1, out i);
                        if (i != 0) feat_num.Add(i);
                        if (s1 == "0") feat_num.Add(0);
                    }
                }
            }
            if (feat_num.Count == 0) feat_num = null;
            else feat_num.Add(0);

            //прогнать цикл по спикерам
            //выбрать тестовые данные
            //создать модель из тестовых данных

            for (int ii = 0; ii < CVCount; ii++)
            {

                right = 0; error = 0; total = 0;
                List<GMM> gmmList = new List<GMM>();
                learnList[0] = new List<string>();
                GMM ubm = null;
                Data learnData = null;
                int gmmN = 0;
                int.TryParse(textBox15.Text, out gmmN);
                //проверить, есть ли gmm?
                if (File.Exists(textBox12.Text + "\\" + "ubm.gmm"))
                {
                    ubm = new GMM(textBox12.Text + "\\" + "ubm.gmm", feat_num);
                }
                else
                {
                    for (int i = 0; i < speakerList.Count; i++)
                    {

                        //Создание списка обучающих сегментов
                        for (int k = 1; k <= 20; k++)
                        {
                            learnList[0].Add(textBox12.Text + "\\" + (i + 1) + " (" + k + ").mcc");
                        }

                    }

                    //сформировать Data из обучающих данных и создать модель UBM
                    learnData = new Data(learnList[0], learnLen, feat_num);

                    if (cutMFT) learnData.CutMftSamples(mft_num, delete_mft);

                        ubm = new GMM(gmmN, learnData.dimension, learnData);
                        ubm.Train(learnData, "asdas", textBox12.Text + "\\" + "ubm.gmm", gmmN, 0.95, 0.01, 100, 1);
                    
                }
                //адаптировать модели дикторов
                for (int i = 0; i < speakerList.Count; i++)
                {
                    learnList[i] = new List<string>();
                    //Создание списка обучающих сегментов
                    for (int k = 21; k <= 30; k++)
                    {
                        learnList[i].Add(textBox12.Text + "\\" + (i + 1) + " (" + k + ").mcc");
                    }

                    learnData = new Data(learnList[i], learnLen, feat_num);
                    if (cutMFT) learnData.CutMftSamples(mft_num, delete_mft);
                    GMM spkr;
                    if (checkBox11.Checked) 
                    {
                        spkr = new GMM(gmmN, learnData.dimension, learnData);
                        spkr.Train(learnData, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 0.95, 0.01, 100,1);
                        //spkr.Adapt(learnData, ubm, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN, 14, 0.95, 0.01, 1, 1);
                        
                    }
                    else
                    {
                        spkr = new GMM(ubm,true);
                        //spkr = new GMM(gmmN, learnData.dimension, learnData);
                        spkr.Adapt(learnData, ubm, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN,alpha, 0.95, 0.01, iter_num, 1);                       
                    }
                    gmmList.Add(spkr);
                }

                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count + 1];
                int[] rejected = new int[gmmList.Count + 1];
                int[] errSp = new int[gmmList.Count + 1]; //1
                int[] falseAlarm = new int[gmmList.Count + 1]; //2


                //вывести параметры теста
                fs.WriteLine("Путь с семплами: " + textBox12.Text);
                fs.WriteLine("Компонент GMM: " + textBox15.Text);
                fs.WriteLine("Длина тестовых данных (сек): " + textBox13.Text);
                fs.WriteLine("Используемые характеристики: " + textBox28.Text);
                fs.WriteLine("Удалять семплы без тона: " + checkBox9.Checked);
                fs.WriteLine("Удалять характеристики после mft: " + checkBox10.Checked);
                fs.WriteLine("Циклов адаптации UBM: " + textBox30.Text);
                fs.WriteLine("Параметр адаптации UBM alpha: " + textBox31.Text);

                fs.WriteLine("-------------------------------------------------------------");

                //создаем список обучающих сегментов

                List<string> DirList;
                List<Data> AllTestData = new List<Data>();

                for (int l = 0; l < speakerList.Count; l++)
                {
                    DirList = new List<string>();
                    //Создание списка обучающих сегментов
                    for (int k = 31; k <= 50; k++)
                    {
                        DirList.Add(textBox12.Text + "\\" + (l + 1) + " (" + k + ").mcc");
                    }

                    // не надо перемешать список!
                    /*int n = DirList.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = r.Next(n + 1);
                        string tmp = DirList[k];
                        DirList[k] = DirList[n];
                        DirList[n] = tmp;
                    }*/
                    AllTestData.AddRange(Data.JoinDataWLen(DirList, testLen, feat_num));
                }
                //объединить все кроме обучающей выборки и разбить на Data по testLen секунд

                foreach (Data d in AllTestData)
                    if (cutMFT) d.CutMftSamples(mft_num, delete_mft);
         
                    //bool[] used = new bool[s.Length];

                    float[] res = new float[gmmList.Count];

                    foreach (Data d in AllTestData)
                    {
                         //проводим классификацию каждого тестового случая по всем моделям и проверяем результат
                        float max = -(float.MaxValue - 1);
                        int numb = 0;
                        float E = 0;
                        float.TryParse(textBox23.Text, out E);
                        d.deleteLowEnergySamples(E);
                        for (int j = 0; j < gmmList.Count; j++)
                        {
                            res[j] = gmmList[j].Classify(d);
                            if (res[j] > max) { max = res[j]; numb = j; }
                        }

                        //вывести инфу

                        if (numb +1 == d.spkrID)
                        {
                            right++;
                            rSp[numb]++;
                        }
                        else
                        {
                            error++;
                            falseAlarm[d.spkrID-1]++;
                            errSp[numb]++;
                        }

                    }

                
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (falseAlarm[i] + rSp[i])) + " ош 1 рода - " + errSp[i] + "; ош 2 рода - " + falseAlarm[i] );
                    rSp[gmmList.Count] += rSp[i];
                    errSp[gmmList.Count] += errSp[i];
                    falseAlarm[gmmList.Count] += falseAlarm[i];
                    rejected[gmmList.Count] += rejected[i];
                }

                 fs.WriteLine("Ошибка 1 рода - " + ((float)errSp[gmmList.Count] / (errSp[gmmList.Count] + rejected[gmmList.Count])).ToString("0.0000"));
                fs.WriteLine("Ошибка 2 рода - " + ((float)falseAlarm[gmmList.Count] / (falseAlarm[gmmList.Count] + rSp[gmmList.Count])).ToString("0.0000"));

                fs.WriteLine("Всего распознано верно - " + right.ToString());
                fs.WriteLine("Всего распознано верно проценты - " + ((float)(100 * right / total)).ToString("0.00"));

                fs.WriteLine("Всего распознано неверно - " + error.ToString());
                fs.WriteLine("Всего распознано неверно проценты - " + ((float)(100 * error / total)).ToString("0.00"));
                fs.Flush();

                Right += right;
                Error += error;

            }
            total = Right + Error;
            fs.WriteLine("-------------------------------------------------------------");
            fs.WriteLine("Всего распознано верно - " + Right.ToString());
            fs.WriteLine("Всего распознано верно проценты - " + ((int)(100 * Right / total)).ToString());

            fs.WriteLine("Всего распознано неверно - " + Error.ToString());
            fs.WriteLine("Всего распознано неверно проценты - " + ((int)(100 * Error / total)).ToString());
            fs.WriteLine("Всего - " + total.ToString());
            fs.WriteLine("-------------------------------------------------------------");


            fs.Close();
        }
    }
}
