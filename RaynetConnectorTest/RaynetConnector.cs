using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RaynetConnectorTest.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RaynetConnectorTest
{
    public interface IRaynetConnector
    {
        Task<(RaynetBusinessCaseListWrapper result, HttpStatusCode statusCode)> GetBusinessCases(string parameters);
        Task<(RaynetBusinessCaseListWrapper result, HttpStatusCode statusCode)> GetAllBusinessCases(DateTime? lastSyncDate, bool includeSubQueries = true);
    }
    public class RaynetConnector : IRaynetConnector
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RaynetConnector> _logger;


        public RaynetConnector(ILogger<RaynetConnector> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<(RaynetBusinessCaseListWrapper result, HttpStatusCode statusCode)> GetBusinessCases(string parameters)
        {
            try
            {
                RaynetBusinessCaseListWrapper data = null;
                var response = await _httpClient.GetAsync("businessCase/" + parameters);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    data = JsonConvert.DeserializeObject<RaynetBusinessCaseListWrapper>(apiResponse);
                }
                else
                {
                    _logger.LogError("Při synchronizaci obchodních případů z Raynet nastala chyba. StatusCode: {0}, {1}", response.StatusCode.ToString(), response.Content.ReadAsStringAsync());
                }
                return (data, response.StatusCode);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Při synchronizaci obchodních případů z Raynet nastala chyba.");
                return (null, System.Net.HttpStatusCode.Forbidden);
            }
        }


        public async Task<(RaynetBusinessCaseListWrapper result, HttpStatusCode statusCode)> GetAllBusinessCases(DateTime? lastSyncDate, bool includeSubQueries = true)
        {
            try
            {
                var query = lastSyncDate != null ? "?rowInfo.updatedAt[GT]=" + lastSyncDate.Value.ToString("yyyy-MM-dd HH:mm") : "";

                RaynetBusinessCaseListWrapper data = null;

                var firstResponse = await GetBusinessCases(query);

                if (firstResponse.statusCode == HttpStatusCode.OK && firstResponse.result != null)
                {
                    data = firstResponse.result;

                    //pokud je celkový počet větší než 1000, tak provolat v cyklu a získat všechny záznamy. Raynet podporuje vrácení max 1000 záznamů v jednom response.
                    if (data.totalCount > 1000)
                    {
                        var numberOfResponses = Math.Ceiling((decimal)data.totalCount / (decimal)1000) - 1;

                        for (var i = 1; i <= numberOfResponses; i++)
                        {
                            var offset = i * 1000;
                            var response = await GetBusinessCases(query + (string.IsNullOrEmpty(query) ? "?" : "&") + "offset=" + offset);
                            if (response.statusCode == HttpStatusCode.OK && response.result != null)
                            {
                                data.data.AddRange(response.result.data);
                            }
                        }
                    }
                }

                var businessCasesAny = data != null && data.data != null && data.data.Any();

                if (businessCasesAny)
                {
                    _logger.LogInformation("Z Raynet bylo načteno {0} obchodních případů", data.data.Count);
                }

                return (data, firstResponse.statusCode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Při synchronizaci obchodních případů z Raynet nastala chyba.");
                return (null, System.Net.HttpStatusCode.Forbidden);
            }
        }
    }
}
