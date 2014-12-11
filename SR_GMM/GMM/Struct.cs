using System;
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
        public Data()
        {

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
        public Data(List<string> Paths)
        {
            
            //LoadSingle(Paths);
            List <float[][]> lst = new List<float[][]>();
            Data d = new Data();
            long samp = 0;
            foreach (string s in Paths)
            {
                d = new Data(s, false);
                lst.Add(d.data);
                samp += d.samples;
            }
            //удилать
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

            //
            data = lst[0];
            dimension = d.dimension;
            
            for (int i = 1; i < lst.Count; i++) { data = data.Concat(lst[i]).ToArray(); }
            samples = samp;
          
            CalcMean();

            //вызов train\

            //GMM g = new GMM(24, dimension, feas );
            //g.Train(feas, "asdas", "model.txt",24, 0.95,0.01,100,1);

        }
        /// <summary>
        /// Создает Data из списка путей к mcc файлам Paths длиной Length
        /// </summary>
        /// <param name="Paths"></param>
        /// <param name="Length"></param>
        public Data(List<string> Paths, int Length)
        {

            //LoadSingle(Paths);
            List<float[][]> lst = new List<float[][]>();
            Data d = new Data();
            long samp = 0;
            foreach (string s in Paths)
            {
                d = new Data(s, false);
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

        public static List<Data> JoinDataWLen(List<string> Paths, int Length)
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
                d = new Data(s, false); //поменял , можно брать только одного диктора данные
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

        public Data(string Path, bool needMean)
        {
            LoadSingle(Path);

            if (needMean) CalcMean();
            
                //взять имя файла
            string tmp = System.IO.Path.GetFileNameWithoutExtension(Path);
                //найти пробел
                   
                //вырезать с начала до пробела
            tmp=tmp.Substring(0, tmp.IndexOf(' '));
            int ind = 0;
                //toInt
            int.TryParse(tmp, out ind);
            this.spkrID = ind;
            

        }

        public Data(string Path)
        {
            LoadSingle(Path);

            CalcMean();
            
           
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
	};
}
