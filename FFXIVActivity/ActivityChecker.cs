using FFXIVActivity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FFXIVActivity
{
    public class ActivityChecker : IActivityChecker
    {
        private readonly HttpClient http;
        private readonly GearRelease[] gearReleases;
        private readonly MinionRelease[] minionReleases;
        private readonly MountRelease[] mountReleases;

        public ActivityChecker(HttpClient http)
        {
            this.http = http;
            this.gearReleases = LoadManifestResource<GearRelease[]>("FFXIVActivity.Data.gearReleases.json");
            this.minionReleases = LoadManifestResource<MinionRelease[]>("FFXIVActivity.Data.minionReleases.json");
            this.mountReleases = LoadManifestResource<MountRelease[]>("FFXIVActivity.Data.mountReleases.json");
        }

        public async Task<DateTime> GetLastActivityTime(ulong lodestoneId)
        {
            var characterPageUri = new Uri($"https://xivapi.com/character/{lodestoneId}?data=AC,MIMO");
            var response = await this.http.GetStringAsync(characterPageUri);
            var jResponse = JObject.Parse(response);

            var lastAchievementDate = GetLastAchievementDate(jResponse);
            var lastMinionDate = GetLastMinionDate(jResponse);
            var lastMountDate = GetLastMountDate(jResponse);
            var lastGearDate = GetLastGearDate(jResponse);

            return new[] { lastAchievementDate, lastMinionDate, lastMountDate, lastGearDate }.Max();
        }

        public async Task<DateTime> GetLastActivityTime(string name, string world)
        {
            name = name.ToLowerInvariant();
            world = world.ToLowerInvariant();

            var characterSearchUri = new Uri($"https://xivapi.com/character/search?name={name}&server={world}");
            var response = await this.http.GetStringAsync(characterSearchUri);
            var jResponse = JObject.Parse(response);
            var character = jResponse["Results"].Children()
                .FirstOrDefault(c => c["Name"].ToObject<string>().ToLowerInvariant() == name
                    && c["Server"].ToObject<string>().ToLowerInvariant().StartsWith(world));
            var lodestoneId = character["ID"].ToObject<ulong>();

            return await GetLastActivityTime(lodestoneId);
        }

        private DateTime GetLastAchievementDate(JObject jResponse)
        {
            DateTime lastActivityTime = default;
            if (jResponse["Achievements"] != null)
            {
                foreach (var achievement in jResponse["Achievements"]["List"].Children())
                {
                    var date = JSTimeToDotnetTime(achievement["Date"].ToObject<long>() * 1000);
                    if (date > lastActivityTime)
                    {
                        lastActivityTime = date;
                    }
                }
            }
            return lastActivityTime;
        }

        private DateTime GetLastMinionDate(JObject jResponse)
        {
            DateTime lastActivityTime = default;
            foreach (var minion in jResponse["Minions"].Children())
            {
                var minionReleaseTime = this.minionReleases
                    .FirstOrDefault(m => m.Name == minion["Name"].ToObject<string>().ToLowerInvariant())?.ReleaseDate;
                if (minionReleaseTime == null) continue;
                var date = JSTimeToDotnetTime((long)minionReleaseTime * 1000);
                if (date > lastActivityTime)
                {
                    lastActivityTime = date;
                }
            }
            return lastActivityTime;
        }

        private DateTime GetLastMountDate(JObject jResponse)
        {
            DateTime lastActivityTime = default;
            foreach (var mount in jResponse["Mounts"].Children())
            {
                var mountReleaseTime = this.mountReleases
                    .FirstOrDefault(m => m.Name == mount["Name"].ToObject<string>().ToLowerInvariant())?.ReleaseDate;
                if (mountReleaseTime == null) continue;
                var date = JSTimeToDotnetTime((long)mountReleaseTime * 1000);
                if (date > lastActivityTime)
                {
                    lastActivityTime = date;
                }
            }
            return lastActivityTime;
        }

        private DateTime GetLastGearDate(JObject jResponse)
        {
            var gearSlots = jResponse["Character"]["GearSet"]["Gear"];

            DateTime lastActivityTime = default;
            foreach (var gearSlot in gearSlots.Children())
            {
                var gearTime = this.gearReleases
                    .FirstOrDefault(g => g.Id == gearSlot.Children().FirstOrDefault()?["ID"].ToObject<int?>())?.ReleaseDate;
                if (gearTime == null) continue;
                var date = JSTimeToDotnetTime((long)gearTime * 1000);
                if (date > lastActivityTime)
                {
                    lastActivityTime = date;
                }
            }
            return lastActivityTime;
        }

        private static T LoadManifestResource<T>(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            using var streamReader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
        }

        private static DateTime JSTimeToDotnetTime(long timeMs)
        {
            return new DateTime(1970, 1, 1) + new TimeSpan(timeMs * 10000);
        }
    }
}
