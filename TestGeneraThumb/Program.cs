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
            thumbGeneator.Path_input = "/Volumes/Almacenamiento/Pruebas/";
            thumbGeneator.Path_output = "/Volumes/Almacenamiento/Pruebas/Salida/";
            thumbGeneator.File_extensions = new List<string> { ".jpg" };
            thumbGeneator.Thumb_sufix = "_tmb";
            thumbGeneator.Thumb_maxSize = 250;
            thumbGeneator.Image_thumbPercentage = 80;
            thumbGeneator.Thumb_sufix = "_tb";
            thumbGeneator.cropShape = ThumbnailGenerator.ThumbnailGenerator.ThumbnailShape.RECTANGLE;
            thumbGeneator.opcionesGeneracionThumbs.Add(ThumbnailGenerator.ThumbnailGenerator.ThumbnailPosition.TOP_CENTER);
            thumbGeneator.opcionesGeneracionThumbs.Add(ThumbnailGenerator.ThumbnailGenerator.ThumbnailPosition.BOTTOM_LEFT);

            thumbGeneator.CreateThumbnails();
            //thumbGeneator.m1("/Volumes/Almacenamiento/Pruebas/", 0.7, 200,"");
        }
    }
}
