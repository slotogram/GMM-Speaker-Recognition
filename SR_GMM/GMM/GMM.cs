using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
/*
 * Изначально decimal переопределено из double, возможно надо заменить(больше памяти тратится - 128 бит вместо 64) 
 * 
 * 
 */


namespace SR_GMM
{
    public class GMM
    {
        const double NUM_PI = 3.14159265358979323846; /* Math PI value.   */

		gauss[] mix;       /* Vector of gaussian distributions (above struct).    */
		int dimension; /* Number of dimensions of the gaussian mixture model. */
		int num;       /* Number of components of the gaussian mixture model. */
		float[] mcov;    /* Minimum allowed covariances to avoid singularities. */
		float Llh;      /* LogLikelihood after the training with EM algorithm. */
        /// <summary>
        /// Обучение GMM модели
        /// </summary>
        /// <param name="fnf">входной файл</param>
        /// <param name="fnm">файл с сохранением модели</param>
        /// <param name="fnr">json - возможно не надо</param>
        /// <param name="nmix">число компонентов смеси 2-524228</param>
        /// <param name="m">параметр, определяющий схожесть компонентов смеси 0.0-1.0</param>
        /// <param name="sigma">параметр, определяющий остановку обучения 0.0-1.0</param>
        /// <param name="imax">максимальное чилсло итераций обучения 1-10000</param>
        /// <param name="t">(необязательно)максимальное число потоков 1-256</param>
        /// <returns></returns>
        public int Train(Data feas,string fnf,string fnm, int nmix, double m = -1, double sigma = 0.01, int imax = 100,int t=0,string fnr = null)
        {
            const int INT_MIN = int.MinValue;
            int i,o,x=0;
            //t=Environment.ProcessorCount;
            t = 1;
	        float last=INT_MIN,llh;
    //Проверка аргументов и инициализация
            if(t>256||t<1) new Exception("Number of threads must be on the 1-256 range");
            if(nmix>524228||nmix<2) throw new Exception("Number of components must be on the 2-32768 range");
            if (imax > 10000 || imax < 1) throw new Exception("Number of iterations must be on the 1-10000 range");
            if (fnf.Length>0) x++;
            if (fnm.Length>0) x++;
            if(m>1.0||m<0.0) throw new Exception("Merge threshold must be on the 0.0-1.0 range");
            if(sigma>1.0||imax<0.0) throw new Exception("Sigma criterion must be on the 0.0-1.0 range");

	        if(x<2) new Exception("Не знаданы входной и выходной файлы"); /* Test if exists all the needed arguments. */

            //http://msdn.microsoft.com/ru-ru/library/h4732ks0.aspx
            ThreadPool.SetMaxThreads(t, 1);
            

	//workers *pool=workers_create(t);
    //data *feas=feas_load(fnf,pool);  /* Load the features from the specified disc file. s*/
    
	/*if(nmix==-1){
		if(m<0)m=0.95;
		nmix=sqrt(feas->samples/2);
	}*/
	//gmm *gmix=gmm_initialize(feas,nmix); /* Good GMM initialization using data.    */
           
	//fprintf(stdout,"Number of Components: %06i\n",gmix->num);
	for(o=1;o<=imax;o++){
		for(i=1;i<=imax;i++){
			llh=gmm_EMtrain(feas,t); /* Compute one iteration of EM algorithm.   */
			//fprintf(stdout,"Iteration: %05i    Improvement: %3i%c    LogLikelihood: %.3f\n",
			//	i,abs(round(-100*(llh-last)/last)),'%',llh); /* Show the EM results.  */
			if(last-llh>-sigma||float.IsNaN(last-llh))break; /* Break with sigma threshold. */
			last=llh;
		}
		x=num;
		if(m>=0){
			//gmm_merge(gmix,feas,m,pool);
			//fprintf(stdout,"Number of Components: %06i\n",gmix->num);
		}
		if(x==num)break;
		last=INT_MIN;
	}
	/*workers_finish(pool);
	feas_delete(feas);*/
	//if(fnr!=NULL)gmm_save_log(fnr,gmix);
	Gmm_init_classifier(); /* Pre-compute the non-data dependant part of classifier. */
	Gmm_save(fnm); /* Save the model with the pre-computed part for fast classify.  */
	//gmm_delete(gmix);
	return 0;

        }
        
