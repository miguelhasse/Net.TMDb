## The Movie Database Client for .NET ##

Implements asynchronous operations and includes support for portable class libraries.

### Usage samples ###

	static async Task Sample(CancellationToken cancellationToken)
	{
	    using (var client = new ServiceClient("<ApiKey>"))
		{
			for (int i = 1, count = 1000; i <= count; i++)
			{
			    var movies = await client.Movies.GetTopRatedAsync(null, i, cancellationToken);
				count = movies.PageCount; // keep track of the actual page count
			
				foreach (Movie m in movies.Results)
				{
					var movie = await client.Movies.GetAsync(m.Id, null, true, cancellationToken);
			
					var personIds = movie.Credits.Cast.Select(s => s.Id)
						.Union(movie.Credits.Crew.Select(s => s.Id));
			
					foreach (var id in personIds)
					{
						var person = await client.People.GetAsync(id, true, cancellationToken);
			
						foreach (var img in person.Images.Results)
						{
							string filepath = Path.Combine("People", img.FilePath.TrimStart('/'));
							await DownloadImage(img.FilePath, filepath, cancellationToken);
						}
					}
				}
			}
		}
	}

    static async Task DownloadImage(string filename, string localpath, CancellationToken cancellationToken)
    {
        if (!File.Exists(localpath))
        {
            string folder = Path.GetDirectoryName(localpath);
            Directory.CreateDirectory(folder);

            var storage = new StorageClient();
            using (var fileStream = new FileStream(localpath, FileMode.Create, 
				FileAccess.Write, FileShare.None, short.MaxValue, true))
            {
                try { await storage.DownloadAsync(filename, fileStream, cancellationToken); }
                catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); }
            }
        }
    }
