using NAudio.Wave;
using NAudio.Codecs;
using NAudio.CoreAudioApi;
using NAudio.Mixer;
using NAudio.Utils;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace NanoMixer {
    class Midi {
        public void Run() {
            Debug.WriteLine("Hello WOrld");

            for (int device = 0; device < MidiIn.NumberOfDevices; device++) {
                Debug.WriteLine(MidiIn.DeviceInfo(device).ProductName);
            }

            MidiIn midiIn = new MidiIn(0);
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.ErrorReceived += midiIn_ErrorReceived;
            midiIn.Start();
            Console.ReadLine();

            lastCall = DateTime.Now;

            //Force the application to not go to sleep
            //TODO: This prevents the main windows from opening
            while (true) { }

        }
        void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e) {
            Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e) {
            //Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
            //  e.Timestamp, e.RawMessage, e.MidiEvent));

            MidiChange c = EvalMessage(e.MidiEvent);

            HandleMessage(c);

            Debug.WriteLine(String.Format("Input {0} Value {1}", c.Track, c.Value));
        }

        MidiChange EvalMessage(MidiEvent e) {
            Regex r = new Regex(@"Ch:\s1\s([A-Za-z0-9\s]*)\sValue\s([0-9]*)");
            Match m = r.Match(e.ToString());

            return new MidiChange(LookUpController(m.Groups[1].Value), Int32.Parse(m.Groups[2].Value));
        }

        Dictionary<string, int[]> pids = new Dictionary<string, int[]>();
        int targetValue = 0;
        DateTime lastCall;
        bool busy = false;

        async void SetTargetValue(string processName, int value) {
            targetValue = value;

            Double elapsedMillisecs = ((TimeSpan)(DateTime.Now - lastCall)).TotalMilliseconds;
            if (elapsedMillisecs<100) return;

            lastCall = DateTime.Now;

            busy = true;
            //Thread.Sleep(10);
            Debug.WriteLine("setting volume");
            foreach (int pid in GetPids(processName)) {
                new Thread(() => {
                    Thread.CurrentThread.IsBackground = true;
                    AudioManager.SetApplicationVolume(pid, MapVolume(targetValue));
                }).Start();
            }
            busy = false;

        }


        void HandleMessage(MidiChange c) {
            switch (c.Track) {
                case Track.Play:
                    if (c.Value == 127) { Playback.PlayPause(); }
                    break;
                case Track.Skip:
                    if (c.Value == 127) { Playback.Skip(); }
                    break;
                case Track.Reverse:
                    if (c.Value == 127) { Playback.Reverse(); }
                    break;
                case Track.Volume7:
                    string procesName = "Spotify";
                    Debug.WriteLine(c.Value);
                    SetTargetValue(procesName, c.Value);

                    /*
                    foreach (int pid in GetPids(procesName)) {
                        
                        new Thread(() => {
                            Thread.CurrentThread.IsBackground = true;
                            try {
                                AudioManager.SetApplicationVolume(pid, MapVolume(c.Value));

                            } catch (Exception) { }
                        }).Start();
                       

                    }
                     */

                    break;

                case Track.Volume8:
                    /*
                    try {
                        NAudio.CoreAudioApi.MMDeviceEnumerator MMDE = new NAudio.CoreAudioApi.MMDeviceEnumerator();

                        MMDeviceCollection DevCol = MMDE.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

                        foreach (MMDevice dev in DevCol) {
                            try {
                                float newVolume = (float) MapVolume(c.Value) / (float) 100.0;

                                //dev.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;

                                var sessions = dev.AudioSessionManager.Sessions;
                                for (int i = 0; i < sessions.Count; i++) {
                                    Process process = Process.GetProcessById((int)sessions[i].GetProcessID);
                                    if (process.ProcessName == "Spotify" && !String.IsNullOrEmpty(process.MainWindowTitle)) {
                                        var pID = process.Id;
                                        sessions[i].SimpleAudioVolume.Volume = newVolume;
                                    }
                                }

                                Debug.WriteLine("Volume of " + dev.DeviceFriendlyName + " is " + dev.AudioEndpointVolume.MasterVolumeLevelScalar.ToString());
                            } catch (Exception ex) {
                                Debug.WriteLine(dev.DeviceFriendlyName + " could not be muted " + ex);
                            }
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine("Error: " + ex.Message);
                    }
                    */

                    break;
            }

        }

        int[] GetPids(string process) {
            if (pids.ContainsKey(process)) {
                return pids[process];
            } else {
                int[] lookup = GetPidsByName(process);
                pids.Add(process, lookup);
                return lookup;
            }
        }

        int[] GetPidsByName(string name) {
            Process[] pList = Process.GetProcessesByName(name);
            List<int> ids = new List<int>();
            foreach (Process p in pList) {
                ids.Add(p.Id);
            }
            return ids.ToArray();
        }

        int MapVolume(int volume) {
            return 0 + (volume - 0) * (100 - 0) / (127 - 0);
        }

        Track LookUpController(string interfaceName) {
            switch (interfaceName) {
                case "Controller BankSelect":
                    return Track.Volume1;

                case "Controller Modulation":
                    return Track.Volume2;

                case "Controller BreathController":
                    return Track.Volume3;

                case "Controller 3":
                    return Track.Volume4;

                case "Controller FootController":
                    return Track.Volume5;

                case "Controller 5":
                    return Track.Volume6;

                case "Controller 6":
                    return Track.Volume7;

                case "Controller MainVolume":
                    return Track.Volume8;

                case "Controller 43":
                    return Track.Reverse;

                case "Controller 44":
                    return Track.Skip;

                case "Controller 42":
                    return Track.Stop;

                case "Controller 41":
                    return Track.Play;

                case "Controller 45":
                    return Track.Record;

                default:
                    return Track.Other;
            }
        }
    }
}
