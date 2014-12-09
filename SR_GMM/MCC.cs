using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SR_GMM
{
    class MCC
    {
        bool gotE, gotD, gotA, meanR , supE, normV;
        float[][] m;

        public float[][] data;    /* The data matrix (samples*dimension).  */
        public float[] mean;     /* The mean by dimension of the samples. */
        public float[] variance; /* Variance of all the loaded samples.   */
        public long samples;    /* Samples number of the overall data.   */
        public int dimension;  /* Dimension number of the data loaded.  */

        //ushort dimension;

        public MCC(string Path)
        {
            FileStream stream = new FileStream(Path, FileMode.Open);

            long len = stream.Length - 10;
            byte Lb = (byte)stream.ReadByte();
            byte Hb = (byte)stream.ReadByte();
            dimension = (int)((ushort)(Hb * byte.MaxValue + Lb));
           
            long n = (long)(len / (dimension*4));

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
            
            bool gotE = false, gotD = false, gotA = false;

            if ((flag[0] & 0x01) != 0) gotE = true; 
            if ((flag[0] & 0x02) != 0) meanR = true;
            if ((flag[0] & 0x04) != 0) supE = true;
            if ((flag[0] & 0x08) != 0) gotD = true; 
            if ((flag[0] & 0x10) != 0) gotA = true; 
            if ((flag[0] & 0x20) != 0) normV = true;

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            float fr_rate = BitConverter.ToSingle(buf, 0);
            
            //------------------------------------
            // Можно оптимизировать, если убрать все условия и сделать отдельные циклы, пока не надо
            //------------------------------------
            m = new float[n][];
            
            for (long i = 0; i < n; i++)
            {
                m[i] = new float[dimension];
                for (int j = 0; j < dimension; j++)
                {
                    m[i] [j] = ReadFloat(stream);
                }
                
            }

            stream.Close();
            Data feas = new Data(m, n, dimension);
            //вызов train\
            
            GMM g = new GMM(24, dimension, feas );
            g.Train(feas, "asdas", "model.txt",24, 0.95,0.01,100,1);

        }

        public float ReadFloat(FileStream stream)
        {

            byte[] buf = new byte[4];

            stream.Read(buf, 0, 4);

            return BitConverter.ToSingle(buf, 0);
        }

       
    }
}
