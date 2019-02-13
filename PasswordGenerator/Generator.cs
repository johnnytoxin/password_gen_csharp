using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace PasswordGenerator
{
    class Generator
    {
        private const string Title = "Johnny Toxin Password Generator";
        private const int SALT_BYTE_SIZE = 16;
        private const int HASH_BYTE_SIZE = 16;
        private const int HASHING_ITERATIONS = 1991;
        private const string RESOURCE_FOLDER = "Resources";
        private const string PLACES_FILE_NAME = "places.txt";
        private const string TITLES_FILE_NAME = "titles.txt";
        private const string ANIMALS_FILE_NAME = "animals.txt";
        // Used to generate random numbers. Instantiated on startup for better randomness
        private static readonly Random _random = new Random();

        //a 'char' array holding all the letters of the alphabet
        private static readonly char[] _lowercaseAlphabet = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        //a 'char' array holding all the letters of the alphabet
        private static readonly char[] _capitalAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        //a 'char' array holding all single digit numbers
        private static readonly char[] _numbersZeroToNine = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        //a 'char' array holding common special characters
        private static readonly char[] _specialCharacters = { '?', '_', '-', '!', '@', '$', '#', '&' };

        //as stated in the lessons, this tells the compiler that this script is the main piece of the program
        static void Main(string[] args)
        {
            //a title to appear on the window
            Console.Title = Title + " v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            RunGenerator();
        }

        static void RunGenerator()
        {
            //a variable type that stores keyboard key information
            //this type points to a particular key that has a machine code identification path 
            //(don't quote me here)
            ConsoleKeyInfo exitOpt = new ConsoleKeyInfo();
            //a do-while loop
            //all task inside the loop will be resolved, then check against a condition
            //     if the condition is still false, the loop continues
            //     else, the condition cause the loop to end and continue past the while statement
            do
            {
                Console.WriteLine(Title);
                Console.WriteLine();
                ConsoleKey key = ChooseOption();

                if (key == ConsoleKey.D1)
                {
                    try
                    {
                        GenerateWordKey();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n\n");
                        // Run start again
                        continue;
                    }
                }
                else if (key == ConsoleKey.D2)
                {
                    try
                    {
                        GenerateCharacterKey();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n\n");
                        // Run start again
                        continue;
                    }
                }
                else if (key == ConsoleKey.D3)
                {
                    Console.Clear();
                    try
                    {
                        string fileFullPath = string.Empty;
                        string masterPassword = string.Empty;

                        GetUserInputForDecryption(out fileFullPath, out masterPassword);
                        DecryptFile(fileFullPath, masterPassword);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\n" + e.Message + "\n\n");
                        continue;
                    }
                }
                else if (key == ConsoleKey.Q)
                {
                    break;
                }

                bool afterFirst = false;
                do
                {
                    if (afterFirst == true)
                    {
                        Console.Clear();
                        Console.WriteLine("You must enter a valid selection.");
                    }
                    Console.WriteLine("\nTo return to the main menu, type 'm'. To quit, type 'q'.");
                    exitOpt = Console.ReadKey();
                    Console.WriteLine("\n");

                    afterFirst = true;
                } while (exitOpt.Key != ConsoleKey.M && exitOpt.Key != ConsoleKey.Q);

                Console.Clear();

            } while (exitOpt.Key != ConsoleKey.Q);
        }

        static void GenerateWordKey()
        {
            Console.Clear();

            bool firstOrSecond;
            string generatedPassword = string.Empty;
            string uniqueTitle = GetStringFromFile(TITLES_FILE_NAME);
            string uniquePlace = GetStringFromFile(PLACES_FILE_NAME);
            string thirdString = GetStringFromFile(ANIMALS_FILE_NAME);

            char numberForPassword = _numbersZeroToNine[GetRandomIntUpTo(_numbersZeroToNine.Length - 1)];
            firstOrSecond = FlipACoin();

            Console.Write("Your new password is:\t");
            generatedPassword = uniqueTitle
                                + ((firstOrSecond) ? "" : "_")
                                + uniquePlace
                                + ((firstOrSecond) ? "_" : "")
                                + thirdString
                                + numberForPassword;
            Console.WriteLine(generatedPassword);

            string fileFullPath = string.Empty;
            string masterPassword = string.Empty;
            GetFilePathAndMasterPassword(out fileFullPath, out masterPassword);
            EncryptFile(fileFullPath, generatedPassword, masterPassword);
            Console.Clear();
            Console.WriteLine("Your password file has been encrypted successfully.");
        }

        static void GenerateCharacterKey()
        {
            Console.Clear();

            const int minimumLength = 8; // average length minimum for password
            // TODO: Replace this with better randomizing
            const int randomRange = 6;
            int length = 0;
            char[] pass;
            string generatedPassword = string.Empty;

            while (length < minimumLength)
            {
                Console.Write("Enter password length (minimum: 8): ");
                try
                {
                    length = int.Parse(Console.ReadLine());
                    if (length < minimumLength)
                        Console.WriteLine("\nLength must be greater than " + minimumLength + ". Please enter again.\n");
                }
                catch (FormatException)
                {
                    Console.WriteLine("\nEntry can only be a numeric value. Please enter again.\n");
                }
            }

            char special = 'a';
            // initial spacing
            Console.WriteLine();
            while (special != 'y' && special != 'n')
            {
                Console.Write("Do you want special characters [y/n]?: ");
                try
                {
                    special = char.Parse(Console.ReadLine());
                    special = char.ToLower(special);
                    if (special != 'y' && special != 'n')
                        Console.WriteLine("That is not an acceptable entry. Please enter again.\n");
                }
                catch (FormatException e)
                {
                    Console.WriteLine("An error has occurred. " + e.Message + " Please enter again.\n");
                }
            }
            pass = new char[length];
            for (int i = 0; i < length; i++)
            {
                // First letter is always a capital letter
                if (i == 0)
                {
                    pass[i] = GenerateLetter(i, false, _capitalAlphabet);
                }
                // Generate a random alphabetical character or special character
                else if (i > 0 && i < length - 1)
                {
                    int randomInt = GetRandomIntUpTo(randomRange);
                    pass[i] = GenerateLetter(randomInt, special.Equals('y'), _lowercaseAlphabet);
                }
                // Generate a guaranteed special character as the last character
                else if (i == length - 1 && special.Equals('y'))
                {
                    pass[i] = GenerateLetter(randomRange, true, _lowercaseAlphabet);
                }
                // Generate a guaranteed alphabetical character as the last character
                else if (i == length - 1 && special.Equals('n'))
                {
                    int randomInt = GetRandomIntUpTo(randomRange);
                    pass[i] = GenerateLetter(randomInt, false, _lowercaseAlphabet);
                }
            }

            Console.Clear();
            Console.Write("Your new password is:\t");
            for (int j = 0; j < pass.Length; j++)
            {
                generatedPassword += pass[j];
            }
            Console.WriteLine(generatedPassword);

            string fileFullPath = string.Empty;
            string masterPassword = string.Empty;
            GetFilePathAndMasterPassword(out fileFullPath, out masterPassword);
            EncryptFile(fileFullPath, generatedPassword, masterPassword);
            Console.Clear();
            Console.WriteLine("Your password file has been encrypted successfully!");
        }

        static void GetUserInputForDecryption(out string fileFullPath, out string password)
        {
            fileFullPath = string.Empty;
            password = string.Empty;

            try
            {
                Console.Write("Please enter the full path with filename and extension: ");
                fileFullPath = Console.ReadLine();
                if (fileFullPath == string.Empty)
                {
                    Console.WriteLine("\nFile path is empty.\n");
                    GetUserInputForDecryption(out fileFullPath, out password);
                }

                FileInfo file = new FileInfo(fileFullPath);
                if (!file.Exists)
                {
                    Console.WriteLine("\nFile not found.\n");
                    GetUserInputForDecryption(out fileFullPath, out password);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message + "\n");
                GetUserInputForDecryption(out fileFullPath, out password);
            }
            password = GetHiddenPasswordEntry();
        }

        static void GetFilePathAndMasterPassword(out string fileFullPath, out string password)
        {
            fileFullPath = string.Empty;
            password = string.Empty;

            try
            {
                Console.Write("\nPlease enter the full path with filename and extension: ");
                fileFullPath = Console.ReadLine();
                if (fileFullPath == string.Empty)
                {
                    Console.WriteLine("\nFile path is empty.");
                    GetFilePathAndMasterPassword(out fileFullPath, out password);
                }

                string directory = Path.GetDirectoryName(fileFullPath);
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine("\nDirectory does not exist.");
                    Console.WriteLine($"Creating directory '{directory}'");
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message + "\n");
                GetFilePathAndMasterPassword(out fileFullPath, out password);
            }
            password = GetHiddenPasswordEntry();
        }

        static string GetHiddenPasswordEntry()
        {
            string password = string.Empty;
            const char star = '*';
            while (password == string.Empty)
            {
                Console.Write("\nPlease enter a password for this file: ");
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (password.Length > 0)
                        {
                            password = password.Substring(0, password.Length - 1);
                            Console.Write("\b \b"); // backspaces, writes an empty character, then backspaces again
                        }
                    }
                    else if (char.IsLetterOrDigit(key.KeyChar)
                        || char.IsSymbol(key.KeyChar)
                        || char.IsPunctuation(key.KeyChar))
                    {
                        password += key.KeyChar;
                        Console.Write(star);
                    }
                }
                if (string.IsNullOrWhiteSpace(password))
                    Console.WriteLine("\nPassword cannot be empty.");
            }
            return password;
        }

        static void EncryptFile(string fullPath, string generatedPassword, string key)
        {
            string EncryptionKey = key;
            byte[] clearBytes = Encoding.Unicode.GetBytes(generatedPassword);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    using (CryptoStream cs = new CryptoStream(fileStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                }
            }
        }

        static void DecryptFile(string fullPath, string key)
        {
            string EncryptionKey = key;
            byte[] cipherBytes = File.ReadAllBytes(fullPath);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    var text = Encoding.Unicode.GetString(ms.ToArray());
                    Console.Clear();
                    Console.WriteLine("The file has been decrypted successfully!");
                    Console.WriteLine("\nYour password is: " + text);
                }
            }
        }

        static char GenerateLetter(int randomInt, bool hasSpecialCharacter, char[] alphabet)
        {
            switch (randomInt)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    return alphabet[GetRandomIntUpTo(alphabet.Length - 1)];
                case 5:
                    return _numbersZeroToNine[GetRandomIntUpTo(_numbersZeroToNine.Length - 1)];
                case 6:
                    if (hasSpecialCharacter)
                        return _specialCharacters[GetRandomIntUpTo(_specialCharacters.Length - 1)];
                    else
                        return _numbersZeroToNine[GetRandomIntUpTo(_numbersZeroToNine.Length - 1)];
                default:
                    Console.WriteLine("Something wrong with GenerateLetter()");
                    return '*';
            }
        }

        //a function (method) that asks a question and returns the user's key press
        static ConsoleKey ChooseOption()
        {
            //a variable to store key press information
            ConsoleKeyInfo choice;
            //a bool to store if the loop has run once already
            bool afterFirst = false;
            //a do-while loop
            do
            {
                //if this loop has already run once before...
                if (afterFirst == true)
                    Console.WriteLine("\n\nInvalid selection. Please choose a valid option.");

                Console.WriteLine("(1) Generate a word key");
                Console.WriteLine("(2) Generate a character key");
                Console.WriteLine("(3) Decrypt file");
                Console.WriteLine("\n(Q) Quit Application");
                Console.Write("\nYour selection: ");
                choice = Console.ReadKey();
                afterFirst = true;
            } while (choice.Key != ConsoleKey.D1 && choice.Key != ConsoleKey.D2 && choice.Key != ConsoleKey.D3 && choice.Key != ConsoleKey.Q);

            //a function with a return condition (System.ConsoleKey, above) must have a 'return' statement
            //this function returns the value of a key press, so either the key '1' or '2'
            return choice.Key;
        }

        //this is a random function that chooses random numbers up to a user-chosen limit
        static int GetRandomIntUpTo(int range)
        {
            return ((int)Math.Floor(_random.NextDouble() * (range + 1)));
        }

        static bool FlipACoin()
        {
            return ((new Random()).NextDouble() < 0.5) ? true : false;
        }

        static string GetStringFromFile(string fileName)
        {
            string value = string.Empty;
            string fileContents = string.Empty;
            string[] stringArray;
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "PasswordGenerator." + RESOURCE_FOLDER + "." + fileName;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                fileContents = reader.ReadToEnd();
            }
            stringArray = fileContents.Split('\n');
            int randomIndex = GetRandomIntUpTo(stringArray.Length - 1);
            return stringArray[randomIndex];
        }
    }
}
