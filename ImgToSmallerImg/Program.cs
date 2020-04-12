using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ImgToSmallerImg
{
    internal static class Program
    {
        // Setting default values
        private static int _binningNum = 4;
        private static string _input;
        private static string _path = "a.jpg";
        private static readonly string[] Extensions =
            {"jpg", "jpeg", "jpe", "jif", "jfif", "jfi", "png", "webp", "bmp", "dib"};
        
        public static void Main()
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Transform single file or transform all \nfrom current directory?");
                Console.WriteLine(" S or A");
                _input = Console.ReadLine();
                _input = _input?.ToLower();
                // Break when input is y or n
            } while (_input != "s" && _input != "a");

            if (_input == "s")
                ManualTransform();
            else
                AutomatedTransform();
        }

        private static void AutomatedTransform()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());
            var images = new List<string>();

            for (var i = 0; i < files.Length; i++)
            {
                var temp = files[i].Split('\\');
                files[i] = temp[temp.Length - 1];
            }

            for (var i = 0; i < files.Length - 1; i++)
                foreach (var extension in Extensions)
                    if (files[i].Split('.')[1] == extension)
                    {
                        images.Add(files[i]);
                    }

            foreach (var s in images)
            {
                _path = s;
                var bitmap = CompressImage(out var image);
                SaveImage(bitmap, new ImageFormat(image.RawFormat.Guid), true, s.Split('.')[0]);
            }
        }

        private static void ManualTransform()
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Would you like to use default values? \n Y or N");
                _input = Console.ReadLine();
                _input = _input?.ToLower();
                // Break when input is y or n
            } while (_input != "y" && _input != "n");

            if (_input == "n")
            {
                do
                {
                    // Choose how many pixels are going to be transformed into one
                    Console.Clear();
                    Console.WriteLine("Choose mode:");
                    Console.WriteLine("1. 2x2");
                    Console.WriteLine("2. 3x3");
                    Console.WriteLine("3. 4x4");
                    _input = Console.ReadLine();
                    // Break when user input isn't null and can be parsed to int
                    // Passing higher number may result in unexpected behavior
                } while (_input != null && !int.TryParse(_input, out _));
                
                _binningNum = int.Parse(_input ?? throw new NullReferenceException()) + 1;
                
                do
                {
                    // Get path to the image
                    Console.Clear();
                    Console.WriteLine("Path to file:");
                    _path = Console.ReadLine();
                    // Breaks if string isn't null and image exist in that path
                } while (_path != null && !CheckExist(_path));
            }

            var bitmap = CompressImage(out var image);

            var temp = _path.Split('\\');
            var name = temp[temp.Length - 1].Split('.')[0];
            
            SaveImage(bitmap, new ImageFormat(image.RawFormat.Guid), false, name);
        }

        private static Image CompressImage(out Image image)
        {
            image = Image.FromFile(_path ?? throw new NullReferenceException());
            var bitmapSizeHeight = image.Height / _binningNum;
            var bitmapSizeWidth = image.Width / _binningNum;
            var bitmap = (Bitmap) image;

            var newBitmap = new Bitmap(bitmapSizeWidth, bitmapSizeHeight);

            for (var i = 0; i < newBitmap.Width - 1; i++)
            for (var j = 0; j < newBitmap.Height - 1; j++)
                newBitmap.SetPixel(i, j, GetAverageColorUnsafe(bitmap, _binningNum * j,
                    _binningNum * j + _binningNum, _binningNum * i, _binningNum * i + _binningNum));

            return newBitmap;
        }

        private static void SaveImage(Image bm, ImageFormat format, bool toDirectory, string name)
        {
            if (toDirectory)
            {
                if (!Directory.Exists("BinnedImages"))
                    Directory.CreateDirectory("BinnedImages");

                bm.Save("BinnedImages" + "\\" +$"{name}{Path.GetExtension(_path)}", format);
            }
            else
            {
                bm.Save($"{name}{Path.GetExtension(_path)}", format);
            }
        }

        private static bool CheckExist(string text)
        {
            text = Regex.Replace(text, "/", "\\");
            return File.Exists(text);
        }
        
        private static Color GetAverageColorUnsafe(Bitmap bm, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            var totals = new long[] {0, 0, 0};
            var bppModifier =
                bm.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;
                                // cutting corners, will fail on anything else but 32 and 24 bit images

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
                    if (p == null) continue;
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
        
        #region Unused
/*
        private static Color GetAverageColor(Bitmap bm, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            double red = 0, green = 0, blue = 0, alpha = 0, pixNum = 0;

            for (var i = fromWidth; i < toWidth; i++)
            for (var j = fromHeight; j < toHeight; j++)
            {
                var pixel = bm.GetPixel(i, j);
                red += Math.Pow(pixel.R, 2);
                green += Math.Pow(pixel.G, 2);
                blue += Math.Pow(pixel.B, 2);
                alpha += Math.Pow(pixel.A, 2);

                pixNum++;
            }

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
*/
        #endregion

    }
}