Let's assume you have taken Neil's excellent comment into consideration, and checked the repeat rate of the `NumericUpDown` control on the other PCs "without" calling the speech engine. Good.

Your code looks right. The `SpeakAsyncCancelAll` and `SpeakAsync` do not block and are "expected" to be running on a background thread. When I attempted to reproduce the problem (not a shocker) your code works fine on my PC using the test condition you describe. That being the case, maybe you could try a few variations on the slim chance that something makes a difference and yields some kind of clue by ruling out some unlikely issues.

![screenshot](https://github.com/IVSoftware/speech-synthesis-repro/blob/master/speech-synthesis-repro/Screenshots/screenshot.png)


***
**Variation 1**

Capture the "text to say" and post the work using `BeginInvoke`. This ensures that nothing could possibly be interfering with the `ValueChanged` or `MouseDown` messages from pumping in the message queue.

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
        // Make 100% sure that the up-down ctrl is decoupled from speak call.
        var say = $"{numericUpDown1.Value}";

        // Ruling out an unlikely problem
        BeginInvoke((MethodInvoker)delegate
        {
            _speech.SpeakAsyncCancelAll();
            _speech.SpeakAsync(say);
        });
    }

***
**Variation 2**

Since you have a suspicion that something is running on the UI thread that shouldn't be, go ahead and give explicit instructions to post it on a background Task. At least we can rule that out.

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
        // Make 100% sure that the up-down ctrl is decoupled from speak call.
        var say = $"{numericUpDown2.Value}";

        // Ruling out an unlikely problem
        Task.Run(() =>
        {
            _speech.SpeakAsyncCancelAll();
            _speech.SpeakAsync(say);
        });
    }

***
**Variation 3 - Inspired by NineBerry's [answer](https://stackoverflow.com/a/74975629/5438626)** (added to test code project repo)

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

