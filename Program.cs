namespace MovieRental
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System;
    using System.Globalization;
    using System.IO;

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

        public static readonly string CredentialsFile = "users.csv";

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
                var lines = File.ReadAllLines(CredentialsFile).Skip(1);
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
         
         Rental.IssueNewRental(this, movie);
        }

        public List<Movie> searchMovieByGenre(string genre)
        {
            // Load all movies
            var genreSearchResult = Movie.LoadMoviesFromCsv();

            // Filter movies by genre (case-insensitive)
            var resultList = genreSearchResult
                .Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Print the results
            foreach (var movie in resultList)
            {
                Console.WriteLine($"Title: {movie.Title}, Genre: {movie.Genre}");
            }
        
            // Return the filtered list
            return resultList;
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
        public string Artist { get; set; }
        public List<string> Tags { get; set; }
        public bool Availability { get; set; }
        public bool IsTrending { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int RentalCount { get; set; }

        private static readonly string MoviesFile = "movies.csv";

        public Movie() { }

        public Movie(int movieID, string title, string genre, string artist, string tags, bool availability, bool isTrending, DateTime releaseDate, int rentalCount)
        {
            MovieID = movieID;
            Title = title;
            Genre = genre;
            Artist = artist;
            Tags = new List<string>(tags.Split(','));
            Availability = availability;
            IsTrending = isTrending;
            ReleaseDate = releaseDate;
            RentalCount = rentalCount;
        }

        public void GetDetails()
        {
            //Console.WriteLine($"Movie ID: {MovieID}");
            Console.WriteLine($"Title: {Title}");
            Console.WriteLine($"Genre: {Genre}");
            Console.WriteLine($"Artist: {Artist}");
            Console.WriteLine($"Tags: {string.Join(", ", Tags)}");
            //Console.WriteLine($"Availability: {Availability}");
            //Console.WriteLine($"Is Trending: {IsTrending}");
            Console.WriteLine($"Release Date: {ReleaseDate.ToShortDateString()}");
            //Console.WriteLine($"Rental Count: {RentalCount}");
        }
    
        public static List<Movie> LoadMoviesFromCsv()
        {
        var movies = new List<Movie>();

        if (!File.Exists(MoviesFile))
        {
            Console.WriteLine("Movies file not found. Please ensure 'movies.csv' exists.");
            return movies;
        }

        using (var reader = new StreamReader(MoviesFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            try
            {
                var records = csv.GetRecords<MovieCsvRecord>();

                foreach (var record in records)
                {
                    try
                    {
                        var movie = new Movie(
                            record.MovieID,
                            record.Title,
                            record.Genre,
                            record.Artist,
                            record.Tags,
                            record.Availability == 1,
                            record.IsTrending == 1,
                            DateTime.Parse(record.ReleaseDate),
                            record.RentalCount
                        );

                        movies.Add(movie);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing movie record: {record.Title}");
                        Console.WriteLine($"Exception: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }
        }

        return movies;
    }
}
    public class MovieCsvRecord
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Artist { get; set; }
        public string Tags { get; set; }
        public int Availability { get; set; }
        public int IsTrending { get; set; }
        public string ReleaseDate { get; set; }
        public int RentalCount { get; set; }
    }


    class Rental
    {
       
        
        private static string rentalRecord = "rentals.csv";
        public static void rentalRecordCsvInitialize()
        {
            if (!File.Exists(rentalRecord))
            {
                using (var createFile = File.CreateText(rentalRecord))
                {
                    createFile.WriteLine("UserId, MovieId, MovieName, RentalDate");
                    Console.WriteLine($"New File created {rentalRecord}");
                }
            }
        }

        public static void IssueNewRental(User user, Movie movie)
        {
            using (var writer = new StreamWriter(rentalRecord, append: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField(user.UserID);
                csv.WriteField(movie.MovieID);
                csv.WriteField(movie.Title);
                csv.WriteField(DateTime.Now);
                csv.NextRecord();
            }
        }

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

    class Suggestion : Movie
    {
        public Suggestion()
        {
            //TODO: Constructor
        }
    }

    class TrendingMovies : Movie
    {
        public TrendingMovies()
        {
            //TODO: Constructor
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            string input;
            User currentUser = null; // Variable to track the logged-in user

            do
            {
                Console.WriteLine("Enter 'signup' to create an account, 'login' to log in, or 'exit' to quit:");
                input = Console.ReadLine().ToLower();

                if (input == "signup")
                {
                    Console.Clear();
                    User.Signup();
                }
                else if (input == "login")
                {
                    Console.Clear();
                    if (Login(out currentUser))
                    {
                        Console.WriteLine("Logged in successfully!");

                        // Load and display movies
                        var movies = Movie.LoadMoviesFromCsv();
                        Console.WriteLine("Available Movies:");
                        foreach (var movie in movies)
                        {
                            movie.GetDetails();
                            Console.WriteLine("------------------------");
                        }

                        Console.WriteLine("\n");
                        Console.WriteLine("Enter genre to search for a movie:");
                        var genre = Console.ReadLine();
                        currentUser.searchMovieByGenre(genre);
                    }
                }
            } while (input != "exit");

            Console.WriteLine("Goodbye!");
        }

        private static bool Login(out User loggedInUser)
        {
            Console.WriteLine("Enter UserID: ");
            string userID = Console.ReadLine();

            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();

            if (File.Exists(User.CredentialsFile))
            {
                var lines = File.ReadAllLines(User.CredentialsFile).Skip(1);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts[0] == userID && parts[3] == password)
                    {
                        loggedInUser = new User(
                            int.Parse(parts[0]), // UserID
                            parts[1],            // Name
                            parts[2],            // Email
                            parts[3]             // Password
                        );
                        Console.WriteLine($"Login successful! Welcome, {loggedInUser.Name}.");
                        return true;
                    }
                }
            }

            Console.WriteLine("Invalid UserID or Password. Please try again.");
            loggedInUser = null;
            return false;
        }
    }


}
