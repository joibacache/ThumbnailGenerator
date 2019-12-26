using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ThumbnailGenerator
{
    public class ThumbnailGenerator
    {
        #region variables declaration
        public int imagePercentageThumbCoverage { get; set; }
        public int thumbnailMaxSize { get; set; }
        public List<string> fileExtensions { get; set; }
        public string outputPath { get; set; }
        public string inputPath { get; set; }
        public string thumbSufix { get; set; }
        public List<ThumbnailPosition> opcionesGeneracionThumbs { get; set; }
        private List<Point> thumbCorner;
        public ThumbnailShape cropShape { get; set; }
        bool shortSideGuidedCalculations { get; set; }
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
        private int _cropRectangleWidth;
        private int _cropRectangleHeight;
        #endregion

        /// <summary>
        /// Creates a new instance of ThumbnailGeneration with default values.
        /// Square 100x100px centered thumbnail for every .jpg in the same folder that the dll is located
        /// </summary>
        public ThumbnailGenerator()
        {
            inputPath = "\\";
            outputPath = inputPath + "\\thumb";
            fileExtensions = new List<string> { ".jpg" };
            imagePercentageThumbCoverage = 100;
            thumbnailMaxSize = 100;
            thumbSufix = string.Empty ;
            opcionesGeneracionThumbs = new List<ThumbnailPosition>();
            opcionesGeneracionThumbs.Add(ThumbnailPosition.MID_CENTER);
            cropShape = ThumbnailShape.SQUARE;
            shortSideGuidedCalculations = true;

        }

        /// <summary>
        /// Creates the anchor point for every thumnail that will be created
        /// </summary>
        private void GenerateThumbnailCorners()
        {
            if (opcionesGeneracionThumbs.Count == 0)
            {
                thumbCorner = new List<Point>();
                thumbCorner.Add(new Point((_imgSize.Width / 2) - (_cropRectangleWidth / 2), (_imgSize.Height / 2) - (_cropRectangleHeight / 2)));
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
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_cropRectangleWidth / 2), 0));
                            break;
                        case ThumbnailPosition.TOP_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _cropRectangleWidth, 0));
                            break;

                        case ThumbnailPosition.MID_LEFT:
                            thumbCorner.Add(new Point(0 , (_imgSize.Height/2)-(_cropRectangleHeight/2)));
                            break;
                        case ThumbnailPosition.MID_CENTER:
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_cropRectangleWidth / 2), (_imgSize.Height / 2) - (_cropRectangleHeight / 2)));
                            break;
                        case ThumbnailPosition.MID_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _cropRectangleWidth, (_imgSize.Height / 2) - (_cropRectangleHeight / 2)));
                            break;
                        case ThumbnailPosition.BOTTOM_LEFT:
                            thumbCorner.Add(new Point(0, _imgSize.Height-_cropRectangleHeight));
                            break;
                        case ThumbnailPosition.BOTTOM_CENTER:
                            thumbCorner.Add(new Point((_imgSize.Width / 2) - (_cropRectangleWidth / 2), _imgSize.Height - _cropRectangleHeight));
                            break;
                        case ThumbnailPosition.BOTTOM_RIGHT:
                            thumbCorner.Add(new Point(_imgSize.Width - _cropRectangleWidth, _imgSize.Height - _cropRectangleHeight));
                            break;
                    }
                }
            }
        }

        //Creates the ResizeOptions needed to resize the image crop to become a thumbnail, depending on the crop ratio
        private ResizeOptions CreateResizeOptions()
        {
            ResizeOptions resizeOptions = new ResizeOptions();
            resizeOptions.Compand = true;
            resizeOptions.Sampler = KnownResamplers.Lanczos3;
            switch (cropShape)
            {
                case ThumbnailShape.SQUARE:
                    resizeOptions.Size = new Size(thumbnailMaxSize, thumbnailMaxSize);
                    break;
                case ThumbnailShape.RECTANGLE_3x2:
                    resizeOptions.Size = new Size(thumbnailMaxSize, (int)Math.Floor(thumbnailMaxSize * 0.75));
                    break;
                case ThumbnailShape.RECTANGLE_16X9:
                    resizeOptions.Size = new Size(thumbnailMaxSize, (int)Math.Floor(thumbnailMaxSize * 0.562));
                    break;
            }
            return resizeOptions;
        }

        //The center of all, this method generates the thumbnails
        public void CreateThumbnails()
        {
            string outputImg;
            string extension;
            
            string outputFullName;

            ResizeOptions ropt = CreateResizeOptions();
            

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
                Console.WriteLine("Error! - " + exc.Message);
                //TODO: implement logging
            }
        }

        //Creates the rectagle object, used to select the area of the image that will be used to create the thumbnail,
        //defines the size of the rectangle, locating it in 0,0 position
        private Rectangle GenerateCropRectangle()
        {
            int ladoGuia;
            switch (cropShape)
            {
                case ThumbnailShape.SQUARE:
                    ladoGuia = _imgSize.Height < _imgSize.Width ? _imgSize.Height : _imgSize.Width; ;
                    _cropRectangleWidth = (int)Math.Floor(ladoGuia * ((double)imagePercentageThumbCoverage / 100));
                    _cropRectangleHeight = _cropRectangleWidth;
                    break;
                case ThumbnailShape.RECTANGLE_3x2:
                    ladoGuia = _imgSize.Width;
                    _cropRectangleWidth = (int)Math.Floor(ladoGuia * (double)imagePercentageThumbCoverage / 100);
                    _cropRectangleHeight = (int)Math.Floor(_cropRectangleWidth * 0.75);
                    break;
                case ThumbnailShape.RECTANGLE_16X9:
                    ladoGuia = _imgSize.Width;
                    _cropRectangleWidth = (int)Math.Floor(ladoGuia * (double)imagePercentageThumbCoverage / 100);
                    _cropRectangleHeight = (int)Math.Floor(_cropRectangleWidth * 0.562);
                    break;

            }
            
            Rectangle rec = new Rectangle(0, 0, _cropRectangleWidth, _cropRectangleHeight);
            return rec;
        }

        /// <summary>
        /// Changes the x and y position of the given rectangle
        /// </summary>
        /// <param name="newPosition">The point where the top-left corner of the rectangle should be placed</param>
        /// <param name="rec">The rectangle that will be repositioned</param>
        private void RectangleReposition(Point newPosition,ref Rectangle rec)
        {
            rec.X = newPosition.X;
            rec.Y = newPosition.Y;
        }

        /// <summary>
        /// Represents the different crops that can be made to the original image to create a thumbnail
        /// </summary>
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

        /// <summary>
        /// Represents the different aspect ratio that are available to crop thumbnails
        /// </summary>
        public enum ThumbnailShape
        {
            SQUARE,
            RECTANGLE_3x2,
            RECTANGLE_16X9
        }
    }
}