﻿
using NAudio.CoreAudioApi;
using NAudio.Wave;
using OSCVRCWiz;
using OSCVRCWiz.Resources;
using OSCVRCWiz.Settings;
using OSCVRCWiz.Text;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using VarispeedDemo.SoundTouch;
//using NAudio.CoreAudioApi;




namespace Resources
{
    public class AudioDevices
    {
        public static int audioOutputIndex = -1;
        public static List<string> comboIn = new List<string>();
        public static List<string> comboOut = new List<string>();
        public static List<string> micIDs = new List<string>();
        public static List<string> speakerIDs = new List<string>();
        public static string currentInputDevice = "";
        public static string currentOutputDevice = "";
        public static string currentInputDeviceName = "Default";
        public static string currentOutputDeviceName = "Default";
        // public static int currentOutputDeviceLite = 0;

        public static string currentOutputDevice2nd = "";
        public static string currentOutputDeviceName2nd = "Default";




        public static void NAudioSetupInputDevices()
        {
            VoiceWizardWindow.MainFormGlobal.comboBoxInput.Items.Clear();
            comboIn.Clear();
            micIDs.Clear();


            comboIn.Add("Default");
            micIDs.Add("Default");
           
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Capture, NAudio.CoreAudioApi.DeviceState.Active))
            {
                System.Diagnostics.Debug.WriteLine("{0} ({1})", endpoint.FriendlyName, endpoint.ID);
                comboIn.Add(endpoint.FriendlyName);
                micIDs.Add(endpoint.ID);

            }
          

            foreach (var i in comboIn)
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxInput.Items.Add(i);
            }

            try
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxInput.SelectedItem = Settings1.Default.MicName;
            }
            catch
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxInput.SelectedItem = "Default";
            }
        }
        public static void NAudioSetupOutputDevices()
        {
            VoiceWizardWindow.MainFormGlobal.comboBoxOutput.Items.Clear();
            VoiceWizardWindow.MainFormGlobal.comboBoxOutput2.Items.Clear();//forgor this :p
            comboOut.Clear();
            speakerIDs.Clear();


            comboOut.Add("Default");
            speakerIDs.Add("Default");
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.Active))
            {
                System.Diagnostics.Debug.WriteLine("{0} ({1})", endpoint.FriendlyName, endpoint.ID);

                comboOut.Add(endpoint.FriendlyName);
                speakerIDs.Add(endpoint.ID);
            }
            foreach (var i in comboOut)
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput.Items.Add(i);
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput2.Items.Add(i);
            }

  
            try
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput.SelectedItem = Settings1.Default.SpeakerName;
            }
            catch
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput.SelectedItem = "Default";
            }
            try
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput2.SelectedItem = Settings1.Default.SpeakerName2;
            }
            catch
            {
                VoiceWizardWindow.MainFormGlobal.comboBoxOutput2.SelectedItem = "Default";
            }



        }
        public static int getCurrentInputDevice() {

            // Setting to Correct Input Device
            int waveInDevices = WaveIn.DeviceCount;
            //InputDevicesDict.Clear();
           Dictionary<string, int> DevicesDict = new Dictionary<string, int>();
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                DevicesDict.Add(deviceInfo.ProductName, waveInDevice);
            }
           
            foreach (var kvp in DevicesDict)
            {
                if (AudioDevices.currentInputDeviceName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                   
                }
            }
            return 0;

        }

       





        public static int getCurrentOutputDevice()
        {
            try
            {
                // Setting to Correct Input Device
                int waveDevices = WaveOut.DeviceCount;
                //InputDevicesDict.Clear();
                Dictionary<string, int> DevicesDict = new Dictionary<string, int>();
                for (int waveDevice = 0; waveDevice < waveDevices; waveDevice++)
                {
                    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveDevice);
                    DevicesDict.Add(deviceInfo.ProductName, waveDevice);
                }

                foreach (var kvp in DevicesDict)
                {
                    if (AudioDevices.currentOutputDeviceName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;

                    }
                }
            }
            catch (Exception ex)
            {
                OutputText.outputLog("[" +ex.Message+"]", Color.Red);
                OutputText.outputLog("[ For the 'An item with the same key has already been added' error go to Control Panel > Sound > right click on one of the devices with the same name > properties > rename the device.]", Color.DarkOrange);
                OutputText.outputLog("[Defaulting to output device 0]", Color.DarkOrange);
                

                return 0;
            }
            return 0;

        }
        public static int getCurrentOutputDevice2()
        {
            try
            {

                // Setting to Correct Input Device
                int waveDevices = WaveOut.DeviceCount;
                //InputDevicesDict.Clear();
                Dictionary<string, int> DevicesDict = new Dictionary<string, int>();
                for (int waveDevice = 0; waveDevice < waveDevices; waveDevice++)
                {
                    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveDevice);
                    DevicesDict.Add(deviceInfo.ProductName, waveDevice);
                }

                foreach (var kvp in DevicesDict)
                {
                    if (AudioDevices.currentOutputDeviceName2nd.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;

                    }
                }
            }
            catch (Exception ex)
            {
                OutputText.outputLog("[" + ex.Message + "]", Color.Red);
                OutputText.outputLog("[ For the 'An item with the same key has already been added' error go to Control Panel > Sound > right click on one of the devices with the same name > properties > rename the device.]", Color.DarkOrange);
                OutputText.outputLog("[Defaulting to output device 0]", Color.DarkOrange);

                return 0;
            }
            return 0;

        }
     

    }
}
