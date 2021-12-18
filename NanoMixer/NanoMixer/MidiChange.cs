using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoMixer {
    enum Track {
        Volume1,
        Volume2,
        Volume3,
        Volume4,
        Volume5,
        Volume6,
        Volume7,
        Volume8,
        Skip,
        Reverse,
        Stop,
        Play,
        Record,
        Other
    }
    class MidiChange {
        public Track Track;        
        public int Value;

        public MidiChange(Track track, int value) {
            this.Track = track;
            this.Value = value;
        }
    }
}
