## The Movie Database Client for .NET ##

Implements asynchronous operations and includes support for portable class libraries.

### Usage samples ###

	static async Task Sample(CancellationToken cancellationToken)
	{
	    var client = new ServiceClient("<ApiKey>");

		var options = new ParallelOptions { CancellationToken = cancellationToken };

		Parallel.For(1, 1000, options, (i, loopState) =>
		{
		    var movies = await client.Movies.GetTopRatedAsync(null, i, cancellationToken);
		
			foreach (Movie m in movies.Results)
			{
				var movie = await client.Movies.GetAsync(m.Id, null, true, cancellationToken);
		
				var personIds = movie.Credits.Cast.Select(s => s.Id)
					.Union(movie.Credits.Crew.Select(s => s.Id));
		
				Parallel.ForEach(personIds, options, async id =>
				{
					var person = await client.People.GetAsync(id, true, cancellationToken);
		
					Parallel.ForEach(person.Images.Results, async img =>
					{
						string filepath = Path.Combine("People", img.FilePath.TrimStart('/'));
						await DownloadImage(img.FilePath, filepath, cancellationToken);
					});
				});
			}
			if (i == movies.PageCount)
				loopState.Break();
		});
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
