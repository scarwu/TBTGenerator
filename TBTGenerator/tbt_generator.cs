using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace TBTGenerator
{
    class tbt_generator
    {
        private string src_folder;
        private string dest_folder;

        private string[] fileList;

        private RichTextBox logList;
        private ProgressBar percent;

        private int[] header, footer, flag;
        private int size;

        /**
         * Constructure
         */
        public tbt_generator(RichTextBox log, ProgressBar bar)
        {
            int[] header = {
		        0x00, 0x00, 0x21, 0x21
	        };
	        int[] footer = {
		        0x28, 0x30, 0x30, 0x2D, 0x32,
		        0x35, 0x2D, 0x32, 0x32,	0x2D,
		        0x42, 0x44, 0x2D, 0x36, 0x42,
		        0x2D, 0x39, 0x36, 0x2F, 0x32,
		        0x30, 0x31, 0x32, 0x2D, 0x30,
		        0x35, 0x2D, 0x31, 0x39, 0x20,
		        0x30, 0x33, 0x3A, 0x33, 0x34,
		        0x3A, 0x34, 0x39, 0x29
	        };
	        int[] flag = {
		        0x01, 0x00, 0x21
	        };

            this.logList = log;
            this.percent = bar;
            this.header = header;
            this.footer = footer;
            this.flag = flag;
            this.size = 33;
        }

        /**
         * Set Input/Output Folder
         */
        public void setIOFloder(string src, string dest)
        {
            this.src_folder = src;
            this.dest_folder = dest;
        }

        /**
         * Read Folder
         */
        private bool readFolderList(string folderName)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderName);
                FileInfo[] sortList = dirInfo.GetFiles();
                string[] tempList = new string[sortList.Length];

                for (int i = 0; i < sortList.Length; i++)
                {
                    tempList[i] = sortList[i].Name.ToString();
                }

                fileList = tempList;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /**
         * RGB Color Convert to TBT Color
         */
        private int[] RGBConvertor(int R, int G, int B)
        {
            int[] buffer = {0, 0};

            // 5bit Mask
            R = (R >> 3) & 0x1F;
            G = (G >> 3) & 0x1F;
            B = (B >> 3) & 0x1F;

            // Lower Buffer
            buffer[1] |= R << 2;
            buffer[1] |= G >> 3;

            // Higher Buffer
            buffer[0] |= G << 5;
            buffer[0] |= B;

            // 8bit Mask
            buffer[0] &= 0xFF;
            buffer[1] &= 0xFF;

            return buffer;
        }

        /**
         * 
         */
        private void generate()
        {
            Regex rgx = new Regex(@"\.\w+$");

            percent.Value = 0;
            percent.Maximum = this.fileList.Length;
            for (int i = 0; i < this.fileList.Length; i++)
            {
                try
                {
                    string outputFile = rgx.Replace(this.fileList[i], @".tbt");

                    // Image Setting
                    Bitmap src = new Bitmap(this.src_folder + "\\" + this.fileList[i]);
                    Bitmap srcResize = new Bitmap(this.size, this.size);

                    Graphics graphic = Graphics.FromImage((Image)srcResize);
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.DrawImage(src, 0, 0, this.size, this.size);
                    graphic.Dispose();

                    BitmapData ImgData = srcResize.LockBits(
                        new Rectangle(0, 0, this.size, this.size),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb
                    );

                    int nOffset = ImgData.Stride - this.size * 3;
                    IntPtr scan = ImgData.Scan0;

                    // TBT Handler
                    Stream sw = new FileStream(this.dest_folder + "\\" + outputFile, FileMode.Create);
                    BinaryWriter output = new BinaryWriter(sw);

                    logList.Text += "OUTPUT: " + outputFile + "\n";

                    // Write header
                    for (int offset = 0; offset < header.Length; offset++)
                    {
                        output.Write(Convert.ToByte(Convert.ToChar(this.header[offset])));
                    }

                    for (int rows = 0; rows < this.size; rows++)
                    {
                        // Write flag
                        for (int offset = 0; offset < flag.Length; offset++)
                        {
                            output.Write(Convert.ToByte(Convert.ToChar(this.flag[offset])));
                        }

                        unsafe
                        {
                            byte* point = (byte*)(void*)scan;
                            // Write color
                            for (int cols = 0; cols < this.size; cols++)
                            {
                                int index = rows * (this.size * 3 + nOffset) + (cols * 3);
                                int[] buffer = this.RGBConvertor(
                                    point[index + 2],
                                    point[index + 1],
                                    point[index + 0]
                                );

                                // Write Color Pixel
                                output.Write(Convert.ToByte(Convert.ToChar(buffer[0])));
                                output.Write(Convert.ToByte(Convert.ToChar(buffer[1])));
                            }
                        }
                    }

                    // Write footer
                    for (int offset = 0; offset < footer.Length; offset++)
                    {
                        output.Write(Convert.ToByte(Convert.ToChar(this.footer[offset])));
                    }

                    srcResize.UnlockBits(ImgData);
                    output.Close();
                    percent.Value++;
                }
                catch (Exception ex)
                {
                    // logList.Text += ex.ToString() + "\n\n";
                    percent.Value++;
                    continue;
                }
            }
        }

        /**
         * Run Generator
         */
        public void run()
        {
            if (this.readFolderList(this.src_folder))
            {
                logList.Text += "Input Folder: " + src_folder + "\n";
                logList.Text += "Output Folder: " + dest_folder + "\n\n";
                logList.Text += "Start Convert Image.\n";
                this.generate();
                logList.Text += "\nDone.\n";
            }
        }
    }
}