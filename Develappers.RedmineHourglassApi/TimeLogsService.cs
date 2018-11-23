﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Develappers.RedmineHourglassApi.Logging;
using Develappers.RedmineHourglassApi.Types;
using Newtonsoft.Json;

namespace Develappers.RedmineHourglassApi
{
    public class TimeLogService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal TimeLogService(Configuration configuration)
        {
            // internal constructor -> configuration is always set and valid
            _httpClient = new HttpClient(configuration.RedmineUrl, configuration.ApiKey);
        }

        /// <summary>
        /// Lists all visible time logs
        /// </summary>
        /// <param name="filter">The filter options.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The paged list of results.</returns>
        public async Task<PaginatedResult<TimeLog>> GetListAsync(BaseListFilter filter, CancellationToken token = default(CancellationToken))
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            try
            {
                var response = await _httpClient.GetStringAsync(new Uri($"time_logs.json?offset={filter.Offset}&limit={filter.Limit}", UriKind.Relative), token);
                return JsonConvert.DeserializeObject<PaginatedResult<TimeLog>>(response);
            }
            catch (Exception ex)
            {
                LogProvider.GetCurrentClassLogger().ErrorException($"unexpected exception {ex} occurred", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a time log by it's id.
        /// </summary>
        /// <param name="id">The id of the time log.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The time log.</returns>
        public async Task<TimeLog> GetAsync(int id, CancellationToken token = default(CancellationToken))
        {
            try
            {

                var response = await _httpClient.GetStringAsync(new Uri($"time_logs/{id}.json", UriKind.Relative), token);
                return JsonConvert.DeserializeObject<TimeLog>(response);
            }
            catch (WebException wex)
                when (wex.Status == WebExceptionStatus.ProtocolError &&
                      (wex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"time log with id {id} not found", wex);
            }
            catch (Exception ex)
            {
                LogProvider.GetCurrentClassLogger().ErrorException($"unexpected exception {ex} occurred", ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a time log.
        /// </summary>
        /// <param name="id">The id of the log.</param>
        /// <param name="token">The cancellation token.</param>
        public async Task DeleteAsync(int id, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await _httpClient.DeleteAsync(new Uri($"time_logs/{id}.json", UriKind.Relative), token);
            }
            catch (WebException wex)
                when (wex.Status == WebExceptionStatus.ProtocolError &&
                      (wex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException($"time log with id {id} not found", wex);
            }
            catch (Exception ex)
            {
                LogProvider.GetCurrentClassLogger().ErrorException($"unexpected exception {ex} occurred", ex);
                throw;
            }
        }

        /// <summary>
        /// Books a time log.
        /// </summary>
        /// <param name="id">The id of the log.</param>
        /// <param name="value">The detail data.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The time tracker data.</returns>
        public async Task<TimeEntry> BookAsync(int id, TimeBookingUpdate value, CancellationToken token = default(CancellationToken))
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            try
            {
                var request = new TimeLogBookRequest { Values = value };
                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var response = await _httpClient.PostStringAsync(new Uri($"time_logs/{id}/book.json", UriKind.Relative), data, token);
                return JsonConvert.DeserializeObject<TimeEntry>(response);
            }
            catch (Exception ex)
            {
                LogProvider.GetCurrentClassLogger().ErrorException($"unexpected exception {ex} occurred", ex);
                throw;
            }
        }
    }
}