        public int Adapt(Data feas, GMM world, string fnf, string fnm, int nmix, int reg=14, double m = -1, double sigma = 0.01, int imax = 1, int t = 0, string fnr = null)
        {
            const int INT_MIN = int.MinValue;
            int i, x = 0;
            t = 1;
            //t = Environment.ProcessorCount;
            float last = INT_MIN, llh;
            //Проверка аргументов и инициализация
            if (t > 256 || t < 1) new Exception("Number of threads must be on the 1-256 range");
            if (nmix > 524228 || nmix < 2) throw new Exception("Number of components must be on the 2-32768 range");
            if (imax > 10000 || imax < 1) throw new Exception("Number of iterations must be on the 1-10000 range");
            if (fnf.Length > 0) x++;
            if (fnm.Length > 0) x++;
            if (m > 1.0 || m < 0.0) throw new Exception("Merge threshold must be on the 0.0-1.0 range");
            if (sigma > 1.0 || imax < 0.0) throw new Exception("Sigma criterion must be on the 0.0-1.0 range");

            if (x < 2) new Exception("Не знаданы входной и выходной файлы"); /* Test if exists all the needed arguments. */

            //http://msdn.microsoft.com/ru-ru/library/h4732ks0.aspx
            ThreadPool.SetMaxThreads(t, 1);


            //workers *pool=workers_create(t);
            //data *feas=feas_load(fnf,pool);  /* Load the features from the specified disc file. s*/

            /*if(nmix==-1){
                if(m<0)m=0.95;
                nmix=sqrt(feas->samples/2);
            }*/
            //gmm *gmix=gmm_initialize(feas,nmix); /* Good GMM initialization using data.    */

            //fprintf(stdout,"Number of Components: %06i\n",gmix->num);

            //задаем число итераций
            
                for (i = 1; i <= imax; i++)
                {
                    llh = gmm_EMtrain(feas, t); /* Compute one iteration of EM algorithm.   */
                    if (last - llh > -sigma || float.IsNaN(last - llh)) break; /* Break with sigma threshold. */
                    last = llh;

                    //MapOccDep Algorithm
                    MapOccDep(world, reg, feas.samples);
                    //change other params to world model - 1-st variant, when we change stats to UBM every cycle
                    /*for (int j = 0; j < this.num; j++)
                    {
                        world.mix[j].dcov.CopyTo(this.mix[j].dcov, 0);
                        this.mix[j].prior = world.mix[j].prior;
                        //and reverse covariance matrix
                        if (i == imax)
                        for (int k = 0; k < this.dimension; k++)
                            this.mix[j].dcov[k] = 1 / this.mix[j].dcov[k];
                    } */                   

                    
                }

                //change other params to world model
                for (int j = 0; j < this.num; j++)
                {
                    world.mix[j].dcov.CopyTo(this.mix[j].dcov, 0);
                    this.mix[j].prior = world.mix[j].prior;
                    //and reverse covariance matrix
                        for (int k = 0; k < this.dimension; k++)
                            this.mix[j].dcov[k] = 1 / this.mix[j].dcov[k];
                }                

            Gmm_init_classifier(); /* Pre-compute the non-data dependant part of classifier. */
            Gmm_save(fnm); /* Save the model with the pre-computed part for fast classify.  */
            //gmm_delete(gmix);
            return 0;

        }

