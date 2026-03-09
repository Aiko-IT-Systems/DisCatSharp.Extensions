// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2025 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using DisCatSharp;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Extensions.Translations.Manager;

public sealed class TranslationManager : IAsyncDisposable
{
	private readonly TranslationsManagerConfiguration _configuration;
	private readonly ITranslationsReloadHandler _reloadHandler;
	private readonly WebApplication _app;
	private readonly ILogger<TranslationManager> _logger;

	public TranslationManager(DiscordClient client, TranslationsManagerConfiguration configuration)
		: this(configuration, new DiscordTranslationsReloadHandler(client))
	{ }

	public TranslationManager(DiscordShardedClient client, TranslationsManagerConfiguration configuration)
		: this(configuration, new DiscordTranslationsReloadHandler(client))
	{ }

	internal TranslationManager(TranslationsManagerConfiguration configuration, ITranslationsReloadHandler? reloadHandler = null)
	{
		this._configuration = configuration;
		this._reloadHandler = reloadHandler ?? new NoOpTranslationsReloadHandler();

		var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
		var translationsPath = this._configuration.TranslationsFolder;
		if (!Path.IsPathRooted(translationsPath))
			translationsPath = Path.Combine(this._configuration.RepoRoot, translationsPath);

		var builder = WebApplication.CreateBuilder(new WebApplicationOptions
		{
			ContentRootPath = AppContext.BaseDirectory,
			WebRootPath = webRoot
		});
		builder.WebHost.UseUrls($"http://localhost:{this._configuration.ServerPort}");
		builder.Services.AddSingleton(this._configuration);
		builder.Services.AddSingleton<ITranslationsReloadHandler>(this._reloadHandler);
		builder.Services.AddSingleton(sp => new StringsStore(translationsPath, this._configuration.TranslationsFileName, this._reloadHandler));
		builder.Services.AddSingleton(LocaleHelper.GetValidLocales());
		builder.Services.AddSingleton<AuditReportProvider>();
		builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

		this._app = builder.Build();
		this._logger = this._app.Services.GetRequiredService<ILogger<TranslationManager>>();
		this.ConfigureEndpoints();
	}

	private void ConfigureEndpoints()
	{
		this._app.UseDefaultFiles();
		this._app.UseStaticFiles();

		this._app.MapGet("/api/report", async (AuditReportProvider provider, CancellationToken cancellationToken) =>
		{
			var report = await provider.GetReportAsync(cancellationToken).ConfigureAwait(false);
			return Results.Json(report);
		});

		this._app.MapGet("/api/strings", (StringsStore store) => Results.Json(store.Load()));

		this._app.MapGet("/api/locales", (string[] locales) => Results.Json(locales));

		this._app.MapPost("/api/strings", async (StringsStore store, StringsMutation payload, string[] locales, CancellationToken cancellationToken) =>
		{
			if (payload is null || string.IsNullOrWhiteSpace(payload.Key))
				return Results.BadRequest(new { message = "Key is required." });
			if (!LocaleHelper.ValidateLocales(payload.Translations, locales, out var error))
				return Results.BadRequest(new { message = error });

			var map = store.Load();
			if (map.ContainsKey(payload.Key))
				return Results.Conflict(new { message = "Key already exists." });
			map[payload.Key] = payload.Translations ?? [];
			await store.SaveAsync(map, cancellationToken: cancellationToken).ConfigureAwait(false);
			return Results.Ok();
		});

		this._app.MapPut("/api/strings/{key}", async (StringsStore store, string key, StringsMutation payload, string[] locales, CancellationToken cancellationToken) =>
		{
			if (payload is null)
				return Results.BadRequest(new { message = "Payload required." });
			if (!LocaleHelper.ValidateLocales(payload.Translations, locales, out var error))
				return Results.BadRequest(new { message = error });

			var map = store.Load();
			map[key] = payload.Translations ?? [];
			await store.SaveAsync(map, cancellationToken: cancellationToken).ConfigureAwait(false);
			return Results.Ok();
		});

		this._app.MapPost("/api/strings/format", async (StringsStore store, CancellationToken cancellationToken) =>
		{
			var map = store.Load();
			await store.SaveAsync(map, sort: false, primaryLocale: this._configuration.DefaultLocale, cancellationToken: cancellationToken).ConfigureAwait(false);
			return Results.Ok();
		});

		this._app.MapDelete("/api/strings/{key}", async (StringsStore store, string key, CancellationToken cancellationToken) =>
		{
			var map = store.Load();
			if (!map.Remove(key))
				return Results.NotFound();
			await store.SaveAsync(map, cancellationToken: cancellationToken).ConfigureAwait(false);
			return Results.Ok();
		});
	}

	public async Task StartAsync(CancellationToken cancellationToken = default)
	{
		this._logger.LogInformation("Serving translation UI at http://localhost:{Port}", this._configuration.ServerPort);
		await this._app.StartAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task RunAsync(CancellationToken cancellationToken = default)
	{
		await this.StartAsync(cancellationToken).ConfigureAwait(false);
		await this._app.WaitForShutdownAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task StopAsync(CancellationToken cancellationToken = default)
	{
		await this._app.StopAsync(cancellationToken).ConfigureAwait(false);
		this._logger.LogInformation("Translation UI server stopped.");
	}

	public async ValueTask DisposeAsync() 
		=> await this._app.DisposeAsync();
}
