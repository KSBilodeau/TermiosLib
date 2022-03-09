using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodingSeb.ExpressionEvaluator;

namespace TermiosLib;

public class Constants : DynamicObject
{
    private readonly string _termiosPath;
    private readonly ExpressionEvaluator _evaluator;

    public Constants(string termiosPath)
    {
        _termiosPath = termiosPath;
        _evaluator = new ExpressionEvaluator();
        ParseConstants();
    }

#nullable enable
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        object temp;
        if ((temp = _evaluator.Evaluate(binder.Name)) != null)
        {
            result = temp;
            return true;
        }

        result = null;
        return false;
    }
#nullable disable

    private void ParseConstants()
    {
        var fileInfo = new FileInfo(_termiosPath);

        using var file = fileInfo.OpenText();

        var fileString =
            Regex.Replace(file.ReadToEnd(), @"\/\*.*?\*\/|^((?!#).)*$|^.*\\.*$^\s+$[\r\n]*(?<=#) *", string.Empty,
                    RegexOptions.Multiline)
                .Replace("\t", " ");

        var stringReader = new StringReader(fileString);
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
                        _evaluator.Variables[splitLine[1]] = new SubExpression(string.Join(' ', splitLine[2..]));
                        break;
                }
            }
        }
    }
}