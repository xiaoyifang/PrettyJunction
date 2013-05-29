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
            using (StreamReader sr = new StreamReader(filename))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    ProcessLine(line);
                }
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
                    foreach (Match match in matches)
                    {
                        ProcessGroup(junctionPoint, target, match);
                    }
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

        private void ProcessGroup(string junctionPoint, string target, Match match)
        {
            string variable = match.Groups["name"].Value;
            if (!junctionVar.ContainsKey(variable))
            {
                WriteError("can not find variable {0}", variable);
            }
            //copy new list
            var availValues = junctionVar[variable].ToList();
            string exclude = match.Groups["exclude"].Value;
            string[] excludes = exclude.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in excludes)
            {
                if (availValues.Contains(s))
                    availValues.Remove(s);
            }
            foreach (var v in availValues)
            {
                string junctionValue, targetValue;
                if (v.Contains("|"))
                {
                    string[] parts = v.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                    junctionValue = parts[0];
                    targetValue = parts[1];
                }
                else
                {
                    junctionValue = v;
                    targetValue = v;
                }
                JunctionPoint.Create(VARIABLE_EVALUATOR.Replace(junctionPoint, junctionValue), VARIABLE_EVALUATOR.Replace(target, targetValue), true);
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
            if(junctionVar.Count>1)
            {
                int count = -1;
                foreach (var kv in junctionVar)
                {
                    if(count==-1)
                    {
                        count = junctionVar[kv.Key].Count;
                    }
                    else
                    {
                        if(junctionVar[kv.Key].Count!=count)
                        {
                            WriteError("variables length are not same,{0}",kv.Key);
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
                JunctionPoint.Create(Evaluate(junctionPoint, node), Evaluate(target, node), true);
            }
        }

        private string Evaluate(string origin,VariableNode node)
        {
            string dest = origin;
            foreach (var key in junctionVar.Keys)
            {
                dest = dest.Replace(FormatKey(key), junctionVar[key][node.Index]);
            }
#if DEBUG
            Console.WriteLine(dest);
#endif
            return dest;
        }

        private string FormatKey(string key)
        {
            return string.Format("{{{0}}}", key);
        }

        private  void WriteError(string error,params object[] arg)
        {
            var fgcolor = Console.ForegroundColor;
            Console.ForegroundColor=ConsoleColor.Red;
            Console.WriteLine(error,arg);
            Console.ForegroundColor = fgcolor;
        }
    }
}