        void MapOccDep(GMM world, int reg, long frame_count)
        {
            float alpha = 0;
            for (int i = 0; i < this.num; i++)
            {
                alpha = (float)(Math.Pow(10,this.mix[i].prior) * frame_count);
                //alpha = this.mix[i].prior * frame_count;
                alpha = (alpha / (alpha + reg));
                for (int j = 0; j < this.dimension; j++)
                {
                    this.mix[i].mean[j] = ((1 - alpha) * world.getMean(i, j) + alpha * this.mix[i].mean[j]);
                }
            }
        } 
        /* Parallel implementation of the E Step of the EM algorithm. */
        void Thread_trainer(Data feas, long ini, long end)
        {
	//trainer *t=(trainer*)tdata; /* Get the data for the thread and alloc memory. */
	//gmm *gmix=t->gmix; data *feas=t->feas;
	float[] zval= new float[num], prob =new float [num];
	float[] mean=new float [num*dimension]; float llh=0;
	float[] dcov = new float[num*dimension]; float x,tz,rmean,max,mep=(float)Math.Log10(float.Epsilon);
	int[] tfreq=new int[num]; int j=0,m,c;
    long i, inc;
	for(i=ini;i<end;i++){
		max= -(float.MaxValue-1); c=-1;
		for(m=0;m<num;m++){ /* Compute expected class value of the sample. */
			prob[m]=mix[m].cgauss;
			for(j=0;j<dimension;j++){
				x=feas.data[i][j]-mix[m].mean[j];
				prob[m]-=(x*x)*mix[m].dcov[j];
			}
			prob[m]*=0.5f;
            if (max < prob[m]) max = prob[m]; c = m;
		}
		for(m=0,x=0;m<num;m++){ /* Do not use Viterbi aproximation. */
			rmean=prob[m]-max;
			if(rmean>mep)x+=(float)Math.Exp(rmean); /* Use machine epsilon to avoid make exp's. */
		}
		llh+=(rmean=(float)(max+Math.Log10(x))); tfreq[c]++;
		for(m=0;m<num;m++){ /* Accumulate counts of the sample in memory. */
			if((x=prob[m]-rmean)>mep){ /* Use machine epsilon to avoid this step. */
                zval[m] += (tz = (float)Math.Exp(x)); inc = m * j;
				for(j=0;j<dimension;j++){
					mean[inc+j]+=(x=tz*feas.data[i][j]);
					dcov[inc+j]+=x*feas.data[i][j];
				}
			}
		}
	}
	//workers_mutex_lock(t->mutex); /* Accumulate counts obtained to the mixture. */
	Llh+=llh; //!!!!
	for(m=0;m<num;m++){
		mix[m]._z+=zval[m]; inc=m*j;
		mix[m]._cfreq+=tfreq[m];
		for(j=0;j<dimension;j++){
			mix[m]._mean[j]+=mean[inc+j];
			mix[m]._dcov[j]+=dcov[inc+j];
		}
	}
	//workers_mutex_unlock(t->mutex);
	//free(zval); free(mean); free(tfreq);
}


        /* Perform one iteration of the EM algorithm with the data and the mixture indicated. */
        float gmm_EMtrain(Data feas, int t, bool init=true)
        {
	int m,i,n; float tz,x;
    long inc, ini, end;
    n = t;
    //ThreadPool.GetMaxThreads(out n,out n1);
	//workers_mutex *mutex=workers_mutex_create();
	//trainer *t=(trainer*)calloc(n,sizeof(trainer));
	/* Calculate expected value and accumulate the counts (E Step). */
	if (init) Gmm_init_classifier();
	for(m=0;m<num;m++)
		mix[m]._cfreq=0;
	inc=feas.samples/n;
	for(i=0;i<n;i++){ /* Set and launch the parallel training. */
		//t[i].feas=feas,t[i].gmix=gmix,t[i].mutex=mutex,t[i].ini=i*inc;
		//t[i].end=(i==n-1)?(feas.samples):((i+1)*inc);
        ini = i * inc;
        end = (i == n - 1) ? (feas.samples) : ((i + 1) * inc);
        Thread_trainer(feas, ini, end);
		//workers_addtask(pool,thread_trainer,(void*)&t[i]);
	}
	//workers_waitall(pool);
	//workers_mutex_delete(mutex);
	/* Estimate the new parameters of the Gaussian Mixture (M Step). */
	for(m=0;m<num;m++){
		mix[m].prior=(float)Math.Log10((tz=mix[m]._z)/feas.samples);
		for(i=0;i<dimension;i++){
			mix[m].mean[i]=(x=mix[m]._mean[i]/tz);
			mix[m].dcov[i]=(mix[m]._dcov[i]/tz)-(x*x);
			if(mix[m].dcov[i]<mcov[i]) /* Smoothing covariances. */
				mix[m].dcov[i]=mcov[i];
		}
	}
	//free(t);
	return Llh/feas.samples;
        }

