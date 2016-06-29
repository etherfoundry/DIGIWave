using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DIGIWave
{
    class StereoWaveFile
    {
        const int HEADER_SIZE = 44;
        const long READ_AHEAD = 100000;

        string ChunkId { get; set; } 
        int ChunkSize  { get; set; }
        string Format { get; set; }
        string Subchunk1ID { get; set; }
        int Subchunk1Size { get; set; }
        int AudioFormat { get; set; }
        int NumChannels { get; set; }
        public int SampleRate { get; private set; }
        int ByteRate { get; set; }
        int BlockAlign { get; set; }
        int BitsPerSample { get; set; }
        string Subchunk2ID { get; set; }
        int Subchunk2Size { get; set; } 

        FileStream fs;
        long fs_length = -1;

        long currentSample;
        byte[] sampleBuffer;
        Queue<Tuple<int, int>> sampleReadAhead = new Queue<Tuple<int, int>>(5000000);
        int sampleOffset;
        public bool EOF;

        public void readSample(out int leftSample, out int rightSample)
        {
            if(sampleReadAhead.Count == 0)
            {
                int readsize = (int)Math.Min((long)this.BlockAlign * READ_AHEAD, getFileLength() - fs.Position);
                if(sampleBuffer.Length != readsize)
                {
                    sampleBuffer = new byte[readsize];
                }
                fs.Read(sampleBuffer, 0, readsize);

                for (int i = 0; i < readsize-1; i += 4)
                {
                    int ls = BitConverter.ToInt16(sampleBuffer, i);
                    int rs = BitConverter.ToInt16(sampleBuffer, i+2);
                    sampleReadAhead.Enqueue(new Tuple<int, int>(ls, rs));
                }
            }
            
            var samp = sampleReadAhead.Dequeue();
            leftSample = samp.Item1;
            rightSample = samp.Item2;

            if (fs.Position > getFileLength() - 2)
            {
                EOF = true;
            }
        }

        public long getFilePosition()
        {
            return fs.Position;
        }

        public long getFileLength()
        {
            if (this.fs_length == -1)
                this.fs_length = fs.Length;
            return this.fs_length;
        }

        public void readSample(int position, out int leftSample, out int rightSample)
        {
            fs.Position = GetSampleOffset(position);
            sampleReadAhead.Clear();
            readSample(out leftSample, out rightSample);
        }

        /// <summary>
        /// Zero Indexed sample
        /// </summary>
        /// <param name="sampleNumber"></param>
        /// <returns></returns>
        protected int GetSampleOffset(int sampleNumber)
        {
            return HEADER_SIZE + (sampleNumber * BlockAlign);
        }

        public static StereoWaveFile OpenWave(string filename)
        {
            var wav = new StereoWaveFile();
            var buffer = new byte[HEADER_SIZE];
            wav.fs = File.OpenRead(filename);

            // Read the WAV header.
            wav.fs.Read(buffer, 0, 44);
            int offset = 0;
            byte[] arr2b = new byte[2];
            byte[] arr4b = new byte[4];

            wav.ChunkId = Encoding.ASCII.GetString(buffer, offset, 4);
            wav.ChunkSize = BitConverter.ToInt32(buffer, offset += 4);
            wav.Format = Encoding.ASCII.GetString(buffer, offset += 4, 4);
            wav.Subchunk1ID = Encoding.ASCII.GetString(buffer, offset += 4, 4);
            wav.Subchunk1Size = BitConverter.ToInt32(buffer, offset += 4);      // 16 for PCM
            wav.AudioFormat = BitConverter.ToInt16(buffer, offset += 4);      // PCM = 1 (Linear Quantization);
            wav.NumChannels = BitConverter.ToInt16(buffer, offset += 2);
            wav.SampleRate = BitConverter.ToInt32(buffer, offset += 2);
            wav.ByteRate = BitConverter.ToInt32(buffer, offset += 4);         // SampleRate * NumChannels * BitsPerSample/8
            wav.BlockAlign = BitConverter.ToInt16(buffer, offset += 4);       // NumChannels * BitsPerSample/8
            wav.BitsPerSample = BitConverter.ToInt16(buffer, offset += 2);
            wav.Subchunk2ID = Encoding.ASCII.GetString(buffer, offset += 2, 4);
            offset += 4;
            wav.Subchunk2Size = BitConverter.ToInt32(buffer, offset);

            wav.sampleBuffer = new byte[wav.BlockAlign];

            return wav;
        }
    }
}
