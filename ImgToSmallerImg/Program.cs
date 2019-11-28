using System.Drawing;
using System.Drawing.Imaging;

namespace ImgToSmallerImg
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var image = Image.FromFile("C:\\a.jpg");
            var bitmap = (Bitmap) image;
            var bitmapSize = 350;

            var height = image.Height;
            var width = image.Width;

            var sectorSizeH = height / bitmapSize;
            var sectorSizeW = width / bitmapSize;

            var newBitmap = new Bitmap(bitmapSize, bitmapSize);

            for (var i = 0; i < newBitmap.Height; i++)
            for (var j = 0; j < newBitmap.Width; j++)
                newBitmap.SetPixel(i, j, GetColor(bitmap, sectorSizeH * j, sectorSizeH * j + sectorSizeH,
                    sectorSizeW * i, sectorSizeW * i + sectorSizeW));

            newBitmap.Save("Binned.jpeg", ImageFormat.Jpeg);
        }

        private static Color GetColor(Bitmap bitmap, int fromHeight, int toHeight, int fromWidth, int toWidth)
        {
            ulong red = 0, green = 0, blue = 0, alpha = 0, pixNum = 1;

            for (var i = fromWidth; i < toWidth; i++)
            for (var j = fromHeight; j < toHeight; j++)
            {
                red += bitmap.GetPixel(i, j).R;
                green += bitmap.GetPixel(i, j).G;
                blue += bitmap.GetPixel(i, j).B;
                alpha += bitmap.GetPixel(i, j).A;

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