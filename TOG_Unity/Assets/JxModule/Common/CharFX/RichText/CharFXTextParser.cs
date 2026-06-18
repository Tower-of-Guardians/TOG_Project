using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JxModule.CharFX
{
    public static class CharFXTextParser
    {
        private readonly struct OpenTag
        {
            public readonly CharFXTagData Tag;
            public readonly int StartIndex;

            public OpenTag(CharFXTagData tag, int startIndex)
            {
                Tag = tag;
                StartIndex = startIndex;
            }
        }

        public static CharFXParseResult Parse(string source)
        {
            var result = new CharFXParseResult();
            var output = new StringBuilder();
            var stack = new Stack<OpenTag>();

            if (string.IsNullOrEmpty(source))
            {
                result.Text = string.Empty;
                return result;
            }

            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];

                if (ch == '<')
                {
                    var closeIndex = source.IndexOf('>', i);

                    if (closeIndex >= 0)
                    {
                        var tagBody = source.Substring(i + 1, closeIndex - i - 1).Trim();

                        if (TryParseOpenTag(tagBody, out var openTag))
                        {
                            stack.Push(new OpenTag(openTag, GetVisibleCharIndex(output.ToString())));
                            i = closeIndex;
                            continue;
                        }

                        if (TryParseCloseTag(tagBody, out var closeTagName))
                        {
                            CloseTag(stack, result, closeTagName, GetVisibleCharIndex(output.ToString()));
                            i = closeIndex;
                            continue;
                        }
                    }
                }

                output.Append(ch);
            }
            
            result.Text = output.ToString();
            return result;
        }

        private static bool TryParseOpenTag(string tagBody, out CharFXTagData tag)
        {
            tag = default;

            if (string.IsNullOrWhiteSpace(tagBody))
            {
                return false;
            }

            if (tagBody.StartsWith("/"))
            {
                return false;
            }

            string name;
            string argBody = null;

            var equalIndex = tagBody.IndexOf('=');
            if(equalIndex >= 0)
            {
                name = tagBody[..equalIndex].Trim();
                argBody = tagBody[(equalIndex + 1)..].Trim();
            }
            else
            {
                name = tagBody.Trim();
            }

            if(!IsSupportedTag(name))
                return false;

            var args = ParseArguments(argBody);
            tag = new CharFXTagData(name, args);
            return true;
        }

        private static bool TryParseCloseTag(string tagBody, out string tagName)
        {
            tagName = null;

            if (string.IsNullOrWhiteSpace(tagBody))
            {
                return false;
            }

            if (!tagBody.StartsWith("/"))
            {
                return false;
            }
            
            tagName = tagBody[1..].Trim().ToLowerInvariant();
            return IsSupportedTag(tagName);
        }
        
        private static bool IsSupportedTag(string name)
        {
            return name switch
            {
                "wave" or "shake" or "pop" or "bounce" or 
                "wiggle" or "pulse" or "float" or "twist" or 
                "swing" or "rainbow" or "color" or "fade" => true,
                _ => false
            };
        }

        private static List<string> ParseArguments(string argBody)
        {
            var args = new List<string>();

            if (string.IsNullOrWhiteSpace(argBody))
            {
                return args;
            }
            
            var splitResult = argBody.Split(',');
            args.AddRange(splitResult.Select(t => t.Trim()));

            return args;
        }

        private static void CloseTag(Stack<OpenTag> stack, CharFXParseResult result, string closeTagName, int currentVisibleIndex)
        {
            if (stack.Count == 0)
            {
                return;
            }

            var temp = new Stack<OpenTag>();

            while(stack.Count > 0)
            {
                var open = stack.Pop();

                if(open.Tag.Name == closeTagName)
                {
                    var start = open.StartIndex;
                    var end = currentVisibleIndex - 1;

                    if(end >= start)
                    {
                        result.Ranges.Add(new CharFXTagRange(open.Tag, start, end));
                    }

                    while (temp.Count > 0)
                    {
                        stack.Push(temp.Pop());
                    }

                    return;
                }

                temp.Push(open);
            }

            while (temp.Count > 0)
            {
                stack.Push(temp.Pop());
            }
        }

        private static int GetVisibleCharIndex(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var count = 0;
            var inTag = false;

            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '<':
                        inTag = true;
                        continue;
                    
                    case '>':
                        inTag = false;
                        continue;
                }

                if (inTag)
                {
                    continue;
                }

                count++;
            }

            return count;
        }
    }
}