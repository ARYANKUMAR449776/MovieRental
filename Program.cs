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
    using System.Reflection.Metadata.Ecma335;
    using System.Transactions;

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
                Console.WriteLine("UserID or Email already exists. Please try logging in.\n");
                return;
            }

            using (var sw = File.AppendText(CredentialsFile))
            {
                sw.WriteLine($"{userID},{name},{email},{password}");
            }

            Console.WriteLine("Signup successful! You can now log in.\n");
        }

        public static bool Login(out User loggedInUser)
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
                        loggedInUser = new User(
                            int.Parse(parts[0]), // UserID
                            parts[1],            // Name
                            parts[2],            // Email
                            parts[3]             // Password
                        );
                        Console.Clear();
                        Console.WriteLine($"Login successful! Welcome, {loggedInUser.Name}.\n");
                        return true;
                    }
                }
            }
            Console.Clear() ;
            Console.WriteLine("Invalid UserID or Password. Please try again.");
            loggedInUser = null;
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

        public void RentMovie(Movie movie)
        {
            Rental.IssueNewRental(this, movie);
        }

        public void SearchAndDisplayMoviesByGenre()
        {
            Console.WriteLine("Enter genre to search for a movie:");
            string genre = Console.ReadLine();

            var genreSearchResult = Movie.LoadMoviesFromCsv();

            var resultList = genreSearchResult
                .Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var movie in resultList)
            {
                Console.WriteLine($"Title: {movie.Title}, Genre: {movie.Genre}");
            }
        }


        public List<Movie> SearchMoviesByGenre(string genre)
        {
            var genreSearchResult = Movie.LoadMoviesFromCsv();
            return genreSearchResult.Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Movie> SearchMoviesByTitle(string title)
        {
            var titleSearchResult = Movie.LoadMoviesFromCsv();
            return titleSearchResult.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Movie> SearchMoviesByArtist(string artist)
        {
            var artistSearchResult = Movie.LoadMoviesFromCsv();
            return artistSearchResult.Where(m => m.Artist.Contains(artist, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Movie> SearchMoviesByTag(string tag)
        {
            var tagSearchResult = Movie.LoadMoviesFromCsv();
            return tagSearchResult.Where(m => m.Tags.Any(t => t.Contains(tag, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        public void ShowRentedMovies()
        {
            if (File.Exists(Rental.userRentalsFile))
            {
                var rentedMovies = new List<Movie>();
                var lines = File.ReadAllLines(Rental.userRentalsFile).Skip(1); // Skip the header row

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    int userId = int.Parse(parts[0]);
                    int movieId = int.Parse(parts[1]);
                    string movieName = parts[2];

                    if (userId == UserID)
                    {
                        // Find the movie by movieId in the Movie list
                        var movie = Movie.LoadMoviesFromCsv().FirstOrDefault(m => m.MovieID == movieId);

                        if (movie != null)
                        {
                            rentedMovies.Add(movie); // Add movie to rentedMovies list
                        }
                    }
                }

                if (rentedMovies.Count == 0)
                {
                    Console.WriteLine("You haven't rented any movies yet.");
                }
                else
                {
                    Console.WriteLine("Rented Movies:");
                    foreach (var movie in rentedMovies)
                    {
                        Console.WriteLine($"Title: {movie.Title}, Genre: {movie.Genre}, Artist: {movie.Artist}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No rental records found.");
            }
        }

        public int GetRentedMoviesCount()
        {
            if (File.Exists(Rental.userRentalsFile))
            {
                var rentedMoviesCount = 0;
                var lines = File.ReadAllLines(Rental.userRentalsFile).Skip(1); // Skip header row

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    int userId = int.Parse(parts[0]);

                    if (userId == UserID)
                    {
                        rentedMoviesCount++;  // Count the rented movies for this user
                    }
                }

                return rentedMoviesCount;
            }
            else
            {
                Console.WriteLine("No rental records found.");
                return 0;
            }
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

        public static readonly string MoviesFile = "movies.csv";

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
            Console.WriteLine($"Title: {Title}");
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
        private static readonly string rentalRecord = "rentals.csv";
        public static readonly string userRentalsFile = "user_rentals.csv";  // New CSV to store user rentals

        public static void RentalRecordCsvInitialize()
        {
            if (!File.Exists(rentalRecord))
            {
                using (var createFile = File.CreateText(rentalRecord))
                {
                    createFile.WriteLine("UserId, MovieId, MovieName, RentalDate");
                    Console.WriteLine($"New File created {rentalRecord}");
                }
            }

            // Create the user_rentals.csv file if it doesn't exist
            if (!File.Exists(userRentalsFile))
            {
                using (var createFile = File.CreateText(userRentalsFile))
                {
                    createFile.WriteLine("UserId, MovieId, MovieName, RentalDate"); // Headers for user rentals
                    Console.WriteLine($"New File created {userRentalsFile}");
                }
            }
        }

        public static void IssueNewRental(User user, Movie movie)
        {
            // Check if the movie is available for rental using the Availability from movies.csv
            int rentedMoviesCount = user.GetRentedMoviesCount();
            if (movie.Availability)
            {
                if (rentedMoviesCount >= 4)
                {
                    Console.WriteLine("Cant Rent More than 4 movies , Kindly return a movie before renting anything else");
                }
                else
                {
                    // Rent the movie by adding it to the rentals and user rentals file
                    AddRentalToFile(user, movie);

                    // Set the availability of the movie to false in the movies.csv
                    UpdateMovieAvailability(movie.MovieID, false);

                    // Notify the user of the successful rental
                    Console.WriteLine($"You have successfully rented {movie.Title}.");
                }
            }
            else
            {
                // If the movie is not available
                Console.WriteLine($"{movie.Title} is currently unavailable.");
            }
        }


        public static void ReturnRental(User user, Movie movie)
        {
        // Load all rental records into a list of lines
        var rentalRecords = File.ReadAllLines(userRentalsFile).Skip(1) // Skip header
                                .Select(line => line.Split(','))
                                .ToList();

        // Find the matching rental record
        var matchingRecord = rentalRecords.FirstOrDefault(record => 
            int.Parse(record[0]) == user.UserID && 
            record[2].Equals(movie.Title, StringComparison.OrdinalIgnoreCase));

        if (matchingRecord != null)
        {
            // Update availability of the movie back to true
            UpdateMovieAvailability(movie.MovieID, true);

            // Remove the matching record from rentalRecords
            rentalRecords.Remove(matchingRecord);

            // Write the updated records back to the file
            var updatedRecords = new List<string> { "UserId, MovieId, MovieName, RentalDate" }; // Header
            updatedRecords.AddRange(rentalRecords.Select(record => string.Join(",", record)));
            File.WriteAllLines(userRentalsFile, updatedRecords);

            Console.WriteLine($"You have successfully returned {movie.Title}.");
        }
        else
        {
            Console.WriteLine($"You cannot return {movie.Title} as it is not rented by you.");
        }
    }




        private static void AddRentalToFile(User user, Movie movie)
        {
            using (var writer = new StreamWriter(userRentalsFile, append: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField(user.UserID);
                csv.WriteField(movie.MovieID);
                csv.WriteField(movie.Title);
                csv.WriteField(DateTime.Now);  // Rental date
                csv.NextRecord();
            }
        }

        private static void UpdateMovieAvailability(int movieID, bool availability)
        {
            // Load all movies from the CSV file
            var movies = Movie.LoadMoviesFromCsv();

            // Find the movie to update by its MovieID
            var movieToUpdate = movies.FirstOrDefault(m => m.MovieID == movieID);

            if (movieToUpdate != null)
            {
                // Update the availability of the found movie
                movieToUpdate.Availability = availability;

                // Prepare movie records to write
                var movieRecords = movies.Select(m => new MovieCsvRecord
                {
                    MovieID = m.MovieID,
                    Title = m.Title,
                    Genre = m.Genre,
                    Artist = m.Artist,
                    Tags = string.Join(",", m.Tags),
                    Availability = m.Availability ? 1 : 0,
                    IsTrending = m.IsTrending ? 1 : 0,
                    ReleaseDate = m.ReleaseDate.ToString("MM/dd/yyyy HH:mm"), // Format to match CSV example
                    RentalCount = m.RentalCount
                }).ToList();

                // Check if the file already exists to avoid rewriting the header
                bool fileExists = File.Exists(Movie.MoviesFile);

                using (var writer = new StreamWriter(Movie.MoviesFile, false)) // Overwrite the file
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    if (!fileExists) // Write header only if file doesn't exist
                    {
                        csv.WriteHeader<MovieCsvRecord>();  // Write header for CSV
                    }
                    csv.WriteRecords(movieRecords);     // Write all records including the updated one
                }

                Console.WriteLine($"Updated availability of movie '{movieToUpdate.Title}' to {availability}.");
            }
            else
            {
                Console.WriteLine("Movie not found.");
            }
        }





    }



    class Favorites
    {
        private static readonly string FavoritesFile = "favorites.csv";

        public static void InitializeFavoritesFile()
        {
            if (!File.Exists(FavoritesFile))
            {
                using (var createFile = File.CreateText(FavoritesFile))
                {
                    createFile.WriteLine("UserID,MovieID,Title,Genre"); // Headers for the CSV file
                    Console.WriteLine($"New file created: {FavoritesFile}");
                }
            }
        }

        public static void AddToFavorites(User user, Movie movie)
        {
            // Check if the movie is already in favorites
            var favoriteMovies = LoadFavorites(user);
            if (favoriteMovies.Any(m => m.MovieID == movie.MovieID))
            {
                Console.WriteLine($"{movie.Title} is already in your favorites.");
                return;
            }
           

                // Add the movie to the favorites file
                using (var writer = new StreamWriter(FavoritesFile, append: true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteField(user.UserID);
                    csv.WriteField(movie.MovieID);
                    csv.WriteField(movie.Title);
                    csv.WriteField(movie.Genre);
                    csv.NextRecord();
                }

                Console.WriteLine($"{movie.Title} has been added to your favorites.");
            
        }

        public static List<Movie> LoadFavorites(User user)
        {
            var favoriteMovies = new List<Movie>();

            if (!File.Exists(FavoritesFile))
            {
                return favoriteMovies; // Return an empty list if the file does not exist
            }

            var lines = File.ReadAllLines(FavoritesFile).Skip(1); // Skip the header row
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                int userId = int.Parse(parts[0]);
                if (userId == user.UserID)
                {
                    // Add the movie to the user's favorites
                    favoriteMovies.Add(new Movie
                    {
                        MovieID = int.Parse(parts[1]),
                        Title = parts[2],
                        Genre = parts[3]
                    });
                }
            }

            return favoriteMovies;
        }

        public static void DisplayFavorites(User user)
        {
            var favoriteMovies = LoadFavorites(user);

            if (!favoriteMovies.Any())
            {
                Console.WriteLine("You have no favorite movies.");
                return;
            }

            Console.WriteLine("Your Favorite Movies:");
            foreach (var movie in favoriteMovies)
            {
                Console.WriteLine($"Title: {movie.Title}, Genre: {movie.Genre}");
            }
        }
    }


    class Hold
    {
        public static readonly string user_OnHoldMovies = "user_OnHoldMovies.csv";

        public static void OnHoldCsvInitialization()
        {
            if (!File.Exists(user_OnHoldMovies))
            {
                using (var createFile = File.CreateText(user_OnHoldMovies))
                {
                    createFile.WriteLine("UserId,MovieId,MovieName,HoldDate"); // Ensure header is added during initialization
                    Console.WriteLine($"New File created {user_OnHoldMovies}");
                }
            }
        }

        public static void PlaceOnHold(User user, Movie movie)
        {
            // Check if the movie is available for placing on hold
            if (movie.Availability)
            {
                // Add the movie to the on-hold records file
                AddToOnHoldFile(user, movie);

                // Set the availability of the movie to false in the movies.csv
                UpdateMovieAvailability(movie.MovieID, false);

                // Notify the user of the successful hold
                Console.WriteLine($"You have successfully placed {movie.Title} on hold.");
            }
            else
            {
                // If the movie is not available
                Console.WriteLine($"{movie.Title} is currently unavailable for hold.");
            }
        }

        private static void AddToOnHoldFile(User user, Movie movie)
        {
            bool fileExists = File.Exists(user_OnHoldMovies);

            using (var writer = new StreamWriter(user_OnHoldMovies, append: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (!fileExists || new FileInfo(user_OnHoldMovies).Length == 0) // Check if the file is empty
                {
                    csv.WriteHeader<OnHoldRecord>(); // Write the header
                    csv.NextRecord();
                }

                csv.WriteField(user.UserID);
                csv.WriteField(movie.MovieID);
                csv.WriteField(movie.Title);
                csv.WriteField(DateTime.Now); // Hold date
                csv.NextRecord();
            }
        }

        public static void removeFromOnHold(User user, Movie movie)
    {
        if (!File.Exists(user_OnHoldMovies))
        {
            Console.WriteLine("No hold records found.");
            return;
        }

        // Load all on-hold records into a list of lines
        var onHoldRecords = File.ReadAllLines(user_OnHoldMovies)
                                 .Skip(1) // Skip header
                                 .Select(line => line.Split(','))
                                 .ToList();

        // Find the matching hold record
        var matchingRecord = onHoldRecords.FirstOrDefault(record =>
            int.Parse(record[0]) == user.UserID &&
            record[2].Equals(movie.Title, StringComparison.OrdinalIgnoreCase));

        if (matchingRecord != null)
        {
            // Update availability of the movie back to true
            UpdateMovieAvailability(movie.MovieID, true);

            // Remove the matching record from the onHoldRecords
            onHoldRecords.Remove(matchingRecord);

            // Write the updated records back to the file with a consistent header
            using (var writer = new StreamWriter(user_OnHoldMovies, false))
            {
                writer.WriteLine("UserId,MovieId,MovieName,HoldDate"); // Always include the header

                foreach (var record in onHoldRecords)
                {
                    writer.WriteLine(string.Join(",", record));
                }
            }

            Console.WriteLine($"You have successfully removed {movie.Title} from hold.");
        }
        else
        {
            Console.WriteLine($"You cannot remove {movie.Title} from hold as it is not placed on hold by you.");
        }
    }


        private static void UpdateMovieAvailability(int movieID, bool availability)
        {
            // Load all movies from the CSV file
            var movies = Movie.LoadMoviesFromCsv();

            // Find the movie to update by its MovieID
            var movieToUpdate = movies.FirstOrDefault(m => m.MovieID == movieID);

            if (movieToUpdate != null)
            {
                // Update the availability of the found movie
                movieToUpdate.Availability = availability;

                // Prepare movie records to write
                var movieRecords = movies.Select(m => new MovieCsvRecord
                {
                    MovieID = m.MovieID,
                    Title = m.Title,
                    Genre = m.Genre,
                    Artist = m.Artist,
                    Tags = string.Join(",", m.Tags),
                    Availability = m.Availability ? 1 : 0,
                    IsTrending = m.IsTrending ? 1 : 0,
                    ReleaseDate = m.ReleaseDate.ToString("MM/dd/yyyy HH:mm"), // Format to match CSV example
                    RentalCount = m.RentalCount
                }).ToList();

                // Check if the file already exists to avoid rewriting the header
                bool fileExists = File.Exists(Movie.MoviesFile);

                using (var writer = new StreamWriter(Movie.MoviesFile, false)) // Overwrite the file
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    if (!fileExists) // Write header only if file doesn't exist
                    {
                        csv.WriteHeader<MovieCsvRecord>();  // Write header for CSV
                    }
                    csv.WriteRecords(movieRecords);     // Write all records including the updated one
                }

                Console.WriteLine($"Updated availability of movie '{movieToUpdate.Title}' to {availability}.");
            }
            else
            {
                Console.WriteLine("Movie not found.");
            }
        }

        private class OnHoldRecord
        {
            public int UserID { get; set; }
            public int MovieID { get; set; }
            public string MovieName { get; set; }
            public DateTime HoldDate { get; set; }
        }

    }

    class Suggestion : Movie
    {
        // Method to display movies with the "Sci-Fi" tag using LINQ and formatting the output
        public static void DisplaySuggestedMovies()
        {
            // Load all movies from the CSV
            var allMovies = Movie.LoadMoviesFromCsv();

            // Use LINQ to filter movies with the "Sci-Fi" tag
            var sciFiMovies = allMovies
                .Where(movie => movie.Tags.Any(tag => tag.Equals("Sci-Fi", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Display the filtered movies
            if (sciFiMovies.Any())
            {
                Console.WriteLine("|Suggested Movies|\n");
  
                Console.WriteLine("====================================================================");
                Console.WriteLine($"{"Title",-30} {"Genre",-15} {"Artist",-25} ");
                Console.WriteLine("====================================================================");

                sciFiMovies.ForEach(movie =>
                {
                    Console.WriteLine($"{movie.Title,-30} {movie.Genre,-15} {movie.Artist,-25}");
                });
                Console.WriteLine("====================================================================\n");
            }
            else
            {
                Console.WriteLine("No Suggested movies found.");
            }
        }
    }




    class TrendingMovies : Movie
    {
        public static void DisplayTrendingMovies()
        {
            // Load all movies from the CSV
            var allMovies = Movie.LoadMoviesFromCsv();

            // Use LINQ to filter movies with the "Sci-Fi" tag
            var trendingMovies = allMovies
                .Where(movie => movie.IsTrending==true)
                .ToList();

            // Display the filtered movies
            if (trendingMovies.Any())
            {
                Console.WriteLine("|Trending Movies|\n");

                Console.WriteLine("====================================================================");
                Console.WriteLine($"{"Title",-30} {"Genre",-15} {"Artist",-25} ");
                Console.WriteLine("====================================================================");

                trendingMovies.ForEach(movie =>
                {
                    Console.WriteLine($"{movie.Title,-30} {movie.Genre,-15} {movie.Artist,-25}");
                });
                Console.WriteLine("====================================================================\n");
            }
            else
            {
                Console.WriteLine("No trending movies found.");
            }
        }
    }

    class MostRentedMovies : Movie {

        public static void DisplayMostRentedMovies()
        {
            // Load all movies from the CSV
            var allMovies = Movie.LoadMoviesFromCsv();

            // Use LINQ to filter movies with the "Sci-Fi" tag
            var MostRented = allMovies
                .Where(movie => movie.RentalCount > 70)
                .ToList();

            // Display the filtered movies
            if (MostRented.Any())
            {
                Console.WriteLine("|Most rented Movies|\n");

                Console.WriteLine("====================================================================");
                Console.WriteLine($"{"Title",-30} {"Genre",-15} {"Artist",-25} ");
                Console.WriteLine("====================================================================");

                MostRented.ForEach(movie =>
                {
                    Console.WriteLine($"{movie.Title,-30} {movie.Genre,-15} {movie.Artist,-25}");
                });
                Console.WriteLine("====================================================================\n");
            }
            else
            {
                Console.WriteLine("No Most Rented movies found.");
            }
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            string input;
            User currentUser = null;

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
                    if (User.Login(out currentUser))
                    {
                        //Console.WriteLine("Logged in successfully!\n");
                        Suggestion.DisplaySuggestedMovies();
                        TrendingMovies.DisplayTrendingMovies();
                        MostRentedMovies.DisplayMostRentedMovies();
                        string command;

                        // After login, allow the user to enter a command
                        do
                        {
                            Console.WriteLine("Available Commands:\n");
                            Console.WriteLine("{0,-20} {1}", "'search'", "Search for movies by genre, title, artist, or tag.");
                            Console.WriteLine("{0,-20} {1}", "'rent'", "Enter the title of the movie you wish to rent.");
                            Console.WriteLine("{0,-20} {1}", "'show rented movies'", "View all movies you have rented.");
                            Console.WriteLine("{0,-20} {1}", "'add to favorites'", "Add a movie to your favorites list.");
                            Console.WriteLine("{0,-20} {1}", "'place on hold'", "Place a movie on hold.");// show on hold and favorited
                            Console.WriteLine("{0,-20} {1}", "'return'", "Return a rented movie.");
                            Console.WriteLine("{0,-20} {1}", "'remove hold'", "Remove a movie from the hold list.");
                            Console.WriteLine("{0,-20} {1}", "'logout'", "Log out from the current session.");
                            Console.WriteLine("{0,-20} {1}", "'exit'", "Exit the application.\n");
                            Console.WriteLine("Please enter a command:");
                            command = Console.ReadLine().ToLower();

                            if (command == "search")
                            {
                                Console.Clear();
                                Console.WriteLine("Do you want to search by 'genre', 'title', 'artist', or 'tag'?");
                                string searchCriteria = Console.ReadLine().ToLower();

                                Console.WriteLine("Enter the search term:");
                                string searchTerm = Console.ReadLine();

                                List<Movie> searchResults = new List<Movie>();

                                // Perform the search based on criteria
                                if (searchCriteria == "genre")
                                {
                                    searchResults = currentUser.SearchMoviesByGenre(searchTerm);
                                }
                                else if (searchCriteria == "title")
                                {
                                    searchResults = currentUser.SearchMoviesByTitle(searchTerm);
                                }
                                else if (searchCriteria == "artist")
                                {
                                    searchResults = currentUser.SearchMoviesByArtist(searchTerm);
                                }
                                else if (searchCriteria == "tag")
                                {
                                    searchResults = currentUser.SearchMoviesByTag(searchTerm);
                                }

                                // Display search results
                                if (searchResults.Any())
                                {
                                    Console.WriteLine("Search Results:");
                                    foreach (var movie in searchResults)
                                    {
                                        Console.WriteLine($"Title: {movie.Title}, Genre: {movie.Genre}, Artist: {movie.Artist}, Tags: {string.Join(", ", movie.Tags)}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No movies found.");
                                }
                            }

                            else if (command == "rent")
                            {
                                Console.Clear();
                                Console.WriteLine("Enter the title of the movie you want to rent:");

                                string movieTitle = Console.ReadLine();

                                // Search for the movie by title
                                var movieToRent = currentUser.SearchMoviesByTitle(movieTitle).FirstOrDefault();

                                if (movieToRent != null)
                                {
                                    // If the movie is found, rent it
                                    Rental.IssueNewRental(currentUser, movieToRent);
                                }
                                else
                                {
                                    Console.WriteLine("Movie not found.");
                                }
                            }

                            else if (command == "show rented movies")
                            {
                                Console.Clear();
                                if (currentUser != null)
                                {
                                    currentUser.ShowRentedMovies(); // Display rented movies for the current user
                                }
                            }

                            else if (command == "add to favorites")
                            {
                                Console.Clear();
                                Console.WriteLine("Enter the movie title to add to favorites:");
                                string movieTitle = Console.ReadLine();

                                var searchResults = currentUser.SearchMoviesByTitle(movieTitle);
                                if (!searchResults.Any())
                                {
                                    Console.WriteLine("No movies found with that title.");
                                }
                                else
                                {
                                    var movieToAdd = searchResults.First();
                                    Favorites.AddToFavorites(currentUser, movieToAdd);
                                }
                            }

                            else if (command == "show favorites")
                            {
                                Console.Clear();
                                if (currentUser != null)
                                {
                                    Favorites.DisplayFavorites(currentUser);
                                }

                            }

                            else if (command == "place on hold"){
                                Console.Clear();
                                Console.WriteLine("Enter the title of the movie you want to place on hold:");
                                string movieTitle = Console.ReadLine();

                                // Search for the movie by title
                                var movieToHold = currentUser.SearchMoviesByTitle(movieTitle).FirstOrDefault();

                                if (movieToHold != null)
                                {
                                    // Call PlaceOnHold to process the hold
                                    Hold.PlaceOnHold(currentUser, movieToHold);
                                }
                                else
                                {
                                    Console.WriteLine("Movie not found.");
                                }
                            }

                            else if (command == "return")
                            {
                                Console.Clear();
                                Console.WriteLine("Enter the title of the movie you want to return:");
                                string movieTitle = Console.ReadLine();

                                // Search for the movie by title
                                var movieToReturn = currentUser.SearchMoviesByTitle(movieTitle).FirstOrDefault();

                                if (movieToReturn != null)
                                {
                                    // Call ReturnRental to process the return
                                    Rental.ReturnRental(currentUser, movieToReturn);
                                }
                                else
                                {
                                    Console.WriteLine("Movie not found.");
                                }
                            }

                            else if(command == "remove hold")
                            {
                                Console.Clear();
                                Console.WriteLine("Enter the title of the movie you want remove from hold :");
                                string movieTitle = Console.ReadLine();

                                // Search for the movie by title
                                var movieToRemoveFromHold = currentUser.SearchMoviesByTitle(movieTitle).FirstOrDefault();

                                if (movieToRemoveFromHold != null)
                                {
                                    // Call ReturnRental to process the return
                                   Hold.removeFromOnHold(currentUser, movieToRemoveFromHold);
                                }
                                else
                                {
                                    Console.WriteLine("Movie not found.");
                                }
                            }


                            else if (command == "logout")
                            {
                                Console.WriteLine("Logging out...");
                                currentUser = null; // Log out the user
                            }

                            else
                            {
                                Console.WriteLine("Invalid Command");
                            }

                        } while (command != "exit" && currentUser != null);
                    }
                }
            } while (input != "exit");

            Console.WriteLine("Goodbye!");
        }
    }


}

