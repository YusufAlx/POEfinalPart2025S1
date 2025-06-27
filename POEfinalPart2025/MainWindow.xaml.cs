using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POE
{
    public partial class MainWindow : Window
    {
        // Assigning variable names and values
        private string userName = "";
        private string userInterest = "";
        private string lastTopic = "";
        private string currentTaskTitle = "";
        private string currentTaskDescription = "";

        private bool isNameCaptured = false;
        private bool awaitingTaskTitle = false;
        private bool awaitingTaskDescription = false;
        private bool awaitingTaskReminder = false;

        private bool quizActive = false;
        private int quizIndex = 0;
        private int quizScore = 0;

        private bool awaitingQuizType = false;
        private string quizType = "";

        private List<string> activityLog = new();
        private Random random = new Random();
        
        //List of questions for mutiple choice

        private List<(string Question, string[] Options, int Correct)> quizQuestions = new()
        {
            ("What is phishing?", new[] { "A type of password", "A hacking tool", "A scam to trick users", "VPN feature" }, 2),
            ("What does HTTPS mean?", new[] { "HyperText Transfer Protocol Secure", "Hyper Transfer Text", "Hack Tool", "Secure Hosting" }, 0),
            ("Strong passwords include?", new[] { "123456", "Pet name", "Name and birthday", "Symbols and numbers" }, 3),
            ("Which is a secure Wi-Fi network?", new[] { "PublicCafeFree", "WPA2-encrypted", "No password", "OpenNet" }, 1),
            ("What should you avoid clicking?", new[] { "Your bookmarks", "Links from unknown emails", "Bank's website", "Trusted ads" }, 1),
            ("What is a VPN?", new[] { "Virtual Private Network", "Virus Protection Net", "Verified Protocol Node", "Video Privacy Name" }, 0),
            ("Which is a sign of phishing?", new[] { "Well-written email", "Email with errors and urgent tone", "From your friend", "Newsletter" }, 1),
            ("What is multi-factor authentication?", new[] { "One password", "Using face ID or code along with your password", "One login method", "Two computers" }, 1),
            ("To avoid malware, you should?", new[] { "Click all ads", "Disable antivirus", "Download from unknown sources", "Update your system" }, 3),
            ("What should you do with suspicious links?", new[] { "Click them to check", "Forward to friends", "Ignore or report", "Bookmark them" }, 2)
        };

        //List of questions for true or false

        private List<(string Question, bool Answer)> trueFalseQuestions = new()
        {
            ("A strong password includes uppercase, lowercase, numbers, and symbols.", true),
            ("It's safe to use the same password on multiple sites.", false),
            ("Phishing emails often create a sense of urgency.", true),
            ("HTTPS is less secure than HTTP.", false),
            ("Using two-factor authentication improves security.", true),
            ("You should click unknown links to test their safety.", false),
            ("Keeping software updated reduces security risks.", true),
            ("Public Wi-Fi is always safe if it has a password.", false),
            ("Antivirus software can help detect malware.", true),
            ("Using your birthday as a password is a good idea.", false)
        };

        //Keywords for cybersecurity questions

        private Dictionary<string, List<string>> responses = new(StringComparer.OrdinalIgnoreCase)
        {
            { "password", new() {
                "Make sure to use strong, unique passwords for each account.",
                "Avoid using personal details in your passwords.",
                "Consider using a password manager to keep your credentials secure."
            }},
            { "phishing", new() {
                "Be cautious of emails asking for personal information.",
                "Don't click on suspicious links in emails or messages.",
                "Always verify the sender's email address."
            }},
            { "privacy", new() {
                "Review privacy settings on all your online accounts regularly.",
                "Limit the amount of personal information you share online.",
                "Use a VPN when accessing public Wi-Fi."
            }},
            { "browsing", new() {
                "Use HTTPS websites whenever possible.",
                "Avoid downloading files from unknown sources.",
                "Keep your browser up to date to patch vulnerabilities."
            }}
        };

        //Method to allow the program to run

        public MainWindow()
        {
            InitializeComponent();
            PlaySound();
            ChatList.Items.Add("   _______     ______   ____ _______ \r\n  / ____\\ \\   / /  _ \\ / __ \\__   __|\r\n | |       \\ \\_/ /| |_) | |  | |     | |   \r\n | |         \\   / |  _ <| |  | |    | |   \r\n| |____      | |  | |_) | |__| |   | |   \r\n  \\_____|  |_|  |____/ \\____/  |_|   ");
            ChatList.Items.Add("💬 Welcome to your personal Cybersecurity expert!");
            ChatList.Items.Add("Please enter your name:");
        }

        //Method to play the sound

        private void PlaySound()
        {
            try
            {
                using SoundPlayer player = new SoundPlayer("C:\\Users\\yusuf\\source\\repos\\POEfinalPart2025\\Greeting.wav");
                player.Play();
            }
            catch
            {
                ChatList.Items.Add("❌ Could not play greeting sound.");
            }
        }

        //Making the send button interactive

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleInput(UserInput.Text.Trim());
            UserInput.Clear();
        }

        //Processing inputed text if the enter button is pressed

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleInput(UserInput.Text.Trim());
                UserInput.Clear();
            }
        }
        
        //Reads the input of the user

        private void HandleInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            ChatList.Items.Add($"🧑 {input}");
            input = input.ToLower();

            if (!isNameCaptured)
            {
                userName = input;
                isNameCaptured = true;
                ChatList.Items.Add($"🤖 Yo, {userName}! I'm here to help you with cybersecurity.");
                return;
            }

            if (awaitingQuizType)
            {
                if (input.Contains("multiple"))
                {
                    quizType = "multiple";
                    StartMultipleChoiceQuiz();
                }
                else if (input.Contains("true") || input.Contains("false"))
                {
                    quizType = "truefalse";
                    StartTrueFalseQuiz();
                }
                else
                {
                    ChatList.Items.Add("🤖 Please type 'multiple choice' or 'true/false'.");
                    return;
                }
                awaitingQuizType = false;
                return;
            }

            if (quizActive && quizType == "truefalse")
            {
                if (input == "true" || input == "false")
                {
                    bool userAnswer = input == "true";
                    if (userAnswer == trueFalseQuestions[quizIndex].Answer)
                        quizScore++;
                    quizIndex++;
                    AskTrueFalseQuestion();
                }
                else
                {
                    ChatList.Items.Add("🤖 Please answer with 'true' or 'false'.");
                }
                return;
            }

            if (quizActive && quizType == "multiple")
            {
                if (int.TryParse(input, out int selected) && selected >= 1 && selected <= 4)
                {
                    if (selected - 1 == quizQuestions[quizIndex].Correct)
                        quizScore++;

                    quizIndex++;
                    AskMultipleChoiceQuestion();
                }
                else
                {
                    ChatList.Items.Add("🤖 Please answer by typing a number between 1 and 4.");
                }
                return;
            }

            if (awaitingTaskTitle)
            {
                currentTaskTitle = input;
                awaitingTaskTitle = false;
                awaitingTaskDescription = true;
                ChatList.Items.Add("🤖 Enter task description:");
                return;
            }

            if (awaitingTaskDescription)
            {
                currentTaskDescription = input;
                awaitingTaskDescription = false;
                awaitingTaskReminder = true;
                ChatList.Items.Add("🤖 Enter reminder (e.g. 2 days or 5 hours):");
                return;
            }

            if (awaitingTaskReminder)
            {
                string reminder = input;
                awaitingTaskReminder = false;

                ChatList.Items.Add($"📌 Task Saved:\n🧾 Title: {currentTaskTitle}\n📋 Description: {currentTaskDescription}\n⏰ Reminder: {reminder}");
                activityLog.Add($"Task added: '{currentTaskTitle}' (Reminder set for {reminder})");

                currentTaskTitle = currentTaskDescription = "";
                return;
            }

            if (input.Contains("remember me") || input.Contains("who am i") || input.Contains("whats my name") || input.Contains("do you know me"))
            {
                ChatList.Items.Add($"🤖 Of course! Your name is {userName}.");
                return;
            }

            if (input.Contains("display") && input.Contains("activity log")|| input.Contains("show") && input.Contains("activity log") || input.Contains("View") && input.Contains("activity log")||input.Contains("activity log"))
            {
                if (activityLog.Count == 0)
                {
                    ChatList.Items.Add("🤖 Activity log is empty.");
                }
                else
                {
                    ChatList.Items.Add("📋 Here's a summary of recent actions:");
                    int count = 1;
                    foreach (var log in activityLog)
                    {
                        ChatList.Items.Add($"{count++}. {log}");
                    }
                }
                return;
            }

            if (input.Contains("more") && responses.ContainsKey(lastTopic))
            {
                ChatList.Items.Add("🧠 Here's more info:");
                GiveResponse(lastTopic);
                return;
            }

            if (input.StartsWith("interested in "))
            {
                userInterest = input.Substring("interested in ".Length).Trim();
                ChatList.Items.Add($"🤖 Got it! I'll remember you're interested in {userInterest}.");
                return;
            }

            if ((input.Contains("add") && input.Contains("task")) || (input.Contains("begin") && input.Contains("task")))
            {
                awaitingTaskTitle = true;
                ChatList.Items.Add("📝 Sure, let's add a task! Enter the task title:");
                return;
            }

            if (input.Contains("start quiz") || input.Contains("begin quiz"))
            {
                ChatList.Items.Add("🧠 Would you like a multiple choice or true/false quiz?");
                awaitingQuizType = true;
                return;
            }

            foreach (var key in responses.Keys)
            {
                if (input.Contains(key))
                {
                    lastTopic = key;
                    GiveResponse(key);
                    return;
                }
            }

            ChatList.Items.Add("❓ I didn't understand that. Try asking about 'password','phishing','browsing','privacy', 'start quiz','activity log', or 'add task'.");
        }

        //Commecing Multiple choice quiz

        private void StartMultipleChoiceQuiz()
        {
            quizActive = true;
            quizIndex = 0;
            quizScore = 0;
            ChatList.Items.Add("🧠 Starting Multiple Choice quiz. Type 1, 2, 3, or 4.");
            AskMultipleChoiceQuestion();
        }

        //Commecing True or false quiz

        private void StartTrueFalseQuiz()
        {
            quizActive = true;
            quizIndex = 0;
            quizScore = 0;
            ChatList.Items.Add("✅ Starting True/False quiz. Type 'true' or 'false'.");
            AskTrueFalseQuestion();
        }

        private void AskMultipleChoiceQuestion()
        {
            if (quizIndex >= quizQuestions.Count)
            {
                quizActive = false;
                activityLog.Add($"Quiz started - {quizIndex} multiple choice questions answered.");
                ChatList.Items.Add($"🎉 Quiz completed! You scored {quizScore}/{quizQuestions.Count}.");
                return;
            }

            var (q, opts, _) = quizQuestions[quizIndex];
            ChatList.Items.Add($"❓ {q}");
            for (int i = 0; i < opts.Length; i++)
                ChatList.Items.Add($"{i + 1}. {opts[i]}");
        }

        private void AskTrueFalseQuestion()
        {
            if (quizIndex >= trueFalseQuestions.Count)
            {
                quizActive = false;
                activityLog.Add($"Quiz started - {quizIndex} true/false questions answered.");
                ChatList.Items.Add($"🎉 Quiz completed! You scored {quizScore}/{trueFalseQuestions.Count}.");
                return;
            }

            var (question, _) = trueFalseQuestions[quizIndex];
            ChatList.Items.Add($"❓ {question} (True/False)");
        }

        //Chatbot responses

        private void GiveResponse(string keyword)
        {
            var options = responses[keyword];
            var response = options[random.Next(options.Count)];
            ChatList.Items.Add($"🤖 {response}");
        }
    }
}
