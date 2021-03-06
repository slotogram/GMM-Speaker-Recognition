﻿using System;
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
 * 1. Создавать списки с файлами для дикторов+
 * 2. Убирать ненужные фичи из файлов
 * 3. Объединять файлы с фичами в один (обучающая выборка) или в несколько (тестовая выборка)
 * 4. Убирать ненужные фреймы (на основе VAD)
 */
namespace SR_GMM
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
            StreamWriter fs = new StreamWriter(path);
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
            //Парсим текстбокс со списком speakers для тестирования
            List<int> SpeakersTest = stringNumParse(textBoxSpeakerSet.Text, false);


            string featPath = textBox21.Text;
            string listPath = textBoxLists.Text;
            //получаем список нужных нам файлов с фичами и выводим их в текстовые лист файлы

            /*  string[] s;
                if (checkBox6.Checked)
                s = System.IO.Directory.GetFiles(featPath, "*" + textBoxFeatureExtension.Text, SearchOption.AllDirectories);
            else s = System.IO.Directory.GetFiles(featPath, "*"+textBoxFeatureExtension.Text);*/

            List<string> filesList = new List<string>();
            List<string> list2 = new List<string>();
            //создаем список для UBM
            foreach (int j in UBM_Speakers)
            {
                foreach (int i in UBM_num)
                {
                    //filesList.Add("\""+featPath + "\\" +j  + " (" + i + ")" + textBoxFeatureExtension.Text+"\"");
                    //list2.Add("\"" + featPath + "\\" + j + " (" + i + ")" + textBoxFeatureExtension.Text + "\" "+j);
                    filesList.Add("\"" + j + " (" + i + ")" + textBoxFeatureExtension.Text + "\"");
                    list2.Add("\"" + j + " (" + i + ")" + textBoxFeatureExtension.Text + "\" "+j);
                }
            }
            //выводим в файл UBM.lst
            OutList(listPath+"\\UBM.lst",filesList);
            
            OutList(listPath + "\\UBM_id.lst", list2);
            
            
            
            //создаем список обучающих файлов

            filesList.Clear();
            foreach (int j in Speakers)
            {   
                foreach (int i in learn_num)
                {
                    filesList.Add(j+ " \""+j + " (" + i + ")" + textBoxFeatureExtension.Text + "\"");
                }
            }
            //выводим в файл Train.lst
            OutList(listPath + "\\Train.lst", filesList);
            //создаем список тестовых файлов. Тестируем всех дикторов, у которых есть модели. А файлы могут быть любых дикторов.
            filesList.Clear();
            foreach (int j in Speakers)
            {
                foreach (int k in SpeakersTest)
                {
                    foreach (int i in test_num)
                    {
                        
                        filesList.Add(j+ " \"" + k + " (" + i + ")" + textBoxFeatureExtension.Text+ "\" "+ ((j == k) ? "1" : "0"));
                    }
                }
                
            }
            //выводим в файл Test.lst
            OutList(listPath + "\\Test.lst", filesList);
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBoxLists.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBoxNewFeatDir.Text = folderBrowserDialog1.SelectedPath;
        }

        private void buttonCutFeat_Click(object sender, EventArgs e)
        {
            List<int> features = stringNumParse(textBox28.Text,true);
            string featDir = textBox21.Text;
            string newfeatDir = textBoxNewFeatDir.Text;
            string ext = textBoxExtension.Text;
            //получаем список файлов
            string[] s = System.IO.Directory.GetFiles(featDir, "*" + ext);

            //test
            /*Data dt = new Data(s[0], false, features);
            dt.SaveHtk("E:\\tst.htk");
            Data dt2 = new Data("E:\\tst.htk",false);
            */

            //в цикле каждый файл открываем с чтением нужных фич и перезаписываем
            for (int i = 0; i < s.Length; i++)
            {
                Data dt = new Data(s[i], false, features);
                dt.SaveHtk(newfeatDir+"\\"+System.IO.Path.GetFileName(s[i]));
            }
            
        }

    }
}
