using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrettyJunction
{
    class JunctionManager
    {
        private static readonly Regex VARIABLE_EVALUATOR = new Regex("\\{(?<name>[a-zA-Z]+)(:-)?(?<exclude>[a-zA-Z]+)?\\}", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private Dictionary<string, List<string>> junctionVar = new Dictionary<string, List<string>>(); 
        public void ProcessFile(string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        ProcessLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
            }
        }
        private void ProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;
            string trimline = line.Trim();
            if(trimline.StartsWith("#"))
                return;
            if(trimline.StartsWith("@:"))
            {
                ProcessVariable(trimline);
                if (!ValidateVariable())
                {
                    throw new Exception(string.Format("Format error,current line:{0}",line));
                }
                return;
            }
            string[] parts = line.Split(new string[] {" ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length!=2)
            {
                WriteError("line format error:{0}",line);
                return;
            }
            CreateJunction(parts[0].Trim(),parts[1].Trim());
        }
        private void ProcessVariable(string line)
        {
            string varline = line.Substring(2);
            string[] parts = varline.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                WriteError("line format error:{0}", line);
                return;
            }
            string[] varValue=parts[1].Split(new string[] { ","}, StringSplitOptions.RemoveEmptyEntries);
            List<string> varList = varValue.Select(v => v.Trim()).ToList();
            junctionVar.Add(string.Format("{0}", parts[0].Trim()), varList);
        }
        private void CreateJunction(string junctionPoint, string target)
        {
            try
            {
                MatchCollection matches = VARIABLE_EVALUATOR.Matches(junctionPoint);
                if(matches.Count==0)
                    matches = VARIABLE_EVALUATOR.Matches(target);
                if (matches.Count > 0)
                {
                    ProcessGroup(junctionPoint, target, matches);
                }
                else
                {
                    JunctionPoint.Create(junctionPoint, target, true);
                }
            }
            catch (JunctionException ex)
            {
                WriteError(ex.Message);
            }
            catch(IOException ex)
            {
                WriteError("[source]:{1},[target]:{2},[error]:{0},[stack trace]:{3}", ex.Message, junctionPoint, target,ex.StackTrace);
            }
            catch(Exception ex)
            {
                WriteError(ex.Message);
            }
        }

        

        private bool ValidateVariable(MatchCollection matchCollection)
        {
            //1,valiate matches
            foreach (Match match in matchCollection)
            {
                string name = match.Groups["name"].Value;
                if(!junctionVar.ContainsKey(name))
                {
                    WriteError("does not have variable {0}",name);
                    return false;
                }
            }
            //2,validate junction variable length
            if (!ValidateVariable()) return false;
            return true;
        }

        private bool ValidateVariable()
        {
            if (junctionVar.Count > 1)
            {
                int count = -1;
                foreach (var kv in junctionVar)
                {
                    if (count == -1)
                    {
                        count = junctionVar[kv.Key].Count;
                    }
                    else
                    {
                        if (junctionVar[kv.Key].Count != count)
                        {
                            WriteError("variables length are not same,{0}", kv.Key);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void ProcessGroup(string junctionPoint, string target, MatchCollection matches)
        {
            if(!ValidateVariable(matches))
            {
                return;
            }

            Match match = matches[0];
            string variable = match.Groups["name"].Value;
            if (!junctionVar.ContainsKey(variable))
            {
                WriteError("can not find variable {0}", variable);
            }
            //copy new list
            List<string> availValues = junctionVar[variable].ToList();
            
            //build VariableNode List
            List<VariableNode> availNodes=new List<VariableNode>();
            for(int i=0;i<availValues.Count;i++)
            {
                availNodes.Add(new VariableNode(){Index =i,Value = availValues[i]});
            }

            string exclude = match.Groups["exclude"].Value;
            string[] excludes = exclude.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in excludes)
            {
                var node = availNodes.Find(n => n.Value == s);
                availNodes.Remove(node);
            }

            foreach (var node in availNodes)
            {
                JunctionPoint.Create(Evaluate(junctionPoint, node,exclude), Evaluate(target, node), true);
            }
        }

        public bool CleanDirectory(string directory)
        {
            directory = Path.GetFullPath(directory);
            if (!Directory.Exists(directory))
                return false;
            string[]directories = Directory.GetDirectories(directory);
            foreach (var s in directories)
            {
                if(JunctionPoint.Exists(s))
                {
                    Directory.Delete(s);
                }
                else
                {
                    CleanDirectory(s);
                }
            }
            return true;
        }

        private string Evaluate(string origin,VariableNode node,string exclude = null)
        {
            string dest = origin;
            foreach (var key in junctionVar.Keys)
            {
                dest = dest.Replace(FormatKey(key,exclude), junctionVar[key][node.Index]);
            }
#if DEBUG
            Console.WriteLine(dest);
#endif
            return dest;
        }

        private string FormatKey(string key,string exclude)
        {
            if(string.IsNullOrEmpty(exclude))
                return string.Format("{{{0}}}", key);
            return string.Format("{{{0}:-{1}}}", key, exclude);
        }

        private  void WriteError(string error,params object[] arg)
        {
            Helper.WriteError(error, arg);
        }

        private void WriteSuccess(string error, params object[] arg)
        {
            var fgcolor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(error, arg);
            Console.ForegroundColor = fgcolor;
        }
    }
}
