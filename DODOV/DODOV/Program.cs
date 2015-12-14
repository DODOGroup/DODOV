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
        static char[] separators = new char[] { '-','\\','/' };
        static void Main(string[] args)
        {
            var start = new List<Tuple<string, Action<string[]>, string>>();
            if (args.Any((s) => is_Command(s, "/v", separators))) { ver=true;}
            start.Add(CreateTuple("/doump", (s) => Doump(s), "/doump Folder"));
            start.Add(CreateTuple("/read", (s) => ReadDoumpAndWriteEXT(s), ""));
            start.Add(CreateTuple("/crime", (s) => Crime(s), "Crime has no explanation (doump path)"));
            start.Add(CreateTuple("/?", (s) =>
            {
                start.ForEach((s1) => Log(string.Format("{0} > {1}", s1.Item1, s1.Item3),true,ConsoleColor.White));
            }, ""));
            if (args.Length == 0) { start.Where((s) => s.Item1 == "/?").First().Item2(args); }
            args.ToList().ForEach((a) =>
            {
                var v = start.Where((s) => is_Command(a,s.Item1,separators)).FirstOrDefault();
                (v ?? CreateTuple("", (s) => { }, "")).Item2(args);
            });   
            Log("[Crime always win!]", true, ConsoleColor.Yellow);

        }
        static void Crime(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (is_Command(command[i],"/crime",separators))
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
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        static void Doump(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (is_Command(command[i], "/doump", separators))
                {
                    try
                    {
                        var doump = new EnDe(command[i + 1]);
                        doump.Save(Directory.GetParent(command[i + 1]).FullName, Directory.GetParent(command[i+1]).Name + ".v");
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
            return rep.ToList().Any((s) => command.Contains(iscontained.Replace('/', s)));
            //var v = false;
            //rep.ToList().ForEach((s) => { if (command.Contains(iscontained.Replace('/', s))) { v=true; } });
            //return v;
        }
        static void ReadDoumpAndWriteEXT(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (is_Command(command[i], "/read", separators))
                {
                    try
                    {
                        var doump = EnDe.Parse(File.ReadAllLines(command[i + 1]));
                        foreach (var item in doump.GetExtensions())
                        {
                            Log(item,true);
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
