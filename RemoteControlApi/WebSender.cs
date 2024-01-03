using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RemoteControlApi
{
	/// <summary>
	/// Сервис загрузки файлов.
	/// </summary>
	public class WebSender
	{
		/// <summary>
		/// Factory for safe use httpClient.
		/// </summary>
		IHttpClientFactory _httpClientFactory;

		/// <summary>
		/// Last error occur in process work service methods.
		/// </summary>
		public string LastError { get; private set; }

		public WebSender()
		{
			var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
			_httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
		}

		/// <summary>
		/// Загрузка файла на сервер.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="filePath"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> Upload(string url, string filePath, string userName, string password, string shopName)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(userName), name: "userName");
				form.Add(new StringContent(password), name: "password");
				form.Add(new StringContent(shopName), name: "shopName");

				byte[] fileData = File.ReadAllBytes(filePath);

				ByteArrayContent byteContent = new ByteArrayContent(fileData);

				byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

				form.Add(byteContent, "file", Path.GetFileName(filePath));
				return await httpClient.PostAsync(url, form);
			}
		}

		public async Task<HttpResponseMessage> PostAsync(string url, KeyValuePair<string,string> content, string userName, string password)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
				form.Add(new StringContent(userName), name: "userName");
				form.Add(new StringContent(password), name: "password");
				form.Add(new StringContent(content.Value), name: content.Key);

				return await httpClient.PostAsync(url, form);
			}
		}

		public async Task<HttpResponseMessage> GetAsync(string url, string userName, string password)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();
			
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "userName", userName },
					{ "password", password }
				});
				
				return await httpClient.SendAsync(request);
			}
		}

		public async Task<HttpResponseMessage> GetByParamAsync(string url, string param, string paramName)
		{
			using (var httpClient = new HttpClient())
			{
				var form = new MultipartFormDataContent();

				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ paramName, param }
				});

				return await httpClient.SendAsync(request);
			}
		}

		/// <summary>
		/// Download file from server.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public async Task<bool> DownloadFile(string url, string userName, string password, string savePath, Action<int, int> progress)
		{
			try
			{
				var client = _httpClientFactory.CreateClient();

				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Post,
					RequestUri = new Uri(url),
				};

				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "userName", userName },
					{ "password", password }
				});

				//Don't copy all file in memory.
				var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

				using var contentStream = await response.Content.ReadAsStreamAsync();

				long totalBytes =  response.Content.Headers.ContentLength.Value;

				long downloadBytes = 0;
				long readCount = 0;
				int progressChangedIterationCount = 100;

				int bufferSize = 8192;
				byte[] buffer = new byte[bufferSize];
				bool hasBytes = true;

				if (!response.Content.Headers.Contains("Content-Disposition"))
				{
					LastError = "Bad Headers: Content-Disposition not found";
					return false;
				}

				//Get file name.
				string cpStr = response.Content.Headers.GetValues("Content-Disposition").ToList().First();
				ContentDisposition contentDisposition = new ContentDisposition(cpStr);

				string filename = contentDisposition.FileName;
				StringDictionary parameters = contentDisposition.Parameters;

				string path = Path.Combine(savePath, filename);
				using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, true);
				
				while (hasBytes)
				{
					int bytesRead = await contentStream.ReadAsync(buffer);
					if (bytesRead == 0)
					{
						progress(100, 100); //100%
						break;
					}

					await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

					downloadBytes += bytesRead;
					readCount++;

					if (readCount % progressChangedIterationCount == 0)
					{
						progress(CalculatedProgress(downloadBytes, totalBytes), 100);
					}
				}
			}
			catch (Exception e)
			{
				LastError = $"{e.Message} {e.StackTrace}";
				return false;
			}
	
			return true;
		}
		
		private int CalculatedProgress(long downloadBytes, long totalBytes)
		{
			return (int)Math.Round((double)downloadBytes / totalBytes * 100, 2);
		}
	}
}
