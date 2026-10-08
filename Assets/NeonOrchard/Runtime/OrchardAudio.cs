using UnityEngine;

namespace NeonOrchard
{
    public sealed class OrchardAudio : MonoBehaviour
    {
        AudioSource music, effects;
        AudioClip tap, eat, crash, win;
        public bool Sound = true;
        void Awake()
        {
            music=gameObject.AddComponent<AudioSource>(); music.loop=true; music.volume=.22f;
            effects=gameObject.AddComponent<AudioSource>(); effects.volume=.48f;
            tap=Tone("Tap",new[]{660f},.065f);
            eat=Tone("Fruit",new[]{784f,1046.5f},.075f);
            crash=Tone("Soft landing",new[]{220f,164.8f,110f},.115f);
            win=Tone("Harvest",new[]{523.25f,659.25f,784f,1046.5f},.12f);
            music.clip=Compose(); music.Play();
        }
        public void Configure(bool musicOn,bool soundOn) { Sound=soundOn; music.mute=!musicOn; }
        public void Tap() { Play(tap); }
        public void Eat() { Play(eat); }
        public void Crash() { Play(crash); }
        public void Win() { Play(win); }
        void Play(AudioClip clip) { if(Sound) effects.PlayOneShot(clip); }
        static AudioClip Tone(string name,float[] notes,float duration)
        {
            const int rate=22050;
            float[] samples=new float[Mathf.CeilToInt(notes.Length*duration*rate)];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate; int n=Mathf.Min(notes.Length-1,(int)(t/duration)); float local=t-n*duration;
                float env=Mathf.Min(local/.005f,1)*Mathf.Pow(1-local/duration,2);
                samples[i]=Mathf.Sin(2*Mathf.PI*notes[n]*local)*env*.45f;
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
        static AudioClip Compose()
        {
            const int rate=22050; const float beat=.375f; const int beats=64;
            float[] s=new float[Mathf.RoundToInt(rate*beat*beats)];
            int[] bass={48,53,57,55}; int[] melody={72,76,79,76,74,77,81,77,76,79,84,79,74,79,83,79};
            for(int b=0;b<beats;b++)
            {
                if(b%2==0) Add(s,rate,b*beat,beat*1.6f,Freq(melody[(b/2)%16]),.13f);
                if(b%4==0)
                {
                    int root=bass[(b/16)%4];
                    Add(s,rate,b*beat,beat*3.7f,Freq(root),.16f);
                    Add(s,rate,b*beat,beat*3.7f,Freq(root+7),.05f);
                    Add(s,rate,b*beat,beat*3.7f,Freq(root+16),.035f);
                }
            }
            var clip=AudioClip.Create("Orchard at dusk - original loop",s.Length,1,rate,false);clip.SetData(s,0);return clip;
        }
        static float Freq(int midi)=>440*Mathf.Pow(2,(midi-69)/12f);
        static void Add(float[] s,int rate,float start,float length,float freq,float volume)
        {
            int offset=Mathf.RoundToInt(start*rate),count=Mathf.RoundToInt(length*rate);
            for(int i=0;i<count && offset+i<s.Length;i++)
            {
                float t=(float)i/rate; float env=Mathf.Min(t/.014f,1)*Mathf.Exp(-3*t/length)*Mathf.Min((length-t)/.045f,1);
                s[offset+i]+=volume*env*(Mathf.Sin(2*Mathf.PI*freq*t)+.2f*Mathf.Sin(4*Mathf.PI*freq*t))*.8f;
            }
        }
        void OnDestroy() { foreach(var clip in new[]{tap,eat,crash,win,music?music.clip:null}) if(clip) Destroy(clip); }
    }
}
