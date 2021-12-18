using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NanoMixer {
    class Midi {
        public void run() {
            Debug.WriteLine("Hello WOrld");

            for (int device = 0; device < MidiIn.NumberOfDevices; device++) {
                Debug.WriteLine(MidiIn.DeviceInfo(device).ProductName);
            }

            MidiIn midiIn = new MidiIn(0);
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.ErrorReceived += midiIn_ErrorReceived;
            midiIn.Start();
            Console.ReadLine();
            int tick = 0;
            while (true) {
                tick++;
            }

        }
        void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e) {
            Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));
        }

        void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e) {
            //Debug.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
            //  e.Timestamp, e.RawMessage, e.MidiEvent));

            MidiChange c = EvalMessage(e.MidiEvent);

            Debug.WriteLine(String.Format("Input {0} Value {1}", c.Track, c.Value));
        }

        MidiChange EvalMessage(MidiEvent e) {
            Regex r = new Regex(@"Ch:\s1\s([A-Za-z0-9\s]*)\sValue\s([0-9]*)");
            Match m = r.Match(e.ToString());

            return new MidiChange(LookUpController(m.Groups[1].Value), Int32.Parse(m.Groups[2].Value));
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
