using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Net.TMDb
{
    //[DataContract]
    //public class ChangedItem
    //{
    //    [DataMember(Name = "id")]
    //    public string Id { get; internal set; }

    //    [DataMember(Name = "action")]
    //    public string Action { get; internal set; }

    //    [DataMember(Name = "time")]
    //    public string Time { get; internal set; }

    //    [DataMember(Name = "value")]
    //    public object Value { get; internal set; }
		
    //    [DataMember(Name = "iso_639_1")]
    //    public string LanguageCode { get; internal set; }
    //}

    public class ChangeList : PagedResult<ChangedItem>
	{
	}
	
	[DataContract]
	public class ChangedItem
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; internal set; }
	}
	
	[DataContract]
	internal class Certifications
	{
		[DataMember(Name = "certifications")]
		public IEnumerable<Certification> Results { get; internal set; }
	}

	[DataContract]
	public class Certification
	{
		[DataMember(Name = "certification")]
		public string Name { get; internal set; }

		[DataMember(Name = "meaning")]
		public string Meaning { get; internal set; }
	}
	
	[DataContract]
	public class Collection
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; internal set; }

		[DataMember(Name = "parts")]
		public IEnumerable<Movie> Parts { get; internal set; }
	}

	public class CollectionList : PagedResult<Collection>
	{
	}

	[DataContract]
	public class Company
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "description")]
		public string Description { get; internal set; }

		[DataMember(Name = "headquarters")]
		public string HeadQuarters { get; internal set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; internal set; }

		[DataMember(Name = "parent_company")]
		public Company Parent { get; internal set; }

		[DataMember(Name = "logo_path")]
		public string Logo { get; internal set; }
	}

	public class CompanyList : PagedResult<Company>
	{
	}

	[DataContract]
	public class Country
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }
	}

	[DataContract]
	public class Genre
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }
	}

    [DataContract]
    internal class Jobs
    {
        [DataMember(Name = "jobs")]
        public IEnumerable<Job> Results { get; internal set; }
    }

    [DataContract]
    public class Job
    {
        [DataMember(Name = "department")]
        public string Department { get; internal set; }

        [DataMember(Name = "job_list")]
        public IEnumerable<string> Items { get; internal set; }
    }

	public class SeriesList : PagedResult<Series>
	{
	}

	[DataContract]
	public class Series
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "original_name")]
		public string OriginalName { get; internal set; }

		[DataMember(Name = "overview")]
		public string Overview { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; internal set; }

		[DataMember(Name = "origin_country")]
		public IEnumerable<Country> Countries { get; internal set; }

		[DataMember(Name = "episode_run_time")]
		public IEnumerable<int> EpisodeRuntimes { get; internal set; }

		[DataMember(Name = "created_by")]
		public IEnumerable<Person> CreatedBy { get; internal set; }

		[DataMember(Name = "first_air_date")]
		public DateTime? FirstAirDate { get; internal set; }

		[DataMember(Name = "last_air_date")]
		public DateTime? LastAirDate { get; internal set; }

		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Genres { get; internal set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; internal set; }

		[DataMember(Name = "in_production")]
		public bool InProduction { get; internal set; }
		
		[DataMember(Name = "number_of_episodes")]
		public int EpisodeCount { get; internal set; }

		[DataMember(Name = "number_of_seasons")]
		public int SeasonCount { get; internal set; }

		[DataMember(Name = "seasons")]
		public IEnumerable<Season> Seasons { get; internal set; }

		[DataMember(Name = "languages")]
		public IEnumerable<string> Languages { get; internal set; }

		[DataMember(Name = "networks")]
		public IEnumerable<Network> Networks { get; internal set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; internal set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; internal set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; internal set; }

		[DataMember(Name = "keywords")]
		public Keywords Keywords { get; internal set; }
		
		[DataMember(Name = "translations")]
		public Translations Translations { get; internal set; }

		[DataMember(Name = "popularity")]
		public double Popularity { get; internal set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; internal set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; internal set; }

		[DataMember(Name = "status")]
		public string Status { get; internal set; }
	}

	[DataContract]
	public class Season
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "overview")]
		public string Overview { get; internal set; }

		[DataMember(Name = "air_date")]
		public DateTime? AirDate { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "season_number")]
		public int SeasonNumber { get; internal set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; internal set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; internal set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; internal set; }

		[DataMember(Name = "episodes")]
		public IEnumerable<Episode> Episodes { get; internal set; }
	}

	[DataContract]
	public class Episode
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "overview")]
		public string Overview { get; internal set; }

		[DataMember(Name = "still_path")]
		public string Backdrop { get; internal set; }

		[DataMember(Name = "production_code")]
		public int? ProductionCode { get; internal set; }

		[DataMember(Name = "air_date")]
		public DateTime? AirDate { get; internal set; }

		[DataMember(Name = "season_number")]
		public int? SeasonNumber { get; internal set; }

		[DataMember(Name = "episode_number")]
		public int EpisodeNumber { get; internal set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; internal set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; internal set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; internal set; }

		[DataMember(Name = "vote_average")]
		public decimal VoteAverage { get; internal set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; internal set; }
	}

	[DataContract]
	public class Network
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }
	}

	[DataContract]
	public class ExternalIds
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "freebase_id")]
		public string Freebase { get; internal set; }

		[DataMember(Name = "freebase_mid")]
		public string FreebaseMid { get; internal set; }

		[DataMember(Name = "tvdb_id")]
		public int? Tvdb { get; internal set; }

		[DataMember(Name = "tvrage_id")]
		public int? Tvrage { get; internal set; }
	}

	[DataContract]
	internal class Genres
	{
		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Results { get; internal set; }
	}

	[DataContract]
	public class Image
	{
		[DataMember(Name = "file_path")]
		public string FilePath { get; internal set; }

		[DataMember(Name = "width")]
		public short Width { get; internal set; }

		[DataMember(Name = "height")]
		public short Height { get; internal set; }

		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; internal set; }

		[DataMember(Name = "aspect_ratio")]
		public double AspectRatio { get; internal set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; internal set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; internal set; }
	}

	[DataContract]
	public class Language
	{
		[DataMember(Name = "iso_639_1")]
		public string Code { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }
	}

	[DataContract]
	public class Movie
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "title")]
		public string Title { get; internal set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; internal set; }

		[DataMember(Name = "tagline")]
		public string TagLine { get; internal set; }

		[DataMember(Name = "overview")]
		public string Overview { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; internal set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; internal set; }

		[DataMember(Name = "belongs_to_collection")]
		public Collection BelongsTo { get; internal set; }

		[DataMember(Name = "budget")]
		public int Budget { get; internal set; }

		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Genres { get; internal set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; internal set; }

		[DataMember(Name = "imdb_id")]
		public string Imdb { get; internal set; }

		[DataMember(Name = "production_companies")]
		public IEnumerable<Company> Companies { get; internal set; }

		[DataMember(Name = "production_countries")]
		public IEnumerable<Country> Countries { get; internal set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; internal set; }

		[DataMember(Name = "revenue")]
		public Int64 Revenue { get; internal set; }

		[DataMember(Name = "runtime")]
		public int? Runtime { get; internal set; }

		[DataMember(Name = "spoken_languages")]
		public IEnumerable<Language> Languages { get; internal set; }
		
		[DataMember(Name = "alternative_titles")]
		public AlternativeTitles AlternativeTitles { get; internal set; }
		
		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; internal set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; internal set; }
		
		[DataMember(Name = "videos")]
		public Videos Videos { get; internal set; }
		
		[DataMember(Name = "keywords")]
		public Keywords Keywords { get; internal set; }

		[DataMember(Name = "releases")]
		public Releases Releases { get; internal set; }
		
		[DataMember(Name = "translations")]
		public Translations Translations { get; internal set; }
		
		[DataMember(Name = "popularity")]
		public double Popularity { get; internal set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; internal set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; internal set; }

		[DataMember(Name = "status")]
		public string Status { get; internal set; }
	}

	[DataContract]
	public class Images
	{
		[DataMember(Name = "backdrops")]
		public IEnumerable<Image> Backdrops { get; internal set; }

		[DataMember(Name = "posters")]
		public IEnumerable<Image> Posters { get; internal set; }
	}

	public class MovieList : PagedResult<Movie>
	{
	}

	[DataContract]
	public class AlternativeTitles
	{
		[DataMember(Name = "titles")]
		public IEnumerable<AlternativeTitle> Results { get; internal set; }
	}

	[DataContract]
	public class AlternativeTitle
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; internal set; }

		[DataMember(Name = "title")]
		public string Title { get; internal set; }
	}

	[DataContract]
	public class MediaCredits
	{
		[DataMember(Name = "cast")]
		public IEnumerable<MediaCast> Cast { get; internal set; }

		[DataMember(Name = "crew")]
		public IEnumerable<MediaCrew> Crew { get; internal set; }
	}

	[DataContract]
	public class MediaCast
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "character")]
		public string Character { get; internal set; }

		[DataMember(Name = "profile_path")]
		public string Profile { get; internal set; }
	}

	[DataContract]
	public class MediaCrew
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "department")]
		public string Department { get; internal set; }

		[DataMember(Name = "job")]
		public string Job { get; internal set; }

		[DataMember(Name = "profile_path")]
		public string Profile { get; internal set; }
	}

	[DataContract]
	public class Keywords
	{
		[DataMember(Name = "keywords")]
		public IEnumerable<Keyword> Results { get; internal set; }
	}

	[DataContract]
	public class Translations
	{
		[DataMember(Name = "translations")]
		public IEnumerable<Translation> Results { get; internal set; }
	}

	[DataContract]
	public class Releases
	{
		[DataMember(Name = "countries")]
		public IEnumerable<CountryRelease> Countries { get; internal set; }
	}

	[DataContract]
	public class CountryRelease
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; internal set; }

		[DataMember(Name = "certification")]
		public string Certification { get; internal set; }

		[DataMember(Name = "release_date")]
		public DateTime ReleaseDate { get; internal set; }
	}

	[DataContract]
	public class Videos
	{
		[DataMember(Name = "results")]
		public IEnumerable<Video> Results { get; internal set; }
	}
	
	[DataContract]
	public class Video
	{
		[DataMember(Name = "id")]
		public string Id { get; internal set; }
		
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; internal set; }

		[DataMember(Name = "key")]
		public string Key { get; internal set; }

		[DataMember(Name = "site")]
		public string Site { get; internal set; }

		[DataMember(Name = "size")]
		public int Size { get; internal set; }

		[DataMember(Name = "type")]
		public string Type { get; internal set; }
	}
	
	[DataContract]
	public class Translation
	{
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "english_name")]
		public string EnglishName { get; internal set; }
	}

	[DataContract]
	public class Keyword
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }
	}

	[DataContract]
	public class FindResult
	{
		[DataMember(Name = "movie_results")]
		public IEnumerable<Movie> Movies { get; internal set; }

		[DataMember(Name = "person_results")]
		public IEnumerable<Person> People { get; internal set; }

		[DataMember(Name = "tv_results")]
		public IEnumerable<Series> Series { get; internal set; }
	}

	[DataContract]
	public class Person
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "name")]
		public string Name { get; internal set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; internal set; }

		[DataMember(Name = "also_known_as")]
		public IEnumerable<string> KnownAs { get; internal set; }

		[DataMember(Name = "biography")]
		public string Biography { get; internal set; }

		[DataMember(Name = "birthday")]
		public string BirthDay { get; internal set; }

		[DataMember(Name = "deathday")]
		public string DeathDay { get; internal set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; internal set; }

		[DataMember(Name = "place_of_birth")]
		public string BirthPlace { get; internal set; }

		[DataMember(Name = "profile_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "credits")]
		public PersonCredits Credits { get; internal set; }
		
		[DataMember(Name = "images")]
		public PersonImages Images { get; internal set; }
	}

	[DataContract]
	public class PersonImages
	{
		[DataMember(Name = "profiles")]
		public IEnumerable<Image> Results { get; internal set; }
	}

	[DataContract]
	public class PersonCredits
	{
		[DataMember(Name = "cast")]
		public IEnumerable<PersonCast> Cast { get; internal set; }

		[DataMember(Name = "crew")]
		public IEnumerable<PersonCrew> Crew { get; internal set; }
	}

	[DataContract]
	public class PersonCast
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "title")]
		public string Title { get; internal set; }

		[DataMember(Name = "character")]
		public string Character { get; internal set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; internal set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; internal set; }
	}

	[DataContract]
	public class PersonCrew
	{
		[DataMember(Name = "id")]
		public int Id { get; internal set; }

		[DataMember(Name = "title")]
		public string Title { get; internal set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; internal set; }

		[DataMember(Name = "department")]
		public string Department { get; internal set; }

		[DataMember(Name = "job")]
		public string Job { get; internal set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; internal set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; internal set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; internal set; }
	}

	public class PersonList : PagedResult<Person>
	{
	}
	
	[DataContract]
	public class Review
	{
		[DataMember(Name = "id")]
		public string Id { get; internal set; }

		[DataMember(Name = "author")]
		public string Author { get; internal set; }

		[DataMember(Name = "content")]
		public string Content { get; internal set; }
		
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; internal set; }

		[DataMember(Name = "media_id")]
		public int MediaId { get; internal set; }

		[DataMember(Name = "media_title")]
		public string MediaTitle { get; internal set; }

		[DataMember(Name = "media_type")]
		public string MediaType { get; internal set; }

		[DataMember(Name = "url")]
		public string Url { get; internal set; }
	}

    [DataContract]
	public class Lists
    {
        [DataMember(Name = "id")]
        public string Id { get; internal set; }

        [DataMember(Name = "name")]
        public string Name { get; internal set; }

        [DataMember(Name = "description")]
        public string Description { get; internal set; }

        [DataMember(Name = "poster_path")]
        public string Poster { get; internal set; }

        [DataMember(Name = "created_by")]
        public string Creator { get; internal set; }

        [DataMember(Name = "iso_639_1")]
        public string LanguageCode { get; internal set; }

        [DataMember(Name = "posters")]
        public IEnumerable<Movie> Items { get; internal set; }

        [DataMember(Name = "favorite_count")]
        public int FavoriteCount { get; internal set; }

        [DataMember(Name = "item_count")]
        public int ItemCount { get; internal set; }
    }

	[DataContract]
	public abstract class PagedResult<T>
	{
		[DataMember(Name = "results")]
		public IEnumerable<T> Results { get; internal set; }
		
		[DataMember(Name = "page")]
		public int PageIndex { get; internal set; }

		[DataMember(Name = "total_pages")]
		public int PageCount { get; internal set; }

		[DataMember(Name = "total_results")]
		public int TotalCount { get; internal set; }
	}

	[DataContract]
	internal class AuthenticationResult
	{
		[DataMember(Name = "request_token")]
		public string Token { get; set; }

		[DataMember(Name = "session_id")]
		public string Session { get; set; }

		[DataMember(Name = "guest_session_id")]
		public string Guest { get; set; }
		
		[DataMember(Name = "expires_at")]
		public DateTime? ExpiresAt { get; set; }

		[DataMember(Name = "success")]
		public bool Success  { get; set; }
	}

	[DataContract]
	internal class Status
	{
		[DataMember(Name = "status_code")]
		public int Code { get; set; }

		[DataMember(Name = "status_message")]
		public string Message { get; set; }
	}
}
