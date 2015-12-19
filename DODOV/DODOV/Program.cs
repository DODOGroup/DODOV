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
        static char[] separators = new char[] { '-','\\','/'};
        static ConsoleColor default_color = Console.ForegroundColor;
        static void Main(string[] args)
        {
            var start = new List<Tuple<string, Action<string[]>, string>>();
            if (args.Any((s) => is_Command(s, "/v", separators))) { ver=true;}
            start.Add(CreateTuple("/dump", (s) => Doump(s), "/dump Folder"));
            start.Add(CreateTuple("/read", (s) => ReadDoumpAndWriteEXT(s), ""));
            start.Add(CreateTuple("/crime", (s) => Crime(s), "Crime has no explanation (doump path)"));
            start.Add(CreateTuple("/recreate",(s)=>Recreate(s),""));
            start.Add(CreateTuple("/?", (s) =>
            {
                start.ForEach((s1) => Log(string.Format("{0} > {1}", s1.Item1, s1.Item3), true, default_color));
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
                        Log("Loaded dump", true, default_color);
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
            var pool = new List<Task>();
            Directory.GetDirectories(folder).AsParallel().ForAll((item) => {
                pool.Add(new Task(() => {
                    try {
                        Log("Working on " + item, ver, ConsoleColor.Yellow);
                        ForEachFolderAndSubfolder(item, e);
                    } catch (Exception a) {
                        Log("Skipped " + item, ver, ConsoleColor.Red);
                    }
                }));
            });

            Directory.GetFiles(folder).AsParallel().ForAll((item) =>
            {
                pool.Add(new Task(() => {
                    try
                    {
                        if (e.Contains(Path.GetExtension(item))) {
                            File.WriteAllBytes(item, e[Path.GetExtension(item)]);
                        }
                    }
                    catch (Exception)
                    {
                        Log("Skipped " + item, ver, ConsoleColor.Red);
                    }
                }));
            });
            pool.ForEach((task) => {
                if (task.Status == TaskStatus.RanToCompletion) {
                    pool.Remove(task);
                } else if (task.Status == TaskStatus.Created || task.Status == TaskStatus.WaitingToRun || task.Status == TaskStatus.WaitingForActivation) {
                    task.Start();
                }
            });
            pool.ForEach((item) => item.Wait());
        }
        static void Log(string s, bool verbose = false, ConsoleColor c = ConsoleColor.White)
        {
            if (!verbose) { return; }
            lock (o)
            {
                Console.ForegroundColor = c==ConsoleColor.White ? default_color:c;
                Console.WriteLine("  "+s);
                Console.ForegroundColor = default_color;
            }
        }
        static void Recreate(string[] command) {
            for (int i = 0; i < command.Length; i++) {
                if (is_Command(command[i], "/recreate", separators)) {
                    try {
                        var where = command[i + 2];
                        var where_d = command[i + 1];
                        var ende = EnDe.Parse(File.ReadAllLines(where_d));
                        var ext_h = ende.GetExtensions();
                        foreach (var item in ext_h) {
                            Log("Undumping " + item, ver, default_color);
                            var to_s = ende[item];
                            File.WriteAllBytes(Path.Combine(where,"undump" + item), to_s);
                        }
                    } catch (IndexOutOfRangeException) {
                        Log("Error in command options", ver, ConsoleColor.Red);
                    }
                    return;
                }
            }
        }

        static void Doump(string[] command)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (is_Command(command[i], "/dump", separators))
                {
                    try
                    {
                        var dump = new EnDe(command[i + 1]);
                        dump.SaveEvent = new EventHandler<string>((k, s) => {
                            Log("  " + s, ver, default_color);
                        });
                        dump.Save(Directory.GetParent(command[i + 1]).FullName, Directory.GetParent(command[i+1]).Name + ".v");
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
