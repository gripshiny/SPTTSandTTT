﻿using Amazon.Polly.Model;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using Octokit.Internal;
using OSCVRCWiz.Resources;
using OSCVRCWiz.Settings;
using OSCVRCWiz.Text;
using Resources;
using SpotifyAPI.Web.Http;
using Swan.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VarispeedDemo.SoundTouch;
using Windows.Media.Protection.PlayReady;
using static OSCVRCWiz.TTS.ElevenLabsTTS;
using static SpotifyAPI.Web.SearchRequest;
using static System.Net.Mime.MediaTypeNames;

namespace OSCVRCWiz.TTS
{
    public class ElevenLabsTTS
    {

        private static readonly HttpClient client = new HttpClient();
        public static Dictionary<string, string> voiceDict =null;
        public static bool elevenFirstLoad = true;
        public static async Task ElevenLabsTextAsSpeech(TTSMessageQueue.TTSMessage TTSMessageQueued, CancellationToken ct = default)
        {

            // if ("tiktokvoice.mp3" == null)
            //   throw new NullReferenceException("Output path is null");
            //text = FormatInputText(text);

            try
            {
                var voiceID = voiceDict.FirstOrDefault(x => x.Value == TTSMessageQueued.Voice).Key;
                //  byte[] result = await CallTikTokAPIAsync(text, voice);
                //  File.WriteAllBytes("TikTokTTS.mp3", result);          
                //  Task.Run(() => PlayAudioHelper());

                MemoryStream memoryStream = new MemoryStream();

                Task<Stream> streamTask = CallElevenLabsAPIAsync(TTSMessageQueued.text, voiceID);
                Stream stream = streamTask.Result;

                AmazonPollyTTS.WriteSpeechToStream(stream, memoryStream);

             

                MemoryStream memoryStream2 = new MemoryStream();
                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);// go to begining before copying
                memoryStream.CopyTo(memoryStream2);

                ///////////////
                ///
                if (VoiceWizardWindow.MainFormGlobal.rjToggleButtonSaveToWav.Checked)
                {
                    MemoryStream memoryStream3 = new MemoryStream();
                    memoryStream.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);// go to begining before copying
                    memoryStream.CopyTo(memoryStream3);

                    memoryStream3.Flush();
                    memoryStream3.Seek(0, SeekOrigin.Begin);// go to begining before copying
                    audioFiles.writeAudioToOutputMp3(memoryStream3);
                }
                //////////


                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);// go to begining before copying
                Mp3FileReader wav = new Mp3FileReader(memoryStream);


                memoryStream2.Flush();
                memoryStream2.Seek(0, SeekOrigin.Begin);// go to begining before copying
                Mp3FileReader wav2 = new Mp3FileReader(memoryStream2);


                //OUTPUT TO FILE
           ////  
                

                //  audioFiles.writeAudioToOutput(memoryStream3);
             /*   using (Mp3FileReader reader = new Mp3FileReader(memoryStream3))
                {
                    using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        WaveFileWriter.CreateWaveFile("TTSVoiceWizard-output.wav", pcmStream);
                    }
                }*/
                ///////


                ///////

                var volume = 5;
                int pitch = 5;
                int rate = 5;
                var volumeFloat = 1f;
                var pitchFloat = 1f;
                var rateFloat = 1f;

                volume = TTSMessageQueued.Volume;
                pitch = TTSMessageQueued.Pitch;
                rate = TTSMessageQueued.Speed;

                volumeFloat = 0.5f + volume * 0.1f;
                pitchFloat = 0.5f + pitch * 0.1f;
                rateFloat = 0.5f + rate * 0.1f;

                bool useTempo = false;
                if (rate != 5)//if rate is changed will use only rate, else use pitch which also changes rate.
                {
                    useTempo = true;
                    pitchFloat = rateFloat;
                }


                var wave32 = new WaveChannel32(wav, volumeFloat, 0f);  //1f volume is normal, keep pan at 0 for audio through both ears
                VarispeedSampleProvider speedControl = new VarispeedSampleProvider(new WaveToSampleProvider(wave32), 100, new SoundTouchProfile(useTempo, false));
                speedControl.PlaybackRate = pitchFloat;
               var AnyOutput = new WaveOut();
                AnyOutput.DeviceNumber = AudioDevices.getCurrentOutputDevice();
                AnyOutput.Init(speedControl);
                AnyOutput.Play();
                ct.Register(async () => AnyOutput.Stop());

                WaveOut AnyOutput2 = new WaveOut();
                VarispeedSampleProvider speedControl_2 = null;
                WaveChannel32 wave32_2 = null;
                if (VoiceWizardWindow.MainFormGlobal.rjToggleButtonUse2ndOutput.Checked == true)//output 2
                {
                    wave32_2 = new WaveChannel32(wav2, volumeFloat, 0f); //output 2
                    wave32_2.PadWithZeroes = false;
                    speedControl_2 = new VarispeedSampleProvider(new WaveToSampleProvider(wave32_2), 2000, new SoundTouchProfile(useTempo, false));//output 2
                    speedControl_2.PlaybackRate = pitchFloat;//output 2
                  //  AnyOutput2 = new WaveOut();
                    AnyOutput2.DeviceNumber = AudioDevices.getCurrentOutputDevice2();
                    AnyOutput2.Init(speedControl_2);
                    AnyOutput2.Play();
                    ct.Register(async () => AnyOutput2.Stop());
                }

                //this is where i would save files
                //  WaveFileWriter.CreateWaveFile(@"TextOut\file.wav", speedControl.ToWaveProvider());


                ct.Register(async () => TTSMessageQueue.PlayNextInQueue());
                float delayTime = pitchFloat;
                if (rate != 5) { delayTime = rateFloat; }
                
