using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToCoreInk
{
    public class ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }

    }
    public class ImageConverter
    {
        public ImageInfo ConvertToImageInfo(string imageFilename)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imageFilename))
            {
                byte[] output = null;
                int height = image.Height;
                int width = image.Width;
                int dataSize = (int)Math.Ceiling(width * height / 8.0);

                output = new byte[dataSize];

                byte tmp = 0;
                int counter = 0;
                int pointer = 0;

                for(int y = 0; y < height; y++)
                {
                    for(int x = 0; x < width; x++)
                    {
                        var pix = image[x, y];

                        // ITU-R Rec BT.601
                        float gray = 0.299f * pix.R + 0.587f * pix.G + 0.114f * pix.B;
                        tmp <<= 1;
                        if (gray > 127f) 
                        {
                            tmp = (byte)(tmp | 1); 
                        }
                        if(counter++ >= 7)
                        {
                            output[pointer++] = tmp;
                            tmp = 0;
                            counter = 0;
                        }
                    }
                }
                if(counter != 0)
                {
                    output[pointer] = (byte)(tmp << (8 - counter));
                }
                return new ImageInfo { Width = width, Height = height, Data = output, FileName = imageFilename };
            }
        }
        public (string, string[]) ConvertToString(string imageFileName)
        {
            List<string> line = new List<string>();

            var info = ConvertToImageInfo(imageFileName);
            string name = CreateDataName(Path.GetFileNameWithoutExtension(imageFileName));
            int dataSize = info.Data.Length;

            // header lines
            line.Add($"// width:{info.Width}, height:{info.Height}");
            line.Add($"const unsigned char {name}[{dataSize}]={{");

            // data
            StringBuilder builder = new StringBuilder();
            for(int i=0; i<dataSize; i++)
            {
                builder.Append("0x" + info.Data[i].ToString("x02"));
                if (i != dataSize - 1)
                {
                    builder.Append(",");
                }
                if( i % 16 == 15)
                {
                    line.Add(builder.ToString());
                    builder.Clear();
                }
            }

            // last line
            if (builder.Length > 0)
            {
                line.Add(builder.ToString()) ;
            }
            line.Add("};");
            return (name,line.ToArray());
        }
        private string CreateDataName(string str)
        {
            StringBuilder builder = new StringBuilder("image_");
            str = str.ToLower();
            for(int i=0; i<str.Length; i++)
            {
                if ("0123456789abcdefghijklmnopqrstuvwxyz_-".IndexOf(str[i]) >= 0)
                {
                    builder.Append(str[i]);
                }
            }
            return builder.ToString();
        }
    }
}
