using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace DODOV {
    class SubstitutorFromXML {
        List<SubstitutionSegment> inners = new List<SubstitutionSegment>();
        public SubstitutorFromXML (){ }
        public void Save(string path="substitutor.xml") {
            var ser = new XmlSerializer(typeof(List<SubstitutionSegment>));
            ser.Serialize(new StreamWriter(path), inners);
        }
        public void Load(string path = "substitutor.xml") {
            var ser = new XmlSerializer(typeof(SubstitutionSegment));
            var deserialized = (List<SubstitutionSegment>)ser.Deserialize(new StreamReader(path));
            inners.Clear();
            inners.AddRange(deserialized);
        }
        public void Add(SubstitutionSegment s) {
            inners.Add(s);
        }
        public bool Contains(string ext) {
            return inners.Any((s) => s.Extension == ext);
        }
        public SubstitutionSegment this[string ext] {
            get {
                return inners.FirstOrDefault((s) => s.Extension == ext);
            }
        }
        public string Elaborate(string input,string ext) {
            return Regex.Replace(input, this[ext].Regex_pattern, this[ext].Substitute_with);
        }
        public string[] GetAllExtensions() {
            return Enumerable.Range(0, inners.Count).Select((s) => {
                return inners[s].Extension;
            }).ToArray();
        }
        public string[] GetAllInfo() {
            return Enumerable.Range(0, inners.Count).Select(((s) => {
                return string.Format("Extension: {0}  Regex patter: {1}  Substitute with: {2}",
                    inners[s].Extension, inners[s].Regex_pattern, inners[s].Substitute_with);
            })).ToArray();
        }
    }
    public class SubstitutionSegment {
        public string Extension{get;set;}
        public string Regex_pattern { get; set; }
        public string Substitute_with { get; set; }
        public SubstitutionSegment() {        }
        public SubstitutionSegment(string regex_pattern, string sub_with) {
            this.Regex_pattern = regex_pattern;
            this.Substitute_with = sub_with;
        }
    }

}
