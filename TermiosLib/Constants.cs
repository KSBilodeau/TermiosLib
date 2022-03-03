using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TermiosLib;

public class Constants : System.Dynamic.DynamicObject
{
    private string _termiosPath;
    private Dictionary<string, nuint> _vars;

    public Constants(string termiosPath, bool useCache = true)
    {
        _termiosPath = termiosPath;
        
        if (useCache)
        {
            var cacheInfo = new FileInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.termiosLib/CACHE.json");

            if (cacheInfo.Exists)
            {
                using var file = cacheInfo.OpenText();

                var text = file.ReadToEnd();

                _vars = JsonSerializer.Deserialize<Dictionary<string, uint>>(text)?
                    .Select(x => new KeyValuePair<string, nuint>(x.Key, (nuint)x.Value))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
            else
            {
                RefreshCache();
            }
        }
        else
        {
            GenerateConstantsFresh(termiosPath);
        }
    }

    #nullable enable
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var attempt = _vars.TryGetValue(binder.Name, out nuint value);
        result = (uint)value;
        return attempt;
    }
    #nullable disable

    private void GenerateConstantsFresh(string termiosPath)
    {
        var fileInfo = new FileInfo(termiosPath);

        using var file = fileInfo.OpenText();

        var fileString =
            Regex.Replace(Regex.Replace(
                        Regex.Replace(file.ReadToEnd(), @"\/\*.*\*\/|^((?!#).)*$|^.*\\.*$", "",
                            RegexOptions.Multiline),
                        @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline), "(?<=#) *", string.Empty,
                    RegexOptions.Multiline)
                .Replace("\t", " ");

        var stringReader = new StringReader(fileString);

        var output = new StringBuilder();

        string line;
        while ((line = stringReader.ReadLine()) != null)
        {
            var splitLine = (from word in line.Split(" ")
                where word != string.Empty
                select word).ToArray();


            if (splitLine.Length != 0 && splitLine.Length > 2)
            {
                switch (splitLine[0])
                {
                    case "#if":
                    case "#error":
                        continue;
                    case "#define" when !splitLine[1].Contains("("):
                        output.Append("public const uint ")
                            .Append(splitLine[1])
                            .Append(" =  unchecked((uint) ")
                            .Append(string.Join(string.Empty, splitLine[2..]))
                            .Append(");\n");
                        break;
                }
            }
        }

        Script<object> script = CSharpScript.Create(output.ToString(), ScriptOptions.Default);

        _vars = script.RunAsync()
            .GetAwaiter()
            .GetResult()
            .Variables
            .ToDictionary(variable => variable.Name, variable => (nuint)(uint)variable.Value);
    }

    public void RefreshCache()
    {
        GenerateConstantsFresh(_termiosPath);
        CacheTo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.termiosLib/CACHE.json");
    }

    private void CacheTo(string placementPath)
    {
        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.termiosLib/"))
        {
            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.termiosLib/");
        }

        using var file = File.OpenWrite(placementPath);

        var vars = _vars.Select(val => new KeyValuePair<string, uint>(val.Key, (uint)val.Value))
            .ToDictionary(x => x.Key, y => y.Value);
        
        file.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(vars)));
    }

    public Dictionary<string, nuint> GetAllConstants()
    {
        return _vars;
    }

    public nuint? GetConstVal(string constName)
    {
        return TryGetConstVal(constName, out var constant) ? constant : null;
    }

    public bool TryGetConstVal(string constName, out nuint constant)
    {
        return _vars.TryGetValue(constName, out constant);
    }
}