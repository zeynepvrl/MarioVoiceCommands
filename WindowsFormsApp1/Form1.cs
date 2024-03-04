using System;
using System.Windows.Forms;
using System.Speech.Recognition;
using WindowsInput;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine speechRecognizer;
        private IKeyboardSimulator keyboardSimulator;
        private bool isCommandInProgress = false;
        private DateTime lastCommandTime = DateTime.MinValue;
        private TimeSpan commandCooldown = TimeSpan.FromSeconds(0.1);
        private RecognitionResult lastRecognitionResult;
        private Timer listeningTimer;

        // for response time
        private DateTime commandStartTime;
        private TimeSpan responseTime;

        public Form1()
        {
            InitializeComponent();
            InitializeSpeechRecognition();
            InitializeTimer();
            DisplayGuide();
            this.TransparencyKey = Color.Magenta;
            this.BackColor = Color.Magenta;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }    

        private void DisplayGuide()
        {
            Guide.Text = "   \r\n   VOICE COMMANDS\r\n\r\n   Pause: Pause the Game\r\n   Play: Continue the Game\r\n   Back : go to the left \r\n   Right : go to the right\r\n   Down: Crouch\r\n   Jump: Jump\r\n   Hop : jump and go right\r\n   Backop : jump and go left\r\n   Stop: stop moving";
        }

        private void HoldKey(WindowsInput.Native.VirtualKeyCode key, int duration)
        {
            keyboardSimulator.KeyDown(key);
            System.Threading.Thread.Sleep(duration);
            keyboardSimulator.KeyUp(key);
        }

        private void HoldKeys(WindowsInput.Native.VirtualKeyCode[] keys, int duration)
        {
            foreach (var key in keys)
            {
                keyboardSimulator.KeyDown(key);
            }

            System.Threading.Thread.Sleep(duration);

            foreach (var key in keys)
            {
                keyboardSimulator.KeyUp(key);
            }
        }

        private void StopCurrentCommand()
        {
            if (isCommandInProgress)
            {
                keyboardSimulator.KeyUp(WindowsInput.Native.VirtualKeyCode.SPACE);
                keyboardSimulator.KeyUp(WindowsInput.Native.VirtualKeyCode.RIGHT);
                keyboardSimulator.KeyUp(WindowsInput.Native.VirtualKeyCode.LEFT);
                isCommandInProgress = false;
            }
        }

        private void InitializeSpeechRecognition()
        {
            speechRecognizer = new SpeechRecognitionEngine();
            Choices commands = new Choices("Jump", "Right", "Back", "Hop", "Backop", "Stop", "Down", "Pause", "Play");
            GrammarBuilder grammarBuilder = new GrammarBuilder(commands);
            Grammar grammar = new Grammar(grammarBuilder);
            speechRecognizer.LoadGrammar(grammar);
            keyboardSimulator = new InputSimulator().Keyboard;

            speechRecognizer.SpeechRecognized += (sender, e) =>
            {
                DateTime now = DateTime.Now;

                if ((now - lastCommandTime) < commandCooldown)
                {
                    return;
                }

                lastCommandTime = now;
                lastRecognitionResult = e.Result;
                ProcessCommand();
            };

            speechRecognizer.SpeechRecognitionRejected += (sender, e) =>
            {
                // Start the timer when speech recognition is rejected
                listeningTimer.Start();
            };

            speechRecognizer.SetInputToDefaultAudioDevice();
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple); // Multiple commands can be recognized
        }

        private void InitializeTimer()
        {
            listeningTimer = new Timer();
            listeningTimer.Interval = 2000; // Set the interval to 2000 milliseconds (2 seconds)
            listeningTimer.Tick += (sender, e) =>
            {
                // Stop the timer and go back to "LISTENING..."
                listeningTimer.Stop();
                textBox1.Clear();
                textBox1.AppendText("\r\nLISTENING...");
            };
        }

        private void ProcessCommand()
        {
            if (lastRecognitionResult != null && !string.IsNullOrEmpty(lastRecognitionResult.Text))
            {
                string command = lastRecognitionResult.Text;

                // Calculate response time
                DateTime now = DateTime.Now;
                responseTime = now - commandStartTime;
                Console.WriteLine("Response Time: " + responseTime.ToString());

                // Display the recognized command for 2 seconds
                textBox1.Clear();
                textBox1.AppendText($"\r\n{command} ");

                // Start the timer to go back to "LISTENING..."
                listeningTimer.Start();
                commandStartTime = now;

                if (command == "Jump")
                {
                    HoldKey(WindowsInput.Native.VirtualKeyCode.SPACE, 2000);
                }
                else if (command == "Right")
                 {
                    HoldKey(WindowsInput.Native.VirtualKeyCode.RIGHT, 1000);
                }
                else if (command == "Back")
                {
                    HoldKey(WindowsInput.Native.VirtualKeyCode.LEFT, 1000);
                }
                else if (command == "Hop")
                {
                    HoldKeys(new[] { WindowsInput.Native.VirtualKeyCode.RIGHT, WindowsInput.Native.VirtualKeyCode.SPACE }, 2000);
                }
                else if (command == "Backop")
                {
                    HoldKeys(new[] { WindowsInput.Native.VirtualKeyCode.LEFT, WindowsInput.Native.VirtualKeyCode.SPACE }, 2000);
                }
                else if (command == "Stop")
                {
                    StopCurrentCommand();
                }
                else if (command == "Down")
                {
                    HoldKey(WindowsInput.Native.VirtualKeyCode.DOWN, 1000);
                }
                else if (command == "Pause") 
                {
                    SendKeys.SendWait("P");
                }
                else if (command == "Play")
                {
                    SendKeys.SendWait("P");
                }
            }
        }
        
    }
}
