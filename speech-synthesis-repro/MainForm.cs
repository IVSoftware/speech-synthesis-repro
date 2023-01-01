using System.Windows.Forms;
using System.Speech;
using System.Speech.Synthesis;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace speech_synthesis_repro
{
    public partial class MainForm : Form
    {
        SpeechSynthesizer _speech = new SpeechSynthesizer();
        public MainForm() => InitializeComponent();

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // Make 100% sure that the up-down ctrl is decoupled from speak call.
            var say = $"{numericUpDown1.Value}";
            Debug.WriteLine(say);
            // I believe we're inside `Mousedown` and a `ValueChanged` synchronous
            // handlers. Let them finish.
            BeginInvoke((MethodInvoker)delegate
            {
                _speech.SpeakAsyncCancelAll();
                _speech.SpeakAsync(say);
            });
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            // Make 100% sure that the up-down ctrl is decoupled from speak call.
            var say = $"{numericUpDown2.Value}";
            // I don't see why this should make any difference.
            Task.Run(() =>
            {
                _speech.SpeakAsyncCancelAll();
                _speech.SpeakAsync(say);
            });
        }

        /// <summary>
        /// Watchdog timer inspired by NineBerry.
        /// https://stackoverflow.com/a/74975629/5438626
        /// Please accept THAT answer if this solves your issue.
        /// </summary>
        int _changeCount = 0;
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            var captureCount = ++_changeCount;
            var say = $"{numericUpDown3.Value}";
            Task
                .Delay(TimeSpan.FromMilliseconds(250))
                .GetAwaiter()
                .OnCompleted(() => 
                {
                    if(captureCount.Equals(_changeCount))
                    {
                        Debug.WriteLine(say);
                        _speech.SpeakAsyncCancelAll();
                        _speech.SpeakAsync(say);
                    }
                });
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://stackoverflow.com/a/74975629/5438626");
        }
    }
}
