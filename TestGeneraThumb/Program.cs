using System;
using System.Collections.Generic;
//using ThumbnailGenerator;

namespace TestGeneraThumb
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            ThumbnailGenerator.ThumbnailGenerator thumbGeneator = new ThumbnailGenerator.ThumbnailGenerator();
            thumbGeneator.inputPath = "/Volumes/Almacenamiento/Pruebas/";
            thumbGeneator.outputPath = "/Volumes/Almacenamiento/Pruebas/Salida/";
            thumbGeneator.fileExtensions = new List<string> { ".jpg" };
            thumbGeneator.thumbnailMaxSize = 250;
            thumbGeneator.imagePercentageThumbCoverage = 80;
            thumbGeneator.cropShape = ThumbnailGenerator.ThumbnailGenerator.ThumbnailShape.SQUARE;

            thumbGeneator.opcionesGeneracionThumbs.Add(ThumbnailGenerator.ThumbnailGenerator.ThumbnailPosition.TOP_CENTER);
            thumbGeneator.opcionesGeneracionThumbs.Add(ThumbnailGenerator.ThumbnailGenerator.ThumbnailPosition.BOTTOM_LEFT);

            thumbGeneator.CreateThumbnails();
        }
    }
}
