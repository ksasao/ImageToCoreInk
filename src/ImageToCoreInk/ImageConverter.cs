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
        Rgba32[] colors = { new Rgba32(255, 255, 255), new Rgba32(0, 0, 0) };
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

                ToGrayScale(image);
                ToDither(image);

                // image.SaveAsPng("output.png");

                for(int y = 0; y < height; y++)
                {
                    for(int x = 0; x < width; x++)
                    {
                        var pix = image[x, y];
                        tmp <<= 1;
                        if (pix.R > 127f) 
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

        private Color GetNearest(Rgba32 inputColor, Rgba32[] colors)
        {
            int distance = int.MaxValue;
            int index = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                Rgba32 c = colors[i];
                int d = (inputColor.R - c.R) * (inputColor.R - c.R)
                      + (inputColor.G - c.G) * (inputColor.G - c.G) 
                      + (inputColor.B - c.B) * (inputColor.B - c.B);
                if (distance > d)
                {
                    distance = d;
                    index = i;
                }
            }
            return colors[index];
        }

        private void ToGrayScale(Image<Rgba32> image)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pix = image[x, y];
                    // ITU-R Rec BT.601
                    byte gray = (byte)(0.299f * pix.R + 0.587f * pix.G + 0.114f * pix.B);
                    image[x, y] = new Rgba32(gray, gray, gray);
                }
            }
        }
        private byte Clip(int val)
        {
            return (byte)(val > 255 ? 255 : (val < 0 ? 0 : val));
        }
        private void ToDither(Image<Rgba32> image)
        {
            int w = image.Width;
            int h = image.Height;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int xx = ((y & 1) >= 0) ? x : w - x - 1;
                    Rgba32 rgba = image[x, y];
                    Rgba32 c = GetNearest(rgba, colors);
                    int dR = rgba.R - c.R;
                    int dG = rgba.G - c.G;
                    int dB = rgba.B - c.B;
                    image[x, y] = new Rgba32(c.R, c.G, c.B);

                    if (xx < w - 1)
                    {
                        // right
                        image[x + 1, y] = new Rgba32(
                            Clip(image[x + 1, y].R + dR * 7 / 16),
                            Clip(image[x + 1, y].G + dG * 7 / 16),
                            Clip(image[x + 1, y].B + dB * 7 / 16));
                        if (y < h - 1)
                        {
                            if (xx > 0)
                            {
                                // left-down
                                image[x - 1, y + 1] = new Rgba32(
                                    Clip(image[x - 1, y + 1].R + dR * 3 / 16),
                                    Clip(image[x - 1, y + 1].G + dG * 3 / 16),
                                    Clip(image[x - 1, y + 1].B + dB * 3 / 16));
                            }
                            // down
                            image[x, y + 1] = new Rgba32(
                                Clip(image[x, y + 1].R + dR * 5 / 16),
                                Clip(image[x, y + 1].G + dG * 5 / 16),
                                Clip(image[x, y + 1].B + dB * 5 / 16));
                            // right-down
                            image[x + 1, y + 1] = new Rgba32(
                                Clip(image[x + 1, y + 1].R + dR * 1 / 16),
                                Clip(image[x + 1, y + 1].G + dG * 1 / 16),
                                Clip(image[x + 1, y + 1].B + dB * 1 / 16));
                        }
                    }
                }
            }
        }
    }
}
