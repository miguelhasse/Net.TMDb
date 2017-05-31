using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    /// <summary>
    /// Contains the various API operations for the service client to interact with The Movie Database.
    /// </summary>
    public sealed class ServiceClient : IDisposable
	{
		private readonly string baseUrl;
		private readonly HttpClient client;
		private bool disposed = false;

		private static readonly JsonSerializerSettings jsonSettings;
		private static readonly string[] externalSources;
		
		#region Constructors

		public ServiceClient(string apiKey)
		{
			this.baseUrl = String.Concat(@"http://api.themoviedb.org/3/{0}?api_key=", apiKey, "&");
			this.client = new HttpClient(new Internal.ServiceMessageHandler(
				new HttpClientHandler
				{
					AllowAutoRedirect = false,
					PreAuthenticate = true,
					UseDefaultCredentials = true,
					UseCookies = false,
					AutomaticDecompression =  DecompressionMethods.GZip
				}));
			this.client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			this.Movies = new MovieContext(this);
			this.Shows = new ShowsContext(this);
			this.Companies = new CompanyContext(this);
			this.Genres = new GenreContext(this);
			this.People = new PeopleContext(this);
			this.Collections = new CollectionContext(this);
			this.Lists = new ListContext(this);
			this.Reviews = new ReviewContext(this);
			this.Settings = new SystemContext(this);
		}

		static ServiceClient()
		{
			ServiceClient.jsonSettings = new JsonSerializerSettings
			{
				Error = new EventHandler<ErrorEventArgs>((s, e) => OnSerializationError(e))
			};
			ServiceClient.jsonSettings.Converters.Add(new Internal.ResourceCreationConverter());
			ServiceClient.externalSources = new string[] { "imdb_id", "freebase_id", "freebase_mid", "tvdb_id", "tvrage_id" };
		}

		#endregion

		public IMovieInfo Movies { get; private set; }

		public IShowInfo Shows { get; private set; }

		public ICompanyInfo Companies { get; private set; }

		public IGenreInfo Genres { get; private set; }

		public IPeopleInfo People { get; private set; }

		public ICollectionInfo Collections { get; private set; }

		public IListInfo Lists { get; private set; }

		public IReviewInfo Reviews { get; private set; }

		public ISystemInfo Settings { get; private set; }

		#region Disposal Implementation
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private void Dispose(bool disposing)
		{
			if(!this.disposed)
			{
				if (disposing) // dispose aggregated resources
					this.client.Dispose();
				this.disposed = true; // disposing has been done
			}
		}
		
		#endregion
		
		#region Authentication Methods

		/// <summary>
		/// This method is used to generate a valid request token and authenticate user with a TMDb username and password.
		/// </summary>
		public Task<string> LoginAsync(string username, string password, CancellationToken cancellationToken)
		{
            return GetTokenAsync(cancellationToken)
                .ContinueWith(t => ValidateAsync(t.Result, username, password, cancellationToken), TaskContinuationOptions.OnlyOnRanToCompletion)
                .Unwrap();
		}

		/// <summary>
		/// This method is used to generate a session id for user based authentication, or a guest session if a null token is used.
		/// A session id is required in order to use any of the write methods.
		/// </summary>
		public Task<string> GetSessionAsync(string token, CancellationToken cancellationToken)
		{
			return (token == null ? OpenGuestSessionAsync(cancellationToken) : OpenSessionAsync(token, cancellationToken));
		}

        private Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            return GetAsync<AuthenticationResult>("authentication/token/new", null, cancellationToken)
                .ContinueWith(t => t.Result.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<string> ValidateAsync(string token, string username, string password, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>
            {
                { "request_token", token },
                { "username", username },
                { "password", password }
            };
            return GetAsync<AuthenticationResult>("authentication/token/validate_with_login", parameters, cancellationToken)
                .ContinueWith(t => t.Result.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<string> OpenSessionAsync(string token, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, object> { { "request_token", token } };
            return GetAsync<AuthenticationResult>("authentication/session/new", parameters, cancellationToken)
                .ContinueWith(t => t.Result.Session, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private Task<string> OpenGuestSessionAsync(CancellationToken cancellationToken)
		{
			return GetAsync<AuthenticationResult>("authentication/guest_session/new", null, cancellationToken)
                .ContinueWith(t => t.Result.Guest, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

		#endregion

		#region General Resource Search

		/// <summary>
		/// The find method makes it easy to search for objects in our database by an external id. For instance, an IMDB ID.
		/// This will search all objects (movies, TV shows and people) and return the results in a single response.
		/// </summary>
		/// <remarks>
		/// The supported external sources for each object are as follows:
		/// <list type="bullet">
		/// <item><description>Movies: imdb_id</description></item>
		/// <item><description>People: imdb_id, freebase_mid, freebase_id, tvrage_id</description></item>
		/// <item><description>TV Series: imdb_id, freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
		/// <item><description>TV Seasons: freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
		/// <item><description>TV Episodes: imdb_id, freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
		/// </list>
		/// </remarks>
		public Task<Resource> FindAsync(string id, string externalSource, CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(id))
				throw new ArgumentNullException("id");

			if (String.IsNullOrWhiteSpace(externalSource) || !ServiceClient.externalSources.Contains(externalSource))
				throw new ArgumentNullException("externalSource", "A supported external source must be specified.");

			string cmd = String.Format("find/{0}", id);
			var parameters = new Dictionary<string, object> { { "external_source", externalSource } };

			return GetAsync<ResourceFindResult>(cmd, parameters, cancellationToken)
                .ContinueWith(t => ((IEnumerable<Resource>)t.Result.Movies).Concat(t.Result.People)
                    .Concat(t.Result.Shows).Concat(t.Result.Seasons).Concat(t.Result.Episodes)
                    .FirstOrDefault(), TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		/// <summary>
		/// Search the movie, tv show and person collections with a single query. Each mapped result is the same response you would get from each independent search.
		/// </summary>
		public Task<Resources> SearchAsync(string query, string language, bool includeAdult, int page, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, object>
			{
				{ "query", query },
				{ "page", page },
				{ "include_adult", includeAdult },
				{ "language", language }
			};
			return GetAsync<Resources>("search/multi", parameters, cancellationToken)
                .ContinueWith(t => t.Result, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		#endregion

		#region Request Handling Methods

		private Task<T> GetAsync<T>(string cmd, IDictionary<string, object> parameters, CancellationToken cancellationToken)
		{
			return this.client.GetAsync(CreateRequestUri(cmd, parameters), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ContinueWith(t => DeserializeAsync<T>(t.Result))
				.Unwrap();
		}

        private Task<dynamic> GetDynamicAsync(string cmd, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return this.client.GetAsync(CreateRequestUri(cmd, parameters), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ContinueWith(t => DeserializeDynamicAsync(t.Result))
                .Unwrap();
        }

		private Task<dynamic> SendDynamicAsync(string cmd, IDictionary<string, object> parameters, HttpContent content, HttpMethod method, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, CreateRequestUri(cmd, parameters)) { Content = content };
            return this.client.SendAsync(request, cancellationToken)
                .ContinueWith(t => DeserializeDynamicAsync(t.Result))
                .Unwrap();
        }
		
		private static Task<T> DeserializeAsync<T>(HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync()
                .ContinueWith<T>(t =>
                {
#if DEBUG
        			System.Diagnostics.Debug.WriteLine(t.Result);
#endif
                    return JsonConvert.DeserializeObject<T>(t.Result, jsonSettings);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private static Task<dynamic> DeserializeDynamicAsync(HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync()
                .ContinueWith<dynamic>(t =>
                {
#if DEBUG
        			System.Diagnostics.Debug.WriteLine(t.Result);
#endif
                    return JsonConvert.DeserializeObject(t.Result, jsonSettings);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private Uri CreateRequestUri(string cmd, IDictionary<string, object> parameters)
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat(baseUrl, cmd);

			if (parameters != null)
			{
				var queryParams = parameters.Where(s => s.Value != null)
					.Select(s => String.Concat(s.Key, "=", ConvertParameterValue(s.Value)));

				if (queryParams.Count() > 0)
					sb.Append(queryParams.Aggregate((s, n) => String.Concat(s, "&", n)));
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
			else return Uri.EscapeDataString(value.ToString());
		}

		#endregion

		#region Serialization Event Handlers

		private static void OnSerializationError(Newtonsoft.Json.Serialization.ErrorEventArgs args)
		{
			System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error.Message);
			args.ErrorContext.Handled = true;
		}

		#endregion

		#region Nested Classes

		private sealed class MovieContext : IMovieInfo
		{
			private ServiceClient client;

			internal MovieContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Movies> SearchAsync(string query, string language, bool includeAdult, int? year, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "include_adult", includeAdult },
					{ "language", language },
					{ "year", year }
				};
				if (autocomplete) parameters.Add("search_type", "ngram");

				return client.GetAsync<Movies>("search/movie", parameters, cancellationToken);
			}

			public Task<Movies> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken)
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
				return client.GetAsync<Movies>("discover/movie", parameters, cancellationToken);
			}

			public Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}", id);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "alternative_titles,images,credits,keywords,releases,videos,translations,reviews,external_ids");

				return client.GetAsync<Movie>(cmd, parameters, cancellationToken);
			}

			public Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/images", id);
				var parameters = new Dictionary<string, object> { { "language", language } };

				return client.GetAsync<Images>(cmd, parameters, cancellationToken);
			}

			public Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/credits", id);
                return client.GetAsync<MediaCredits>(cmd, null, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<MediaCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Video>> GetVideosAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/videos", id);
				var parameters = new Dictionary<string, object> { { "language", language } };

                return client.GetAsync<Videos>(cmd, parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Reviews> GetReviewsAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/reviews", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

				return client.GetAsync<Reviews>(cmd, parameters, cancellationToken);
			}

			public Task<Lists> GetListsAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/lists", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

				return client.GetAsync<Lists>(cmd, parameters, cancellationToken);
			}

			public Task<Movies> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/similar_movies", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

				return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
			}

			public Task<Movies> GetGuestRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("guest_session/{0}/rated_movies", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

				return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
            }

            public Task<Movies> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/popular", parameters, cancellationToken);
            }

            public Task<Movies> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/top_rated", parameters, cancellationToken);
            }

            public Task<Movies> GetNowPlayingAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/now_playing", parameters, cancellationToken);
            }

            public Task<Movies> GetUpcomingAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/upcoming", parameters, cancellationToken);
            }

            public Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/alternative_titles", id);
				var parameters = new Dictionary<string, object> { { "country", language } };

				return client.GetAsync<AlternativeTitles>(cmd, parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Keyword>> GetKeywordsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/keywords", id);
				return client.GetAsync<Keywords>(cmd, null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Release>> GetReleasesAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/releases", id);
				return client.GetAsync<Releases>(cmd, null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/translations", id);
				return client.GetAsync<Translations>(cmd, null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
				return client.GetAsync<Changes>("movie/changes", parameters, cancellationToken);
			}

			public Task<Movies> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/rated/movies", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
			}

			public Task<Movies> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/favorite/movies ", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
            }

            public Task<Movies> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/watchlist/movies", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
            }

            public Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("movie/{0}/rating", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/favorite", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"movie\",\"media_id\":\"{0}\",\"favorite\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/watchlist", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"movie\",\"media_id\":\"{0}\",\"watchlist\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

		private sealed class ShowsContext : IShowInfo
		{
			private ServiceClient client;

			internal ShowsContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Shows> SearchAsync(string query, string language, DateTime? firstAirDate, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "language", language },
					{ "first_air_date_year", firstAirDate }
				};
				if (autocomplete) parameters.Add("search_type", "ngram");

                return client.GetAsync<Shows>("search/tv", parameters, cancellationToken);
			}

			public Task<Shows> DiscoverAsync(string language, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string networks, int page, CancellationToken cancellationToken)
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
                return client.GetAsync<Shows>("discover/tv", parameters, cancellationToken);
			}

			public Task<Show> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}", id);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,keywords,videos,translations,external_ids");

                return client.GetAsync<Show>(cmd, parameters, cancellationToken);
			}

			public Task<Show> GetLatestAsync(CancellationToken cancellationToken)
			{
                return client.GetAsync<Show>("tv/latest", null, cancellationToken);
            }

            public Task<Season> GetSeasonAsync(int id, int season, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}", id, season);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,videos,external_ids");

                return client.GetAsync<Season>(cmd, parameters, cancellationToken);
			}

			public Task<Episode> GetEpisodeAsync(int id, int season, int episode, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}/episode/{2}", id, season, episode);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,videos,external_ids");

                return client.GetAsync<Episode>(cmd, parameters, cancellationToken);
			}

			public Task<ExternalIds> GetIdsAsync(int id, int? season, int? episode, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/external_ids");

                return client.GetAsync<ExternalIds>(sb.ToString(), null, cancellationToken);
			}

			public Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/credits", id);
                return client.GetAsync<MediaCredits>(cmd, null, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<MediaCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Images> GetImagesAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
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

                return client.GetAsync<Images>(sb.ToString(), parameters, cancellationToken);
			}

			public Task<Shows> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/similar", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Shows>(cmd, parameters, cancellationToken);
			}

			public Task<IEnumerable<Video>> GetVideosAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
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

				return client.GetAsync<Videos>(sb.ToString(), parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/translations", id);
				return client.GetAsync<Translations>(cmd, null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<Shows> GetOnAirAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/on_the_air ", parameters, cancellationToken);
			}

			public Task<Shows> GetAiringAsync(string language, int page, string timezone, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language }, { "timezone", timezone } };
                return client.GetAsync<Shows>("tv/airing_today", parameters, cancellationToken);
            }

            public Task<Shows> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/popular", parameters, cancellationToken);
            }

            public Task<Shows> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/top_rated", parameters, cancellationToken);
            }

            public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
                return client.GetAsync<Changes>("tv/changes", parameters, cancellationToken);
			}

			public Task<string> GetNetworkAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("network/{0}", id);
                return client.GetDynamicAsync(cmd, null, cancellationToken)
                    .ContinueWith(t => (string)t.Result.Value<string>("name"), TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			
			public Task<Shows> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/rated/tv", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Shows>(cmd, parameters, cancellationToken);
            }

            public Task<Shows> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/favorite/tv ", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Shows>(cmd, parameters, cancellationToken);
            }

            public Task<Shows> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/watchlist/tv", session);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Shows>(cmd, parameters, cancellationToken);
            }

            public Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/rating", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 1), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetRatingAsync(string session, int id, int season, int episode, decimal value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("tv/{0}/season/{1}/episode/{2}/rating", id, season, episode);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

				return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 1), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/favorite", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"tv\",\"media_id\":\"{0}\",\"favorite\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				string cmd = String.Format("account/{0}/watchlist", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"tv\",\"media_id\":\"{0}\",\"watchlist\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

		private sealed class CollectionContext : ICollectionInfo
		{
			private ServiceClient client;

			internal CollectionContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Collection> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("collection/{0}", id);
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images");

                return client.GetAsync<Collection>(cmd, parameters, cancellationToken);
			}

			public Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				string cmd = String.Format("collection/{0}/images", id);
				var parameters = new Dictionary<string, object> { { "language", language } };

                return client.GetAsync<Images>(cmd, parameters, cancellationToken);
			}

			public Task<Collections> SearchAsync(string query, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page }, { "language", language } };
                return client.GetAsync<Collections>("search/collection", parameters, cancellationToken);
			}
		}

		private sealed class CompanyContext : ICompanyInfo
		{
			private ServiceClient client;

			internal CompanyContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Company> GetAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("company/{0}", id);
                return client.GetAsync<Company>(cmd, null, cancellationToken);
			}

			public Task<Movies> GetMoviesAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("company/{0}/movies", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };

                return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
			}

			public Task<Companies> SearchAsync(string query, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page } };
                return client.GetAsync<Companies>("search/company", parameters, cancellationToken);
			}
		}

		private sealed class GenreContext : IGenreInfo
		{
			private ServiceClient client;

			internal GenreContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<IEnumerable<Genre>> GetAsync(DataInfoType type, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder("genre");

				switch (type)
				{
					case DataInfoType.Movie: sb.Append("/movie/list"); break;
					case DataInfoType.Television: sb.Append("/tv/list"); break;
					case DataInfoType.Combined: sb.Append("/list"); break;
				}
                return client.GetAsync<Genres>(sb.ToString(), null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Movies> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				string cmd = String.Format("genre/{0}/movies", id);
				var parameters = new Dictionary<string, object> { { "page", page }, { "include_adult", includeAdult }, { "language", language } };

                return client.GetAsync<Movies>(cmd, parameters, cancellationToken);
			}
		}

		private sealed class PeopleContext : IPeopleInfo
		{
			private ServiceClient client;

			internal PeopleContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}", id);
				var parameters = new Dictionary<string, object>();
				if (appendAll) parameters.Add("append_to_response", "images,external_ids");

                return client.GetAsync<Person>(cmd, parameters, cancellationToken);
			}

			public Task<IEnumerable<PersonCredit>> GetCreditsAsync(int id, string language, DataInfoType type, CancellationToken cancellationToken)
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

                return client.GetAsync<PersonCredits>(sb.ToString(), parameters, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<PersonCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Image>> GetImagesAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}/images", id);
                return client.GetAsync<PersonImages>(cmd, null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<ExternalIds> GetIdsAsync(int id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("person/{0}/external_ids", id);
                return client.GetAsync<ExternalIds>(cmd, null, cancellationToken);
			}

			public Task<People> SearchAsync(string query, bool includeAdult, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page }, { "include_adult", includeAdult } };
				if (autocomplete) parameters.Add("search_type", "ngram");

                return client.GetAsync<People>("search/person", parameters, cancellationToken);
			}

			public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
                return client.GetAsync<Changes>("person/changes", parameters, cancellationToken);
			}
		}

		private sealed class ListContext : IListInfo
		{
			private ServiceClient client;

			internal ListContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<List> GetAsync(string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}", id);
                return client.GetAsync<List>(cmd, null, cancellationToken);
			}

			public Task<bool> ContainsAsync(string id, int movieId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/item_status", id);
				var parameters = new Dictionary<string, object> { { "movie_id", movieId } };
                return client.GetDynamicAsync(cmd, parameters, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<bool>("item_present") == true), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            public Task<string> CreateAsync(string session, string name, string description, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format((language == null) ? "{{\"name\":\"{0}\",\"description\":\"{1}\"}}" :
					"{{\"name\":\"{0}\",\"description\":\"{1}\",\"language\":\"{2}\"}}", name, description, language);

                return client.SendDynamicAsync("list", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (string)t.Result.Value<string>("list_id"), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> InsertAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/add_item", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> RemoveAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/remove_item", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

                return client.SendDynamicAsync(cmd, parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            public Task<bool> ClearAsync(string session, string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}/clear", id);
				var parameters = new Dictionary<string, object> { { "session_id", session }, { "confirm", "true" } };
                return client.SendDynamicAsync(cmd, parameters, null, HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<bool> DeleteAsync(string session, string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("list/{0}", id);
				var parameters = new Dictionary<string, object> { { "session_id", session } };
                return client.SendDynamicAsync(cmd, parameters, null, HttpMethod.Delete, cancellationToken)
    				.ContinueWith(t => (bool)(t.Result.Value<int>("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

		private sealed class ReviewContext : IReviewInfo
		{
			private ServiceClient client;

			internal ReviewContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Review> GetAsync(string id, CancellationToken cancellationToken)
			{
				string cmd = String.Format("review/{0}", id);
                return client.GetAsync<Review>(cmd, null, cancellationToken);
			}

		}
		private sealed class SystemContext : ISystemInfo
		{
			private ServiceClient client;

			internal SystemContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Account> GetAccountAsync(string session, CancellationToken cancellationToken)
			{
                var parameters = new Dictionary<string, object> { { "session_id", session }};
                return client.GetAsync<Account>("account", parameters, cancellationToken);
			}

			public Task<IEnumerable<Certification>> GetCertificationsAsync(DataInfoType type, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder("certification");

				switch (type)
				{
					case DataInfoType.Movie: sb.Append("/movie/list"); break;
					case DataInfoType.Television: sb.Append("/tv/list"); break;
					case DataInfoType.Combined: sb.Append("/list"); break;
				}
				return client.GetAsync<Certifications>(sb.ToString(), null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<dynamic> GetConfigurationAsync(CancellationToken cancellationToken)
			{
                return client.GetDynamicAsync("configuration", null, cancellationToken);
			}

			public Task<dynamic> GetTimezonesAsync(CancellationToken cancellationToken)
			{
                return client.GetDynamicAsync("timezones/list", null, cancellationToken);
			}

			public Task<IEnumerable<Job>> GetJobsAsync(CancellationToken cancellationToken)
			{
                return client.GetAsync<Jobs>("job/list", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}
		}

		#endregion
	}

	public class ServiceRequestException : HttpRequestException
	{
		internal ServiceRequestException(int statusCode, int serviceCode, string message) : base(message)
		{
			this.ServiceCode = serviceCode;
			this.StatusCode = statusCode;
		}

#if !PORTABLE
		protected ServiceRequestException(SerializationInfo info, StreamingContext context)
		{
			info.GetInt32("ServiceCode");
			info.GetInt32("StatusCode");
		}
#endif
		public int ServiceCode { get; private set; }

		public int StatusCode { get; private set; }

#if !PORTABLE
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ServiceCode", ServiceCode);
			info.AddValue("StatusCode", StatusCode);
		}
#endif
        internal static Task<HttpResponseMessage> ConvertResponseAsync(HttpResponseMessage response)
        {
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            int statusCode = (int)response.StatusCode;

            if (response.Content != null)
            {
                response.Content.ReadAsStringAsync().ContinueWith(t2 =>
                {
                    dynamic status = JsonConvert.DeserializeObject(t2.Result);
                    int serviceCode = (status.status_code != null) ? status.status_code : 0;
                    string message = (status.errors != null) ? String.Join(Environment.NewLine, status.errors) : status.status_message;
                    tcs.TrySetException(new ServiceRequestException(statusCode, serviceCode, message));
                });
            }
            else tcs.TrySetException(new ServiceRequestException(statusCode, 0, response.ReasonPhrase));
            return tcs.Task;
        }
	}
}

#pragma warning restore 1591