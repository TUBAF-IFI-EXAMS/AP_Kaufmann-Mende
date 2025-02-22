﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLData;
using Tools;

namespace Classes
{
    ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="CustomBuilder"]/*'/>
    public class CustomBuilder
    {
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="Labels"]/*'/>
        public static List<Dataset> Labels { get; private set; }
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="Data"]/*'/>
        public static DataCollection Data { get; private set; }
        static CustomBuilder()
        {
            Labels = new List<Dataset>();

        }
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="Initialization"]/*'/>
        public static void Initialization(string path)
        {
            {

                try
                {
                    int[] maxitems = null;
                    while (maxitems == null || maxitems[0] < 1)
                    {
                        maxitems = ConsoleTools.VarInput("Wie viele Bilder sollen pro Kategorie heruntergeladen werden? (min. 1)");
                    }
                    Data = new DataCollection(path, maxitems[0]);
                    bool MinLabels = false;
                    while (!MinLabels)
                    {
                        Console.WriteLine("Auswahl der Kategorien, bitte insgesamt mindestens zwei Auswählen! ");
                        bool run = true;



                        while (run)
                        {


                            Labels.Clear();
                            while (Labels.Count == 0)
                            {
                                Console.WriteLine("Bitte eingeben, was in der Kategoriebezeichnung enthalten sein soll!");
                                string Input = ConsoleTools.NonEmptyInput();
                                Labels = Data.FindLables(Input);
                                foreach (Dataset item in Labels)
                                {
                                    Console.WriteLine("{0}: {1}: {2}", Labels.IndexOf(item), item.Key, item.Label);
                                }
                                if (Labels.Count == 0) Console.WriteLine("Leider keine passenden Einträge gefunden.\nBitte neuen Suchbegriff eingeben");
                            }

                            bool ValidIndexes = false;

                            while (!ValidIndexes)
                            {
                                ValidIndexes = true;
                                int[] ReturnedVal = null;
                                while ((ReturnedVal = ConsoleTools.VarInput("Bitte Kategorienummer eingeben  oder -1, um Eingabe neuzustarten, bei mehreren mit Leerzeichen getrennt")) == null)
                                {
                                    Console.WriteLine("Bitte gültigen Input tätigen");
                                }

                                int[] index = ReturnedVal;


                                foreach (var item in index)
                                {
                                    if (item == -1)
                                    {
                                        break;
                                    }
                                    if (item < -1 || item >= Labels.Count)
                                    {
                                        Console.WriteLine("Mindestens ein Index ist zu groß/klein!");

                                        ValidIndexes = false;
                                        break;
                                    }
                                    else if (!Data.Labels.Contains(new Dataset(Labels[item].Key, Labels[item].Label)))
                                    {
                                        Data.Labels.Add(Labels[item]);
                                        if (Data.Labels.Count >= 2) MinLabels = true;
                                    }


                                }

                            }

                            run = ConsoleTools.YesNoInput("Nach neuer Kategorie suchen");

                        }

                    }
                    Data.DownloadAllDatasets(path);
                    TSVMaker.LogAllData(PathFinder.ImageDir, Data.Labels);

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }
            }
        }
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="GenerateModel"]/*'/>
        public static ITransformer GenerateModel(MLContext mlContext)
        {
            try
            {

                string ModelFolder = PathFinder.ModelDir;
                string ModelLocation = Path.Combine(ModelFolder, "tensorflow_inception_graph.pb");
                if (!File.Exists(ModelLocation)) { Console.WriteLine($"Modell unter {ModelLocation} nicht gefunden! Programmabbruch!"); throw new FileNotFoundException(); }

                string TrainingTags = Path.Combine(PathFinder.ImageDir, TSVMaker.TrainData);
                string TestTags = Path.Combine(PathFinder.ImageDir, TSVMaker.TestData);
                Console.WriteLine(nameof(Image.Path));

                //Tranformationen der Eingaben für nachfolgende Verarbeitungsschritte
                IEstimator<ITransformer> pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: null, inputColumnName: nameof(Image.Path)) //_imagesFolder
                                                                                                                                                                       // The image transforms transform the images into the model's expected format.
                                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                                .Append(mlContext.Model.LoadTensorFlowModel(ModelLocation).
                                ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: "LabeledAs"))
                                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey", featureColumnName: "softmax2_pre_activation"))
                                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedImageLabel", "PredictedLabel"))
                                .AppendCacheCheckpoint(mlContext);



                IDataView TrainingData = mlContext.Data.LoadFromTextFile<Image>(path: TrainingTags, separatorChar: ';');

                Console.WriteLine("Training Gestartet\nDies kann je nach Anzahl der Bilder einige Zeit dauern!");
                ITransformer TrainedModel = pipeline.Fit(TrainingData);

                Console.WriteLine("Trainiertes Modell testen");
                IDataView TestData = mlContext.Data.LoadFromTextFile<Image>(path: TestTags, separatorChar: ';');
                IDataView TestPredictions = TrainedModel.Transform(TestData);

                IEnumerable<CategorizedImage> ImagePredictionData = mlContext.Data.CreateEnumerable<CategorizedImage>(TestPredictions, true);
                DisplayResults(ImagePredictionData);

                Console.WriteLine("Statistiken zum Training: ");
                MulticlassClassificationMetrics metrics =
                    mlContext.MulticlassClassification.Evaluate(TestPredictions,
                      labelColumnName: "LabelKey",
                      predictedLabelColumnName: "PredictedImageLabel");

                Console.WriteLine($"LogLoss: {metrics.LogLoss}");
                Console.WriteLine($"PerClassLogLoss: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");


                Console.WriteLine("Sie können das Modell jetzt speichern. Unter welchem Namen sol das Modell gespeichert werden? (Ohne Extension)");
                bool CorrectName = false;
                bool FileExists = true;
                string Input = "";
                do
                {
                    Input = Console.ReadLine();
                    CorrectName = ConsoleTools.FileNameInput(Input);
                    FileExists = File.Exists(Path.Combine(ModelFolder, Input + ".model")) ? true : false;
                    if (FileExists) Console.WriteLine("File existiert schon, bitte neuen Namen ausdenken");
                }
                while (!CorrectName || FileExists);

                string ModelName = Input + ".model";
                string NewModelPath = Path.Combine(ModelFolder, ModelName);
                mlContext.Model.Save(TrainedModel, TrainingData.Schema, NewModelPath);
                Console.WriteLine($"Das Modell ist unter {NewModelPath} gespeichert");
                AddModelInfo(NewModelPath);


                return TrainedModel;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Fehler beim Training. Die Trainingsdaten sind korrumpiert.");
                return null;
            }
            catch (Exception)
            {
                Console.WriteLine("Allgemeiner Fehler beim Training.");
                return null;
            }
        }

        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="AddModelInfo"]/*'/>
        public static bool AddModelInfo(string ModelPath)
        {
            string ModelName = Path.GetFileName(ModelPath);
            string SingleLabels = "";
            foreach (var SingleLabel in TSVMaker.LabelNames) SingleLabels += SingleLabel + "-";
            try
            {
                using (StreamWriter sw = File.AppendText(Path.Combine(PathFinder.ModelDir, ".Info")))
                {
                    sw.WriteLine(ModelName + ';' + SingleLabels);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Indizieren des Modells nicht möglich. Existiert Zugriff auf {Path.Combine(PathFinder.ModelDir, ".Info")}?");
                return false;
            }
            return true;
        }
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="GetModelNames"]/*'/>
        public static string GetModelNames()
        {
            string line = null;
            List<string> ModelNames = new List<string>();
            Console.WriteLine("Folgende Modelle sind vorhanden:\n ");
            using (StreamReader sr = new StreamReader(Path.Combine(PathFinder.ModelDir, ".Info")))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] Parts = line.Split(';');
                    ModelNames.Add(Parts[0]);
                    Console.WriteLine($"Modell: *{Parts[0].Split('.')[0]}* mit Kategorien: *{Parts[1]} (Nr. {ModelNames.IndexOf(Parts[0]) })*");
                }
            }

            Console.WriteLine("Bitte Entscheidung für ein Modell durch Eingabe der jeweiligen Nummer treffen und mit Enter bestätigen\nEingabe muss wiederholt werden, wenn inkorrekt");
            bool IsNumber = false;
            int Choice = -1;
            bool IsValidNumber = false;
            while (!IsNumber || !IsValidNumber)
            {
                IsNumber = int.TryParse(Console.ReadLine(), out Choice);
                IsValidNumber = (Choice >= 0 && Choice < ModelNames.Count) ? true : false;
            }


            Console.WriteLine($"Sie haben sich für {ModelNames[Choice].Split('.')[0]} entschieden");
            return ModelNames[Choice];


        }

        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="DisplayResults"]/*'/>
        private static void DisplayResults(IEnumerable<CategorizedImage> PredictedData)
        {
            foreach (CategorizedImage Result in PredictedData)
            {

                string Category = null;
                for (int i = 0; i < TSVMaker.LabelNames.Length; i++)
                {
                    if (Result.Score.Max() == Result.Score[i]) Category = TSVMaker.LabelNames[i];
                }

                Console.WriteLine($"Bild: {Path.GetFileName(Result.Path)} Gelabelt Als: {Result.GetLabelFromPath()} Bestimmt Als: {Category} Sicherheit: {Result.Score.Max() * 100:F1}% ");
            }

        }
        ///<include file='ClassesDoc/CustomModelBuilder.xml' path='CustomModelBuilder/Member[@name="InceptionSettings"]/*'/>
        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

    }
}
