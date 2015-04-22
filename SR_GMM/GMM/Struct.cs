﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace SR_GMM
{
    class Struct
    {
    }
    public class gauss
    {
        public float prior;  /* Prior probability of this component of the mixture.   */
        public float cgauss; /* Cache storage in order to make faster the classifier. */
        public float[] mean;  /* Means vector of gaussian multivariate distribution.   */
        public float[] dcov;  /* Diagonal covariances, when classify are the inverses. */
        public float[] _mean; /* Counts to estimate the future parameter (used by EM). */
        public float[] _dcov; /* Counts to estimate the future parameter (used by EM). */
        public float _z;     /* Counts to estimate the future parameter (used by EM). */
        public int _cfreq;  /* Number of data samples classified on this component.  */
        public gauss(int d)
        {
            mean = new float[d];
            dcov = new float[d];
            _mean = new float[d];
            _dcov = new float[d];
        }
    };
    
    public class Data
    {
		bool gotE, gotD, gotA, meanR , supE, normV;
        //float[][] m;

        public float[][] data;    /* The data matrix (samples*dimension).  */
        public float[] mean;     /* The mean by dimension of the samples. */
        public float[] variance; /* Variance of all the loaded samples.   */
        public long samples;    /* Samples number of the overall data.   */
        public int dimension;  /* Dimension number of the data loaded.  */
        public float frame_rate;
        public int spkrID;
        //ushort dimension;

        //private void LoadSingle(string Path)
        //public static List<Data> JoinDataWLen(List<string> Paths, int Length)


        public Data()
        {

        }

        public Data(bool isMFT, string path, bool loadData, bool needMean)
        {
            if (!isMFT) throw new Exception("not valid class creation");
            else

            if (loadData)
            {
                LoadSingleMFT(path);

                if (needMean) CalcMean();

                //взять имя файла
                string tmp = System.IO.Path.GetFileNameWithoutExtension(path);
                //найти пробел
                //вырезать с начала до пробела
                //!!!!!!!!!!!!!!!!!!!!!
                //Ставлю заглушку, если у нас другое имя, без пробелов
                if (tmp.IndexOf(' ') > 0)
                    tmp = tmp.Substring(0, tmp.IndexOf(' '));
                int ind = 0;
                //toInt
                int.TryParse(tmp, out ind);
                this.spkrID = ind;
            }
            else
            {
                LoadWithoutDataMFT(path);
            }
        }

        public void deleteLowEnergySamples(float E)
        {
            List<float[]> fList = new List<float[]>();

            for (int i = 0; i < samples; i++)
            {
                if (data[i][dimension - 1] > E) fList.Add(data[i]);
 
            }
            data = fList.ToArray();
            samples = fList.Count;
        }

        public static void writeMFCC_to_SVM_GMM(StreamWriter stream, Data mcc)
        {
            NumberFormatInfo nfi;
            nfi = new CultureInfo("en-US", false).NumberFormat;
            nfi.CurrencyDecimalSeparator = ".";
            string s;
            for (int i = 0; i < mcc.samples; i++)
            {
                s = "+1";
                for (int j = 0; j < mcc.dimension; j++)
                {
                    s += " " + (j + 1).ToString() + ":" + mcc.data[i][j].ToString(nfi);
                }
                stream.WriteLine(s);
            }

        }

        private void LoadDataToArray(string s,ref long counter, int dim, ref float[][] res)
        {
            FileStream stream = new FileStream(s, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            
            long n = (long)(len / (dimension * 4));

            byte[] flag = new byte[4];

            stream.Read(flag, 0, 4);

            /*TODO: Actions w flags

             *E 1 bit -вектор содержит log E
             *Z 2 bit - удалена средняя
             *N 3 - удалена статическая составляющая E (всегда с 4 и 1)
             *D 4 - есть дельты
             *A 5 - есть 2 дельты (всегда с 4)
             *R 6 - дисперсия нормализована (всегда с 2)
            */

            /*            bool gotE = false, gotD = false, gotA = false;

                        if ((flag[0] & 0x01) != 0) gotE = true; 
                        if ((flag[0] & 0x02) != 0) meanR = true;
                        if ((flag[0] & 0x04) != 0) supE = true;
                        if ((flag[0] & 0x08) != 0) gotD = true; 
                        if ((flag[0] & 0x10) != 0) gotA = true; 
                        if ((flag[0] & 0x20) != 0) normV = true;
            */

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            //frame_rate = BitConverter.ToSingle(buf, 0);

            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------
            //data = new float[n][];
            //samples = n;

            for (long i = counter; i < counter + n; i++)
            {
                //data[i] = new float[dimension];
                for (int j = 0; j < dimension; j++)
                {
                    res[i][j] = ReadFloat(stream);
                }

            }
            counter += n;
            stream.Close();
        }   

        private float[][] LoadDataFromList(List<string> s, long samp, int dim)
        {
            float[][] res = new float[samp][];
            long counter = 0;
            //выделяем память
            for (long i = 0; i < samp; i++)
            {
                res[i] = new float[dim];
            }

            this.dimension = dim;
            this.samples = samp;
            foreach (string s1 in s)
            {
                Data d = new Data();// = new Data(s1,false);
                //создаем отдельный метод загружающий дата в выделенный массив
                LoadDataToArray(s1, ref counter, dim, ref res);

            }
            
            return res;
        }

        public Data(string path, bool loadData, bool needMean)
        {
            if (loadData)
            {
                LoadSingle(path);

                if (needMean) CalcMean();

                //взять имя файла
                string tmp = System.IO.Path.GetFileNameWithoutExtension(path);
                //найти пробел

                //вырезать с начала до пробела
                //!!!!!!!!!!!!!!!!!!!!!
                //Ставлю заглушку, если у нас другое имя, без пробелов
                if (tmp.IndexOf(' ') > 0)
                    tmp = tmp.Substring(0, tmp.IndexOf(' '));
                int ind = 0;
                //toInt
                int.TryParse(tmp, out ind);
                this.spkrID = ind;
            }
            else
            {
                LoadWithoutData(path);
            }
        }


        /// <summary>
        /// Создает единый дата из нескольких дата файлов
        /// </summary>
        /// <param name="Paths">список путей к файлам mcc с параметрами</param>
        public Data(List<string> Paths)
        {
            
            //LoadSingle(Paths);
            //List <float[][]> lst = new List<float[][]>();
            Data d = new Data();
            long samp = 0;
            //узнаем статы и кол-во семплов
            
            foreach (string s in Paths)
            {
                d = new Data(s, false,false);
                samp += d.samples;
            }
            //удилать ???
            //Это я вроде бы использовал для оценки влияния энергиии в features

            /* 
            int count = 0;
            int count2 = 0;
            List<int> chng = new List<int>();
                         
            for (int i = 1; i < d.samples; i++)
            {
                //if (d.data[i][0] > 0) count++;
                //if ((d.data[i][0] > 0)&&(d.data[i][12] < 0)) count2++;
                if ((d.data[i][12] - d.data[i - 1][12] > 0.5) && (d.data[i][12] > 0.5)) { 
                    count++;
                    chng.Add(i);
                }
                if ((d.data[i][0] > 0.5) || (d.data[i][3] > 0.5) || (d.data[i][12] > 0.5)) count2++;
            }
            float flt = (float)count / d.samples;
            */
            //создаем новый дата, с кол-вом семлов samp, и список
            
            //создаем флоатовский список

            this.data = LoadDataFromList(Paths,samp, d.dimension);

            //data = lst[0];
            //dimension = d.dimension;
            frame_rate = d.frame_rate;
            
            
            //for (int i = 1; i < lst.Count; i++) { data = data.Concat(lst[i]).ToArray(); }
            //samples = samp;
          
            CalcMean();
                     

        }
        /// <summary>
        /// Создает Data из списка путей к mcc файлам Paths длиной Length, если есть список характеристик, то feat_list
        /// </summary>
        /// <param name="Paths"></param>
        /// <param name="Length"></param>
        public Data(List<string> Paths, int Length, List<int> feat_list=null)
        {

            //LoadSingle(Paths);
            List<float[][]> lst = new List<float[][]>();
            Data d = new Data();
            long samp = 0;
            foreach (string s in Paths)
            {
                if (feat_list == null) d = new Data(s, false);
                else d = new Data(s,false,feat_list);
                samp += d.samples;

                if ((samp / d.frame_rate) > Length)
                {
                    long diff = samp - d.samples;
                    long needSamp = (long)((Length - (diff / d.frame_rate)) * d.frame_rate);
                    float[][] newD = new float[needSamp][];
                    for (int i = 0; i< needSamp; i++)
                    {
                        newD[i] = new float[d.dimension];
                        d.data[i].CopyTo(newD[i],0);
                    }
                    samp = diff + needSamp;
                    lst.Add(newD);
                }
                else lst.Add(d.data);
            }
            data = lst[0];
            dimension = d.dimension;

            for (int i = 1; i < lst.Count; i++) { data = data.Concat(lst[i]).ToArray(); }
            samples = samp;

            CalcMean();

        }

        public static List<Data> JoinDataWLen(List<string> Paths, int Length, List<int> feat_list=null)
        {

            //LoadSingle(Paths);
            List<Data> result = new List<Data>();

            List<float[][]> lst = new List<float[][]>();
            Data d = new Data();
            long samp = 0;
            long frames = 0;
            
            float[][] buffer = null;

            foreach (string s in Paths)
            {
                d = new Data(s, false,feat_list); //поменял , можно брать только одного диктора данные
                samp += d.samples;
                
                if ((samp / d.frame_rate) > Length)
                {
                    if (buffer != null) buffer = buffer.Concat(d.data).ToArray();
                    else buffer = d.data;

                    frames = (long)(Length * d.frame_rate);
                    float[][] newD = new float[frames][];

                    for (int i = 0; i < frames; i++)
                    {
                        newD[i] = new float[d.dimension];
                        buffer[i].CopyTo(newD[i], 0);
                    }
                    d.data = newD;
                    d.samples = frames;
                    d.CalcMean();
                    result.Add(d);

                    long diff = samp - frames;
                    newD = new float[diff][];
                    for (int i = 0; i < diff; i++)
                    {
                        newD[i] = new float[d.dimension];
                        buffer[i+frames].CopyTo(newD[i], 0);
                    }
                    buffer = newD;
                    samp = diff;
                }
                else //lst.Add(d.data);
                    if (buffer != null) buffer = buffer.Concat(d.data).ToArray(); 
                            else buffer = d.data;
                    
            }
            /*data = lst[0];
            dimension = d.dimension;

            for (int i = 1; i < lst.Count; i++) { data = data.Concat(lst[i]).ToArray(); }
            samples = samp;

            CalcMean();
            */
            return result;
        }

        public Data(string Path, bool needMean, List<int> feat_list=null)
        {
            if (feat_list == null) LoadSingle(Path);
            else LoadSingle(Path, feat_list);

            if (needMean) CalcMean();
            
            //записываем номер диктора, если есть

                //взять имя файла
            string tmp = System.IO.Path.GetFileNameWithoutExtension(Path);
            
            int ind = 0;
            if (tmp.IndexOf(' ') >= 0)
            {
                //найти пробел
                //вырезать с начала до пробела
                tmp = tmp.Substring(0, tmp.IndexOf(' '));

                //toInt
                int.TryParse(tmp, out ind);
            }
            this.spkrID = ind;
            

        }
        /// <summary>
        /// Похоже, что этот конструктор не стоит использовать, я добавил код для id, но не уверен
        /// </summary>
        /// <param name="Path"></param>
        public Data(string Path)
        {
            LoadSingle(Path);

            CalcMean();

            //взять имя файла
            string tmp = System.IO.Path.GetFileNameWithoutExtension(Path);
            //найти пробел

            //вырезать с начала до пробела
            //!!!!!!!!!!!!!!!!!!!!!
            //Ставлю заглушку, если у нас другое имя, без пробелов
            if (tmp.IndexOf(' ') > 0)
            tmp = tmp.Substring(0, tmp.IndexOf(' '));
            int ind = 0;
            //toInt
            int.TryParse(tmp, out ind);
            this.spkrID = ind;
            //вызов train\
            
            //GMM g = new GMM(24, dimension, feas );
            //g.Train(feas, "asdas", "model.txt",24, 0.95,0.01,100,1);

        }

        private void CalcMean()
        {
            mean = new float[dimension];
            variance = new float[dimension];

            for (int i = 0; i < samples; i++)
                for (int j = 0; j < dimension; j++)
                {
                    mean[j] += data[i][j];
                    variance[j] += data[i][j] * data[i][j];
                }
            for (int i = 0; i < dimension; i++)
            { /* Compute the mean and variance of the data. */

                mean[i] /= samples;
                variance[i] = (variance[i] / samples) - (mean[i] * mean[i]);
                if (variance[i] < 0.000001) variance[i] = 0.000001f;
            }
        }

 
        private void LoadSingle(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            dimension = (int)((ushort)(Hb * byte.MaxValue + Lb));

            long n = (long)(len / (dimension * 4));

            byte[] flag = new byte[4];

            stream.Read(flag, 0, 4);

            /*TODO: Actions w flags

             *E 1 bit -вектор содержит log E
             *Z 2 bit - удалена средняя
             *N 3 - удалена статическая составляющая E (всегда с 4 и 1)
             *D 4 - есть дельты
             *A 5 - есть 2 дельты (всегда с 4)
             *R 6 - дисперсия нормализована (всегда с 2)
            */

            /*            bool gotE = false, gotD = false, gotA = false;

                        if ((flag[0] & 0x01) != 0) gotE = true; 
                        if ((flag[0] & 0x02) != 0) meanR = true;
                        if ((flag[0] & 0x04) != 0) supE = true;
                        if ((flag[0] & 0x08) != 0) gotD = true; 
                        if ((flag[0] & 0x10) != 0) gotA = true; 
                        if ((flag[0] & 0x20) != 0) normV = true;
            */

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            frame_rate = BitConverter.ToSingle(buf, 0);

            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------
            data = new float[n][];
            samples = n;

            for (long i = 0; i < n; i++)
            {
                data[i] = new float[dimension];
                for (int j = 0; j < dimension; j++)
                {
                    data[i][j] = ReadFloat(stream);
                }

            }

            stream.Close();
        }

        private void LoadSingle(string Path,List<int> feat_list)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            dimension = (int)((ushort)(Hb * byte.MaxValue + Lb));

            long n = (long)(len / (dimension * 4));

            byte[] flag = new byte[4];

            stream.Read(flag, 0, 4);

            /*TODO: Actions w flags

             *E 1 bit -вектор содержит log E
             *Z 2 bit - удалена средняя
             *N 3 - удалена статическая составляющая E (всегда с 4 и 1)
             *D 4 - есть дельты
             *A 5 - есть 2 дельты (всегда с 4)
             *R 6 - дисперсия нормализована (всегда с 2)
            */

            /*            bool gotE = false, gotD = false, gotA = false;

                        if ((flag[0] & 0x01) != 0) gotE = true; 
                        if ((flag[0] & 0x02) != 0) meanR = true;
                        if ((flag[0] & 0x04) != 0) supE = true;
                        if ((flag[0] & 0x08) != 0) gotD = true; 
                        if ((flag[0] & 0x10) != 0) gotA = true; 
                        if ((flag[0] & 0x20) != 0) normV = true;
            */

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);
            
            frame_rate = BitConverter.ToSingle(buf, 0);

            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------

            int new_dimension = feat_list.Count;
            
            if (dimension < new_dimension-1) throw new Exception("Not enough features in "+Path);
            data = new float[n][];
            samples = n;
            if (feat_list[new_dimension - 1] == 0) new_dimension--; //убираем выравниватель
            
            int counter;
            for (long i = 0; i < n; i++)
            {
                data[i] = new float[new_dimension];
                counter=0;
                for (int j = 0; j < dimension; j++)
                {
                    if (j == feat_list[counter]) { data[i][counter] = ReadFloat(stream); counter++; }
                    else SkipBytes(stream,sizeof(float));
                   
                }
                //stream.Position++;
            }
            
            dimension = new_dimension;
            stream.Close();
        }


        private void LoadWithoutData(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            dimension = (int)((ushort)(Hb * byte.MaxValue + Lb));

            long n = (long)(len / (dimension * 4));

            byte[] flag = new byte[4];

            stream.Read(flag, 0, 4);

            /*TODO: Actions w flags

             *E 1 bit -вектор содержит log E
             *Z 2 bit - удалена средняя
             *N 3 - удалена статическая составляющая E (всегда с 4 и 1)
             *D 4 - есть дельты
             *A 5 - есть 2 дельты (всегда с 4)
             *R 6 - дисперсия нормализована (всегда с 2)
            */

            /*            bool gotE = false, gotD = false, gotA = false;

                        if ((flag[0] & 0x01) != 0) gotE = true; 
                        if ((flag[0] & 0x02) != 0) meanR = true;
                        if ((flag[0] & 0x04) != 0) supE = true;
                        if ((flag[0] & 0x08) != 0) gotD = true; 
                        if ((flag[0] & 0x10) != 0) gotA = true; 
                        if ((flag[0] & 0x20) != 0) normV = true;
            */

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            frame_rate = BitConverter.ToSingle(buf, 0);

            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------
            samples = n;
            stream.Close();
        }
        private void SkipBytes(FileStream str, int n)
        {
            str.Position += n;
        }
        private void LoadSingleMFT(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            dimension = (int)ReadFloat(stream);

            long n = (long)ReadFloat(stream);
                 
            frame_rate = ReadFloat(stream);

            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------
            data = new float[n][];
            samples = n;

            for (long i = 0; i < n; i++)
            {
                data[i] = new float[dimension];
                for (int j = 0; j < dimension; j++)
                {
                    data[i][j] = ReadFloat(stream);
                }

            }

            stream.Close();
        }

        public FileStream getStreamtoDataMFCC(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
           
            byte[] flag = new byte[4];

            stream.Read(flag, 0, 4);
            byte[] buf = new byte[4];
            stream.Read(buf, 0, 4);
            return stream;
        }

        public FileStream getStreamtoDataMFT(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);
            ReadFloat(stream);
            ReadFloat(stream);
            ReadFloat(stream);

            return stream;
        }

        private void LoadWithoutDataMFT(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            dimension = (int)ReadFloat(stream);

            long n = (long)ReadFloat(stream);
            frame_rate = ReadFloat(stream);

            data = new float[n][];
            samples = n;
            stream.Close();
        }


        /// <summary>
        /// Объединяем данный пустой Дата без данных, с ЧОТ, при условии что размерность одинакова.
        /// </summary>
        /// <param name="d"></param>
        /// 
        public void combine_mfcc_mft(Data d,string path1, string path2)
        {
            if ((this.samples != d.samples)&&(d.samples != samples+1)) throw new Exception("number of samples is not equal");

            FileStream str1 = this.getStreamtoDataMFCC(path1);
            FileStream str2 = this.getStreamtoDataMFT(path2);

            int new_dimension = this.dimension + d.dimension;

            data = new float[this.samples][];
            
            for (long i = 0; i < samples; i++)
            {
                data[i] = new float[new_dimension];
                for (int j = 0; j < dimension; j++)
                {
                    data[i][j] = ReadFloat(str1);
                }
                for (int j = dimension; j < new_dimension; j++)
                {
                    data[i][j] = ReadFloat(str2);
                }
            }
            dimension = new_dimension;
            str1.Close();
            str2.Close();
        }

        public float ReadFloat(FileStream stream)
        {

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            return BitConverter.ToSingle(buf, 0);
        }
        public Data(float[][] dat , long s, int d)
        {
            samples = s;
            dimension = d;
            data = dat;
            mean = new float[dimension];
            variance = new float[dimension];

            for (int i=0; i<samples; i++)
                for (int j = 0; j < dimension; j++)
                {
                    mean[j] += data[i][j];
                    variance[j] += data[i][j] * data[i][j];
                }
            for (int i = 0; i < dimension; i++)
            { /* Compute the mean and variance of the data. */
               
                mean[i] /= samples;
                variance[i] = (variance[i] / samples) - (mean[i] * mean[i]);
                if (variance[i] < 0.000001) variance[i] = 0.000001f;
            }
        }

        /// <summary>
        /// Убираем все семплы, значение feat_num которых == -1
        /// </summary>
        /// <param name="feat_num"></param>
        public void CutMftSamples(int feat_num)
        {
            long counter = 0;
            
            if (feat_num >= this.dimension) throw new Exception("mft feature number is more than dimension of the data");
            long start;
            for (start = 0; start < this.samples; start++)
                if (data[start][feat_num] != -1) counter++;
                else break;
            for (long i = start; i < this.samples; i++)
            {
                if (data[i][feat_num] != -1)
                {
                        data[counter] = data[i];
                        counter++;
                }
            }
            this.samples = counter;
            //обрезаем все характеристики, что были после нашей ЧОТ.
            this.dimension = feat_num;
        }

        public void Save(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Create);

            stream.WriteByte((byte)dimension);
            stream.WriteByte((byte)0);

            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);

            stream.Write(BitConverter.GetBytes(frame_rate), 0, 4);

            for (long i = 0; i < this.samples; i++)
            {
                    for (int j = 0; j < this.dimension; j++)
                    {
                        stream.Write(BitConverter.GetBytes(this.data[i][j]), 0, 4);
                    }
            }

            stream.Close();


        }
        public void Save(string path, float thr)
        {
            FileStream stream = new FileStream(path, FileMode.Create);
                        
            stream.WriteByte((byte)dimension);
            stream.WriteByte((byte)0);

            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);
            stream.WriteByte((byte)0);

            stream.Write(BitConverter.GetBytes(frame_rate), 0, 4);

            for (long i = 0; i < this.samples; i++)
            {
                if (data[i][dimension-1]>thr)

                for (int j=0; j<this.dimension; j++)
                {
                    stream.Write(BitConverter.GetBytes(this.data[i][j]), 0, 4);
                }
            }

            stream.Close();


        }
        public static void SaveCSV(string[] path, string fname)
        {
            Data dt;
            System.IO.StreamWriter stream = new StreamWriter(fname, false);
            foreach (string s1 in path)
            {
                dt = new Data(s1, false);
                
                for (long i = 0; i < dt.samples; i++)
                {
                    for (int j = 0; j < dt.dimension; j++)
                    {
                        stream.Write(dt.data[i][j]);
                        stream.Write(';');
                    }
                    stream.WriteLine(dt.spkrID);
                }
                
            }
            stream.Close();
        }
        
	};
}
