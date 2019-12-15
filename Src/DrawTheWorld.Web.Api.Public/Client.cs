using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FLib;
//using Newtonsoft.Json;

namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Provides access to the REST Api.
	/// </summary>
	public sealed class Client
		: IDisposable
	{
		private const string HttpScheme = "http";
		private const string HttpsScheme = "https";

		private readonly HttpClient _HttpClient = null;

		private string AuthToken = null;
		private User _User = null;

		/// <summary>
		/// Gets underlying <see cref="HttpClient"/>.
		/// </summary>
		public HttpClient HttpClient
		{
			get { return this._HttpClient; }
		}

		/// <summary>
		/// Gets currently logged-in user.
		/// </summary>
		public User User
		{
			get { return this._User; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="baseAddress"></param>
		public Client(Uri baseAddress)
		{
			Validate.Debug(() => baseAddress, v => v.NotNull());

			this._HttpClient = new HttpClient
			{
				BaseAddress = baseAddress
			};
			this._HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			this._HttpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
		}

		/// <summary>
		/// Adjusts the connection so that it can use supported features.
		/// </summary>
		/// <returns></returns>
		public Task AdjustConnection()
		{
            return Task.CompletedTask;
            // New comment - hehe, manual implementation of HSTS :D
			//if (this._HttpClient.BaseAddress.Scheme == HttpScheme && await this.GetAsync<bool>("misc/issslenabled"))
			//{
			//	var builder = new UriBuilder(this._HttpClient.BaseAddress);
			//	builder.Scheme = HttpsScheme;
			//	this._HttpClient.BaseAddress = builder.Uri;
			//}
		}

		/// <summary>
		/// Tries to sign in existing user.
		/// </summary>
		/// <param name="authToken"></param>
		/// <returns></returns>
		public Task<User> SignIn(string authToken)
        {
            return PrepareUser();

            //Validate.All(() => this.AuthToken, v => v.Null());
            //Validate.All(() => authToken, v => v.NotNull());

            //this.AuthToken = authToken;
            //try
            //{
            //    var response = await this.GetAsync("account", true);

            //    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            //        throw new UserDoesNotExistException();
            //    else if (!response.IsSuccessStatusCode)
            //        throw new CannotSignInException("Cannot sign user in. Status code: {0}, reason phrase: {1}.".FormatWith(response.StatusCode, response.ReasonPhrase));

            //    this._User = await this.Deserialize<User>(response);

            //    if (this._User == null)
            //        throw new HttpRequestException("There was problem with deserialization of the content.");
            //}
            //catch (UserDoesNotExistException)
            //{
            //    this.AuthToken = null;
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    this.AuthToken = null;
            //    throw new CannotSignInException("Cannot sign user in. See inner exception for more details.", ex);
            //}
            //return this._User;
        }

        /// <summary>
        /// Tries to register new user and automatically signs-in.
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public Task<User> Register(User userData, string authToken)
		{
            return PrepareUser();

			//Validate.All(() => this.AuthToken, v => v.Null());
			//Validate.All(() => userData, v => v.NotNull());
			//Validate.All(() => authToken, v => v.NotNull());

			//this.AuthToken = authToken;
			//try
			//{
			//	this._User = await this.PostAsync<User>("account/register", userData, true);

			//	if (this._User == null)
			//		throw new HttpRequestException("There was problem with deserialization of the content.");
			//}
			//catch
			//{
			//	this.AuthToken = null;
			//	throw;
			//}
			//return this._User;
		}

		/// <summary>
		/// Logs out user.
		/// </summary>
		public void SignOut()
		{
			this.AuthToken = null;
			this._User = null;
		}

		/// <summary>
		/// Refreshes information about the user.
		/// </summary>
		/// <returns></returns>
		public Task RefreshUser()
		{
            return Task.CompletedTask;
			//this._User = await this.GetAsync<User>("account", true);
		}

		/// <summary>
		/// Disposes the HttpClient.
		/// </summary>
		public void Dispose()
		{
			this.SignOut();
			this._HttpClient.Dispose();
		}

		/// <summary>
		/// Sends GET request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> GetAsync_(string requestUri, CancellationToken cancellationToken, bool authorize = false)
		{
			Validate.All(() => requestUri, v => v.NotNullAndNotWhiteSpace());

			if (authorize)
				requestUri = this.AuthorizeRequest(requestUri);

			try
			{
				return await this.HttpClient.GetAsync(requestUri, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				return null;
			}
		}

		/// <summary>
		/// Sends GET request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> GetAsync_(string requestUri, bool authorize = false)
		{
			return this.GetAsync_(requestUri, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Sends GET request to the resource.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public async Task<T> GetAsync_<T>(string requestUri, CancellationToken cancellationToken, bool authorize = false)
		{
			var response = await this.GetAsync_(requestUri, cancellationToken, authorize);
			if (response == null)
				return default(T);
			response.EnsureSuccessStatusCode();
			var stringResponse = await response.Content.ReadAsStringAsync();
			if (cancellationToken.IsCancellationRequested)
				return default(T);
            //return JsonConvert.DeserializeObject<T>(stringResponse);
            return default;
		}

		/// <summary>
		/// Sends GET request to the resource.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<T> GetAsync_<T>(string requestUri, bool authorize = false)
		{
			return this.GetAsync_<T>(requestUri, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Sends POST request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> PostAsync_(string requestUri, object content, CancellationToken cancellationToken, bool authorize = false)
		{
			Validate.All(() => requestUri, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => content, v => v.NotNull());

            //if (authorize)
            //	requestUri = this.AuthorizeRequest(requestUri);
            //var serialized = JsonConvert.SerializeObject(content, Formatting.None);
            //if (cancellationToken.IsCancellationRequested)
            //	return null;

            //var stringContent = new StringContent(serialized, Encoding.UTF8, "application/json");
            //try
            //{
            //	return await this.HttpClient.PostAsync(requestUri, stringContent, cancellationToken);
            //}
            //catch (TaskCanceledException)
            //{
            //	return null;
            //}
            return Task.FromResult<HttpResponseMessage>(null);
		}

		/// <summary>
		/// Sends POST request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> PostAsync_(string requestUri, object content, bool authorize = false)
		{
			return this.PostAsync_(requestUri, content, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Sends POST request and deserializes the response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public async Task<T> PostAsync_<T>(string requestUri, object content, CancellationToken cancellationToken, bool authorize = false)
		{
			var response = await this.PostAsync_(requestUri, content, cancellationToken, authorize);
			if (response == null)
				return default(T);
			response.EnsureSuccessStatusCode();
			var stringResponse = await response.Content.ReadAsStringAsync();
			if (cancellationToken.IsCancellationRequested)
				return default(T);
			//return JsonConvert.DeserializeObject<T>(stringResponse);
            return default;
		}

		/// <summary>
		/// Sends POST request and deserializes the response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<T> PostAsync_<T>(string requestUri, object content, bool authorize = false)
		{
			return this.PostAsync_<T>(requestUri, content, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Sends PATCH request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> PatchAsync_(string requestUri, object content, CancellationToken cancellationToken, bool authorize = false)
		{
			Validate.All(() => requestUri, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => content, v => v.NotNull());

            //if (authorize)
            //	requestUri = this.AuthorizeRequest(requestUri);
            //var serialized = JsonConvert.SerializeObject(content, Formatting.None);
            //if (cancellationToken.IsCancellationRequested)
            //	return null;

            //var stringContent = new StringContent(serialized, Encoding.UTF8, "application/json");
            //var msg = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = stringContent };
            //try
            //{
            //	return await this.HttpClient.SendAsync(msg, cancellationToken);
            //}
            //catch (TaskCanceledException)
            //{
            //	return null;
            //}
            return Task.FromResult<HttpResponseMessage>(null);
		}

		/// <summary>
		/// Sends PATCH request to the resource.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> PatchAsync_(string requestUri, object content, bool authorize = false)
		{
			return this.PatchAsync_(requestUri, content, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Sends PATCH request and deserializes the response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public async Task<T> PatchAsync_<T>(string requestUri, object content, CancellationToken cancellationToken, bool authorize = false)
		{
			var response = await this.PatchAsync_(requestUri, content, cancellationToken, authorize);
			if (response == null)
				return default(T);
			response.EnsureSuccessStatusCode();
			var stringResponse = await response.Content.ReadAsStringAsync();
			if (cancellationToken.IsCancellationRequested)
				return default(T);
            //return JsonConvert.DeserializeObject<T>(stringResponse);
            return default;
		}

		/// <summary>
		/// Sends PATCH request and deserializes the response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requestUri"></param>
		/// <param name="content"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		public Task<T> PatchAsync_<T>(string requestUri, object content, bool authorize = false)
		{
			return this.PatchAsync_<T>(requestUri, content, CancellationToken.None, authorize);
		}

		/// <summary>
		/// Deserializes the response using internal deserializer.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<T> Deserialize_<T>(HttpResponseMessage response)
		{
			Validate.All(() => response, v => v.NotNull());
			response.EnsureSuccessStatusCode();
			var stringResponse = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<T>(stringResponse);
            return default;
		}

		/// <summary>
		/// Deserializes the response as string. Checks if it is encapsuleted in JSON envelope and decodes if needed.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<string> DeserializeString_(HttpResponseMessage response)
		{
			Validate.All(() => response, v => v.NotNull());
			response.EnsureSuccessStatusCode();
			var stringResponse = await response.Content.ReadAsStringAsync();
			//if (response.Content.Headers.ContentType.MediaType == "application/json")
			//	return JsonConvert.DeserializeObject<string>(stringResponse);
			return stringResponse;
		}

		private string AuthorizeRequest(string requestUri)
		{
			Validate.All(() => this.AuthToken, v => v.NotNull());
			requestUri += requestUri.IndexOf('?') == -1 ? "?" : "&";
			requestUri += "auth_type=live&auth_token=" + this.AuthToken;
			return requestUri;
		}

        private static Task<User> PrepareUser()
        {
            return Task.FromResult(new User
            {
                FirstName = "Have",
                LastName = "Fun",
                Address = "full_option@example.com",
                Coins = 10_000,
            });
        }
	}

	/// <summary>
	/// User cannot be signed in.
	/// </summary>
	public sealed class CannotSignInException
		: Exception
	{
		public CannotSignInException() { }
		public CannotSignInException(string message) : base(message) { }
		public CannotSignInException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// User does not exist and needs to be registered.
	/// </summary>
	public class UserDoesNotExistException
		: Exception
	{
		public UserDoesNotExistException() { }
		public UserDoesNotExistException(string message) : base(message) { }
		public UserDoesNotExistException(string message, Exception inner) : base(message, inner) { }
	}
}