                int delayInt = (int)Math.Ceiling((int)wave32.TotalTime.TotalMilliseconds / delayTime);
                Thread.Sleep(delayInt);
                //Thread.Sleep((int)wave32.TotalTime.TotalMilliseconds * 2);// VERY IMPORTANT HIS IS x2 since THE AUDIO CAN ONLY GO AS SLOW AS .5 TIMES SPEED IF IT GOES SLOWER THIS WILL NEED TO BE CHANGED
                Thread.Sleep(100);

             



                AnyOutput.Stop();
                AnyOutput.Dispose();
             //   AnyOutput = null;
                speedControl.Dispose();
                speedControl = null;
                wave32.Dispose();
                wave32 = null;
                wav.Dispose();
                wav = null;
                memoryStream.Dispose();
              //  synthesizerLite.Dispose();
                memoryStream = null;
              //  synthesizerLite = null;

              
                    AnyOutput2.Stop();
                    AnyOutput2.Dispose();
                  //  AnyOutput2 = null;

                if (wave32_2 != null)
                {
                    speedControl_2.Dispose();
                    speedControl_2 = null;
                    wave32_2.Dispose();
                    wave32_2 = null;
                    wav2.Dispose();
                    wav2 = null;
                }
                if (!ct.IsCancellationRequested)
                {
                    TTSMessageQueue.PlayNextInQueue();
                }


            }
            catch (Exception ex)
            {
                OutputText.outputLog("[ElevenLabs TTS Error: " + ex.Message + "]", Color.Red);
                TTSMessageQueue.PlayNextInQueue();

            }
            //System.Diagnostics.Debug.WriteLine("tiktok speech ran"+result.ToString());v
        }

        public static async Task<Stream> CallElevenLabsAPIAsync(string textIn, string voice)
        {

            //modified from https://github.com/connorbutler44/bingbot/blob/main/Service/ElevenLabsTextToSpeechService.cs

            int optimize = 0;
            int stabilities = 0;
            int similarities = 0;
            string modelID = "eleven_monolingual_v1";
            VoiceWizardWindow.MainFormGlobal.Invoke((MethodInvoker)delegate ()
            {
               

                optimize = Int32.Parse(VoiceWizardWindow.MainFormGlobal.comboBoxLabsOptimize.SelectedItem.ToString());
                stabilities = VoiceWizardWindow.MainFormGlobal.trackBarStability.Value;
                similarities = VoiceWizardWindow.MainFormGlobal.trackBarSimilarity.Value;
                modelID = VoiceWizardWindow.MainFormGlobal.comboBoxLabsModelID.SelectedItem.ToString();

                Debug.WriteLine(optimize);
                Debug.WriteLine(stabilities);
                Debug.WriteLine(similarities);
                Debug.WriteLine(modelID);


            });

          
            var similarityFloat = similarities * 0.1f;
            var stabilityFloat =  stabilities * 0.1f;

            var url = $"https://api.elevenlabs.io/v1/text-to-speech/{voice}?optimize_streaming_latency={optimize}";
          //  var url = $"https://api.elevenlabs.io/v1/text-to-speech/{voice}";
            var apiKey = Settings1.Default.elevenLabsAPIKey;

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Content = JsonContent.Create(new { text = textIn,
                model_id = modelID,
                voice_settings = new
                {
                    stability = stabilityFloat,
                    similarity_boost = similarityFloat
                }
            });

           
            request.Headers.Add("xi-api-key", apiKey);
            request.Headers.Add("Accept", "audio/mpeg");

            //  var data = "{\"text\":\"" + text + "\"}";

            HttpResponseMessage response = await client.SendAsync(request);

            System.Diagnostics.Debug.WriteLine("Eleven Response:"+ response.StatusCode);



           
            return await response.Content.ReadAsStreamAsync();

        }
        public class Voice
        {
            public string voice_id { get; set; }
            public string name { get; set; }
        }
        public class Voices
        {
            public List<Voice> voices { get; set; }
        }


        public static void CallElevenVoices()
        {
            try
            {

                //modified from https://github.com/connorbutler44/bingbot/blob/main/Service/ElevenLabsTextToSpeechService.cs


                var url = $"https://api.elevenlabs.io/v1/voices";
            var apiKey = Settings1.Default.elevenLabsAPIKey;
          




        WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("xi-api-key", apiKey);
            using (WebResponse response = request.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {

                    using (var streamReader = new StreamReader(stream))
                    {
                        var result = streamReader.ReadToEnd();
                        // var dataHere = JObject.Parse(result.ToString()).SelectToken("data").ToString();
                        // audioInBase64 = dataHere.ToString();

                        System.Diagnostics.Debug.WriteLine(result.ToString());
                        var json = result.ToString();

                        Voices voices = JsonConvert.DeserializeObject<Voices>(json);
                       voiceDict = voices.voices.ToDictionary(v => v.voice_id, v => v.name);
                    }

                }
            }
                /*foreach (KeyValuePair<string, string> kvp in dict)
                  {
                      Console.WriteLine("Voice ID: " + kvp.Key + ", Name: " + kvp.Value);
                  }*/

                // System.Diagnostics.Debug.WriteLine(audioInBase64);
                // return Convert.FromBase64String(audioInBase64);
                elevenFirstLoad = false;
            }
            catch (Exception ex)
            {
                OutputText.outputLog("[ElevenLabs Voice Load Error: " + ex.Message + "]", Color.Red);
                OutputText.outputLog("[You appear to be using an incorrect ElevenLabs Key, make sure to follow the setup guide: https://github.com/VRCWizard/TTS-Voice-Wizard/wiki/ElevenLabs-TTS ]", Color.DarkOrange);
                TTSMessageQueue.PlayNextInQueue();

            }

        }




    }
}
