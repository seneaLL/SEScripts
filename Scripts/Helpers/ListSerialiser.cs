using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEngineers.Helpers.Serialization
{
    public static class ListSerialiser {
            public static string Serialize(List<KeyValuePair<string, string>> obj)
            {
                List<KeyValuePair<string, string>> list = obj;
                StringBuilder output = new StringBuilder();
                output.Append("{\n");

                foreach (var i in list) {
                    output.Append($"\"{i.Key}\": \"{i.Value}\",\n");
                }
                output.Append("}");

                return output.ToString();
            }

            public static List<KeyValuePair<string, string>> Deserialize(string input)
            {
                List<KeyValuePair<string, string>> output = new List<KeyValuePair<string, string>>();
                input = GetOuterJsonContent(input);
                if (input.Length == 0)
                    return output;

                var length = input.Length;
                string bufKey = "";
                string bufValue = "";

                for (int i = 0; i < length; i++) {
                    if (input[i] != '"') {
                        continue;
                    }

                    if (i + 1 < length && input[i + 1] == '{') {
                        int brackets = 1;
                        for (int j = 2; j + i < length; j++) {
                            if (input[j + i] == '{')
                                brackets++;

                            if (input[j + i] != '}')
                                continue;
                            brackets--;
                            if (brackets != 0) continue;

                            var content = input.Substring(i + 1, j);
                            bufValue = content;
                            output.Add(new KeyValuePair<string, string>(bufKey, bufValue));
                            i = i + j + 2;
                            break;
                        }
                        continue;
                    }

                    for (int j = 1; j + i < length; j++) {
                        if (input[i + j] != '"') {
                            continue;
                        }

                        bool isValue;
                        var contnet = GetValueOrKey(i, i + j, input, out isValue);
                        if (!isValue) {
                            bufKey = contnet;
                            i = i + j + 1;
                            break;
                        }

                        bufValue = contnet;
                        output.Add(new KeyValuePair<string, string>(bufKey, bufValue));
                        i = i + j + 1;
                        break;
                    }
                }

                return output;
            }

            private static string GetValueOrKey(int startIndex, int endIndex, string text, out bool isValue)
            {
                var content = text.Substring(startIndex + 1, endIndex - startIndex - 1);

                if (text[endIndex + 1] == ':') {
                    isValue = false;
                    return content;
                }

                if (text[endIndex + 1] == ',') {
                    isValue = true;
                    return content;
                }

                isValue = false;
                return "";
            }

            private static string GetOuterJsonContent(string text)
            {
                int startIndex = text.IndexOf('{');
                int endIndex = text.LastIndexOf('}');

                return text.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            public static string GetValue(List<KeyValuePair<string, string>> values, string key)
            {
                return values.First(it => it.Key == key).Value;
            }
        }
}