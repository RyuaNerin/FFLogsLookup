using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using FFLogsLookup.Game;
using Newtonsoft.Json;
using static FFLogsLookup.FFlogs.FFlogsLog;

namespace FFLogsLookup.FFlogs
{
    internal partial class FFlogsClient : IDisposable
    {
        private const float CacheExpires = 7; // days

        [DebuggerDisplay("j:{GameJob} e:{GameEncounter} pi:{PropertyInfo}")]
        private struct EncounterPropertyInfo
        {
            public PropertyInfo    PropertyInfo    { get; set; }
            public GameEncounter   GameEncounter   { get; set; }
            public GameJob         GameJob         { get; set; }
            public bool            NonEcho         { get; set; }
        }
        private static readonly EncounterPropertyInfo[] encounterPropertyInfos;

        private readonly HttpClient httpClient;

        private long validToken; // 0=false, 1=true
        public bool Authorized {
            get => Interlocked.Read(ref this.validToken) != 0;
            set => Interlocked.Exchange(ref this.validToken, value ? 1 : 0);
        }

        static FFlogsClient()
        {
            var type = typeof(Character);

            var encounterDic = new Dictionary<string, GameEncounter>
            {
                { "E9S"  , GameEncounter.E9s        },
                { "E10S" , GameEncounter.E10s       },
                { "E11S" , GameEncounter.E11s       },
                { "E12Sd", GameEncounter.E12sDoor   },
                { "E12So", GameEncounter.E12sOracle },
            };
            var partitionDic = new Dictionary<string, FFlogsPartition>
            {
                { "Ne", FFlogsPartition.GameNonEcho },
                //{ "Ec", FFlogsPartition.GameEcho },
            };

            var flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;

            var lst = new List<EncounterPropertyInfo>();
            foreach (var enc in encounterDic)
            {
                foreach (var part in partitionDic)
                {
                    foreach (var job in Enum.GetValues<GameJob>())
                    {
                        if (job == GameJob.Best || job == GameJob.None) continue;

                        var epi = new EncounterPropertyInfo
                        {
                            PropertyInfo  = type.GetProperty($"{enc.Key}{part.Key}{job}", flags),
                            GameEncounter = enc.Value,
                            GameJob       = job,
                            NonEcho       = part.Value == FFlogsPartition.GameNonEcho,
                        };

                        lst.Add(epi);

                        if (epi.PropertyInfo == null)
                        {
                            throw new Exception($"{enc.Key}{part.Key}{job} Not found");
                        }
                    }
                }
            }

            var encounterRaidDic = new Dictionary<string, GameEncounter>
            {
                { "Tea" , GameEncounter.Tea  },
                { "Ucob", GameEncounter.Ucob },
                { "Uwu" , GameEncounter.Uwu  },
            };
            foreach (var enc in encounterRaidDic)
            {
                foreach (var job in Enum.GetValues<GameJob>())
                {
                    if (job == GameJob.Best || job == GameJob.None) continue;

                    var epi = new EncounterPropertyInfo
                    {
                        PropertyInfo = type.GetProperty($"{enc.Key}{job}", flags),
                        GameEncounter = enc.Value,
                        GameJob = job,
                        NonEcho = true,
                    };

                    lst.Add(epi);

                    if (epi.PropertyInfo == null)
                    {
                        throw new Exception($"{enc.Key}{job} Not found");
                    }
                }
            }

            encounterPropertyInfos = lst.ToArray();
        }

        private readonly string baseDir;

        internal FFlogsClient()
        {
            this.httpClient = new HttpClient();

            var dir = DalamudInstance.PluginInterface.ConfigDirectory;
            this.baseDir = dir.FullName;

            // 일정 시간 이상 지난 로그 삭제
            var minDate = DateTime.UtcNow.AddDays(-CacheExpires);
            foreach (var f in dir.GetFiles())
            {
                if (f.CreationTimeUtc < minDate)
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {
                    }
                }
            }
        }
        ~FFlogsClient()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool disposed;
        protected void Dispose(bool disposing)
        {
            if (this.disposed) return;
            this.disposed = true;

            if (disposing)
            {
                this.httpClient.Dispose();
            }
        }

