using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
	public interface IMovieInfo
	{
		/// <summary>
		/// Get the basic movie information for a specific movie id.
		/// </summary>
		Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Search for movies by title.
		/// </summary>
        Task<Movies> SearchAsync(string query, string language, bool includeAdult, int? year, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Discover movies by different types of data like average rating, number of votes, genres and certifications. You can get a valid list of certifications from the /certifications method.
		/// </summary>
		Task<Movies> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images (posters and backdrops) for a specific movie id.
		/// </summary>
		Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the reviews for a particular movie id.
		/// </summary>
		Task<Reviews> GetReviewsAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the lists that the movie belongs to.
		/// </summary>
		Task<Lists> GetListsAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the cast and crew information for a specific movie id.
		/// </summary>
		Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the videos (trailers, teasers, clips, etc...) for a specific movie id.
		/// </summary>
		Task<IEnumerable<Video>> GetVideosAsync(int id, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the similar movies for a specific movie id.
		/// </summary>
		Task<Movies> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get a list of rated movies for a specific guest session id.
		/// </summary>
		Task<Movies> GetGuestRatedAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of popular movies on The Movie Database. This list refreshes every day.
		/// </summary>
		Task<Movies> GetPopularAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of top rated movies. By default, this list will only include movies that have 10 or more votes. This list refreshes every day.
		/// </summary>
		Task<Movies> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of movies playing in theatres. This list refreshes every day. The maximum number of items this list will include is 100.
		/// </summary>
		Task<Movies> GetNowPlayingAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of upcoming movies. This list refreshes every day. The maximum number of items this list will include is 100.
		/// </summary>
		Task<Movies> GetUpcomingAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the alternative titles for a specific movie id.
		/// </summary>
		Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the plot keywords for a specific movie id.
		/// </summary>
		Task<IEnumerable<Keyword>> GetKeywordsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the release date and certification information by country for a specific movie id.
		/// </summary>
		Task<IEnumerable<Release>> GetReleasesAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the translations for a specific movie id.
		/// </summary>
		Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get a list of movie ids that have been edited. The maximum number of days that can be returned in a single request is 14.
		/// </summary>
		Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of rated movies (and associated rating) for an account.
		/// </summary>
		Task<Movies> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of favorite movies for an account.
		/// </summary>
		Task<Movies> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of movies on an accounts watchlist.
		/// </summary>
		Task<Movies> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users rate a movie. A valid session id or guest session id is required.
		/// </summary>
		Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken);

		/// <summary>
		/// Add or remove a movie to an accounts favorite list.
		/// </summary>
		Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken);

		/// <summary>
		/// Add or remove a movie to an accounts watch list.
		/// </summary>
		Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken);
	}

	public interface IShowInfo
	{
		/// <summary>
		/// Search for TV shows by title.
		/// </summary>
		Task<Shows> SearchAsync(string query, string language, DateTime? firstAirDate, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Discover TV shows by different types of data like average rating, number of votes, genres, the network they aired on and air dates.
		/// </summary>
		Task<Shows> DiscoverAsync(string language, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string networks, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the primary information about a TV series by id.
		/// </summary>
		Task<Show> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken);

        /// <summary>
        /// Get the latest TV show id.
        /// </summary>
        Task<Show> GetLatestAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Get the primary information about a TV season by its season number.
		/// </summary>
		Task<Season> GetSeasonAsync(int id, int season, string language, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Get the primary information about a TV episode by combination of a season and episode number.
		/// </summary>
		Task<Episode> GetEpisodeAsync(int id, int season, int episode, string language, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Get the external ids for a TV series by id, TV season by its season number, or TV episode by combination of a season and episode number.
		/// </summary>
		Task<ExternalIds> GetIdsAsync(int id, int? season, int? episode, CancellationToken cancellationToken);

		/// <summary>
        /// Get the cast &amp; crew information about a TV series. Just like the website, we pull this information from the last season of the series.
		/// </summary>
		Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images (episode stills) for a TV series by id, TV season by its season number, or TV episode by combination of a season and episode number.
		/// Since episode stills don't have a language, this call will always return all images.
		/// </summary>
		Task<Images> GetImagesAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the similar TV shows for a specific tv id.
		/// </summary>
		Task<Shows> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the videos that have been added to a TV series (trailers, opening credits, etc...)
		/// </summary>
		Task<IEnumerable<Video>> GetVideosAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of translations that exist for a TV series. These translations cascade down to the episode level.
		/// </summary>
		Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Get the list of TV shows that are currently on the air. This query looks for any TV show that has an episode with an air date in the next 7 days.
        /// </summary>
        Task<Shows> GetOnAirAsync(string language, int page, CancellationToken cancellationToken);

        /// <summary>
        /// Get the list of TV shows that air today. Without a specified timezone, this query defaults to EST (Eastern Time UTC-05:00).
        /// </summary>
        Task<Shows> GetAiringAsync(string language, int page, string timezone, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of popular TV shows. This list refreshes every day.
		/// </summary>
		Task<Shows> GetPopularAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of top rated TV shows. By default, this list will only include TV shows that have 2 or more votes. This list refreshes every day.
		/// </summary>
		Task<Shows> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get a list of TV show ids that have been edited. The maximum number of days that can be returned in a single request is 14.
		/// </summary>
		Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken);

		/// <summary>
		/// This method is used to retrieve the basic information about a TV network. You can use this ID to search for TV shows with the discover. 
		/// </summary>
		Task<string> GetNetworkAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of rated TV shows (and associated rating) for an account.
		/// </summary>
		Task<Shows> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of favorite TV shows for an account.
		/// </summary>
		Task<Shows> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of TV shows on an accounts watchlist.
		/// </summary>
		Task<Shows> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users rate a TV show. A valid session id or guest session id is required.
		/// </summary>
		Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users rate a TV episode. A valid session id or guest session id is required.
		/// </summary>
		Task<bool> SetRatingAsync(string session, int id, int season, int episode, decimal value, CancellationToken cancellationToken);
		
		/// <summary>
		/// Add or remove a TV show to an accounts favorite list.
		/// </summary>
		Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken);

		/// <summary>
		/// Add or remove a TV show to an accounts watch list.
		/// </summary>
		Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken);
	}

	public interface IGenreInfo
	{
		/// <summary>
		/// Get the list of genres.
		/// </summary>
		Task<IEnumerable<Genre>> GetAsync(DataInfoType type, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of movies for a particular genre by id. By default, only movies with 10 or more votes are included.
		/// </summary>
		Task<Movies> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken);
	}

	public interface ICollectionInfo
	{
		/// <summary>
		/// Get the basic collection information for a specific collection id.
		/// </summary>
		Task<Collection> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Search for collections by name.
		/// </summary>
		Task<Collections> SearchAsync(string query, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get all of the images for a particular collection by collection id.
		/// </summary>
		Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken);
	}
	
	public interface ICompanyInfo
	{
		/// <summary>
		/// This method is used to retrieve all of the basic information about a company.
		/// </summary>
		Task<Company> GetAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Search for companies by name.
		/// </summary>
		Task<Companies> SearchAsync(string query, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of movies associated with a particular company.
		/// </summary>
		Task<Movies> GetMoviesAsync(int id, string language, int page, CancellationToken cancellationToken);
	}

	public interface IPeopleInfo
	{
		/// <summary>
		/// Get the general person information for a specific id.
		/// </summary>
		Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Search for people by name.
		/// </summary>
		Task<People> SearchAsync(string query, bool includeAdult, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the movie credits for a specific person id.
		/// </summary>
		Task<IEnumerable<PersonCredit>> GetCreditsAsync(int id, string language, DataInfoType type, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images for a specific person id.
		/// </summary>
		Task<IEnumerable<Image>> GetImagesAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the external ids for a specific person id.
		/// </summary>
		Task<ExternalIds> GetIdsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get a list of people ids that have been edited. The maximum number of days that can be returned in a single request is 14.
		/// </summary>
		Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken);
	}

	public interface IListInfo
	{
		/// <summary>
		/// Get a list by id.
		/// </summary>
		Task<List> GetAsync(string id, CancellationToken cancellationToken);

		/// <summary>
		/// Check to see if a movie ID is already added to a list.
		/// </summary>
		Task<bool> ContainsAsync(string id, int movieId, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users create a new list. A valid session id is required.
		/// </summary>
		Task<string> CreateAsync(string session, string name, string description, string language, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users add new movies to a list that they created. A valid session id is required.
		/// </summary>
		Task<bool> InsertAsync(string session, string id, string mediaId, CancellationToken cancellationToken);

		/// <summary>
		/// This method lets users delete movies from a list that they created. A valid session id is required.
		/// </summary>
		Task<bool> RemoveAsync(string session, string id, string mediaId, CancellationToken cancellationToken);

		/// <summary>
		/// Clear all of the items within a list. This is a irreversible action and should be treated with caution. A valid session id is required.
		/// </summary>
		Task<bool> ClearAsync(string session, string id, CancellationToken cancellationToken);

		/// <summary>
		/// Delete a list by id.
		/// </summary>
		Task<bool> DeleteAsync(string session, string id, CancellationToken cancellationToken);
	}

	public interface IReviewInfo
	{
		/// <summary>
		/// Get the full details of a review by id.
		/// </summary>
		Task<Review> GetAsync(string id, CancellationToken cancellationToken);
	}

	public interface ISystemInfo
	{
		/// <summary>
		/// Get the basic information for an account. You will need to have a valid session id.
		/// </summary>
		Task<Account> GetAccountAsync(string session, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of supported certifications for movie and/or tv shows.
		/// </summary>
		Task<IEnumerable<Certification>> GetCertificationsAsync(DataInfoType type, CancellationToken cancellationToken);

		/// <summary>
		/// Get the system wide configuration information. Some elements of the API require some knowledge of this configuration data. 
		/// </summary>
		Task<dynamic> GetConfigurationAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of supported timezones for the API methods that support them.
		/// </summary>
		Task<dynamic> GetTimezonesAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Get a list of valid jobs.
		/// </summary>
		Task<IEnumerable<Job>> GetJobsAsync(CancellationToken cancellationToken);
	}

	[Flags]
	public enum DataInfoType
	{
		Movie = 1,
		Television = 2,
		Combined = 3
	}

	public enum AccountListType
	{
		Favorite,
		Rated,
		Watchlist
	}
}

#pragma warning restore 1591