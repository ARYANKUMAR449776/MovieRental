namespace MovieRental // added namespace
{
   
    class User
    {
        public int UserID {  get; set; }
        public int Name { get; set; }
        public string Email { get; set; }
     
        public List <Movie>[] rentedMovies { get; private set; }
        public List<Movie>[] favoriteMovies { get; set; }
        public List<Movie>[] onHoldMovies { get; private set; }
        public List<Rental>[] rentalHistory { get; private set; }

        public double charges { get; private set; }

        public void rentMovie(Movie movie)
        {
            //TODO:
        }
        public List<Movie> searchMovieByGenre(string title)
        {
            //TODO:
            return null;
        }
        public List<Movie> searchMovieByTags(string[] tags)
        {
            //TODO:
            return null;
        }
        public List<Movie> searchMovieByArtist(string Artists)
        {
            //TODO:
            return null;
        }
        public void addMovieToFavorites(Movie movie)
        {
            //TODO:
        }
        public void placeOnHold(Movie movie)
        {
            //TODO:
        }
        public void returnMovie(Rental rental)
        {
            //TODO:
        }

        public void reRentMovie(Rental rental)
        {
            //TODO:
        }

        public User()
        {
            //TODO: Constructor 
        }
    }

    class Movie
    {
        public int movieID { get; set; }
        public string title { get; set; }
        public string genre { get; set; }
        public List <string>[] artist { get; set; }
        public List<string>[] tags { get; set; }
        public bool availability { get; set; }
        public bool isTrending {  get; set; }
        public string releaseDate { get; set; }
        public int rentalCount { get; set; }

        public void getDetails()
        {
            //TODO:
        }
        public void checkAvailability()
        {
            //TODO:
        }
        public void incrementRentalCount()
        {
            //TODO:
        }
        public Movie()
        {
            //TODO: Constructor
        }
    }

    class Rental
    {
        public Rental()
        {
            //TODO: Constructor
        }
    }

    class Favorites:User
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

    class Suggestion:Movie
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

    class program
    {
        static void Main(string[] args)
        {
            var input = Console.ReadLine();
            input.ToLower();

            while (input != "exit")
            {
                //basic template to implement test cases
            }
        }
    }
}