        private readonly object cacheLock = new();
        private string GetCachePath(string name)
        {
            return Path.Join(this.baseDir, $"{name}.json");
        }
        private void SaveCache(string name, FFlogsLog data)
        {
            lock (this.cacheLock)
            {
                var path = this.GetCachePath(name);
                try
                {
                    using var fs = File.Create(path);
                    using var w = new StreamWriter(fs, Encoding.UTF8);

                    jsonSerializer.Serialize(w, data);
                }
                catch
                {
                }
            }
        }
        private FFlogsLog LoadCache(string name)
        {
            lock (this.cacheLock)
            {
                var path = this.GetCachePath(name);
                try
                {
                    using var fs = File.OpenRead(path);
                    using var r = new StreamReader(fs, Encoding.UTF8);
                    using var jr = new JsonTextReader(r);

                    return jsonSerializer.Deserialize<FFlogsLog>(jr);
                }
                catch
                {
                }
            }

            return null;
        }

        internal class Token
        {
            [JsonProperty("access_token")] internal string AccessToken { get; set; }
            [JsonProperty("token_type"  )] internal string TokenType   { get; set; }
            [JsonProperty("expires_in"  )] internal int    ExpiresIn   { get; set; }
            [JsonProperty("error"       )] internal string Error       { get; set; }
        }
        public async Task Authorize(string clientId, string clientSecret)
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type"   , "client_credentials" },
                { "client_id"    , clientId             },
                { "client_secret", clientSecret         },
            };

            using var reqContent = new FormUrlEncodedContent(form);
            using var resp = await this.httpClient.PostAsync("https://www.fflogs.com/oauth/token", reqContent);

            var respBody = await resp.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<Token>(respBody);

            if (!string.IsNullOrWhiteSpace(token.Error))
            {
                throw new FFlogsException(token.Error);
            }

            this.Authorized = true;

            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        public async Task<(FFlogsLog data, bool cached)> GetLogs(string charName, GameServer gameServer, bool forced, CancellationToken token)
        {
            var cacheKey = GetKey(charName, gameServer);
            if (!forced)
            {
                var cache = this.LoadCache(cacheKey);
                if (cache != null)
                {
                    return (cache, true);
                }
            }

            if (!this.Authorized)
            {
                throw new InvalidOperationException("Not authorized");
            }

            using var content = new StringContent(string.Format(BaseQuery, charName, gameServer.ToString()), Encoding.UTF8, "application/json");
            using var resp = await this.httpClient.PostAsync("https://ko.fflogs.com/api/v2/client", content, token);

            var respBody = await resp.Content.ReadAsStringAsync(token);
            var respData = JsonConvert.DeserializeObject<ResponseData>(respBody, jsonSerializerSettings);

            var returnData = new FFlogsLog()
            {
                CharName     = charName,
                CharServer   = gameServer,
                UpdatedAtUtc = DateTime.UtcNow,
                Hidden       = respData.Data.CharacterData.Character.Hidden,
            };

            if (!returnData.Hidden)
            {
                UpdateAllstar(returnData.RaidAllstarNe, respData.Data.CharacterData.Character.EdenPromiseNe);
                //UpdateAllstar(returnData.RaidAllstarEc, respData.Data.CharacterData.Character.EdenPromiseEc);

                foreach (var epi in encounterPropertyInfos)
                {
                    var encounterRankings = (EncounterRankings)epi.PropertyInfo.GetValue(respData.Data.CharacterData.Character);
                    if (encounterRankings == null) continue;

                    var key = new EncounterDataKey
                    {
                        EncounterId = epi.GameEncounter,
                        JobId = epi.GameJob,
                    };

                    if (encounterRankings.TotalKills == 0)
                        continue;

                    // (epi.NonEcho ? returnData.EncountersNe : returnData.EncountersEc)[key] = new()
                    returnData.EncountersNe[key] = new()
                    {
                        Kills   = encounterRankings.TotalKills,
                        MaxRdps = encounterRankings.BestAmount,
                        MaxPer  = encounterRankings.Ranks.Max(e => e.RankPercent),
                        MedPer  = encounterRankings.MedianPerformance.Value,
                    };
                }
            }

            this.SaveCache(cacheKey, returnData);
            return (returnData, false);
        }

        private static void UpdateAllstar(Dictionary<GameJob, ZoneData> dic, ZoneRankings zoneRankings)
        {
            var jobs = zoneRankings.AllStars.Select(e => e.Spec).Distinct().ToArray();

            foreach (var job in jobs)
            {
                var allStar =
                    zoneRankings.AllStars
                    .Where(e => e.Spec == job)
                    .OrderBy(e => e.Rank)
                    .First();
                    
                dic[job] = new ZoneData
                {
                    Point = allStar.Points,
                    Rank  = allStar.Rank,
                    Total = allStar.Total,
                };
            }
        }
    }
}
