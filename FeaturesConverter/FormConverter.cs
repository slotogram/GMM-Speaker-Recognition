using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


/*
 * 
 * Что должен делать конвертер:
 * 1. Создавать списки с файлами для дикторов
 * 2. Убирать ненужные фичи из файлов
 * 3. Объединять файлы с фичами в один (обучающая выборка) или в несколько (тестовая выборка)
 * 4. Убирать ненужные вреймы (на основе VAD)
 */
namespace FeaturesConverter
{
    public partial class FormConverter : Form
    {
        public FormConverter()
        {
            InitializeComponent();
        }


        private void button12_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox21.Text = folderBrowserDialog1.SelectedPath;
        }

        private void OutList(string path, List<string> fileList)
        {
            StreamWriter fs = new StreamWriter(Environment.CurrentDirectory + "\\" + path + ".txt");
            foreach (string s in fileList)
            {
                fs.WriteLine(s);
            }
            fs.Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //Парсим текстбокс с номерами характеристик
            List<int> feat_num = stringNumParse(textBox28.Text, true);

            //Парсим текстбокс со списком файлов, используемых для обучения моделей дикторов
            List<int> learn_num = stringNumParse(textBoxLearnPhrases.Text, false);
            //Парсим текстбокс со списком файлов, используемых для тестирования моделей дикторов
            List<int> test_num = stringNumParse(textBoxTestPhrases.Text, false);
            //Парсим текстбокс со списком файлов, используемых для создания UBM
            List<int> UBM_num = stringNumParse(textBoxUBMPhrases.Text, false);
            List<int> UBM_Speakers = stringNumParse(textBoxUBMSpeakersNum.Text, false);
            //Парсим текстбокс со списком speakers
            List<int> Speakers = stringNumParse(textBoxSpeakerSet.Text, false);

            string featPath = textBoxLists.Text;

            //получаем список нужных нам файлов с фичами и выводим их в текстовые лист файлы

            /*  string[] s;
                if (checkBox6.Checked)
                s = System.IO.Directory.GetFiles(featPath, "*" + textBoxFeatureExtension.Text, SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(featPath, "*"+textBoxFeatureExtension.Text);*/

            List<string> filesList = new List<string>();
            //создаем список для UBM
            foreach (int i in UBM_num)
            {
                foreach (int j in UBM_Speakers)
                {
                    filesList.Add(j + " (" + i + ")" + textBoxFeatureExtension.Text);
                }
            }
            //выводим в файл UBM.lst
            OutList(featPath+"\\UBM.lst",filesList);
            //создаем список обучающих файлов

            filesList.Clear();
            foreach (int j in Speakers)
            {   
                foreach (int i in learn_num)
                {
                    filesList.Add(j+ " "+j + " (" + i + ")" + textBoxFeatureExtension.Text);
                }
            }
            //выводим в файл Train.lst
            OutList(featPath + "\\Train.lst", filesList);
            //создаем список тестовых файлов
            filesList.Clear();
            foreach (int j in Speakers)
            {
                foreach (int k in Speakers)
                {
                    foreach (int i in learn_num)
                    {
                        
                        filesList.Add(j+ " " + k + " (" + i + ")" + textBoxFeatureExtension.Text+ " "+ ((j == k) ? "1" : "0"));
                    }
                }
                
            }
            //выводим в файл Test.lst
            OutList(featPath + "\\Test.lst", filesList);
        }

        private List<int> stringNumParse(string s, bool needZeroEnd)
        {
            string[] numbers = s.Replace(',', ' ').Split(' ');
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
            else if (needZeroEnd) feat_num.Add(0);

            return feat_num;
        }

    }
}
