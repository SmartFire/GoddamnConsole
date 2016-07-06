using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GoddamnConsole.DataBinding
{
    internal class BindingPath
    {
        public List<BindingPathNode> Nodes { get; set; }

        public BindingPath(string path)
        {
            Nodes = PathParser.Parse(path);
        }
    }

    internal class BindingPathNode
    {
        public string Property { get; set; }
        public List<Index> Indices { get; set; } = new List<Index>();
    }

    internal enum IndexType
    {
        Path,
        String,
        Number,
        Boolean
    }

    internal class Index
    {
        public IndexType Type { get; set; }
        public List<BindingPathNode> Path { get; set; }
        public string String { get; set; }
        public decimal Number { get; set; }
        public bool Boolean { get; set; }
    }

    internal class PathParserResult<T>
    {
        public bool Succeeded;
        public T Result;

        public static PathParserResult<T> Error() => new PathParserResult<T> { Succeeded = false };
        public static PathParserResult<T> Success(T result) => new PathParserResult<T> { Succeeded = true, Result = result };
    }

    internal class PathParser
    {
        private readonly string _input;
        private int _pos;

        private PathParser(string input)
        {
            _input = input;
        }

        public static List<BindingPathNode> Parse(string input)
        {
            return new PathParser(input).ParseInternal();
        }

        public List<BindingPathNode> ParseInternal()
        {
            var result = ParseNodes();
            if (_pos != _input.Length) // eof
                throw new ParserException();
            return result.Result;
        }

        private class ParserException : Exception { }

        public PathParserResult<List<BindingPathNode>> ParseNodes()
        {
            var state = _pos;
            try
            {
                var fst = ParseNode(true);
                var list = new List<BindingPathNode> { fst.Result };
                while (true)
                {
                    try
                    {
                        ParseChar(x => x == '.');
                    }
                    catch (ParserException)
                    {
                        return PathParserResult<List<BindingPathNode>>.Success(list);
                    }
                    var node = ParseNode(false);
                    list.Add(node.Result);
                }
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<BindingPathNode> ParseNode(bool first)
        {
            var state = _pos;
            try
            {
                string id;
                try
                {
                    id = ParseIdentifier().Result;
                }
                catch (ParserException)
                {
                    id = null;
                }
                List<Index> indices;
                try
                {
                    indices = ParseIndices(id == null).Result;
                }
                catch
                {
                    indices = null;
                }
                if ((indices == null && id == null) || (!first && id == null)) throw new ParserException();
                return PathParserResult<BindingPathNode>.Success(new BindingPathNode
                {
                    Property = id,
                    Indices = indices
                });
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<List<Index>> ParseIndices(bool failOnAbsence)
        {
            var state = _pos;
            try
            {
                try
                {
                    ParseChar(x => x == '[');
                }
                catch (ParserException)
                {
                    if (failOnAbsence) throw;
                    return
                           PathParserResult<List<Index>>.Success(
                               new List<Index>());
                }
                var fst = ParseIndex();
                var list = new List<Index>
                {
                    fst.Result
                };
                while (true)
                {
                    try
                    {
                        ParseChar(x => x == ',');
                    }
                    catch (ParserException)
                    {
                        break;
                    }
                    var index = ParseIndex();
                    list.Add(index.Result);
                }
                ParseChar(x => x == ']');
                return PathParserResult<List<Index>>.Success(list);
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<Index> ParseIndex()
        {
            var state = _pos;
            try
            {
                try
                {
                    var b = ParseBoolean();
                    return PathParserResult<Index>.Success(new Index { Boolean = b.Result, Type = IndexType.Boolean });
                }
                catch { }
                try
                {
                    var h = ParseNodes();
                    return PathParserResult<Index>.Success(new Index { Path = h.Result, Type = IndexType.Path });
                }
                catch { }
                try
                {
                    var str = ParseStringLiteral();
                    return PathParserResult<Index>.Success(new Index { String = str.Result, Type = IndexType.String });
                }
                catch { }
                var n = ParseNumber();
                return PathParserResult<Index>.Success(new Index {Number = n.Result, Type = IndexType.Number});
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<bool> ParseBoolean()
        {
            var state = _pos;
            try
            {
                var id = ParseIdentifier().Result;
                if (id == "true") return PathParserResult<bool>.Success(true);
                if (id == "false") return PathParserResult<bool>.Success(false);
                throw new ParserException();
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }
        public PathParserResult<decimal> ParseNumber()
        {
            var state = _pos;
            try
            {
                var str = new StringBuilder();
                try
                {
                    ParseChar(x => x == '-');
                    str.Append('-');
                }
                catch
                {
                }
                var hasDot = false;
                try
                {
                    ParseChar(x => x == '.');
                    str.Append('.');
                    hasDot = true;
                    while (true)
                    {
                        try
                        {
                            var chr = ParseChar(char.IsDigit);
                            str.Append(chr.Result);
                        }
                        catch
                        {
                            break;
                        }
                    }
                    goto exponentPart;
                }
                catch
                {
                }
                while (true)
                {
                    try
                    {
                        var chr = ParseChar(char.IsDigit);
                        str.Append(chr.Result);
                    }
                    catch
                    {
                        break;
                    }
                }
                if (!hasDot)
                    try
                    {
                        ParseChar(x => x == '.');
                        str.Append('.');
                        while (true)
                        {
                            try
                            {
                                var chr = ParseChar(char.IsDigit);
                                str.Append(chr.Result);
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                exponentPart:
                try
                {
                    ParseChar(x => x == 'e' || x == 'E');
                    str.Append('e');
                    try
                    {
                        ParseChar(x => x == '-');
                        str.Append('-');
                    }
                    catch { }
                    while (true)
                    {
                        try
                        {
                            var chr = ParseChar(char.IsDigit);
                            str.Append(chr.Result);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                catch { }
                decimal number;
                if (!decimal.TryParse(str.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number)) throw new ParserException();
                return PathParserResult<decimal>.Success(number);
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<string> ParseStringLiteral()
        {
            var state = _pos;
            try
            {

                var delim = ParseChar(x => x == '"' || x == '\'').Result;
                var str = new StringBuilder();
                var escape = false;
                const string hexLetters = "abcdef";
                Predicate<char> isHex = c => char.IsDigit(c) || hexLetters.IndexOf(char.ToLower(c)) > 0;
                while (true)
                {
                    var chr =
                        ParseChar(x => true).Result;
                    if (escape)
                    {
                        switch (chr)
                        {
                            case '\\':
                                str.Append('\\');
                                break;
                            case '\'':
                                str.Append('\'');
                                break;
                            case '"':
                                str.Append('"');
                                break;
                            case 'n':
                                str.Append('\n');
                                break;
                            case 'r':
                                str.Append('\r');
                                break;
                            case 't':
                                str.Append('\t');
                                break;
                            case 'x':
                                var x1 = ParseChar(isHex);
                                var x2 = ParseChar(isHex);
                                str.Append((char)int.Parse($"{x1}{x2}", NumberStyles.HexNumber));
                                break;
                            case 'u':
                                var u1 = ParseChar(isHex);
                                var u2 = ParseChar(isHex);
                                var u3 = ParseChar(isHex);
                                var u4 = ParseChar(isHex);
                                str.Append((char)int.Parse($"{u1}{u2}{u3}{u4}", NumberStyles.HexNumber));
                                break;
                            default:
                                throw new ParserException();
                        }
                        escape = false;
                    }
                    else if (chr == delim) return PathParserResult<string>.Success(str.ToString());
                    else if (chr == '\\') escape = true;
                    else str.Append(chr);
                }
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<string> ParseIdentifier()
        {
            var state = _pos;
            try
            {
                var first = ParseChar(x => char.IsLetter(x) || x == '_');
                var sb = new StringBuilder();
                sb.Append(first.Result);
                while (true)
                {
                    try
                    {
                        var chr = ParseChar(x => char.IsLetterOrDigit(x) || x == '_');
                        sb.Append(chr.Result);
                    }
                    catch (ParserException)
                    {
                        break;
                    }
                }
                return PathParserResult<string>.Success(sb.ToString());
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }

        public PathParserResult<char> ParseChar(Predicate<char> predicate)
        {
            var state = _pos;
            try
            {
                if (_pos == _input.Length) throw new ParserException();
                var ch = _input[_pos++];
                var succ = predicate(ch);
                if (!succ) throw new ParserException();
                return PathParserResult<char>.Success(ch);
            }
            catch (ParserException)
            {
                _pos = state;
                throw;
            }
        }
    }
}
