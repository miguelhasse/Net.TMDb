## The Movie Database Client for .NET ##

Implements asynchronous operations and includes support for portable class libraries.

### Usage samples ###

    var client = new ServiceClient("<ApiKey>");
    var movies = await client.Movies.GetTopRatedAsync(null, 1, cancellationToken);

	foreach (Movie m in movies.Results)
	{
		var movie = await client.Movies.GetAsync(m.Id, null, true, cancellationToken);

		foreach (MediaCast c in movie.Credits.Cast)
		{
			var person = await client.People.GetAsync(c.Id, true, cancellationToken);

			foreach (Image img in person.Images.Results)
			{
				string filepath = Path.Combine("People", img.FilePath.TrimStart('/'));
				await DownloadImage(img.FilePath, filepath, cancellationToken);
			}
		}
		foreach (MediaCrew c in movie.Credits.Crew)
		{
			var person = await client.People.GetAsync(c.Id, true, cancellationToken);

			foreach (Image img in person.Images.Results)
			{
				string filepath = Path.Combine("People", img.FilePath.TrimStart('/'));
				await DownloadImage(img.FilePath, filepath, cancellationToken);
			}
		}
	}

----------
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
