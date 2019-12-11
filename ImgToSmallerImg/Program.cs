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
            TryAgain:

            // setting default values
            string text = "3", path = "D:\\a.jpg";

            // if you want to set them in cli comment this goto
            goto Skip;


            // choose binning mode
            Console.WriteLine("Choose mode:");
            Console.WriteLine("1. 2x2");
            Console.WriteLine("2. 3x3");
            Console.WriteLine("3. 4x4");
            text = Console.ReadLine();

            // if everything is alright pass, else do again
            if (!int.TryParse(text, out _))
            {
                Console.Clear();
                goto TryAgain;
            }

            // get file path
            Console.WriteLine("Path to file:");
            path = Console.ReadLine();

            // if everything is alright pass, else do again
            if (!CheckPath(path))
            {
                Console.Clear();
                goto TryAgain;
            }

            Skip:
            var binningNum = int.Parse(text) + 1;

            var image = Image.FromFile(path);
            var bitmapSizeHeight = image.Height / binningNum;
            var bitmapSizeWidth = image.Width / binningNum;
            var bitmap = (Bitmap) image;


            var newBitmap = new Bitmap(bitmapSizeWidth, bitmapSizeHeight);

            for (var i = 0; i < newBitmap.Width - 1; i++)
            for (var j = 0; j < newBitmap.Height - 1; j++)
                newBitmap.SetPixel(i, j, GetAverageColorUnsafe(bitmap, binningNum * j, binningNum * j + binningNum,
                    binningNum * i, binningNum * i + binningNum));

            var finalImage = newBitmap;

            var format = new ImageFormat(image.RawFormat.Guid);

            finalImage.Save("Binned" + Path.GetExtension(path), format);
        }

        private static bool CheckPath(string text)
        {
            text = Regex.Replace(text, "/", "\\");
            return File.Exists(text);
        }

        private static Color GetAverageColor(Bitmap bitmap, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            double red = 0, green = 0, blue = 0, alpha = 0, pixNum = 1;

            for (var i = fromWidth; i < toWidth; i++)
            for (var j = fromHeight; j < toHeight; j++)
            {
                var pixel = bitmap.GetPixel(i, j);
                red += Math.Pow(pixel.R, 2);
                green += Math.Pow(pixel.G, 2);
                blue += Math.Pow(pixel.B, 2);
                alpha += Math.Pow(pixel.A, 2);

                pixNum++;
            }

            pixNum--;

            red = red / pixNum;
            red = Math.Sqrt(red);

            green = green / pixNum;
            green = Math.Sqrt(green);

            blue = blue / pixNum;
            blue = Math.Sqrt(blue);

            alpha = alpha / pixNum;
            alpha = Math.Sqrt(alpha);

            return Color.FromArgb((int) alpha, (int) red, (int) green, (int) blue);
        }

        private static Color GetAverageColorUnsafe(Bitmap bm, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            long[] totals = {0, 0, 0};
            var bppModifier =
                bm.PixelFormat == PixelFormat.Format24bppRgb
                    ? 3
                    : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            var srcData = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            unsafe
            {
                var p = (byte*) (void*) scan0;

                for (var y = fromHeight; y < toHeight; y++)
                for (var x = fromWidth; x < toWidth; x++)
                {
                    var idx = y * stride + x * bppModifier;
                    int red = p[idx + 2];
                    int green = p[idx + 1];
                    int blue = p[idx];
                    totals[2] += red;
                    totals[1] += green;
                    totals[0] += blue;
                }
            }

            var count = (toWidth - fromWidth) * (toHeight - fromHeight);
            var avgR = (int) (totals[2] / count);
            var avgG = (int) (totals[1] / count);
            var avgB = (int) (totals[0] / count);

            bm.UnlockBits(srcData);

            return Color.FromArgb(avgR, avgG, avgB);
        }
    }
}