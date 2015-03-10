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
            string[] s = System.IO.Directory.GetFiles(textBox1.Text);
            
            
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

                Directory.CreateDirectory(dir);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\sfbcep.exe",
                    WindowStyle = ProcessWindowStyle.Hidden

                };
                foreach (string s1 in s)
                {

                    startInfo.Arguments = shortArgs + "\"" + s1 + "\" \"" + dir + "\\" + Path.GetFileNameWithoutExtension(s1) + ".mcc\"";
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
            string[] s = System.IO.Directory.GetFiles(textBox21.Text, "*.mcc");

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
                    ubm = new GMM(textBox12.Text + "\\" + "ubm.gmm");
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
                    learnData = new Data(learnList[0], learnLen);
                    
                    

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

                    learnData = new Data(learnList[i], learnLen);
                    GMM spkr = new GMM(gmmN, learnData.dimension, learnData);
                    spkr.Adapt(learnData, ubm, "asdas", textBox12.Text + "\\" + speakerList[i].ToString() + ".gmm", gmmN,14, 0.95, 0.01, 1, 1);
                    gmmList.Add(spkr);
                }

                //сформировать тестовую выборку и начать тестирование
                int[] rSp = new int[gmmList.Count];
                int[] errSp = new int[gmmList.Count]; //1
                int[] falseAlarm = new int[gmmList.Count]; //2

                //создаем список обучающих сегментов
                Random r = new Random(DateTime.Now.Millisecond);
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
                    AllTestData.AddRange(Data.JoinDataWLen(DirList, testLen));
                }
                    //объединить все кроме обучающей выборки и разбить на Data по testLen секунд
                 


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

                            //if (gmmList[i].Classify(d, 1, null, ubm) > thrsh)
                            if (gmmList[i].Classify(d, 1, null) - (ubm.Classify(d, 1, null)) > thrsh)
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
                                    rSp[d.spkrID - 1]++;
                                }                               
                                
                            }


                        }

                    }
                total = right + error;
                for (int i = 0; i < gmmList.Count; i++)
                {
                    fs.WriteLine("Диктор - " + i + " распознано " + rSp[i] + " ; процентов - " + (rSp[i] * 100 / (errSp[i] + rSp[i])) + " ош 1 рода - " + errSp[i] + "; ош 2 рода - " + falseAlarm[i]);
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

        private void button16_Click(object sender, EventArgs e)
        {
            string[] s = System.IO.Directory.GetFiles(textBox21.Text, "*.mcc");

            //загружаем Data;

            Data.SaveCSV(s,textBox24.Text);
        }
    }
}
