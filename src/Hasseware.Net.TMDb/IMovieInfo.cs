using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.TMDb
{
	public interface IMovieInfo
	{
		/// <summary>
		/// The find method makes it easy to search for objects in our database by an external id. For instance, an IMDB ID. This will search all objects (movies, TV shows and people) and return the results in a single response. TV season and TV episode searches will be supported shortly.
		/// </summary>
		Task<FindResult> FindAsync(string id, string externalSource, CancellationToken cancellationToken);

		/// <summary>
		/// Search for movies by title.
		/// </summary>
		Task<MovieList> SearchAsync(string query, string language, bool includeAdult, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Discover movies by different types of data like average rating, number of votes, genres and certifications. You can get a valid list of certifications from the /certifications method.
		/// </summary>
		Task<MovieList> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the basic movie information for a specific movie id.
		/// </summary>
		Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images (posters and backdrops) for a specific movie id.
		/// </summary>
		Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the cast and crew information for a specific movie id.
		/// </summary>
		Task<MediaCredits> GetCreditsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the videos (trailers, teasers, clips, etc...) for a specific movie id.
		/// </summary>
		Task<IEnumerable<Video>> GetVideosAsync(int id, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the similar movies for a specific movie id.
		/// </summary>
		Task<MovieList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of popular movies on The Movie Database. This list refreshes every day.
		/// </summary>
		Task<MovieList> GetPopularAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of top rated movies. By default, this list will only include movies that have 10 or more votes. This list refreshes every day.
		/// </summary>
		Task<MovieList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken);

		Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken);

		Task<IEnumerable<Keyword>> GetKeywordsAsync(int id, CancellationToken cancellationToken);

		Task<IEnumerable<CountryRelease>> GetReleasesAsync(int id, CancellationToken cancellationToken);

		Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken);
	}

	public interface ISeriesInfo
	{
		/// <summary>
		/// Search for TV shows by title.
		/// </summary>
		Task<SeriesList> SearchAsync(string query, string language, DateTime? firstAirDate, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the primary information about a TV series by id.
		/// </summary>
		Task<Series> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken);

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
		/// Get the cast & crew information about a TV series. Just like the website, we pull this information from the last season of the series.
		/// </summary>
		Task<MediaCredits> GetCreditsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images (episode stills) for a TV series by id, TV season by its season number, or TV episode by combination of a season and episode number.
		/// Since episode stills don't have a language, this call will always return all images.
		/// </summary>
		Task<Images> GetImagesAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the similar TV shows for a specific tv id.
		/// </summary>
		Task<SeriesList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the videos that have been added to a TV series (trailers, opening credits, etc...)
		/// </summary>
		Task<IEnumerable<Video>> GetVideosAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of translations that exist for a TV series. These translations cascade down to the episode level.
		/// </summary>
		Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of popular TV shows. This list refreshes every day.
		/// </summary>
		Task<SeriesList> GetPopularAsync(string language, int page, CancellationToken cancellationToken);

		/// <summary>
		/// Get the list of top rated TV shows. By default, this list will only include TV shows that have 2 or more votes. This list refreshes every day.
		/// </summary>
		Task<SeriesList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken);
	}

	public interface IGenreInfo
	{
		/// <summary>
		/// Get the list of genres.
		/// </summary>
		Task<IEnumerable<Genre>> GetAsync(CancellationToken cancellationToken);

		Task<MovieList> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken);
	}

	public interface ICollectionInfo
	{
		Task<CollectionList> SearchAsync(string query, string language, int page, CancellationToken cancellationToken);
	}
	
	public interface ICompanyInfo
	{
		Task<Company> GetAsync(int id, CancellationToken cancellationToken);

		Task<MovieList> GetMoviesAsync(int id, string language, int page, CancellationToken cancellationToken);

		Task<CompanyList> SearchAsync(string query, int page, CancellationToken cancellationToken);
	}

	public interface IPeopleInfo
	{
		/// <summary>
		/// Get the general person information for a specific id.
		/// </summary>
		Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken);

		Task<PersonCredits> GetCreditsAsync(int id, string language, DataInfoType type, CancellationToken cancellationToken);

		/// <summary>
		/// Get the images for a specific person id.
		/// </summary>
		Task<IEnumerable<Image>> GetImagesAsync(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Get the external ids for a specific person id.
		/// </summary>
		Task<ExternalIds> GetIdsAsync(int id, CancellationToken cancellationToken);

		Task<PersonList> SearchAsync(string query, bool includeAdult, int page, CancellationToken cancellationToken);
	}

	[Flags]
	public enum DataInfoType
	{
		Movie = 1,
		Television = 2,
		Combined = 3
	}
}
