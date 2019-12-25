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
        public int imagePercentageThumbCoverage { get; set; }
        public int thumbnailMaxSize { get; set; }
        public List<string> fileExtensions { get; set; }
        public string outputPath { get; set; }
        public string inputPath { get; set; }
        public string thumbSufix { get; set; }
        public List<ThumbnailPosition> opcionesGeneracionThumbs { get; set; }
        private List<Point> thumbCorner;
        public ThumbnailShape cropShape { get; set; }
        public List<Point> ThumbCorner
        {
            get
            {
                if(thumbCorner == null)
                    GenerateThumbnailCorners();
                return thumbCorner;
            }
            set
            {
                thumbCorner = value;
            }
        }
        private Size _imgSize;
        private int _ladoCuadroCropX;
        private int _ladoCuadroCropY;

        public ThumbnailGenerator()
        {
            imagePercentageThumbCoverage = 100;
            thumbnailMaxSize = 100;
            fileExtensions = new List<string> { ".jpg" };
            inputPath = "\\";
            outputPath = inputPath + "\\thumb";
            thumbSufix = string.Empty ;
            opcionesGeneracionThumbs = new List<ThumbnailPosition>();
            cropShape = ThumbnailShape.SQUARE;
        }

        private void GenerateThumbnailCorners()
        {
            if (opcionesGeneracionThumbs.Count == 0)
            {
                thumbCorner = new List<Point>();
                thumbCorner.Add(new Point((_imgSize.Width / 2) - (_ladoCuadroCropX / 2), (_imgSize.Height / 2) - (_ladoCuadroCropY / 2)));
            }
            else
            {
                thumbCorner = new List<Point>();
                foreach (ThumbnailPosition configPosicion in opcionesGeneracionThumbs)
                {
                    switch(configPosicion)
                    {
                        case ThumbnailPosition.TOP_LEFT:
                            thumbCorner.Add(new Point(0, 0));
                            break;
                        case ThumbnailPosition.TOP_CENTER:
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_ladoCuadroCropX / 2), 0));
                            break;
                        case ThumbnailPosition.TOP_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _ladoCuadroCropX, 0));
                            break;

                        case ThumbnailPosition.MID_LEFT:
                            thumbCorner.Add(new Point(0 , (_imgSize.Height/2)-(_ladoCuadroCropY/2)));
                            break;
                        case ThumbnailPosition.MID_CENTER:
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_ladoCuadroCropX / 2), (_imgSize.Height / 2) - (_ladoCuadroCropY / 2)));
                            break;
                        case ThumbnailPosition.MID_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _ladoCuadroCropX, (_imgSize.Height / 2) - (_ladoCuadroCropY / 2)));
                            break;
                        case ThumbnailPosition.BOTTOM_LEFT:
                            thumbCorner.Add(new Point(0, _imgSize.Height-_ladoCuadroCropY));
                            break;
                        case ThumbnailPosition.BOTTOM_CENTER:
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_ladoCuadroCropX / 2), _imgSize.Height - _ladoCuadroCropY));
                            break;
                        case ThumbnailPosition.BOTTOM_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _ladoCuadroCropX , _imgSize.Height - _ladoCuadroCropY));
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
                Size = new Size(thumbnailMaxSize, thumbnailMaxSize)
            };

            try
            {
                List<string> archivos = new List<string>();
                foreach (string fileExtension in fileExtensions)
                    archivos.AddRange(Directory.EnumerateFiles(inputPath, "*" + fileExtension));

                foreach(string archivo in archivos)
                using (Image image = Image.Load(archivo))
                {
                    outputImg = Path.GetFileNameWithoutExtension(archivo);
                    extension = Path.GetExtension(archivo);
                    Console.WriteLine(outputImg + extension);
                    _imgSize = image.Size();
                        Rectangle rec = GenerateCropRectangle();
                    
                    GenerateThumbnailCorners();
                    foreach(Point punto in ThumbCorner)
                    {
                        RectangleReposition(punto,ref rec);
                        var imgnva = image.Clone(x => x.Crop(rec).Resize(ropt));
                        outputFullName = outputPath + "/" + outputImg + thumbSufix + "_" + ThumbCorner.IndexOf(punto) + extension;
                        if (!Directory.Exists(outputPath))
                            Directory.CreateDirectory(outputPath);
                        if (File.Exists(outputFullName))
                        File.Delete(outputFullName);
                        imgnva.Save(outputFullName);
                    }
                }
            }
            catch(Exception exc)
            {
                var error = exc;
                //TODO: implement logging
            }
        }

        private Rectangle GenerateCropRectangle()
        {
            int ladoCorto;

            switch (cropShape)
            {
                case ThumbnailShape.SQUARE:
                    ladoCorto = _imgSize.Height < _imgSize.Width ? _imgSize.Height : _imgSize.Width;
                    _ladoCuadroCropX = (int)Math.Floor(ladoCorto * ((double)imagePercentageThumbCoverage / 100));
                    _ladoCuadroCropY = _ladoCuadroCropX;
                    break;
                //TODO: add rectangle support
            }
            
            Rectangle rec = new Rectangle(0, 0, _ladoCuadroCropX, _ladoCuadroCropY);
            return rec;
        }

        private void RectangleReposition(Point newPosition,ref Rectangle rec)
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
            SQUARE
            //TODO: add rectangle shape
        }
    }
}