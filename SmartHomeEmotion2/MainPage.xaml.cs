using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Common.Contract;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.ProjectOxford.Face;
using System.Threading.Tasks;
using Windows.Foundation;
using System.IO;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

// Author: Elham Binshaflout , email: e.binshaflout.16@ucl.ac.uk 
/*References:
 - Emotion detection : https://msdn.microsoft.com/en-us/magazine/mt742868.aspx
*/
namespace SmartHomeEmotion2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    //Face Emotion detection class
    class FaceEmotionDetection
    {
        public string Emotion { get; set; }
        public double Smile { get; set; }
        public string Glasses { get; set; }
        public string Gender { get; set; }
        public double Age { get; set; }
        public double Beard { get; set; }
        public double Moustache { get; set; }
        public string Key { get; internal set; }
    }


    public sealed partial class MainPage : Page
    {
        CameraCaptureUI captureUI = new CameraCaptureUI();

        //To Store Image -- Delete--
        StorageFile photo;

        //to pass the photo to cognitive services API
        IRandomAccessStream imageStream;


        private readonly IFaceServiceClient faceServiceClient;
        private readonly EmotionServiceClient emotionServiceClient;


        // Provides functionality to preview and capture the photograph
        // private MediaCapture _mediaCapture;
        // private bool PictureButton; 
        MediaPlayer player;
        bool playing;

        public MainPage()
        {
            this.InitializeComponent();

            // Provides access to the Face APIs
            this.faceServiceClient = new FaceServiceClient("827b567f43bb405abf65e9ec3dbdd476");
            // Provides access to the Emotion APIs
            this.emotionServiceClient = new EmotionServiceClient("827b567f43bb405abf65e9ec3dbdd476");

            //const string APIKey = "1617d8c5cf1145fcabe716e600b6b6ae";
            //EmotionServiceClient emotionserviceclient = new EmotionServiceClient(APIKey);


            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(500, 500);
            player = new MediaPlayer();
            playing = false;

        }


        private Task DisplayAlert(string v1, string message, string v2)
        {
            throw new NotImplementedException();
        }

        private async Task<FaceEmotionDetection> DetectFaceAndEmotionsAsync(IRandomAccessStream imageStream)//MediaFile inputFile)
        {
            try
            {
                // Get emotions from the specified stream
                //This method can receive either a stream or a URL as an argument
                Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(imageStream.AsStream());
                // Assuming the picture has one face, retrieve emotions for the
                // first item in the returned array
                var faceEmotion = emotionResult[0]?.Scores.ToRankedList().Cast<FaceEmotionDetection>();
                //tblEmotion.Text = faceEmotion.ToString() ;

                // Create a list of face attributes that the
                // app will need to retrieve
                var requiredFaceAttributes = new FaceAttributeType[] {
                                                FaceAttributeType.Age,
                                                FaceAttributeType.Gender,
                                                FaceAttributeType.Smile,
                                                FaceAttributeType.FacialHair,
                                                FaceAttributeType.HeadPose,
                                                FaceAttributeType.Glasses
                                                };
                // Get a list of faces in a picture
                var faces = await faceServiceClient.DetectAsync(imageStream.AsStream(),
                  false, false, requiredFaceAttributes);
                // Assuming there is only one face, store its attributes
                var faceAttributes = faces[0]?.FaceAttributes;

                FaceEmotionDetection faceEmotionDetection = new FaceEmotionDetection();
                faceEmotionDetection.Age = faceAttributes.Age;
                faceEmotionDetection.Emotion = faceEmotion.FirstOrDefault().Key;
                faceEmotionDetection.Glasses = faceAttributes.Glasses.ToString();
                faceEmotionDetection.Smile = faceAttributes.Smile;
                faceEmotionDetection.Gender = faceAttributes.Gender;
                faceEmotionDetection.Moustache = faceAttributes.FacialHair.Moustache;
                faceEmotionDetection.Beard = faceAttributes.FacialHair.Beard;


                return faceEmotionDetection;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                return null;
            }
        }


        private async void BtnTakePhoto_Click(object sender, RoutedEventArgs e)

        {

            try

            {

                photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

                if (photo == null)

                {

                    return;

                }

                else
                {

                    imageStream = await photo.OpenAsync(FileAccessMode.Read);

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);

                    SoftwareBitmap softwarebitmap = await decoder.GetSoftwareBitmapAsync();

                    SoftwareBitmap softwarebitmapBGRB = SoftwareBitmap.Convert(softwarebitmap,

                    BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    SoftwareBitmapSource bitmapsource = new SoftwareBitmapSource();

                    await bitmapsource.SetBitmapAsync(softwarebitmapBGRB);

                    imgEmotion.Source = bitmapsource;

                }

            }

            catch

            {

                tblEmotion.Text = "Error Campturing Photo";

            }

        }

        private async void BtnEmotions_Click(object sender, RoutedEventArgs e)

        {

            try

            {
                FaceEmotionDetection faceEmotion = await DetectFaceAndEmotionsAsync(imageStream);

                //    emotionresult = await this.emotionServiceClient.RecognizeAsync(imageStream.AsStream());

                //    if (emotionresult != null)

                //    {

                //        EmotionScores score = emotionresult[0].Scores;

                //        tblEmotion.Text = "Your Emotions are : \n" +

                //            "Happiness: " + (score.Happiness) * 100 + " %" + "\n" +

                //            "Sadness: " + (score.Sadness) * 100 + " %" + "\n" +

                //            "Surprise: " + (score.Surprise) * 100 + " %" + "\n" +

                //            "Neutral: " + (score.Neutral) * 100 + " %" + "\n" +

                //            "Anger: " + (score.Anger) * 100 + " %" + "\n" +

                //            "Contempt: " + (score.Contempt) * 100 + " %" + "\n" +

                //            "Disgust: " + (score.Disgust) * 100 + " %" + "\n" +

                //            "Fear: " + (score.Fear) * 100 + " %" + "\n";

                //    }

            }

            catch

            {

                tblEmotion.Text = "Error Returning the Emotions from API";

            }

        }

        private async void PlayMusic_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            StorageFile file = await folder.GetFileAsync("My piano playing 2.m4a");

            player.AutoPlay = false;
            player.Source = MediaSource.CreateFromStorageFile(file);

            if (playing)
            {
                player.Source = null;
                playing = false;
            }
            else
            {
                player.Play();
                playing = true;
            }
        }

    }
}
