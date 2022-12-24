using System.Text.RegularExpressions;
namespace WordGuessingGame
{
    class Program
    {
        public static Mutex MyMutex= new Mutex(false);
        class Guesser
        {
            private readonly string _sentence;
            public Guesser(string sentence)
            {
                if (sentence != null && !(Regex.Match(sentence, "^[A-Za-z ]+$").Success))
                    throw new Exception(
                        "Sentence may contain only lowercase and uppercase letters of the English alphabet and spaces");
                //replace multiple spaces with one
                sentence = Regex.Replace(sentence, @"\s+", " ");
                _sentence = sentence;
            }

            public bool GuessWord(string word,int start, int end )
            {
                Console.WriteLine($"Compare words {_sentence.Substring(start, end-start+1)} and {word}");
                return string.Equals(_sentence.Substring(start, end - start + 1), word);
            }

            //check if user guessed the sentence
            public  bool CheckResult(string? sentence)
            {
                return string.Equals("puppy drives stupid", sentence);
            }
        }

        class Game
        {
            public object Play(Guesser guess, int start, int end)
            {
                MyMutex.WaitOne();
                Console.WriteLine("{0} has entered in the critical section", Thread.CurrentThread.Name);
                var words = new List<string> 
                {
                    "puppy",
                    "drives",
                    "stupid"
                };
                foreach (var word in words)
                {
                    var resultOfSearch = guess.GuessWord(word, start, end);
                    if (resultOfSearch)
                    {
                        //thread exit
                        Console.WriteLine($"word {word} found, {Thread.CurrentThread.Name} left critical section");
                        MyMutex.ReleaseMutex();
                        return word;
                    }
                    else
                        Console.WriteLine($"word {word} not found");
                }
                Console.WriteLine("{0} has left the critical section", Thread.CurrentThread.Name);
                MyMutex.ReleaseMutex();
                return 0;
            }
        }
        struct Params
        {
            public int A, B;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the sentence:");
            string sen = Console.ReadLine();
            sen = Regex.Replace(sen, @"\s+", " ");
            Console.WriteLine($"You entered (without multiple spaces): {sen}");
            try
            {
                var listOfWords = sen.Split(" ");
                int n = listOfWords.Length;
                if ( n > 1)
                {
                    Params[] parameters = new Params[n];
                    var guess = new Guesser(sen);
                    var game = new Game();
                    object res = "";
                    for (int i = 0; i < n; ++i)
                    {
                        if (i == 0)
                        {
                            parameters[i].A = listOfWords[i].LastIndexOf(" ") + 1;
                            parameters[i].B = listOfWords[i].Length - 1;
                        }
                        else
                        {
                            parameters[i].A = parameters[i - 1].B + 2;
                            parameters[i].B = parameters[i].A + listOfWords[i].Length - 1;
                        }
                        Console.WriteLine($"A: {parameters[i].A} B: {parameters[i].B}\n");
                    }
                    for (int i = 0; i < n; i++)
                    {
                        var thr = new Thread(() => { res += game.Play(guess, parameters[i].A, parameters[i].B).ToString(); });
                        thr.Name = $"Thread #{i + 1}";
                        thr.Start();
                        thr.Join();
                        if (i != n - 1)
                            res += " ";
                    }
                    Console.WriteLine($"Result: {res}");
                    if (guess.CheckResult(res.ToString()))
                        Console.WriteLine("you won");
                    else
                        Console.WriteLine("you lost");
                }
                else
                {
                    throw new ("Enter at least 2 words!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
        }

    }
}