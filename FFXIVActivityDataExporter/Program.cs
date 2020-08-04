using FFXIVActivity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FFXIVActivityDataExporter
{
    class Program
    {
        const string GearReleaseDataExportPath = "../../../../FFXIVActivity/Data/gearReleases.json";
        const string MinionReleaseDataExportPath = "../../../../FFXIVActivity/Data/minionReleases.json";
        const string MountReleaseDataExportPath = "../../../../FFXIVActivity/Data/mountReleases.json";

        static void Main(string[] args)
        {
            var http = new HttpClient();

            var gearReleases = new List<GearRelease>();

            {
                var page = 1;
                var pageTotal = 1;
                while (page <= pageTotal)
                {
                    var dataStore2Raw = http.GetStringAsync(new Uri($"https://xivapi.com/Item?columns=ID,GamePatch.ReleaseDate&Page={page}")).GetAwaiter().GetResult();
                    var dataStore2 = JObject.Parse(dataStore2Raw);

                    pageTotal = dataStore2["Pagination"]["PageTotal"].ToObject<int>();

                    foreach (var child in dataStore2["Results"].Children())
                    {
                        int id;
                        long releaseDate;

                        try
                        {
                            id = child["ID"].ToObject<int>();
                            releaseDate = child["GamePatch"]["ReleaseDate"].ToObject<long>();
                        }
                        catch
                        {
                            continue;
                        }

                        gearReleases.Add(new GearRelease
                        {
                            Id = id,
                            ReleaseDate = releaseDate,
                        });
                    }

                    page++;
                }
            }

            File.WriteAllText(GearReleaseDataExportPath, JsonConvert.SerializeObject(gearReleases));

            var mountReleases = new List<MountRelease>();

            {
                var page = 1;
                var pageTotal = 1;
                while (page <= pageTotal)
                {
                    var dataStore2Raw = http.GetStringAsync(new Uri($"https://xivapi.com/Mount?columns=Name,GamePatch.ReleaseDate&Page={page}")).GetAwaiter().GetResult();
                    var dataStore2 = JObject.Parse(dataStore2Raw);

                    pageTotal = dataStore2["Pagination"]["PageTotal"].ToObject<int>();

                    foreach (var child in dataStore2["Results"].Children())
                    {
                        string name;
                        long releaseDate;

                        try
                        {
                            name = child["Name"].ToObject<string>().ToLowerInvariant();
                            releaseDate = child["GamePatch"]["ReleaseDate"].ToObject<long>();
                        }
                        catch
                        {
                            continue;
                        }

                        if (name == "") continue;

                        mountReleases.Add(new MountRelease
                        {
                            Name = name,
                            ReleaseDate = releaseDate,
                        });
                    }

                    page++;
                }
            }

            File.WriteAllText(MountReleaseDataExportPath, JsonConvert.SerializeObject(mountReleases));

            var minionReleases = new List<MinionRelease>();

            {
                var page = 1;
                var pageTotal = 1;
                while (page <= pageTotal)
                {
                    var dataStore2Raw = http.GetStringAsync(new Uri($"https://xivapi.com/Companion?columns=Name,GamePatch.ReleaseDate&Page={page}")).GetAwaiter().GetResult();
                    var dataStore2 = JObject.Parse(dataStore2Raw);

                    pageTotal = dataStore2["Pagination"]["PageTotal"].ToObject<int>();

                    foreach (var child in dataStore2["Results"].Children())
                    {
                        string name;
                        long releaseDate;

                        try
                        {
                            name = child["Name"].ToObject<string>().ToLowerInvariant();
                            releaseDate = child["GamePatch"]["ReleaseDate"].ToObject<long>();
                        }
                        catch
                        {
                            continue;
                        }

                        if (name == "") continue;

                        minionReleases.Add(new MinionRelease
                        {
                            Name = name,
                            ReleaseDate = releaseDate,
                        });
                    }

                    page++;
                }
            }

            File.WriteAllText(MinionReleaseDataExportPath, JsonConvert.SerializeObject(minionReleases));
        }
    }
}