        public float Classify(string feat, int t = 1, string log = null, GMM w = null)
        {
            if (t > 256 || t < 1) new Exception("Number of threads must be on the 1-256 range");

            int i, n; float result = 0;

            Data fd = new Data(feat);
            //ThreadPool.GetMaxThreads(out n, out n1);
            long inc = fd.samples / t, ini, end;
            n = t;
            float[] res = new float[t];
            //workers_mutex *mutex=workers_mutex_create();
            //classifier *t=(classifier*)calloc(n,sizeof(classifier));
            //number *flag=(number*)calloc(1,sizeof(number));
            //FILE *f=fopen(filename,"w");
            //if(!f)fprintf(stderr,"Error: Can not write to %s file.\n",filename),exit(1);
            //fprintf(f,"{\n\t\"samples\": %i,\n\t\"classes\": %i,",feas->samples,gmix->num);
            //fprintf(f,"\n\t\"samples_results\": [ ");
            for (i = 0; i < num; i++)
                mix[i]._cfreq = 0;
            for (i = 0; i < t; i++)
            { /* Set and launch the parallel classify. */
                ini = i * inc;
                end = (i == n - 1) ? (fd.samples) : ((i + 1) * inc);
                //workers_addtask(pool,thread_classifier,(void*)&t[i]);
                Thread_classifier(fd,ref res, i, ini, end, w);
            }
            //workers_waitall(pool); /* Wait to the end of the parallel classify. */
            for (i = 0; i < n; i++)
                result += res[i];
            /*workers_mutex_delete(mutex);
            fprintf(f,"\n\t],\n\t\"mixture_occupation\": [ %i",gmix->mix[0]._cfreq);
            for(i=1;i<gmix->num;i++)
                fprintf(f,", %i",gmix->mix[i]._cfreq);
            fprintf(f," ],\n\t\"global_score\": %.10f\n}",result);
            fclose(f); free(t); free(flag); */
            return result / fd.samples;

        }
        public float Classify(Data fd, int t = 1, string log = null, GMM w = null)
        {
            if (t > 256 || t < 1) new Exception("Number of threads must be on the 1-256 range");

            int i, n; float result = 0;

            long inc = fd.samples / t, ini, end;
            n = t;
            float[] res = new float[t];
       
            for (i = 0; i < num; i++)
                mix[i]._cfreq = 0;
            for (i = 0; i < t; i++)
            { /* Set and launch the parallel classify. */
                ini = i * inc;
                end = (i == n - 1) ? (fd.samples) : ((i + 1) * inc);
                Thread_classifier(fd, ref res, i, ini, end, w);
            }
           for (i = 0; i < n; i++)
                result += res[i];
            
            return result / fd.samples;

        }

