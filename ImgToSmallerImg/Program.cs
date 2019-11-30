using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace ImgToSmallerImg
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var texts = new string[2];
            bool first, second;
            do
            {
                Console.Clear();
                Console.WriteLine("---- Binning image ----");
                Console.WriteLine();

                Console.WriteLine("Enter path to file:");
                texts[0] = Console.ReadLine();
                first = CheckPath(texts[0]);

                Console.WriteLine();
                Console.WriteLine("Output size:");

                texts[1] = Console.ReadLine();
                second = int.TryParse(texts[1], out var result);

                Console.Clear();
            } while (!first || !second);

            var image = Image.FromFile(texts[0]);
            var bitmapSize = int.Parse(texts[1]);
            var bitmap = (Bitmap) image;

            var sectorSizeH = image.Height / bitmapSize;
            var sectorSizeW = image.Width / bitmapSize;

            var newBitmap = new Bitmap(bitmapSize, bitmapSize);

            for (var i = 0; i < newBitmap.Height; i++)
            for (var j = 0; j < newBitmap.Width; j++)
                newBitmap.SetPixel(i, j, GetAverageColor(bitmap, sectorSizeH * j, sectorSizeH * j + sectorSizeH,
                    sectorSizeW * i, sectorSizeW * i + sectorSizeW));

            newBitmap.Save("Binned.jpeg", ImageFormat.Bmp);
        }

        private static bool CheckPath(string text)
        {
            text = Regex.Replace(text, "/", "\\");
            return File.Exists(text);
        }

        private static Color GetAverageColor(Bitmap bitmap, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            ulong red = 0, green = 0, blue = 0, alpha = 0, pixNum = 1;

            for (var i = fromWidth; i < toWidth; i++)
            for (var j = fromHeight; j < toHeight; j++)
            {
                var pixel = bitmap.GetPixel(i, j);
                red += pixel.R;
                green += pixel.G;
                blue += pixel.B;
                alpha += pixel.A;

                pixNum++;
            }

            red /= pixNum;
            green /= pixNum;
            blue /= pixNum;
            alpha /= pixNum;

            return Color.FromArgb((int) alpha, (int) red, (int) green, (int) blue);
        }
    }
}