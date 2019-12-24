using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ThumbnailGenerator
{
    public class ThumbnailGenerator
    {
        public int Image_thumbPercentage { get; set; }
        public int Thumb_maxSize { get; set; }
        public List<string> File_extensions { get; set; }
        public string Path_output { get; set; }
        public string Path_input { get; set; }
        public string Thumb_sufix { get; set; }
        public List<ThumbnailPosition> opcionesGeneracionThumbs { get; set; }
        private List<Point> thumb_corner;
        public ThumbnailShape cropShape { get; set; }
        public List<Point> Thumb_corner
        {
            get
            {
                if(thumb_corner == null)
                    GetThumbnailCorners();
                return thumb_corner;
            }
            set
            {
                thumb_corner = value;
            }
        }
        private Size imgSize;
        private int ladoCuadroCropX;
        private int ladoCuadroCropY;

        public ThumbnailGenerator()
        {
            Image_thumbPercentage = 100;
            Thumb_maxSize = 100;
            File_extensions = new List<string> { ".jpg" };
            Path_input = "\\";
            Path_output = Path_input + "\\thumb";
            Thumb_sufix = "_thmb";
            opcionesGeneracionThumbs = new List<ThumbnailPosition>();
            cropShape = ThumbnailShape.SQUARE;
        }

        private void GetThumbnailCorners()
        {
            if (opcionesGeneracionThumbs.Count == 0)
            {
                thumb_corner = new List<Point>();
                thumb_corner.Add(new Point((imgSize.Width / 2) - (ladoCuadroCropX / 2), (imgSize.Height / 2) - (ladoCuadroCropY / 2)));
            }
            else
            {
                thumb_corner = new List<Point>();
                foreach (ThumbnailPosition configPosicion in opcionesGeneracionThumbs)
                {
                    switch(configPosicion)
                    {
                        case ThumbnailPosition.TOP_LEFT:
                            thumb_corner.Add(new Point(0, 0));
                            break;
                        case ThumbnailPosition.TOP_CENTER:
                            thumb_corner.Add(new Point((imgSize.Width / 2) - (ladoCuadroCropX / 2), 0));
                            break;
                        case ThumbnailPosition.TOP_RIGHT:
                            thumb_corner.Add(new Point(imgSize.Width - ladoCuadroCropX, 0));
                            break;

                        case ThumbnailPosition.MID_LEFT:
                            thumb_corner.Add(new Point(0 , (imgSize.Height/2)-(ladoCuadroCropY/2)));
                            break;
                        case ThumbnailPosition.MID_CENTER:
                            thumb_corner.Add(new Point((imgSize.Width / 2) - (ladoCuadroCropX / 2), (imgSize.Height / 2) - (ladoCuadroCropY / 2)));
                            break;
                        case ThumbnailPosition.MID_RIGHT:
                            thumb_corner.Add(new Point(imgSize.Width - ladoCuadroCropX, (imgSize.Height / 2) - (ladoCuadroCropY / 2)));
                            break;

                        case ThumbnailPosition.BOTTOM_LEFT:
                            thumb_corner.Add(new Point(0, imgSize.Height-ladoCuadroCropY));
                            break;
                        case ThumbnailPosition.BOTTOM_CENTER:
                            thumb_corner.Add(new Point((imgSize.Width / 2) - (ladoCuadroCropX / 2), imgSize.Height - ladoCuadroCropY));
                            break;
                        case ThumbnailPosition.BOTTOM_RIGHT:
                            thumb_corner.Add(new Point(imgSize.Width - ladoCuadroCropX , imgSize.Height - ladoCuadroCropY));
                            break;
                    }
                }
            }
        }

        public void CreateThumbnails()
        {
            string outputImg;
            string extension;
            
            string outputFullName;
            
            ResizeOptions ropt = new ResizeOptions()
            {
                Compand = true,
                Sampler = KnownResamplers.Lanczos3,
                Size = new Size(Thumb_maxSize, Thumb_maxSize)
            };

            try
            {
                List<string> archivos = new List<string>();
                foreach (string fileExtension in File_extensions)
                    archivos.AddRange(Directory.EnumerateFiles(Path_input, "*" + fileExtension));

                foreach(string archivo in archivos)
                using (Image image = Image.Load(archivo))
                {
                    outputImg = Path.GetFileNameWithoutExtension(archivo);
                    extension = Path.GetExtension(archivo);
                    Console.WriteLine(outputImg + extension);
                    imgSize = image.Size();
                        Rectangle rec = generaRectanguloCrop();
                    
                    GetThumbnailCorners();
                    foreach(Point punto in Thumb_corner)
                    {
                        reubicarRectangulo(punto,rec);
                        var imgnva = image.Clone(x => x.Crop(rec).Resize(ropt));
                        outputFullName = Path_output + "/" + outputImg + Thumb_sufix + "_" + Thumb_corner.IndexOf(punto) + extension;
                        if (!Directory.Exists(Path_output))
                            Directory.CreateDirectory(Path_output);
                        if (File.Exists(outputFullName))
                        File.Delete(outputFullName);
                        imgnva.Save(outputFullName);
                    }
                }
            }
            catch(Exception exc)
            {
                var error = exc;
            }
        }

        private Rectangle generaRectanguloCrop()
        {
            int ladoCorto;

            switch (cropShape)
            {
                case ThumbnailShape.SQUARE:
                    if (imgSize.Height < imgSize.Width)
                        ladoCorto = imgSize.Height;
                    else
                        ladoCorto = imgSize.Width;
                    ladoCuadroCropX = (int)Math.Floor(ladoCorto * ((double)Image_thumbPercentage / 100));
                    ladoCuadroCropY = (int)Math.Floor(ladoCorto * ((double)Image_thumbPercentage / 100));
                    break;
                case ThumbnailShape.RECTANGLE:
                    ladoCuadroCropX = (int)Math.Floor(imgSize.Width * ((double)Image_thumbPercentage / 100));
                    ladoCuadroCropY = (int)Math.Floor(imgSize.Height * ((double)Image_thumbPercentage / 100));
                    break;
            }
            
            Rectangle rec = new Rectangle(0, 0, ladoCuadroCropX, ladoCuadroCropY);
            return rec;
        }

        private void reubicarRectangulo(Point newPosition,Rectangle rec)
        {
            rec.X = newPosition.X;
            rec.Y = newPosition.Y;
        }

        public enum ThumbnailPosition
        {
            TOP_RIGHT,
            TOP_LEFT,
            TOP_CENTER,
            MID_RIGHT,
            MID_CENTER,
            MID_LEFT,
            BOTTOM_RIGHT,
            BOTTOM_CENTER,
            BOTTOM_LEFT
        }

        public enum ThumbnailShape
        {
            SQUARE,
            RECTANGLE
        }
    }
}