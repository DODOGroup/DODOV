using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FileSystemCreator;

namespace V
{
    class Program
    {
        static object o = new object();
        static bool ver = false;
        static void Main(string[] args)
        {
            var start = new List<Tuple<string, Action<string[]>, string>>();
            //if (args.ToList().ForEach((s)=>is_Command(s," /v ",))) { ver = true; }
            start.Add(CreateTuple("/doump", (s) => Doump(s), "/doump Folder"));
            start.Add(CreateTuple("/read", (s) => ReadDoumpAndWriteEXT(s), ""));
            start.Add(CreateTuple("/crime", (s) => Crime(s), "Crime has no explanation (doump path)"));
            start.Add(CreateTuple("/?", (s) =>
            {
                start.ForEach((s1) => Log(string.Format("{0} > {1}", s1.Item1, s1.Item3)));
            }, ""));
            if (args.Length == 0) { start.Where((s) => s.Item1 == "/?").First().Item2(args); }
            args.ToList().ForEach((a) =>
            {
                var v = start.Where((s) => s.Item1 == a).FirstOrDefault();
                (v ?? CreateTuple("", (s) => { }, "")).Item2(args);
            });
            Log("[Crime always win!]", true, ConsoleColor.Yellow);
        }
        static void Crime(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == "/crime")
                {
                    try
                    {
                        var doump = EnDe.Parse(File.ReadAllLines(command[i + 1]));
                        var root = command[i + 2];
                        ForEachFolderAndSubfolder(root, doump);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Log("Error in command options", ver, ConsoleColor.Red);
                    }
                    return;
                }
            }
        }
        static void ForEachFolderAndSubfolder(string folder, EnDe e)
        {
            Directory.GetDirectories(folder).AsParallel().ForAll((item) =>
            {
                try
                {
                    ForEachFolderAndSubfolder(item, e);
                }
                catch (Exception a)
                {
                    Log("Skipped " + item, ver, ConsoleColor.Red);
                }
            });
            Directory.GetFiles(folder).AsParallel().ForAll((item) =>
            {
                try
                {
                    File.WriteAllBytes(item, e[Path.GetExtension(item)]);
                }
                catch (ArgumentException)
                { //Do not have this doump

                }
                catch (Exception err)
                {
                    Log("Skipped " + item, ver, ConsoleColor.Red);
                }
            });
        }
        static void Log(string s, bool verbose = false, ConsoleColor c = ConsoleColor.White)
        {
            if (!verbose) { return; }
            lock (o)
            {
                Console.ForegroundColor = c;
                Console.WriteLine(s);
            }
        }
        static void Doump(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == "/doump")
                {
                    try
                    {
                        var doump = new EnDe(command[i + 1]);
                        doump.Save(Path.Combine(command[i + 1], "..\\"), command[i + 1].Split('/', '\\').Last() + ".v");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Log("Error in command options", ver, ConsoleColor.Red);
                    }
                    return;
                }
            }
        }
        static bool is_Command(string command, string iscontained,char[] rep) {
            var v = false;
            rep.ToList().ForEach((s) => { if (command.Contains(iscontained.Replace('/', s))) { v=true; } });
            return v;
        }
        static void ReadDoumpAndWriteEXT(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == "/read")
                {
                    try
                    {
                        var doump = EnDe.Parse(File.ReadAllLines(command[i + 1]));
                        foreach (var item in doump.GetExtensions())
                        {
                            Log(item);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Log("Error in command options", ver, ConsoleColor.Red);
                    }
                    return;
                }
            }
        }

        static Tuple<string, Action<string[]>, string> CreateTuple(string s, Action<string[]> a, string doc)
        {
            return Tuple.Create(s, a, doc);
        }
    }
}
