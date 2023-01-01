using System.Windows.Forms;
using System.Speech;
using System.Speech.Synthesis;
using System;
using System.Threading.Tasks;

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
    }
}
