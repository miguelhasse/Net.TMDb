using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.TMDb
{
	public sealed class ServiceClient
	{
		private readonly string BaseUrl;

		#region Constructors

		public ServiceClient(string apiKey)
		{
			this.BaseUrl = String.Concat(@"http://api.themoviedb.org/3/{0}?api_key=", apiKey, "&");

			this.Movies = new MovieContext(this);
			this.Series = new SeriesContext(this);
			this.Companies = new CompanyContext(this);
			this.Genres = new GenreContext(this);
			this.People = new PeopleContext(this);
			this.Collections = new CollectionContext(this);
		}

		#endregion

		public IMovieInfo Movies { get; private set; }

		public ISeriesInfo Series { get; private set; }

		public ICompanyInfo Companies { get; private set; }

		public IGenreInfo Genres { get; private set; }

		public IPeopleInfo People { get; private set; }

		public ICollectionInfo Collections { get; private set; }

		private Task<HttpResponseMessage> GetAsync(string cmd, IDictionary<string, string> parameters, CancellationToken cancellationToken)
		{
			var client = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = false,
				PreAuthenticate = true,
				UseDefaultCredentials = true,
				UseCookies = false
			});
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();

			client.GetAsync(CreateRequestUri(cmd, parameters),
				HttpCompletionOption.ResponseHeadersRead, cancellationToken).ContinueWith(t =>
				{
					if (t.IsCanceled) tcs.TrySetCanceled();
					else if (!t.Result.IsSuccessStatusCode)
					{
						if (t.Result.Content != null)
						{
							t.Result.Content.ReadAsStringAsync().ContinueWith(t2 =>
							{
								var status = JsonConvert.DeserializeObject<Status>(t2.Result);
								tcs.TrySetException(new ServiceRequestException((int)t.Result.StatusCode,
									status.Code, status.Message));
							});
						}
						else tcs.TrySetException(new ServiceRequestException(
							(int)t.Result.StatusCode, -1, t.Result.ReasonPhrase));
					}
					else if (t.IsFaulted) tcs.TrySetException(t.Exception);
					else if (t.IsCompleted) tcs.TrySetResult(t.Result);
				});
			return tcs.Task;
		}

		private static async Task<T> Deserialize<T>(HttpResponseMessage response)
		{
			string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
			System.Diagnostics.Debug.WriteLine(responseJson);
#endif
			return JsonConvert.DeserializeObject<T>(responseJson);
		}

		private Uri CreateRequestUri(string cmd, IDictionary<string, string> parameters)
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat(BaseUrl, cmd);

			if (parameters != null)
			{
				sb.Append(parameters.Where(s => s.Value != null)
					.Select(s => String.Concat(s.Key, "=", s.Value))
					.Aggregate((s, n) => String.Concat(s, "&", n)));
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine(sb);
#endif
			return new Uri(sb.ToString(), UriKind.Absolute);
		}

		#region Nested Classes

		private class MovieContext : IMovieInfo
		{
			private ServiceClient client;

			public MovieContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<FindResult> FindAsync(string id, string externalSource, CancellationToken cancellationToken)
			{
				string cmd = String.Format("find/{0}", id);
				var parameters = new Dictionary<string, string> { { "external_source", externalSource } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<FindResult>(response);
			}

			public async Task<MovieList> SearchAsync(string query, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string>
				{
					{ "query", query },
					{ "page", page.ToString() },
					{ "include_adult", includeAdult.ToString() },
					{ "language", language }
				};
				var response = await client.GetAsync("search/movie", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<MovieList> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string>
				{
					{ "page", page.ToString() },
					{ "include_adult", includeAdult.ToString() },
					{ "language", language }
				};
				if (year.HasValue) parameters.Add("year", year.Value.ToString());
				if (minimumDate.HasValue) parameters.Add("release_date.gte", minimumDate.Value.ToString("yyyy-MM-dd"));
				if (maximumDate.HasValue) parameters.Add("release_date.lte", maximumDate.Value.ToString("yyyy-MM-dd"));
				if (voteCount.HasValue) parameters.Add("vote_count.gte", voteCount.Value.ToString(CultureInfo.InvariantCulture));
				if (voteAverage.HasValue) parameters.Add("vote_average.gte", voteAverage.Value.ToString(CultureInfo.InvariantCulture));
				if (!String.IsNullOrWhiteSpace(genres)) parameters.Add("with_genres", genres);
				if (!String.IsNullOrWhiteSpace(companies)) parameters.Add("with_companies", companies);

				var response = await client.GetAsync("discover/movie", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}", id);
				var parameters = new Dictionary<string, string> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","alternative_titles,images,credits,keywords,releases,videos,translations");
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Movie>(response);
			}

			public async Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/images", id);
				var parameters = new Dictionary<string, string> { { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Images>(response);
			}

			public async Task<MediaCredits> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/credits", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MediaCredits>(response);
			}

			public async Task<IEnumerable<Video>> GetVideosAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/videos", id);
				var parameters = new Dictionary<string, string> { { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Videos>(response)).Results;
			}

			public async Task<MovieList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/similar_movies", id);
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<MovieList> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync("movie/popular", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<MovieList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync("movie/top_rated", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/alternative_titles", id);
				var parameters = new Dictionary<string, string> { { "country", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<AlternativeTitles>(response)).Results;
			}

			public async Task<IEnumerable<Keyword>> GetKeywordsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/keywords", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Keywords>(response)).Results;
			}

			public async Task<IEnumerable<CountryRelease>> GetReleasesAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/releases", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Releases>(response)).Countries;
			}

			public async Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/translations", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Translations>(response)).Results;
			}
		}

		private class SeriesContext : ISeriesInfo
		{
			private ServiceClient client;

			public SeriesContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<SeriesList> SearchAsync(string query, string language, DateTime? firstAirDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string>
				{
					{ "query", query },
					{ "page", page.ToString() },
					{ "language", language }
				};
				if (firstAirDate.HasValue) parameters.Add("first_air_date_year", firstAirDate.Value.ToString());
				var response = await client.GetAsync("search/tv", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<Series> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}", id);
				var parameters = new Dictionary<string, string> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","images,credits,keywords,videos,translations");
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Series>(response);
			}

			public async Task<Season> GetSeasonAsync(int id, int season, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}", id, season);
				var parameters = new Dictionary<string, string> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","images,credits,videos");
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Season>(response);
			}

			public async Task<Episode> GetEpisodeAsync(int id, int season, int episode, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}/episode/{2}", id, season, episode);
				var parameters = new Dictionary<string, string> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","images,credits,videos");
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Episode>(response);
			}

			public async Task<ExternalIds> GetIdsAsync(int id, int? season, int? episode, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/external_ids");
				var response = await client.GetAsync(sb.ToString(), null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<ExternalIds>(response);
			}

			public async Task<MediaCredits> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/credits", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MediaCredits>(response);
			}

			public async Task<Images> GetImagesAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/images");
				var parameters = new Dictionary<string, string> { { "language", language } };
				var response = await client.GetAsync(sb.ToString(), parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Images>(response);
			}

			public async Task<SeriesList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/similar", id);
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<IEnumerable<Video>> GetVideosAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/videos"); 
				var parameters = new Dictionary<string, string> { { "language", language } };
				var response = await client.GetAsync(sb.ToString(), parameters, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Videos>(response)).Results;
			}

			public async Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/translations", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Translations>(response)).Results;
			}

			public async Task<SeriesList> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync("tv/popular", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<SeriesList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync("tv/top_rated", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}
		}

		private class CollectionContext : ICollectionInfo
		{
			private ServiceClient client;

			public CollectionContext(ServiceClient client)
			{
				this.client = client;
			}

			async Task<CollectionList> ICollectionInfo.SearchAsync(string query, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "query", query }, { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync("search/collection", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<CollectionList>(response);
			}
		}

		private class CompanyContext : ICompanyInfo
		{
			private ServiceClient client;

			public CompanyContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<Company> GetAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("company/{0}", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Company>(response);
			}

			public async Task<MovieList> GetMoviesAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("company/{0}/movies", id);
				var parameters = new Dictionary<string, string> { { "page", page.ToString() }, { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<CompanyList> SearchAsync(string query, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string> { { "query", query }, { "page", page.ToString() } };
				var response = await client.GetAsync("search/company", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<CompanyList>(response);
			}
		}

		private class GenreContext : IGenreInfo
		{
			private ServiceClient client;

			public GenreContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<IEnumerable<Genre>> GetAsync(CancellationToken cancellationToken)
			{
				var response = await client.GetAsync("genre/list", null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Genres>(response)).Results;
			}

			public async Task<MovieList> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("genre/{0}/movies", id);
				var parameters = new Dictionary<string, string>
				{
					{ "page", page.ToString() },
					{ "include_adult", includeAdult.ToString() },
					{ "language", language }
				};
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}
		}

		private class PeopleContext : IPeopleInfo
		{
			private ServiceClient client;

			public PeopleContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}", id);
				var parameters = new Dictionary<string, string>();
				if (appendAll) parameters.Add("append_to_response","images");
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Person>(response);
			}

			public async Task<PersonCredits> GetCreditsAsync(int id, string language, DataInfoType type, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("person/{0}/", id);

				switch (type)
				{
					case DataInfoType.Movie: sb.Append("movie_credits"); break;
					case DataInfoType.Television: sb.Append("tv_credits"); break;
					case DataInfoType.Combined: sb.Append("combined_credits"); break;
				}
				var parameters = new Dictionary<string, string> { { "language", language } };
				var response = await client.GetAsync(sb.ToString(), parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<PersonCredits>(response);
			}

			public async Task<IEnumerable<Image>> GetImagesAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}/images", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<PersonImages>(response)).Results;
			}

			public async Task<ExternalIds> GetIdsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}/external_ids", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<ExternalIds>(response);
			}

			public async Task<PersonList> SearchAsync(string query, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, string>
				{
					{ "query", query },
					{ "page", page.ToString() },
					{ "include_adult", includeAdult.ToString() }
				};
				var response = await client.GetAsync("search/person", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<PersonList>(response);
			}
		}

		#endregion
	}

	public sealed class ServiceRequestException : HttpRequestException
	{
		public ServiceRequestException(int statusCode, int serviceCode, string message) : base(message)
		{
			this.StatusCode = statusCode;
			this.ServiceCode = serviceCode;
		}

		public int ServiceCode { get; private set; }

		public int StatusCode { get; private set; }
	}
}
