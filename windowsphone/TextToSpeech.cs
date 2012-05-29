using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel.Description;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class TextToSpeech : BingTranslator.LanguageServiceClient
    {
        IsolatedStorageFile store;
        DictionaryRecord record;

        public TextToSpeech(DictionaryRecord r) : base()
        {
            FrameworkDispatcher.Update(); // initialize XNA (required)
            SpeakCompleted += new EventHandler<BingTranslator.SpeakCompletedEventArgs>(TextToSpeech_SpeakCompleted);
            store = IsolatedStorageFile.GetUserStoreForApplication();
            record = r;
        }

        ~TextToSpeech()
        {
            if (store != null)
                store.Dispose();
        }

        string _directory = "audio";
        string AudioFile
        {
            get
            {
                string pinyin = "";
                foreach (Chinese.Character c in record.Chinese.Characters)
                    pinyin += c.Pinyin.Original;
                return String.Format("{0}/{1}.wav", _directory, pinyin);
            }
        }

        public bool AudioFileExists
        {
            get
            {
                return store.FileExists(this.AudioFile);
            }
        }

        string appId = "50A71A52E6F8D1634BDDE49F7AC78CF17FFB6840";
        public bool Speak(bool hifi)
        {
            if (AudioFileExists)
            {
                PlayAudioFile();
                return true;
            }
            else if (NetworkInterface.GetIsNetworkAvailable())
            {
                Debug.WriteLine(String.Format("Requesting {0}-fi audio for {1}", hifi ? "hi" : "lo", record.Chinese.Pinyin));
                SpeakAsync(appId, record.Chinese.Simplified, "zh-chs", "audio/wav", hifi ? "MaxQuality" : "MinSize");
                return true;
            }
            return false;
        }

        void TextToSpeech_SpeakCompleted(object sender, BingTranslator.SpeakCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Debug.WriteLine("TextToSpeech_SpeakCompleted failed: " + e.Error.Message);
                    return;
                }
                WebClient client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler((s, args) =>
                {
                    if (args.Error != null)
                    {
                        Debug.WriteLine("WebClient.OpenReadCompleted failed:" + args.Error);
                        return;
                    }
                    TextToSpeech tts = (TextToSpeech)args.UserState;
                    tts.SaveAudioFile(args.Result);
                    tts.PlayAudioFile();
                });
                client.OpenReadAsync(new Uri(e.Result), sender);
            }
            catch (Exception)
            {
                // currently this fails silently in the background
                // TODO: expose the exception to MainPage (via their chained SpeakCompleted handler?)
            }
        }

        void SaveAudioFile(Stream stream)
        {
            if (!store.DirectoryExists(_directory))
                store.CreateDirectory(_directory);
            IsolatedStorageFileStream file = store.OpenFile(this.AudioFile, FileMode.Create);
            int total = 0;
            int size = 32768;
            byte[] data = new byte[size];
            while ((size = stream.Read(data, 0, data.Length)) != 0)
            {
                file.Write(data, 0, size);
                total += size;
            }
            file.Close();
            Debug.WriteLine(String.Format("SaveAudioFile: {0} ({1} bytes)", this.AudioFile, total));
        }

        void PlayAudioFile()
        {
            Debug.WriteLine("PlayAudioFile: " + this.AudioFile);
            IsolatedStorageFileStream file = store.OpenFile(this.AudioFile, FileMode.Open);
            SoundEffect audio = SoundEffect.FromStream(file);
            audio.Play();
            file.Close();
        }
    }
}
