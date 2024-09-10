using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Quiz_App
{
    public partial class Form1 : Form
    {
        private string apiKey = "API";

        private List<Question> questions = new List<Question>();
        private Dictionary<int, string> selectedAnswers = new Dictionary<int, string>();
        private int currentQuestionIndex = 0;

        TextBox questionTextBox;
        TextBox option1TextBox;
        TextBox option2TextBox;
        TextBox option3TextBox;
        TextBox option4TextBox;
        Label questionNumberLabel;
        Button nextButton;
        Button prevButton;
        Button submitAllButton;
        Label loaderLabel;
        RadioButton radioOption1, radioOption2, radioOption3, radioOption4;
        Label resultLabel;

        public Form1()
        {
            InitializeComponent();
            InitializeQuizUI();
        }

        private void InitializeQuizUI()
        {
            // Input TextBox for entering a topic
            TextBox inputTextBox = new TextBox();
            inputTextBox.Location = new System.Drawing.Point(20, 20);
            inputTextBox.Width = 300;
            inputTextBox.Name = "inputTextBox";
            this.Controls.Add(inputTextBox);

            // Button to submit topic
            Button submitButton = new Button();
            submitButton.Text = "Generate Questions";
            submitButton.Location = new System.Drawing.Point(330, 20);
            submitButton.Click += async (sender, e) => await SubmitTopic(inputTextBox.Text);
            this.Controls.Add(submitButton);

            // Label for showing a loader
            loaderLabel = new Label();
            loaderLabel.Text = "Loading questions...";
            loaderLabel.AutoSize = true;
            loaderLabel.Location = new System.Drawing.Point(20, 50);
            loaderLabel.Visible = false;  // Initially hidden
            this.Controls.Add(loaderLabel);

            // Question Number label
            questionNumberLabel = new Label();
            questionNumberLabel.Location = new System.Drawing.Point(20, 80);
            questionNumberLabel.Width = 300;
            this.Controls.Add(questionNumberLabel);

            // TextBox for Question
            questionTextBox = new TextBox();
            questionTextBox.Location = new System.Drawing.Point(20, 110);
            questionTextBox.Width = 500;
            questionTextBox.Multiline = true;
            questionTextBox.Height = 60;
            this.Controls.Add(questionTextBox);

            // TextBoxes for Options
            option1TextBox = new TextBox { Location = new System.Drawing.Point(50, 180), Width = 500 };
            option2TextBox = new TextBox { Location = new System.Drawing.Point(50, 220), Width = 500 };
            option3TextBox = new TextBox { Location = new System.Drawing.Point(50, 260), Width = 500 };
            option4TextBox = new TextBox { Location = new System.Drawing.Point(50, 300), Width = 500 };

            this.Controls.Add(option1TextBox);
            this.Controls.Add(option2TextBox);
            this.Controls.Add(option3TextBox);
            this.Controls.Add(option4TextBox);

            // Radio buttons for options
            radioOption1 = new RadioButton { Location = new System.Drawing.Point(20, 180) };
            radioOption2 = new RadioButton { Location = new System.Drawing.Point(20, 220) };
            radioOption3 = new RadioButton { Location = new System.Drawing.Point(20, 260) };
            radioOption4 = new RadioButton { Location = new System.Drawing.Point(20, 300) };

            radioOption1.CheckedChanged += Option_CheckedChanged;
            radioOption2.CheckedChanged += Option_CheckedChanged;
            radioOption3.CheckedChanged += Option_CheckedChanged;
            radioOption4.CheckedChanged += Option_CheckedChanged;

            this.Controls.Add(radioOption1);
            this.Controls.Add(radioOption2);
            this.Controls.Add(radioOption3);
            this.Controls.Add(radioOption4);

            // Buttons for navigation
            prevButton = new Button { Text = "Previous", Location = new System.Drawing.Point(20, 340) };
            prevButton.Click += PrevButton_Click;
            this.Controls.Add(prevButton);

            nextButton = new Button { Text = "Next", Location = new System.Drawing.Point(100, 340) };
            nextButton.Click += NextButton_Click;
            this.Controls.Add(nextButton);

            submitAllButton = new Button { Text = "Submit All", Location = new System.Drawing.Point(180, 340) };
            submitAllButton.Click += SubmitAllButton_Click;
            this.Controls.Add(submitAllButton);

            // Label for showing result
            resultLabel = new Label();
            resultLabel.Location = new System.Drawing.Point(20, 380);
            resultLabel.AutoSize = true;
            this.Controls.Add(resultLabel);
        }

        private void Option_CheckedChanged(object sender, EventArgs e)
        {
            // Store selected answer
            if (radioOption1.Checked)
                selectedAnswers[currentQuestionIndex] = option1TextBox.Text;
            else if (radioOption2.Checked)
                selectedAnswers[currentQuestionIndex] = option2TextBox.Text;
            else if (radioOption3.Checked)
                selectedAnswers[currentQuestionIndex] = option3TextBox.Text;
            else if (radioOption4.Checked)
                selectedAnswers[currentQuestionIndex] = option4TextBox.Text;
        }

        private async Task SubmitTopic(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("Please enter a topic.");
                return;
            }

            try
            {
                loaderLabel.Visible = true;

                string questionsText = await GetQuizQuestions(topic);
                loaderLabel.Visible = false;

                ParseQuestions(questionsText);
                DisplayCurrentQuestion();
            }
            catch (Exception ex)
            {
                loaderLabel.Visible = false;
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task<string> GetQuizQuestions(string topic)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4",
                    messages = new object[]
                    {
                        new { role = "system", content = "You are a helpful assistant." },
                        new { role = "user", content = $"Generate 10 multiple-choice questions with 4 options each on the topic '{topic}'. Include the correct answer for each question." }
                    },
                    max_tokens = 3000
                };

                var json = JObject.FromObject(requestBody).ToString();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(responseBody);

                return result["choices"][0]["message"]["content"].ToString().Trim();
            }
        }

        private void ParseQuestions(string questionsText)
        {
            var questionBlocks = questionsText.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in questionBlocks)
            {
                var lines = block.Split('\n');
                var questionText = lines[0];
                var options = new List<string> { lines[1], lines[2], lines[3], lines[4] };

                string correctAnswer = options[2]; // Assuming correct answer is always C for testing.

                questions.Add(new Question { QuestionText = questionText, Options = options, CorrectAnswer = correctAnswer });
            }
        }

        private void DisplayCurrentQuestion()
        {
            if (questions.Count == 0) return;

            var currentQuestion = questions[currentQuestionIndex];
            questionNumberLabel.Text = $"Question {currentQuestionIndex + 1} out of {questions.Count}";
            questionTextBox.Text = currentQuestion.QuestionText;
            option1TextBox.Text = currentQuestion.Options[0];
            option2TextBox.Text = currentQuestion.Options[1];
            option3TextBox.Text = currentQuestion.Options[2];
            option4TextBox.Text = currentQuestion.Options[3];

            // Restore the selected answer for this question
            if (selectedAnswers.ContainsKey(currentQuestionIndex))
            {
                string selectedOption = selectedAnswers[currentQuestionIndex];
                radioOption1.Checked = selectedOption == option1TextBox.Text;
                radioOption2.Checked = selectedOption == option2TextBox.Text;
                radioOption3.Checked = selectedOption == option3TextBox.Text;
                radioOption4.Checked = selectedOption == option4TextBox.Text;
            }
            else
            {
                // Clear radio buttons
                radioOption1.Checked = false;
                radioOption2.Checked = false;
                radioOption3.Checked = false;
                radioOption4.Checked = false;
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (currentQuestionIndex < questions.Count - 1)
            {
                currentQuestionIndex++;
                DisplayCurrentQuestion();
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (currentQuestionIndex > 0)
            {
                currentQuestionIndex--;
                DisplayCurrentQuestion();
            }
        }

        private void SubmitAllButton_Click(object sender, EventArgs e)
        {
            int correctAnswersCount = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                if (selectedAnswers.ContainsKey(i))
                {
                    string selectedAnswer = selectedAnswers[i];
                    string correctAnswer = questions[i].CorrectAnswer;

                    if (string.Equals(selectedAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswersCount++;
                    }
                }
            }

            resultLabel.Text = $"Score: {correctAnswersCount}/{questions.Count}";

            // Display all questions and indicate if answers were correct or wrong
            string resultMessage = "Results:\n";
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                string selectedAnswer = selectedAnswers.ContainsKey(i) ? selectedAnswers[i] : "No Answer";
                string correctAnswer = question.CorrectAnswer;

                resultMessage += $"{i + 1}. {question.QuestionText}\n";
                resultMessage += $"Selected Answer: {selectedAnswer}\n";
                resultMessage += $"Correct Answer: {correctAnswer}\n";
                resultMessage += selectedAnswer == correctAnswer ? "Correct\n" : "Wrong\n";
                resultMessage += "\n";
            }

            MessageBox.Show(resultMessage);
        }
    }

    public class Question
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
