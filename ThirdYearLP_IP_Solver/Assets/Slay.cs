using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ThirdYearLP_IP_Solver.Assets
{
   
    public class Slay
    {
 
        private string filePath;

        private IWavePlayer waveOutDevice;
        private AudioFileReader audioFileReader;

        public Slay(string path) {
            filePath = path;

            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
                waveOutDevice.Dispose();
                audioFileReader.Dispose();
            }
            waveOutDevice = new WaveOut();
            audioFileReader = new AudioFileReader(filePath);
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();

        }
       
      
       
        public static void PlayMusic(string filePath)
        {
           

        }
    }

}
