using System;
using System.Collections;
using Realms;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets;
using System.Linq;
using osu.Game.Extensions;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework.Internal;
using Vortice.Win32;

#nullable enable
namespace TestHasher
{
    public static class Tools
    {
        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }

    static class Extension
    {
        //récupère le chemin du fichier préciser en argument 
        public static string? GetPathForFile(this BeatmapSetInfo set, string filename) => set.Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.File.GetStoragePath();
        /*function analysis
         static string?  <==>  return is string or null
        
         GetPathForFile(this BeatmapSetInfo set   <==> méthode static dans classe static créer une méthode d'extension set.GetPathForFile(...) autoriser
        
        ,string filename) =>    <==> définition sous forme de lambda
        
        set.Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase)) <==> comme filter en java
        
        ?.File.GetStoragePath();  <==> récupère le chemin du fichier si non null ('?' = RealmNamedFileUsage)
         
         */
        public static string GetExportFileName(this LimitedBeatmapInfo l) => Tools.ReplaceInvalidChars(l.Id+" "+l.Author+" - "+l.Name+".mp3");
    }

    

    class LimitedBeatmapInfo{
        public string Name { get; private set; } = string.Empty;
        public int Id { get; private set; }
        public string Author { get; private set; } = string.Empty;
        public string FilePath { get; private set; } = string.Empty;

        public LimitedBeatmapInfo(string name, string author, string filePath, int id) { Name = name; Author = author; FilePath = filePath;Id = id; }
       
    }

    class Program
    {
        static void Print(Object o)
        {
            Console.WriteLine(o);
        }
        static string ToStringRuleset(RulesetInfo rsi)
        {
            string ret = "";
            ret += rsi.ShortName + " (" + rsi.OnlineID + ")[" + rsi.Name + "] : " + rsi.InstantiationInfo;
            return ret;
        }

        static string ToStringDifficulty(BeatmapDifficulty bmd)
        {
            string ret = "";
            ret += "[Drain rate]" + bmd.DrainRate
                + ":[CircleSize]" + bmd.CircleSize
                + ":[OverlapDifficulty]" + bmd.OverallDifficulty
                + ":[ApproachRate]" + bmd.ApproachRate
                + ":[SliderMultiplier]" + bmd.SliderMultiplier
                + ":[SliderTickRate]" + bmd.SliderTickRate;
            return ret;
        }

        static string StringOrNone(string str)
        {
            return (String.IsNullOrEmpty(str) ? "none" : str);
        }

        static string ToStringMetadata(IBeatmapMetadataInfo bmm)
        {
            string ret;
            ret = "\n\ttitle           : " + StringOrNone(bmm.Title) + "\n\t"
                + "title unicode   : " + StringOrNone(bmm.TitleUnicode) + "\n\t"
                + "artist          : " + StringOrNone(bmm.Artist) + "\n\t"
                + "artiste unicode : " + StringOrNone(bmm.ArtistUnicode) + "\n\t"
                + "Author          : " + StringOrNone(bmm.Author.Username) + "\n\t"
                + "source          : " + StringOrNone(bmm.Source) + "\n\t"
                + "tags            : " + StringOrNone(bmm.Tags) + "\n\t"
                + "preview time    : " + StringOrNone(bmm.PreviewTime.ToString()) + "\n\t"
                + "audio file      : " + StringOrNone(bmm.AudioFile) + "\n\t"
                + "bckground file  : " + StringOrNone(bmm.BackgroundFile);
            return ret;
        }

#pragma warning disable IDE0051 // Remove unused private members
        static void PrintData(BeatmapInfo btm)
#pragma warning restore IDE0051 // Remove unused private members
        {

            Print("Guid(byte array): " + btm.ID);
            Print("Difficulty name(string): " + btm.DifficultyName);
            Print("Ruleset: " + ToStringRuleset(btm.Ruleset));
            Print("Difficulty: " + ToStringDifficulty(btm.Difficulty));
            Print("Metadata: " + ToStringMetadata(btm.Metadata));

            Print("user settings: " + btm.UserSettings);
            Print("beatmap set: " + btm.BeatmapSet);
            Print("beatmap set hash: " + btm.BeatmapSet?.Hash);
            Print("Audio file: " + btm.BeatmapSet?.GetPathForFile(btm.BeatmapSet.Metadata.AudioFile));
            Print("status: " + btm.Status);
            Print("status int: " + btm.StatusInt);
            Print("OnlineId: " + btm.OnlineID);
            Print("Length: " + btm.Length);
            Print("BPM: " + btm.BPM);
            Print("Hash: " + btm.Hash);
            Print("Star rating: " + btm.StarRating);
            Print("MD5Hash: " + btm.MD5Hash);
            Print("hidden: " + btm.Hidden);
        }

        static void PrintSetData(BeatmapSetInfo btm)
        {
            Print("Guid(byte array) : " + btm.ID);
            Print("beatmap set      : " + btm);
            Print("beatmap set hash : " + btm.Hash);
            Print("Metadata         : " + ToStringMetadata(btm.Metadata));
            Print("Audio file       : " + btm.GetPathForFile(btm.Metadata.AudioFile));
            Print("status           : " + btm.Status);
            Print("status int       : " + btm.StatusInt);
            Print("OnlineId         : " + btm.OnlineID);
            Print("Hash             : " + btm.Hash);
        }

        static string[] ReadDirectoryFiles(string directory)
        {
            string[] res = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Select(v=>Path.GetFileName(v).Split(' ')[0]).ToArray();
            return res;
        }

        static string GetExportDir(string dest)
        {
            string dt = DateTime.Today.ToString("yyyy-MM-dd");
            string dir = dest+'/'+dt;
            var i = 1;
            while (Directory.Exists(dir))
            {
                dir = dest + '/' + dt + ' ' + i;
                i++;
            }
            Directory.CreateDirectory(dir);
            return dir;
        }

        static void ExportAudios(string src,string dest, LimitedBeatmapInfo[] to_export)
        {
            int counter = 0;
            const int max = 200;
            string dest_dir = GetExportDir(dest);
            foreach(LimitedBeatmapInfo f in to_export)
            {
                File.Copy(src + '/' + f.FilePath, dest_dir + "/" + f.GetExportFileName(), false);
                counter++;
                if (counter > max)
                {
                    counter = 0;
                    dest_dir= GetExportDir(dest);
                }
            }
        }

        static void Main()
        {
            RealmConfiguration config = new("E:\\Projects\\OsuAudioExtractor\\client.realm")
            {
                SchemaVersion = 26
            };
            var realm = Realm.GetInstance(config);
            Console.WriteLine("realm status : " + realm.IsClosed + " " + realm.IsFrozen + " " + realm.Schema + "\n");

            var test = realm.All<BeatmapSetInfo>();
            var already_exported = ReadDirectoryFiles("E:\\osu songs");
            LimitedBeatmapInfo[] to_export = test.ToList().Select(
                v => new LimitedBeatmapInfo(
                    v.Metadata.Title, 
                    v.Metadata.Artist, 
                    v.GetPathForFile(v.Metadata.AudioFile), 
                    v.OnlineID))
                .Where(v=>!already_exported.Contains(v.Id.ToString()))
            .ToArray();
            ExportAudios("E:\\Program files (emergency)\\osu!lazer\\files", "E:\\osu songs", to_export);
        }
    }
}
