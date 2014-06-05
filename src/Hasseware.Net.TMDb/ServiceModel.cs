using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Net.TMDb
{
	[DataContract]
	public class Changes
	{
		[DataMember(Name = "changes")]
		public IEnumerable<Change> Results { get; set; }
	}
	
	[DataContract]
	public class Change
	{
		[DataMember(Name = "key")]
		public string Key { get; set; }

		[DataMember(Name = "items")]
		public IEnumerable<ChangedItem> Items { get; set; }
	}
	
	[DataContract]
	public class ChangedItem
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "action")]
		public string Action { get; set; }

		[DataMember(Name = "time")]
		public string Time { get; set; }

		[DataMember(Name = "value")]
		public object Value { get; set; }
		
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; set; }
	}

	public class ChangeList : PagedResult<ChangedListItem>
	{
	}
	
	[DataContract]
	public class ChangedListItem
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; set; }
	}
	
	[DataContract]
	public class Certifications
	{
		[DataMember(Name = "certifications")]
		public IEnumerable<Certification> Results { get; set; }
	}

	[DataContract]
	public class Certification
	{
		[DataMember(Name = "certification")]
		public string Name { get; set; }

		[DataMember(Name = "meaning")]
		public string Meaning { get; set; }
	}
	
	[DataContract]
	public class Collection
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; set; }

		[DataMember(Name = "parts")]
		public IEnumerable<Movie> Parts { get; set; }
	}

	public class CollectionList : PagedResult<Collection>
	{
	}

	[DataContract]
	public class Company
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "description")]
		public string Description { get; set; }

		[DataMember(Name = "headquarters")]
		public string HeadQuarters { get; set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; set; }

		[DataMember(Name = "parent_company")]
		public Company Parent { get; set; }

		[DataMember(Name = "logo_path")]
		public string Logo { get; set; }
	}

	public class CompanyList : PagedResult<Company>
	{
	}

	[DataContract]
	public class Country
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}

	[DataContract]
	public class Genre
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}

	public class SeriesList : PagedResult<Series>
	{
	}

	[DataContract]
	public class Series
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "original_name")]
		public string OriginalName { get; set; }

		[DataMember(Name = "overview")]
		public string Overview { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; set; }

		[DataMember(Name = "origin_country")]
		public IEnumerable<Country> Countries { get; set; }

		[DataMember(Name = "episode_run_time")]
		public IEnumerable<int> EpisodeRuntimes { get; set; }

		[DataMember(Name = "created_by")]
		public IEnumerable<Person> CreatedBy { get; set; }

		[DataMember(Name = "first_air_date")]
		public DateTime? FirstAirDate { get; set; }

		[DataMember(Name = "last_air_date")]
		public DateTime? LastAirDate { get; set; }

		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Genres { get; set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; set; }

		[DataMember(Name = "in_production")]
		public bool InProduction { get; set; }
		
		[DataMember(Name = "number_of_episodes")]
		public int EpisodeCount { get; set; }

		[DataMember(Name = "number_of_seasons")]
		public int SeasonCount { get; set; }

		[DataMember(Name = "seasons")]
		public IEnumerable<Season> Seasons { get; set; }

		[DataMember(Name = "languages")]
		public IEnumerable<string> Languages { get; set; }

		[DataMember(Name = "networks")]
		public IEnumerable<Network> Networks { get; set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; set; }

		[DataMember(Name = "keywords")]
		public Keywords Keywords { get; set; }
		
		[DataMember(Name = "translations")]
		public Translations Translations { get; set; }

		[DataMember(Name = "popularity")]
		public double Popularity { get; set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }
	}

	[DataContract]
	public class Season
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "overview")]
		public string Overview { get; set; }

		[DataMember(Name = "air_date")]
		public DateTime? AirDate { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "season_number")]
		public int SeasonNumber { get; set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; set; }

		[DataMember(Name = "episodes")]
		public IEnumerable<Episode> Episodes { get; set; }
	}

	[DataContract]
	public class Episode
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "overview")]
		public string Overview { get; set; }

		[DataMember(Name = "still_path")]
		public string Backdrop { get; set; }

		[DataMember(Name = "production_code")]
		public int? ProductionCode { get; set; }

		[DataMember(Name = "air_date")]
		public DateTime? AirDate { get; set; }

		[DataMember(Name = "season_number")]
		public int? SeasonNumber { get; set; }

		[DataMember(Name = "episode_number")]
		public int EpisodeNumber { get; set; }

		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; set; }
				
		[DataMember(Name = "videos")]
		public Videos Videos { get; set; }

		[DataMember(Name = "vote_average")]
		public decimal VoteAverage { get; set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; set; }
	}

	[DataContract]
	public class Network
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}

	[DataContract]
	public class ExternalIds
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "freebase_id")]
		public string Freebase { get; set; }

		[DataMember(Name = "freebase_mid")]
		public string FreebaseMid { get; set; }

		[DataMember(Name = "tvdb_id")]
		public int? Tvdb { get; set; }

		[DataMember(Name = "tvrage_id")]
		public int? Tvrage { get; set; }
	}

	[DataContract]
	public class Genres
	{
		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Results { get; set; }
	}

	[DataContract]
	public class Image
	{
		[DataMember(Name = "file_path")]
		public string FilePath { get; set; }

		[DataMember(Name = "width")]
		public short Width { get; set; }

		[DataMember(Name = "height")]
        public short Height { get; set; }

		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; set; }

		[DataMember(Name = "aspect_ratio")]
		public double AspectRatio { get; set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; set; }
	}

	[DataContract]
	public class Language
	{
		[DataMember(Name = "iso_639_1")]
		public string Code { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}

	[DataContract]
	public class Movie
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; set; }

		[DataMember(Name = "tagline")]
		public string TagLine { get; set; }

		[DataMember(Name = "overview")]
		public string Overview { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "backdrop_path")]
		public string Backdrop { get; set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; set; }

		[DataMember(Name = "belongs_to_collection")]
		public Collection BelongsTo { get; set; }

		[DataMember(Name = "budget")]
		public int Budget { get; set; }

		[DataMember(Name = "genres")]
		public IEnumerable<Genre> Genres { get; set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; set; }

		[DataMember(Name = "imdb_id")]
		public string Imdb { get; set; }

		[DataMember(Name = "production_companies")]
		public IEnumerable<Company> Companies { get; set; }

		[DataMember(Name = "production_countries")]
		public IEnumerable<Country> Countries { get; set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; set; }

		[DataMember(Name = "revenue")]
		public Int64 Revenue { get; set; }

		[DataMember(Name = "runtime")]
		public int? Runtime { get; set; }

		[DataMember(Name = "spoken_languages")]
		public IEnumerable<Language> Languages { get; set; }
		
		[DataMember(Name = "alternative_titles")]
		public AlternativeTitles AlternativeTitles { get; set; }
		
		[DataMember(Name = "credits")]
		public MediaCredits Credits { get; set; }
		
		[DataMember(Name = "images")]
		public Images Images { get; set; }
		
		[DataMember(Name = "videos")]
		public Videos Videos { get; set; }
		
		[DataMember(Name = "keywords")]
		public Keywords Keywords { get; set; }

		[DataMember(Name = "releases")]
		public Releases Releases { get; set; }
		
		[DataMember(Name = "translations")]
		public Translations Translations { get; set; }
		
		[DataMember(Name = "popularity")]
		public double Popularity { get; set; }

		[DataMember(Name = "vote_average")]
		public double VoteAverage { get; set; }

		[DataMember(Name = "vote_count")]
		public int VoteCount { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }
	}

	[DataContract]
	public class Images
	{
		[DataMember(Name = "backdrops")]
		public IEnumerable<Image> Backdrops { get; set; }

		[DataMember(Name = "posters")]
		public IEnumerable<Image> Posters { get; set; }
	}

	public class MovieList : PagedResult<Movie>
	{
	}

	[DataContract]
	public class AlternativeTitles
	{
		[DataMember(Name = "titles")]
		public IEnumerable<AlternativeTitle> Results { get; set; }
	}

	[DataContract]
	public class AlternativeTitle
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; set; }

		[DataMember(Name = "title")]
		public string Title { get; set; }
	}

	[DataContract]
	public class MediaCredits
	{
		[DataMember(Name = "cast")]
		public IEnumerable<MediaCast> Cast { get; set; }

		[DataMember(Name = "crew")]
		public IEnumerable<MediaCrew> Crew { get; set; }
	}

	[DataContract]
	public class MediaCast
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "character")]
		public string Character { get; set; }

		[DataMember(Name = "profile_path")]
		public string Profile { get; set; }
	}

	[DataContract]
	public class MediaCrew
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "department")]
		public string Department { get; set; }

		[DataMember(Name = "job")]
		public string Job { get; set; }

		[DataMember(Name = "profile_path")]
		public string Profile { get; set; }
	}

	[DataContract]
	public class Keywords
	{
		[DataMember(Name = "keywords")]
		public IEnumerable<Keyword> Results { get; set; }
	}

	[DataContract]
	public class Translations
	{
		[DataMember(Name = "translations")]
		public IEnumerable<Translation> Results { get; set; }
	}

	[DataContract]
	public class Releases
	{
		[DataMember(Name = "countries")]
		public IEnumerable<CountryRelease> Countries { get; set; }
	}

	[DataContract]
	public class CountryRelease
	{
		[DataMember(Name = "iso_3166_1")]
		public string Code { get; set; }

		[DataMember(Name = "certification")]
		public string Certification { get; set; }

		[DataMember(Name = "release_date")]
		public DateTime ReleaseDate { get; set; }
	}

	[DataContract]
	public class Videos
	{
		[DataMember(Name = "results")]
		public IEnumerable<Video> Results { get; set; }
	}
	
	[DataContract]
	public class Video
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }
		
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; set; }

		[DataMember(Name = "key")]
		public string Key { get; set; }

		[DataMember(Name = "site")]
		public string Site { get; set; }

		[DataMember(Name = "size")]
		public int Size { get; set; }

		[DataMember(Name = "type")]
		public string Type { get; set; }
	}
	
	[DataContract]
	public class Translation
	{
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "english_name")]
		public string EnglishName { get; set; }
	}

	[DataContract]
	public class Keyword
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}

	[DataContract]
	public class FindResult
	{
		[DataMember(Name = "movie_results")]
		public IEnumerable<Movie> Movies { get; set; }

		[DataMember(Name = "person_results")]
		public IEnumerable<Person> People { get; set; }

		[DataMember(Name = "tv_results")]
		public IEnumerable<Series> Series { get; set; }
	}

	[DataContract]
	public class Person
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; set; }

		[DataMember(Name = "also_known_as")]
		public IEnumerable<string> KnownAs { get; set; }

		[DataMember(Name = "biography")]
		public string Biography { get; set; }

		[DataMember(Name = "birthday")]
		public string BirthDay { get; set; }

		[DataMember(Name = "deathday")]
        public string DeathDay { get; set; }

		[DataMember(Name = "homepage")]
		public string HomePage { get; set; }

		[DataMember(Name = "place_of_birth")]
		public string BirthPlace { get; set; }

		[DataMember(Name = "profile_path")]
		public string Poster { get; set; }

		[DataMember(Name = "credits")]
		public PersonCredits Credits { get; set; }
		
		[DataMember(Name = "images")]
		public PersonImages Images { get; set; }
	}

	[DataContract]
	public class PersonImages
	{
		[DataMember(Name = "profiles")]
		public IEnumerable<Image> Results { get; set; }
	}

	[DataContract]
	public class PersonCredits
	{
		[DataMember(Name = "cast")]
		public IEnumerable<PersonCast> Cast { get; set; }

		[DataMember(Name = "crew")]
		public IEnumerable<PersonCrew> Crew { get; set; }
	}

	[DataContract]
	public class PersonCast
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "character")]
		public string Character { get; set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; set; }
	}

	[DataContract]
	public class PersonCrew
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "original_title")]
		public string OriginalTitle { get; set; }

		[DataMember(Name = "department")]
		public string Department { get; set; }

		[DataMember(Name = "job")]
		public string Job { get; set; }

		[DataMember(Name = "poster_path")]
		public string Poster { get; set; }

		[DataMember(Name = "release_date")]
		public DateTime? ReleaseDate { get; set; }

		[DataMember(Name = "adult")]
		public bool Adult { get; set; }
	}

	public class PersonList : PagedResult<Person>
	{
	}
	
	[DataContract]
	public class Review
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "author")]
		public string Author { get; set; }

		[DataMember(Name = "content")]
		public string Content { get; set; }
		
		[DataMember(Name = "iso_639_1")]
		public string LanguageCode { get; set; }

		[DataMember(Name = "media_id")]
		public int MediaId { get; set; }

		[DataMember(Name = "media_title")]
		public string MediaTitle { get; set; }

		[DataMember(Name = "media_type")]
		public string MediaType { get; set; }

		[DataMember(Name = "url")]
		public string Url { get; set; }
	}

	[DataContract]
	public abstract class PagedResult<T>
	{
		[DataMember(Name = "results")]
		public IEnumerable<T> Results { get; set; }
		
		[DataMember(Name = "page")]
		public int PageIndex { get; set; }

		[DataMember(Name = "total_pages")]
		public int PageCount { get; set; }

		[DataMember(Name = "total_results")]
		public int TotalCount { get; set; }
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