        void Thread_classifier(Data feas, ref float[] res, int i1, long ini, long end, GMM w)
        {
            
	float x,max1,max2,prob; int m,j,c;
            long i;
	//char *buf1=(char*)calloc(s=25*t->gmix->num,sizeof(char));
	//char *buf2=(char*)calloc(s=25*t->gmix->num,sizeof(char)),*bufi;
	for(i=ini;i<end;i++){
		if(w!=null){ /* If the world model is defined, use it. */
			max2=-(float.MaxValue-1);
			for(m=0;m<w.num;m++){
				prob=w.mix[m].cgauss;
				if(prob<max2)continue; /* Speed-up the classifier. */
				for(j=0;j<w.dimension;j++){
					x=feas.data[i][j]-w.mix[m].mean[j];
					prob-=(x*x)*w.mix[m].dcov[j];
				}
				if(max2<prob)max2=prob;
			}
		}else max2=0;
		/* Separated calculation of the first component to speed-up. */
		max1=mix[0].cgauss;c=0;
		for(j=0;j<dimension;j++){
			x=feas.data[i][j]-mix[0].mean[j];
			max1-=(x*x)*mix[0].dcov[j];
		}
		//snprintf(buf1,s,"%.10f",(max1-max2)*0.5);
		for(m=1;m<num;m++){
			prob=mix[m].cgauss; /* The precalculated non-data dependant part. */
			for(j=0;j<dimension;j++){
				x=feas.data[i][j]-mix[m].mean[j];
				prob-=(x*x)*mix[m].dcov[j];
			}
            if (max1 < prob) { max1 = prob; c = m; } /* ??? Fast classifier using Viterbi aproximation. */
			//bufi=buf1; buf1=buf2; buf2=bufi;
			//snprintf(buf1,s,"%s, %.10f",buf2,(prob-max2)*0.5);
		}
		res[i1]+=(max1-max2)*0.5f;
		//workers_mutex_lock(t->mutex); /* Write the classifier log on the jSON file. */
		mix[c]._cfreq++;
		/*if(t->flag[0]==0){
			fprintf(t->f,"\n\t\t{ \"sample\": %i, \"lprob\": [ %s ], \"class\": %i }",i,buf1,c);
			t->flag[0]=1;
		}else fprintf(t->f,",\n\t\t{ \"sample\": %i, \"lprob\": [ %s ], \"class\": %i }",i,buf1,c);
		workers_mutex_unlock(t->mutex);*/
	}
	
        }

        public void Gmm_save(string filename)
        {
           
            
            BinaryWriter stream = new BinaryWriter(File.Open(filename,FileMode.Create));
            //StreamWriter stream = new StreamWriter(filename);
            
	        int m;
            stream.Write(dimension); 
            stream.Write(num);
 
            for (int i=0;i<dimension;i++)
            stream.Write(mcov[i]);
            
	        for(m=0;m<num;m++){
                stream.Write(mix[m].prior);
                stream.Write(mix[m].cgauss);
                for (int i = 0; i < dimension; i++)
                {
                    stream.Write(mix[m].mean[i]);
                    stream.Write(mix[m].dcov[i]);
                }
	        }
            stream.Close();
        }

        public GMM(string filename, List<int> num_list = null)
        {

            BinaryReader stream = new BinaryReader(File.Open(filename, FileMode.Open));
            //StreamWriter stream = new StreamWriter(filename);
            if (num_list == null)
            {
                int m;
                dimension = stream.ReadInt32();
                num = stream.ReadInt32();
                mcov = new float[dimension];
                mix = new gauss[num];

                for (int i = 0; i < dimension; i++)
                    mcov[i] = stream.ReadSingle();

                for (m = 0; m < num; m++)
                {
                    mix[m] = new gauss(dimension);
                    mix[m].prior = stream.ReadSingle();
                    mix[m].cgauss = stream.ReadSingle();
                    for (int i = 0; i < dimension; i++)
                    {
                        mix[m].mean[i] = stream.ReadSingle();
                        mix[m].dcov[i] = stream.ReadSingle();
                    }
                }
            }
            else loadCutted(stream,num_list);
            stream.Close();
        }

