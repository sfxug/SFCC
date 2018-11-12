using Newtonsoft.Json;
using Polly;
using SFCC.Common.Managers;
using SFCC.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SFCC.Common.Network
{
    public class ServiceClient
    {
#pragma warning disable 0414
        private bool isInitialized;
#pragma warning restore 0414

        public void Init()
        {
            isInitialized = true;

            client = new HttpClient();
            client.BaseAddress = new Uri(Globals.ApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Version", AppVersion);
            client.DefaultRequestHeaders.Add("X-OS", Device.RuntimePlatform);
            client.DefaultRequestHeaders.Add("X-OS-VERSION", Xamarin.Essentials.DeviceInfo.VersionString);
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);

        }

        private static ServiceClient instance;
        public static ServiceClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceClient();
                    instance.Init();
                }
                return instance;
            }
        }

        public void RefreshBearer()
        {
            client.DefaultRequestHeaders.Authorization = null;
            var token = Globals.UserIdToken;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private HttpClient client;

        private const int DefaultTimeoutInSeconds = 60;

        public static string AppVersion
        {
            get
            {
                return Xamarin.Essentials.VersionTracking.CurrentVersion;
            }
        }

        public async Task<ServiceResponse<T>> GET<T>(string requestUri, int retryCount = 0)
        {
            string resultMessage = "";
            T result = default(T);
            HttpResponseMessage response = null;

            try
            {

                response = await Policy.Handle<Exception>()
                    .OrResult<HttpResponseMessage>(r => r.StatusCode != HttpStatusCode.OK)
                    .RetryAsync(retryCount)
                    .ExecuteAsync(async () =>
                    {
                        return await client.GetAsync(requestUri).ConfigureAwait(false);
                    });

                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(resultStr);
                    }
                    catch (System.Exception ex)
                    {
                        LoggingManager.LogException("Failed to GET", ex);
                    }
                }
                else
                {
                    LoggingManager.LogMessage($"Http Error Status Code = {response.StatusCode} for {requestUri}");
                    resultMessage = "Error";
                }
            }
            catch (TaskCanceledException canEx)
            {
                //Assume timeout
                if (!canEx.CancellationToken.IsCancellationRequested)
                {
                    IDictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("RequestUri", requestUri ?? "null");
                    LoggingManager.LogException("Request Timed-out", canEx);
                }
            }
            catch (WebException webEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException(string.Empty, webEx);
            }

            try
            {
                var serviceResponse = new ServiceResponse<T> { Message = resultMessage, Result = result, Response = response };
                return serviceResponse;
            }
            catch (System.Exception ex)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException("Error occurred with response.", ex);

                return new ServiceResponse<T> { Authorized = false, Message = "", Result = default(T) };
            }
        }

        public async Task<ServiceResponse<T>> GET<T>(string requestUri, TimeSpan timeout)
        {
            string resultMessage = "";
            T result = default(T);
            HttpResponseMessage response = null;

            try
            {
                response = await client.GetAsync(requestUri).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(resultStr);
                    }
                    catch (System.Exception ex)
                    {
                        LoggingManager.LogException(@"Error occurred deserializing result from {requestUri}.", ex);
                    }
                }
                else
                {
                    LoggingManager.LogException($"Http Error Status Code= {response.StatusCode} for {requestUri}", null);
                    resultMessage = "Error";
                }
            }
            catch (TaskCanceledException canEx)
            {
                //Assume timeout
                if (!canEx.CancellationToken.IsCancellationRequested)
                {
                    IDictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("RequestUri", requestUri ?? "null");
                    LoggingManager.LogException("Request Timed-out", canEx);
                }
            }
            catch (WebException webEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException(string.Empty, webEx);
            }

            try
            {
                var serviceResponse = new ServiceResponse<T> { Message = resultMessage, Result = result, Response = response };
                return serviceResponse;
            }
            catch (System.Exception ex)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException("Error occurred with response.", ex);

                return new ServiceResponse<T> { Authorized = false, Message = "", Result = default(T) };
            }
        }

        public async Task<ServiceResponse<T>> POST<T>(string requestUri, object query, Dictionary<string, string> customHeaders = null)
        {
            string resultMessage = "";
            T result = default(T);
            HttpResponseMessage response = null;

            try
            {
                client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);

                if (customHeaders != null && customHeaders.Count > 0)
                {
                    foreach (var entry in customHeaders)
                    {
                        client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                    }
                }

                var stream = query as MemoryStream;

                if (stream != null)
                {
                    response = await client.PostAsync(requestUri, new ByteArrayContent(stream.ToArray())).ConfigureAwait(false);
                }
                else
                {
                    response = await client.PostAsync(requestUri, new StringContent(
                        JsonConvert.SerializeObject(query),
                        Encoding.UTF8,
                        "application/json"
                    )).ConfigureAwait(false);
                }
                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(resultStr);
                    }
                    catch (System.Exception ex)
                    {
                        LoggingManager.LogException(@"Error occurred deserializing result from {requestUri}.", ex);
                    }
                }
                else
                {
                    LoggingManager.LogException($"Http Error Status Code= {response.StatusCode} for {requestUri}", null);
                    resultMessage = "Error";
                }
            }
            catch (TaskCanceledException canEx)
            {
                //Assume timeout
                if (!canEx.CancellationToken.IsCancellationRequested)
                {
                    IDictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("RequestUri", requestUri ?? "null");
                    LoggingManager.LogException("Request Timed-out", canEx);
                }
            }
            catch (WebException webEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException(string.Empty, webEx);
            }
            catch (Exception ex)
            {
                LoggingManager.LogException(string.Empty, ex);
            }

            try
            {
                var serviceResponse = new ServiceResponse<T> { Message = resultMessage, Result = result, Response = response };
                return serviceResponse;
            }
            catch (System.Exception ex)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException("Error occurred with response.", ex);

                return new ServiceResponse<T> { Message = "", Result = default(T) };
            }

        }

        public async Task<ServiceResponse<T>> PUT<T>(string requestUri, object query)
        {
            string resultMessage = "";
            T result = default(T);
            HttpResponseMessage response = null;

            try
            {
                response = await client.PutAsync(requestUri, new StringContent(
                    JsonConvert.SerializeObject(query),
                    Encoding.UTF8,
                    "application/json"
                )).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(resultStr);
                    }
                    catch (System.Exception ex)
                    {
                        IDictionary<string, string> data = new Dictionary<string, string>();
                        LoggingManager.LogException(@"Error occurred deserializing result from {requestUri}.", ex);
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        string resultStr = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var message = JsonConvert.DeserializeObject<Dictionary<string, T>>(resultStr);
                            result = message["message"];
                        }
                        catch (System.Exception ex)
                        {
                            IDictionary<string, string> data = new Dictionary<string, string>();
                            LoggingManager.LogException(@"Error occurred deserializing result from {requestUri}.", ex);
                        }
                    }
                    else
                    {
                        LoggingManager.LogException($"Http Error Status Code= {response.StatusCode} for {requestUri}", null);
                    }
                    resultMessage = "Error";
                }
            }
            catch (TaskCanceledException canEx)
            {
                //Assume timeout
                if (!canEx.CancellationToken.IsCancellationRequested)
                {
                    IDictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("RequestUri", requestUri ?? "null");
                    LoggingManager.LogException("Request Timed-out", canEx);
                    response = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
                }
            }
            catch (WebException webEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException(string.Empty, webEx);

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception gEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException($"General PUT Exception for {requestUri}", gEx);

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            try
            {
                var serviceResponse = new ServiceResponse<T> { Message = resultMessage, Result = result, Response = response };
                return serviceResponse;
            }
            catch (System.Exception ex)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException("Error occurred with response.", ex);

                return new ServiceResponse<T> { Message = "", Result = default(T) };
            }
        }


        public async Task<ServiceResponse<T>> DELETE<T>(string requestUri, object query)
        {
            string resultMessage = "";
            T result = default(T);
            HttpResponseMessage response = null;

            try
            {

                var request = new HttpRequestMessage
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(query),
                    Encoding.UTF8,
                    "application/json"),
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(requestUri)
                };

                response = await client.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string resultStr = await response.Content.ReadAsStringAsync();
                    try
                    {
                        result = JsonConvert.DeserializeObject<T>(resultStr);
                    }
                    catch (System.Exception ex)
                    {
                        IDictionary<string, string> data = new Dictionary<string, string>();
                        LoggingManager.LogException(@"Error occurred deserializing result from {requestUri}.", ex);
                    }
                }
                else
                {
                    LoggingManager.LogException($"Http Error Status Code= {response.StatusCode} for {requestUri}", null);
                    resultMessage = "Error";
                }
            }
            catch (TaskCanceledException canEx)
            {
                //Assume timeout
                if (!canEx.CancellationToken.IsCancellationRequested)
                {
                    IDictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("RequestUri", requestUri ?? "null");
                    LoggingManager.LogException("Request Timed-out", canEx);
                    response = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
                }
            }
            catch (WebException webEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException(string.Empty, webEx);

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception gEx)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException($"General PUT Exception for {requestUri}", gEx);

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            try
            {
                var serviceResponse = new ServiceResponse<T> { Message = resultMessage, Result = result, Response = response };
                return serviceResponse;
            }
            catch (System.Exception ex)
            {
                IDictionary<string, string> data = new Dictionary<string, string>();
                data.Add("RequestUri", requestUri ?? "null");
                LoggingManager.LogException("Error occurred with response.", ex);

                return new ServiceResponse<T> { Message = "", Result = default(T) };
            }

        }
    }
}
