using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartImageAnalysis
{
    class Program
    {
        const string API_key = "7302a0fa5aeb4fff956365e050c3051f";
        const string API_location = "https://southcentralus.api.cognitive.microsoft.com/vision/v1.0/";

        static void Main(string[] args)
        {
            string imageToAnalyze = @"C:\Tests\testimage.jpg";

            SmartImageAnalysisResults(imageToAnalyze, "analyze");
            SmartImageAnalysisResults(imageToAnalyze, "describe");
            SmartImageAnalysisResults(imageToAnalyze, "tag");


            Console.ReadKey();
        }

        //List all features needed 
        public static IEnumerable<VisualFeature> GetVisualFeatures()
        {
            return new VisualFeature[]
            {
                VisualFeature.Adult,
                VisualFeature.Categories,
                VisualFeature.Color,
                VisualFeature.Description,
                VisualFeature.Faces,
                VisualFeature.ImageType,
                VisualFeature.Tags
            };
        }


        //This will call SmartImageProcess and display the results obtained.
        //We will do this by wrapping the internal logic of SmartImageAnalysisResults method with an anonymous async object passed to Task.Run()
        public static void SmartImageAnalysisResults(string fname, string method)
        {
            Task.Run(async () =>
            {

                string img = Path.GetFileName(fname);
                Console.WriteLine($"Checking image: {img}");

                AnalysisResult analyzed = await SmartImageProcess(fname, method);

                //This one handles output
                switch (method)
                {
                    case "analyze":
                        ShowResults(analyzed, analyzed.Categories, "Categories");
                        ShowFaces(analyzed);
                        break;

                    case "describe":
                        ShowCaptions(analyzed);
                        break;

                    case "tag":
                        ShowTags(analyzed, 0.9);
                        break;
                }

            }).Wait();
        }



        //Core image processing method -> Internally, this method will be able to invoke several CV API methods, such as analyze and describe.
        //This is why, we specify a second parameter named method.
        public static async Task<AnalysisResult> SmartImageProcess(string fname, string method)
        {
            AnalysisResult analyzed = null;
            VisionServiceClient client = new VisionServiceClient(API_key, API_location);

            //We call GetVisualFeatures() and assign it to an internal variable, which we will pass onto API method calls.
            IEnumerable<VisualFeature> visualFeatures = GetVisualFeatures();

            //Then, we read the input image, and with a switch statement, check which CV API method we want to invoke.
            if (File.Exists(fname))
                using (Stream stream = File.OpenRead(fname))
                    switch (method)
                    {
                        case "analyze":
                            analyzed = await client.AnalyzeImageAsync(stream, visualFeatures);
                            break;

                        case "describe":
                            analyzed = await client.DescribeAsync(stream);
                            break;

                        case "tag":
                            analyzed = await client.GetTagsAsync(stream);
                            break;
                    }

            return analyzed;
        }

        public static void ShowCaptions(AnalysisResult analyzed)
        {
            //Captions is a variable that is a result of selecting the text and confidence properties for all the analyzed.Description.Captions occurencces.
            var captions = from caption in analyzed.Description.Captions select caption.Text + " - " + caption.Confidence;

            //These occurences are then joined as a single string and written to console
            if (captions.Count() > 0)
            {
                Console.WriteLine("Captions >>>>");
                Console.WriteLine(string.Join(", ", captions));
            }
        }

        public static void ShowResults(AnalysisResult analyzed, NameScorePair[] nps, string ResName)
        {
            var results = from result in nps select result.Name + " - " + result.Score.ToString();

            if (results.Count() > 0)
            {
                Console.WriteLine($"{ResName} >>>>");
                Console.WriteLine(string.Join(", ", results));
            }
        }

        public static void ShowFaces(AnalysisResult analyzed)
        {
            var faces = from face in analyzed.Faces select face.Gender + " - " + face.Age.ToString();

            if (faces.Count() > 0)
            {
                Console.WriteLine("Faces >>>>");
                Console.WriteLine(string.Join(", ", faces));
            }
        }

        public static void ShowTags(AnalysisResult analyzed, double confidence)
        {
            var tags = from tag in analyzed.Tags where tag.Confidence > confidence select tag.Name;

            if (tags.Count() > 0)
            {
                Console.WriteLine("Tags >>>>");
                Console.WriteLine(string.Join(", ", tags));
            }
        }

    }
}
