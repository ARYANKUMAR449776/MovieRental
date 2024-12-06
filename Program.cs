namespace MovieRental
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public List<Movie> rentedMovies { get; private set; }
        public List<Movie> favoriteMovies { get; set; }
        public List<Movie> onHoldMovies { get; private set; }
        public List<Rental> rentalHistory { get; private set; }

        public double charges { get; private set; }

        private static readonly string CredentialsFile = "users.csv";

        public User()
        {
            rentedMovies = new List<Movie>();
            favoriteMovies = new List<Movie>();
            onHoldMovies = new List<Movie>();
            rentalHistory = new List<Rental>();
        }

        public User(int userID, string name, string email, string password)
        {
            UserID = userID;
            Name = name;
            Email = email;
            Password = password;

            rentedMovies = new List<Movie>();
            favoriteMovies = new List<Movie>();
            onHoldMovies = new List<Movie>();
            rentalHistory = new List<Rental>();
        }

        public static void Signup()
        {
            Console.WriteLine("Enter UserID: ");
            int userID = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter Name: ");
            string name = Console.ReadLine();

            Console.WriteLine("Enter Email: ");
            string email = Console.ReadLine();

            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();

            if (!File.Exists(CredentialsFile))
            {
                using (var sw = File.CreateText(CredentialsFile))
                {
                    sw.WriteLine("UserID,Name,Email,Password");
                }
            }

            if (IsUserExists(userID, email))
            {
                Console.WriteLine("UserID or Email already exists. Please try logging in.");
                return;
            }

            using (var sw = File.AppendText(CredentialsFile))
            {
                sw.WriteLine($"{userID},{name},{email},{password}");
            }

            Console.WriteLine("Signup successful! You can now log in.");
        }

        public static bool Login()
        {
            Console.WriteLine("Enter UserID: ");
            string userID = Console.ReadLine();

            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();

            if (File.Exists(CredentialsFile))
            {
                var lines = File.ReadAllLines(CredentialsFile).Skip(1); // Skip header row
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts[0] == userID && parts[3] == password)
                    {
                        Console.WriteLine($"Login successful! Welcome, {parts[1]}.");
                        return true;
                    }
                }
            }

            Console.WriteLine("Invalid UserID or Password. Please try again.");
            return false;
        }

        private static bool IsUserExists(int userID, string email)
        {
            if (!File.Exists(CredentialsFile))
                return false;

            return File.ReadAllLines(CredentialsFile)
                       .Skip(1) // Skip header row
                       .Select(line => line.Split(','))
                       .Any(parts => int.Parse(parts[0]) == userID || parts[2] == email);
        }

        public void rentMovie(Movie movie)
        {
            // TODO: Add logic to rent a movie
        }

        public List<Movie> searchMovieByGenre(string genre)
        {
            // TODO: Add logic to search for movies by genre
            return null;
        }

        public List<Movie> searchMovieByTags(string[] tags)
        {
            // TODO: Add logic to search for movies by tags
            return null;
        }

        public List<Movie> searchMovieByArtist(string artist)
        {
            // TODO: Add logic to search for movies by artist
            return null;
        }

        public void addMovieToFavorites(Movie movie)
        {
            // TODO: Add logic to add a movie to favorites
        }

        public void placeOnHold(Movie movie)
        {
            // TODO: Add logic to place a movie on hold
        }

        public void returnMovie(Rental rental)
        {
            // TODO: Add logic to return a movie
        }

        public void reRentMovie(Rental rental)
        {
            // TODO: Add logic to re-rent a movie
        }
    }

    class Movie
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public List<string> Artists { get; set; }
        public List<string> Tags { get; set; }
        public bool Availability { get; set; }
        public bool IsTrending { get; set; }
        public string ReleaseDate { get; set; }
        public int RentalCount { get; set; }

        public Movie(int movieID, string title, string genre, string artists, string tags, bool availability, bool isTrending, string releaseDate, int rentalCount)
        {
            MovieID = movieID;
            Title = title;
            Genre = genre;
            Artists = new List<string>(artists.Split(','));
            Tags = new List<string>(tags.Split(','));
            Availability = availability;
            IsTrending = isTrending;
            ReleaseDate = releaseDate;
            RentalCount = rentalCount;
        }

        public void getDetails()
        {
            Console.WriteLine($"Movie ID: {MovieID}");
            Console.WriteLine($"Title: {Title}");
            Console.WriteLine($"Genre: {Genre}");
            Console.WriteLine($"Artists: {string.Join(", ", Artists)}");
            Console.WriteLine($"Tags: {string.Join(", ", Tags)}");
            Console.WriteLine($"Availability: {Availability}");
            Console.WriteLine($"Is Trending: {IsTrending}");
            Console.WriteLine($"Release Date: {ReleaseDate}");
            Console.WriteLine($"Rental Count: {RentalCount}");
        }

        public void checkAvailability()
        {
            //TODO:
        }

        public void incrementRentalCount()
        {
            //TODO:
        }
    }

    class Rental
    {
        public Rental()
        {
            //TODO: Constructor
        }
    }

    class Favorites : User
    {
        public Favorites()
        {
            //TODO: Constructor
        }
    }

    class Hold : User
    {
        public Hold()
        {
            //TODO: Constructor
        }
    }

    //class Suggestion : Movie
    //{
    //    public Suggestion()
    //    {
    //        //TODO: Constructor
    //    }
    //}

    //class TrendingMovies : Movie
    //{
    //    public TrendingMovies()
    //    {
    //        //TODO: Constructor
    //    }
    //}

    class Program
    {
        static void Main(string[] args)
        {
            string input;
            do
            {
                Console.WriteLine("Enter 'signup' to create an account, 'login' to log in, or 'exit' to quit:");
                input = Console.ReadLine().ToLower();

                if (input == "signup")
                {
                    User.Signup();
                }
                else if (input == "login")
                {
                    bool success = User.Login();
                    if (success)
                    {
                        Console.WriteLine("Logged in successfully!");
                        // Add logic here for what happens after a successful login
                    }
                }

            } while (input != "exit");

            Console.WriteLine("Goodbye!");
        }
    }
}
