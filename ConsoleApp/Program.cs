﻿using System;
using System.Collections.Generic;
using System.IO;
using CategorizingImages;
using Classes;
using HTMLTools;
using Microsoft.ML;
using Tools;


namespace ConsoleApp
{
    ///<include file='ConsoleDoc/Program.xml' path='Program/Member[@name="Program"]/*'/>
    class Program
    {
        ///<include file='ConsoleDoc/Program.xml' path='Program/Member[@name="CategorizeChoice"]/*'/>
        public static void CategorizeChoice(ITransformer trainedModel, MLContext mlContext)
        {

            List<Image> input = ImageCategorizer.Initialization();
            IEnumerable<IHtmlable> prediction = ImageCategorizer.Categorizer(input, trainedModel, mlContext);
            HTMLCreator.Result(prediction, PathFinder.OwnImagesDir, @"Website");
        }
        ///<include file='ConsoleDoc/Program.xml' path='Program/Member[@name="TrainingChoice"]/*'/>
        public static void TrainingChoice(MLContext mlContext)
        {

            CustomBuilder.Initialization(PathFinder.FindOrigin());
            ITransformer GeneratedModel = CustomBuilder.GenerateModel(mlContext);
            if (GeneratedModel == null) return;
            Console.WriteLine("Modell erfolgreich trainiert!\nEs wurden folgende Kategorien trainiert: ");

            foreach (string Label in TSVMaker.LabelNames) Console.WriteLine($"***{Label}***");
            if (ConsoleTools.YesNoInput("Möchten Sie das (oder andere) Modelle direkt zur Klassifizierung nutzen?")) ClassificationChoice(new MLContext());


        }
        ///<include file='ConsoleDoc/Program.xml' path='Program/Member[@name="ClassificationChoice"]/*'/>
        public static void ClassificationChoice(MLContext mlContext)
        {
            if (!File.Exists(Path.Combine(PathFinder.ModelDir, ".Info")))
            {
                Console.WriteLine("Die .Info-Datei mit Informationen über trainierte Modelle ist nicht vorhanden. Haben Sie bereits Modelle trainiert?");
                Console.WriteLine("Programm beendet");
                return;
            }
            Console.WriteLine("Wählen Sie bitte unter den folgenden Modellen aus: ");
            string ChosenModel = CustomBuilder.GetModelNames();
            string ModelPath = Path.Combine(PathFinder.ModelDir, ChosenModel);



            DataViewSchema TrainedModelSchema;
            ITransformer TrainedModel = mlContext.Model.Load(ModelPath, out TrainedModelSchema);
            CategorizeChoice(TrainedModel, mlContext);
        }
        ///<include file='ConsoleDoc/Program.xml' path='Program/Member[@name="ForceDeleteDirectory"]/*'/>
        public static void ForceDeleteDirectory(string Dir)
        {
            string[] files = Directory.GetFiles(Dir);
            string[] dirs = Directory.GetDirectories(Dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                ForceDeleteDirectory(dir);
            }

            Directory.Delete(Dir, false);
        }


        static void Main(string[] args)
        {


            try
            {
                ForceDeleteDirectory(PathFinder.ImageDir);
            }
            catch (Exception) { }

            MLContext myContext = new MLContext();

            string OriginPath = null;
            try
            {
                OriginPath = PathFinder.FindOrigin();
            }
            catch (Exception) { Console.WriteLine(@"Couldn't find .Index-File"); }
            Console.WriteLine("Willkommen in der Konsolen-App zur Bildklassifizierung auf Grundlage von Machine Learning");

            bool IsValidKey = false;
            char PressedKey = ' ';
            while (!IsValidKey)
            {
                Console.WriteLine("Möchten Sie (1) Bilder kategorisieren oder (2) das Modell neu trainieren?");
                IsValidKey = ConsoleTools.IsValidKey(ref PressedKey, 1);
            }

            if (PressedKey == '1')
            {

                ClassificationChoice(myContext);

            }
            else if (PressedKey == '2')
            {
                TrainingChoice(myContext);
            }

            Console.WriteLine("Beende Programm");
            Console.ReadKey(true);
        }
    }
}
