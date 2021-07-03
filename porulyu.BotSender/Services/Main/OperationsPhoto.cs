using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace porulyu.BotSender.Services.Main
{
    public class OperationsPhoto
    {
        public string Combine(List<string> FileNames, string Id, long ChatId)
        {
            switch (FileNames.Count)
            {
                case 1:
                    return FileNames[0];
                case 2:
                    return CombineTwoImage(FileNames, Id, ChatId);
                case 3:
                    return CombineThreeImage(FileNames, Id, ChatId);
                default:
                    return null;
            }
        }
        private string CombineTwoImage(List<string> FileNames, string Id, long ChatId)
        {
            Bitmap first = new Bitmap(FileNames[0]);
            Bitmap second = new Bitmap(FileNames[1]);

            if (first.Width >= first.Height && second.Width >= second.Height)
            {
                Bitmap firstResize = new Bitmap(first, new Size(1920, Convert.ToInt32(first.Height / (first.Width / 1920.0))));
                Bitmap secondResize = new Bitmap(second, new Size(1920, Convert.ToInt32(second.Height / (second.Width / 1920.0))));

                first.Dispose();
                second.Dispose();

                Bitmap newImage = new Bitmap(firstResize.Width, firstResize.Height + secondResize.Height);
                Graphics g = Graphics.FromImage(newImage);
                g.DrawImage(firstResize, 0, 0);
                g.DrawImage(secondResize, 0, firstResize.Height);

                g.Dispose();
                firstResize.Dispose();
                secondResize.Dispose();

                newImage.Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg", ImageFormat.Jpeg);
                newImage.Dispose();

                return Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg";
            }
            else if (first.Width <= first.Height && second.Width <= second.Height)
            {
                Bitmap firstResize = new Bitmap(first, new Size(Convert.ToInt32(first.Width / (first.Height / 900.0)), 900));
                Bitmap secondResize = new Bitmap(second, new Size(Convert.ToInt32(second.Width / (second.Height / 900.0)), 900));

                first.Dispose();
                second.Dispose();

                Bitmap newImage = new Bitmap(firstResize.Width + secondResize.Width, firstResize.Height);
                Graphics g = Graphics.FromImage(newImage);
                g.DrawImage(firstResize, 0, 0);
                g.DrawImage(secondResize, firstResize.Width, 0);

                g.Dispose();
                firstResize.Dispose();
                secondResize.Dispose();

                newImage.Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg", ImageFormat.Jpeg);
                newImage.Dispose();

                return Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg";
            }
            else
            {
                second.Dispose();

                first.Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg", ImageFormat.Jpeg);
                first.Dispose();

                return Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg";
            }
        }
        private string CombineThreeImage(List<string> FileNames, string Id, long ChatId)
        {
            Bitmap first = new Bitmap(FileNames[0]);
            Bitmap second = new Bitmap(FileNames[1]);
            Bitmap third = new Bitmap(FileNames[2]);

            if (first.Width >= first.Height && second.Width >= second.Height && third.Width >= third.Height)
            {
                Bitmap secondResize = new Bitmap(second, new Size(Convert.ToInt32(second.Width / (second.Height / 450.0)), 450));
                Bitmap thirdResize = new Bitmap(third, new Size(Convert.ToInt32(third.Width / (third.Height / 450.0)), 450));
                Bitmap firstResize = new Bitmap(first, new Size(secondResize.Width + thirdResize.Width, Convert.ToInt32(first.Height / (first.Width / (double)(secondResize.Width + thirdResize.Width)))));

                first.Dispose();
                second.Dispose();
                third.Dispose();

                Bitmap newImage = new Bitmap(firstResize.Width, firstResize.Height + secondResize.Height);
                Graphics g = Graphics.FromImage(newImage);
                g.DrawImage(firstResize, 0, 0);
                g.DrawImage(secondResize, 0, firstResize.Height);
                g.DrawImage(thirdResize, secondResize.Width, firstResize.Height);

                g.Dispose();
                firstResize.Dispose();
                secondResize.Dispose();
                thirdResize.Dispose();

                newImage.Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg", ImageFormat.Jpeg);
                newImage.Dispose();

                return Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg";
            }
            else if (first.Width <= first.Height && second.Width <= second.Height && third.Width <= third.Height)
            {
                Bitmap secondResize = new Bitmap(second, new Size(450, Convert.ToInt32(second.Height / (second.Width / 450.0))));
                Bitmap thirdResize = new Bitmap(third, new Size(450, Convert.ToInt32(third.Height / (third.Width / 450.0))));
                Bitmap firstResize = new Bitmap(first, new Size(Convert.ToInt32(first.Width / (first.Height / (double)(secondResize.Height + thirdResize.Height))), secondResize.Height + thirdResize.Height));

                first.Dispose();
                second.Dispose();
                third.Dispose();

                Bitmap newImage = new Bitmap(firstResize.Width + secondResize.Width, firstResize.Height);
                Graphics g = Graphics.FromImage(newImage);
                g.DrawImage(firstResize, 0, 0);
                g.DrawImage(secondResize, firstResize.Width, 0);
                g.DrawImage(thirdResize, firstResize.Width, secondResize.Height);

                g.Dispose();
                firstResize.Dispose();
                secondResize.Dispose();
                thirdResize.Dispose();

                newImage.Save(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg", ImageFormat.Jpeg);
                newImage.Dispose();

                return Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName) + $@"\Temp\{ChatId}\{Id}\Combine.jpg";
            }
            else
            {
                first.Dispose();
                second.Dispose();
                third.Dispose();

                return CombineTwoImage(FileNames, Id, ChatId);
            }
        }
    }
}
