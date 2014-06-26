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
			this.Lists = new ListContext(this);
			this.Settings = new SystemContext(this);
		}

		#endregion

		public IMovieInfo Movies { get; private set; }

		public ISeriesInfo Series { get; private set; }

		public ICompanyInfo Companies { get; private set; }

		public IGenreInfo Genres { get; private set; }

		public IPeopleInfo People { get; private set; }

		public ICollectionInfo Collections { get; private set; }

		public IListInfo Lists { get; private set; }

		public ISystemInfo Settings { get; private set; }

		#region Authentication Methods

		public async Task<string> LoginAsync(string username, string password, CancellationToken cancellationToken)
		{
			var response = await GetAsync("authentication/token/new", null, cancellationToken).ConfigureAwait(false);
			var parameters = new Dictionary<string, object>
			{
				{ "request_token", (await Deserialize<AuthenticationResult>(response)).Token },
				{ "username", username },
				{ "language", password }
			};
			response = await GetAsync("authentication/token/validate_with_login", parameters, cancellationToken).ConfigureAwait(false);
			return (await Deserialize<AuthenticationResult>(response)).Token;
		}

		public async Task<string> GetSessionAsync(string token, CancellationToken cancellationToken)
		{
			return await (token == null ? OpenGuestSessionAsync(cancellationToken) : OpenSessionAsync(token, cancellationToken));
		}

		private async Task<string> OpenSessionAsync(string token, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, object> { { "request_token", token } };
			var response = await GetAsync("authentication/session/new", parameters, cancellationToken).ConfigureAwait(false);
			return (await Deserialize<AuthenticationResult>(response)).Session;
		}

		private async Task<string> OpenGuestSessionAsync(CancellationToken cancellationToken)
		{
			var response = await GetAsync("authentication/guest_session/new", null, cancellationToken).ConfigureAwait(false);
			return (await Deserialize<AuthenticationResult>(response)).Guest;
		}

		#endregion

		#region Request Handling Methods

		private Task<HttpResponseMessage> GetAsync(string cmd, IDictionary<string, object> parameters, CancellationToken cancellationToken)
		{
			var client = CreateHttpClient();
			TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();

			client.GetAsync(CreateRequestUri(cmd, parameters),
				HttpCompletionOption.ResponseHeadersRead, cancellationToken)
				.ContinueWith(t => HandleResponseCompletion(t, tcs));
			return tcs.Task;
		}

		private Task<HttpResponseMessage> SendAsync(string cmd, IDictionary<string, object> parameters, HttpContent content, HttpMethod method, CancellationToken cancellationToken)
		{
			var client = CreateHttpClient();
			TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
			var request = new HttpRequestMessage(method, CreateRequestUri(cmd, parameters)) { Content = content };

			client.SendAsync(request, cancellationToken)
				.ContinueWith(t => HandleResponseCompletion(t, tcs));
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

		private static async Task<dynamic> DeserializeDynamic(HttpResponseMessage response)
		{
			string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
			System.Diagnostics.Debug.WriteLine(responseJson);
#endif
			return JsonConvert.DeserializeObject(responseJson);
		}

		private static void HandleResponseCompletion(Task<HttpResponseMessage> task, TaskCompletionSource<HttpResponseMessage> tcs)
		{
			if (task.IsCanceled) tcs.TrySetCanceled();
			else if (!task.Result.IsSuccessStatusCode)
			{
				if (task.Result.Content != null)
				{
					task.Result.Content.ReadAsStringAsync().ContinueWith(t2 =>
					{
						var status = JsonConvert.DeserializeObject<Status>(t2.Result);
						tcs.TrySetException(new ServiceRequestException((int)task.Result.StatusCode,
							status.Code, status.Message));
					});
				}
				else tcs.TrySetException(new ServiceRequestException(
					(int)task.Result.StatusCode, -1, task.Result.ReasonPhrase));
			}
			else if (task.IsFaulted) tcs.TrySetException(task.Exception);
			else if (task.IsCompleted) tcs.TrySetResult(task.Result);
		}

		private static HttpClient CreateHttpClient()
		{
			var client = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = false,
				PreAuthenticate = true,
				UseDefaultCredentials = true,
				UseCookies = false,
				AutomaticDecompression =  DecompressionMethods.GZip
			});
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return client;
		}

		private Uri CreateRequestUri(string cmd, IDictionary<string, object> parameters)
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat(BaseUrl, cmd);

			if (parameters != null)
			{
				sb.Append(parameters.Where(s => s.Value != null)
					.Select(s => String.Concat(s.Key, "=", ConvertParameterValue(s.Value)))
					.Aggregate((s, n) => String.Concat(s, "&", n)));
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine(sb);
#endif
			return new Uri(sb.ToString(), UriKind.Absolute);
		}

		private static string ConvertParameterValue(object value)
		{
			Type t = value.GetType();
			t = Nullable.GetUnderlyingType(t) ?? t;
			
			if (t == typeof(DateTime)) return ((DateTime)value).ToString("yyyy-MM-dd");
			else if (t == typeof(Decimal)) return ((Decimal)value).ToString(CultureInfo.InvariantCulture);
			else return value.ToString();
		}

		#endregion

		#region Nested Classes

		private sealed class MovieContext : IMovieInfo
		{
			private ServiceClient client;

			public MovieContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<FindResult> FindAsync(string id, string externalSource, CancellationToken cancellationToken)
			{
				string cmd = String.Format("find/{0}", id);
				var parameters = new Dictionary<string, object> { { "external_source", externalSource } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<FindResult>(response);
			}

			public async Task<MovieList> SearchAsync(string query, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "include_adult", includeAdult },
					{ "language", language }
				};
				var response = await client.GetAsync("search/movie", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<MovieList> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "include_adult", includeAdult },
					{ "language", language },
					{ "year", year },
					{ "release_date.gte", minimumDate },
					{ "release_date.lte", maximumDate },
					{ "vote_count.gte", voteCount },
					{ "vote_average.gte", voteAverage },
					{ "with_genres", genres },
					{ "with_companies", companies },
				};
				var response = await client.GetAsync("discover/movie", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}", id);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","alternative_titles,images,credits,keywords,releases,videos,translations");

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Movie>(response);
			}

			public async Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/images", id);
				var parameters = new Dictionary<string, object> { { "language", language } };

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
				var parameters = new Dictionary<string, object> { { "language", language } };

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Videos>(response)).Results;
			}

			public async Task<MovieList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/similar_movies", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

            public async Task<MovieList> GetGuestRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
            {
                string cmd = String.Format("guest_session/{0}/rated_movies", session);
                var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<MovieList>(response);

            }

			public async Task<MovieList> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync("movie/popular", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<MovieList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync("movie/top_rated", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

            public async Task<MovieList> GetNowPlayingAsync(string language, int page, CancellationToken cancellationToken)
            {
                var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                var response = await client.GetAsync("movie/now_playing", parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<MovieList>(response);
            }

            public async Task<MovieList> GetUpcomingAsync(string language, int page, CancellationToken cancellationToken)
            {
                var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                var response = await client.GetAsync("movie/upcoming", parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<MovieList>(response);
            }

			public async Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/alternative_titles", id);
				var parameters = new Dictionary<string, object> { { "country", language } };
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

            public async Task<ChangeList> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
            {
                var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "start_date", minimumDate },
					{ "end_date", maximumDate }
				};
                var response = await client.GetAsync("movie/changes", parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<ChangeList>(response);
            }
		}

		private sealed class SeriesContext : ISeriesInfo
		{
			private ServiceClient client;

			public SeriesContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<SeriesList> SearchAsync(string query, string language, DateTime? firstAirDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "language", language },
					{ "first_air_date_year", firstAirDate }
				};
				var response = await client.GetAsync("search/tv", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<SeriesList> DiscoverAsync(string language, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string networks, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "language", language },
					{ "first_air_date_year", year },
					{ "first_air_date.gte", minimumDate },
					{ "first_air_date.lte", maximumDate },
					{ "vote_count.gte", voteCount },
					{ "vote_average.gte", voteAverage },
					{ "with_genres", genres },
					{ "with_networks", networks },
				};
				var response = await client.GetAsync("discover/tv", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<Series> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}", id);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","images,credits,keywords,videos,translations");

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Series>(response);
			}

			public async Task<Season> GetSeasonAsync(int id, int season, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}", id, season);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response","images,credits,videos");

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Season>(response);
			}

			public async Task<Episode> GetEpisodeAsync(int id, int season, int episode, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}/episode/{2}", id, season, episode);
				var parameters = new Dictionary<string, object> { { "language", language } };
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
				var parameters = new Dictionary<string, object> { { "language", language } };

				var response = await client.GetAsync(sb.ToString(), parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Images>(response);
			}

			public async Task<SeriesList> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/similar", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

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
				var parameters = new Dictionary<string, object> { { "language", language } };

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
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync("tv/popular", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

			public async Task<SeriesList> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync("tv/top_rated", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<SeriesList>(response);
			}

            public async Task<ChangeList> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
            {
                var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "start_date", minimumDate },
					{ "end_date", maximumDate }
				};
                var response = await client.GetAsync("tv/changes", parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<ChangeList>(response);
            }

			public async Task<string> GetNetworkAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("network/{0}", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return (await DeserializeDynamic(response)).name;
			}
		}

		private sealed class CollectionContext : ICollectionInfo
		{
			private ServiceClient client;

			public CollectionContext(ServiceClient client)
			{
				this.client = client;
			}

            public async Task<Collection> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
            {
                string cmd = String.Format("collection/{0}", id);
                var parameters = new Dictionary<string, object> { { "language", language } };
                if (appendAll) parameters.Add("append_to_response", "images");

                var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<Collection>(response);
            }

            public async Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
            {
                string cmd = String.Format("collection/{0}/images", id);
                var parameters = new Dictionary<string, object> { { "language", language } };

                var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<Images>(response);
            }

			async Task<CollectionList> ICollectionInfo.SearchAsync(string query, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "language", language }
				};
				var response = await client.GetAsync("search/collection", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<CollectionList>(response);
			}
		}

		private sealed class CompanyContext : ICompanyInfo
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
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}

			public async Task<CompanyList> SearchAsync(string query, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page } };
				var response = await client.GetAsync("search/company", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<CompanyList>(response);
			}
		}

		private sealed class GenreContext : IGenreInfo
		{
			private ServiceClient client;

			public GenreContext(ServiceClient client)
			{
				this.client = client;
			}

            public async Task<IEnumerable<Genre>> GetAsync(DataInfoType type, CancellationToken cancellationToken)
			{
                var sb = new System.Text.StringBuilder("genre");

                switch (type)
                {
                    case DataInfoType.Movie: sb.Append("/movie/list"); break;
                    case DataInfoType.Television: sb.Append("/tv/list"); break;
                    case DataInfoType.Combined: sb.Append("/list"); break;
                }
                var response = await client.GetAsync(sb.ToString(), null, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Genres>(response)).Results;
			}

			public async Task<MovieList> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("genre/{0}/movies", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "include_adult", includeAdult }, { "language", language } };

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<MovieList>(response);
			}
		}

		private sealed class PeopleContext : IPeopleInfo
		{
			private ServiceClient client;

			public PeopleContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}", id);
				var parameters = new Dictionary<string, object>();
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
				var parameters = new Dictionary<string, object> { { "language", language } };

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
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "include_adult", includeAdult }
				};
				var response = await client.GetAsync("search/person", parameters, cancellationToken).ConfigureAwait(false);
				return await Deserialize<PersonList>(response);
			}

            public async Task<ChangeList> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
            {
                var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "start_date", minimumDate },
					{ "end_date", maximumDate }
				};
                var response = await client.GetAsync("person/changes", parameters, cancellationToken).ConfigureAwait(false);
                return await Deserialize<ChangeList>(response);
            }
        }

		private sealed class ListContext : IListInfo
		{
			private ServiceClient client;

			public ListContext(ServiceClient client)
			{
				this.client = client;
			}

			public async Task<Lists> GetAsync(string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}", id);
				var response = await client.GetAsync(cmd, null, cancellationToken).ConfigureAwait(false);
				return await Deserialize<Lists>(response);
			}

			public async Task<bool> ContainsAsync(string id, int movieId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/item_status", id);
				var parameters = new Dictionary<string, object> { { "movie_id", movieId } };

				var response = await client.GetAsync(cmd, parameters, cancellationToken).ConfigureAwait(false);
				return (await DeserializeDynamic(response)).item_present == true;
			}

			public async Task<string> CreateAsync(string session, string name, string description, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format((language == null) ? "{{\"name\":\"{0}\",\"description\":\"{1}\"}}" :
					"{{\"name\":\"{0}\",\"description\":\"{1}\",\"language\":\"{2}\"}}", name, description, language);

				var response = await client.SendAsync("list", parameters, new StringContent(content, null, "application/json"),
					HttpMethod.Post, cancellationToken).ConfigureAwait(false);
				return (await DeserializeDynamic(response)).list_id;
			}

			public async Task<bool> InsertAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/add_item", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

				var response = await client.SendAsync(cmd, parameters, new StringContent(content, null, "application/json"),
					HttpMethod.Post, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Status>(response)).Code == 12;
			}

			public async Task<bool> RemoveAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/remove_item", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

				var response = await client.SendAsync(cmd, parameters, new StringContent(content, null, "application/json"),
					HttpMethod.Post, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Status>(response)).Code == 12;
			}

			public async Task<bool> ClearAsync(string session, string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/clear", id);
				var parameters = new Dictionary<string, object> { { "session_id", session }, { "confirm", "true" } };
				var response = await client.SendAsync(cmd, parameters, null, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Status>(response)).Code == 12;
			}

			public async Task<bool> DeleteAsync(string session, string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				var response = await client.SendAsync(cmd, parameters, null, HttpMethod.Delete, cancellationToken).ConfigureAwait(false);
				return (await Deserialize<Status>(response)).Code == 12;
			}
		}

		private sealed class SystemContext : ISystemInfo
		{
			private ServiceClient client;

			public SystemContext(ServiceClient client)
			{
				this.client = client;
			}

            public async Task<IEnumerable<Certification>> GetCertificationsAsync(DataInfoType type, CancellationToken cancellationToken)
			{
                var sb = new System.Text.StringBuilder("certification");

                switch (type)
                {
                    case DataInfoType.Movie: sb.Append("/movie/list"); break;
                    case DataInfoType.Television: sb.Append("/tv/list"); break;
                    case DataInfoType.Combined: sb.Append("/list"); break;
                }
                var response = await client.GetAsync(sb.ToString(), null, cancellationToken).ConfigureAwait(false);
                return (await Deserialize<Certifications>(response)).Results;
			}

			public async Task<dynamic> GetConfigurationAsync(CancellationToken cancellationToken)
			{
				var response = await client.GetAsync("configuration", null, cancellationToken).ConfigureAwait(false);
				return await DeserializeDynamic(response);
			}

			public async Task<dynamic> GetTimezonesAsync(CancellationToken cancellationToken)
			{
				var response = await client.GetAsync("timezones/list", null, cancellationToken).ConfigureAwait(false);
				return await DeserializeDynamic(response);
			}

            public async Task<IEnumerable<Job>> GetJobsAsync(CancellationToken cancellationToken)
            {
                var response = await client.GetAsync("job/list ", null, cancellationToken).ConfigureAwait(false);
                return (await Deserialize<Jobs>(response)).Results;
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