        private void loadCutted(BinaryReader stream, List<int> feat_list)
        {
            int m;
            dimension = stream.ReadInt32();
            num = stream.ReadInt32();
           
            mix = new gauss[num];

            int new_dimension = feat_list.Count;

            if (dimension < new_dimension - 1) throw new Exception("Not enough features in GMM stream");
            if (feat_list[new_dimension - 1] == 0) new_dimension--; //убираем выравниватель
            int counter = 0;


            mcov = new float[new_dimension];
            for (int i = 0; i < dimension; i++)
            {
                if (i == feat_list[counter]) { mcov[counter] = stream.ReadSingle(); counter++; }
                else stream.ReadSingle();
            }
            
            for (m = 0; m < num; m++)
            {
                mix[m] = new gauss(new_dimension);
                mix[m].prior = stream.ReadSingle();
                mix[m].cgauss = stream.ReadSingle();
                counter = 0;
                for (int i = 0; i < dimension; i++)
                {
                    if (i == feat_list[counter]) { mix[m].mean[counter] = stream.ReadSingle(); mix[m].dcov[counter] = stream.ReadSingle(); counter++; }
                    else { stream.ReadSingle(); stream.ReadSingle();}                   
                }
            }
            dimension = new_dimension;
            
        }

        void Gmm_init_classifier()
        {
            float cache = (float)(dimension * Math.Log10(2 * NUM_PI));
            int m, j; Llh = 0;
            for (m = 0; m < num; m++)
            {
                mix[m].cgauss = mix[m]._z = 0;
                for (j = 0; j < dimension; j++)
                {
                    mix[m].cgauss += (float)Math.Log10(mix[m].dcov[j]);
                    mix[m].dcov[j] = 1 / mix[m].dcov[j];
                    mix[m]._mean[j] = mix[m]._dcov[j] = 0; /* Caches to 0. */
                }
                mix[m].cgauss = (2 * mix[m].prior - (mix[m].cgauss + cache));
            }
        }

        public GMM (int n, int d, Data feas)
        {
            this.mcov = new float[dimension=d];
            this.mix = new gauss[num=n];
            for (int ii = 0; ii < n; ii++)
            {
                mix[ii] = new gauss(d);
            }

            long i, j, k, b = feas.samples / n, bc = 0;
            float x = (float)(1.0 / num);
            /* Initialize the first Gaussian with maximum likelihood. */
            mix[0].prior = (float)Math.Log10(x);
            for (j = 0; j < dimension; j++)
            {
                mix[0].dcov[j] = x * feas.variance[j];
                mcov[j] = (float)0.001 * mix[0].dcov[j];
            }
            /* Disturb all the means creating C blocks of samples. */
            for (i = num - 1; i >= 0; i--, bc += b)
            {
                mix[i].prior = mix[0].prior;
                for (j = bc, x = 0; j < bc + b; j++) /* Compute the mean of a group of samples. */
                    for (k = 0; k < dimension; k++)
                        mix[i]._mean[k] += feas.data[j][k];
                for (k = 0; k < dimension; k++)
                { /* Disturbe the sample mean for each mixture. */
                    mix[i].mean[k] = (float)((feas.mean[k] * 0.9) + (0.1 * mix[i]._mean[k] / b));
                    mix[i].dcov[k] = mix[0].dcov[k];
                }
            }
        }
        public GMM (GMM gmm, bool swap_cov = false)
        {
            this.dimension = gmm.dimension;
            this.num = gmm.num;
            this.Llh = gmm.Llh;
            this.mcov = new float[dimension];
            gmm.mcov.CopyTo(this.mcov, 0);
            this.mix = new gauss[num];
            for (int ii = 0; ii < num; ii++)
            {
                mix[ii] = new gauss(dimension);
                mix[ii].cgauss = gmm.mix[ii].cgauss;
                gmm.mix[ii].dcov.CopyTo(this.mix[ii].dcov, 0);
                if (swap_cov)
                    for (int j = 0; j < dimension; j++)
                        mix[ii].dcov[j] = 1 / mix[ii].dcov[j];
                gmm.mix[ii].mean.CopyTo(this.mix[ii].mean, 0);
                mix[ii].prior = gmm.mix[ii].prior;
                

            }
            

            
            
            


        }
        public float getMean(int n, int coef)
        {
            return this.mix[n].mean[coef];
        }
        public int getNumMix()
        {
            return this.num;
        }
    }
}
