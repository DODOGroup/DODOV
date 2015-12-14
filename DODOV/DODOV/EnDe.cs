using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystemCreator {
    class EnDe {
        List<Extension> ls_E = new List<Extension>();
        public EnDe() { }
        public EnDe(string path) {
            Load(path);
        }
        public void Load(string path) {
            List<string> files = new List<string>();
            List<string> ext = new List<string>();
            Directory.GetFiles(path).ToList().ForEach((s) => { if (!ext.Contains(Path.GetExtension(s))) { ext.Add(Path.GetExtension(s)); files.Add(s); } });
            foreach (var item in files) {
                ls_E.Add(new Extension(Path.GetFullPath(item)));
            }
        }
        public void Save(string path, string name) {
            string fullpath = Path.Combine(path, name);
            var to_save = new List<string>();
            Parallel.ForEach(ls_E,(s)=>{
                to_save.Add(s.ToString());
            });
            File.WriteAllLines(fullpath, to_save);
        }
        public static EnDe Parse(string[] s) {
            var to_r = new EnDe();
            foreach (var item in s) {  
                var ext = item.Split(',').First();
                var bytes = item.Split(',')[1];
                to_r.ls_E.Add(new Extension(ext, bytes));
            }
            return to_r;
        }
        public bool Contains(string ext) {
            return ls_E.Where((s) => s.Extension_string == ext).Count() != 0;
        }
        public string[] GetExtensions() {
            return Enumerable.Range(0,ls_E.Count).Select((x)=>ls_E[x].Extension_string).ToArray();
        }
        public byte[] this[string ext] {
            get {
                var file=this.ls_E.Where((s) => s.Extension_string == ext);
                if ( file.Count() == 0) { throw new ArgumentException("No such extension in db"); } else {
                    return file.First().Sample_file;
                }
            }
        }
    }
    class Extension {
        public string Extension_string { get; set; }
        public byte[] Sample_file { get; set; }
        public Extension() { }
        public Extension(string ex, string bArr) {
            Extension_string = ex;
            List<byte> y = new List<byte>();
            bArr.Split('.').ToList().ForEach((z) => y.Add(Convert.ToByte(z)));
            Sample_file = y.ToArray();
        }
        public Extension(string path) {
            Load(path);
        }
        public void Load(string path) {
            Sample_file = File.ReadAllBytes(path);
            Extension_string = Path.GetExtension(path);
        }
        public static Extension Parse(string s) {
            return new Extension(s.Split(',')[0],s.Split(',')[1]);
        }
        public override string ToString() {
            var v ="";
            Sample_file.ToList().ForEach((s) => v+= Convert.ToString(s) + ".");
            return Extension_string + "," + v.Remove(v.Length-1); //1.1.2.3. to 1.1.2.3
        }
    }
}
