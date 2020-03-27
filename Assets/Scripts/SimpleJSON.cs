/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * [2012-06-09 First Version]
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser now properly identifies types. So generating JSON with this framework should work.
 * - only double quotes (") are used for quoting strings.
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser tries to avoid errors, but if malformed JSON is parsed the result is more or less undefined
 * - It can serialize/deserialize a node tree into/from an experimental compact binary format. It might
 *   be handy if you want to store things in a file and don't want it to be easily modifiable
 * 
 * 
 * [2012-12-17 Update]
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * [2017-03-08 Update]
 * - Optimised parsing by using a StringBuilder for token. This prevents performance issues when large
 *   string data fields are contained in the json data.
 * - Finally refactored the badly named JSONClass into JSONObject.
 * - Replaced the old JSONData class by distict typed classes ( JSONString, JSONNumber, JSONBool, JSONNull ) this
 *   allows to propertly convert the node tree back to json without type information loss. The actual value
 *   parsing now happens at parsing time and not when you actually access one of the casting properties.
 * 
 * [2017-04-11 Update]
 * - Fixed parsing bug where empty string values have been ignored.
 * - Optimised "ToString" by using a StringBuilder internally. This should heavily improve performance for large files
 * - Changed the overload of "ToString(string aIndent)" to "ToString(int aIndent)"
 * 
 * [2017-11-29 Update]
 * - Removed the IEnumerator implementations on JSONArray & JSONObject and replaced it with a common
 *   struct Enumerator in JSONNode that should avoid garbage generation. The enumerator always works
 *   on KeyValuePair<string, JSONNode>, even for JSONArray.
 * - Added two wrapper Enumerators that allows for easy key or value enumeration. A JSONNode now has
 *   a "Keys" and a "Values" enumerable property. Those are also struct enumerators / enumerables
 * - A KeyValuePair<string, JSONNode> can now be implicitly converted into a JSONNode. This allows
 *   a foreach loop over a JSONNode to directly access the values only. Since KeyValuePair as well as
 *   all the Enumerators are structs, no garbage is allocated.
 * - To add Linq support another "LinqEnumerator" is available through the "Linq" property. This
 *   enumerator does implement the generic IEnumerable interface so most Linq extensions can be used
 *   on this enumerable object. This one does allocate memory as it's a wrapper class.
 * - The Escape method now escapes all control characters (# < 32) in strings as uncode characters
 *   (\uXXXX) and if the static bool JSONNode.forceASCII is set to true it will also escape all
 *   characters # > 127. This might be useful if you require an ASCII output. Though keep in mind
 *   when your strings contain many non-ascii characters the strings become much longer (x6) and are
 *   no longer human readable.
 * - The node types JSONObject and JSONArray now have an "Inline" boolean switch which will default to
 *   false. It can be used to serialize this element inline even you serialize with an indented format
 *   This is useful for arrays containing numbers so it doesn't place every number on a new line
 * - Extracted the binary serialization code into a seperate extension file. All classes are now declared
 *   as "partial" so an extension file can even add a new virtual or abstract method / interface to
 *   JSONNode and override it in the concrete type classes. It's of course a hacky approach which is
 *   generally not recommended, but i wanted to keep everything tightly packed.
 * - Added a static CreateOrGet method to the JSONNull class. Since this class is immutable it could
 *   be reused without major problems. If you have a lot null fields in your data it will help reduce
 *   the memory / garbage overhead. I also added a static setting (reuseSameInstance) to JSONNull
 *   (default is true) which will change the behaviour of "CreateOrGet". If you set this to false
 *   CreateOrGet will not reuse the cached instance but instead create a new JSONNull instance each time.
 *   I made the JSONNull constructor private so if you need to create an instance manually use
 *   JSONNull.CreateOrGet()
 * 
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2017 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleJSON
{
	public enum JsonNodeType
	{
		Array = 1,
		Object = 2,
		String = 3,
		Number = 4,
		NullValue = 5,
		Boolean = 6,
		None = 7,
		Custom = 0xFF,
	}
	public enum JsonTextMode
	{
		Compact,
		Indent
	}

	public abstract partial class JsonNode
	{
		#region Enumerators
		public struct Enumerator
		{
			private enum Type { None, Array, Object }
			private Type _type;
			private Dictionary<string, JsonNode>.Enumerator _mObject;
			private List<JsonNode>.Enumerator _mArray;
			public bool IsValid { get { return _type != Type.None; } }
			public Enumerator(List<JsonNode>.Enumerator aArrayEnum)
			{
				_type = Type.Array;
				_mObject = default(Dictionary<string, JsonNode>.Enumerator);
				_mArray = aArrayEnum;
			}
			public Enumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum)
			{
				_type = Type.Object;
				_mObject = aDictEnum;
				_mArray = default(List<JsonNode>.Enumerator);
			}
			public KeyValuePair<string, JsonNode> Current
			{
				get {
					if (_type == Type.Array)
						return new KeyValuePair<string, JsonNode>(string.Empty, _mArray.Current);
					else if (_type == Type.Object)
						return _mObject.Current;
					return new KeyValuePair<string, JsonNode>(string.Empty, null);
				}
			}
			public bool MoveNext()
			{
				if (_type == Type.Array)
					return _mArray.MoveNext();
				else if (_type == Type.Object)
					return _mObject.MoveNext();
				return false;
			}
		}
		public struct ValueEnumerator
		{
			private Enumerator _mEnumerator;
			public ValueEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
			public ValueEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
			public ValueEnumerator(Enumerator aEnumerator) { _mEnumerator = aEnumerator; }
			public JsonNode Current { get { return _mEnumerator.Current.Value; } }
			public bool MoveNext() { return _mEnumerator.MoveNext(); }
			public ValueEnumerator GetEnumerator() { return this; }
		}
		public struct KeyEnumerator
		{
			private Enumerator _mEnumerator;
			public KeyEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
			public KeyEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
			public KeyEnumerator(Enumerator aEnumerator) { _mEnumerator = aEnumerator; }
			public JsonNode Current { get { return _mEnumerator.Current.Key; } }
			public bool MoveNext() { return _mEnumerator.MoveNext(); }
			public KeyEnumerator GetEnumerator() { return this; }
		}

		public class LinqEnumerator : IEnumerator<KeyValuePair<string, JsonNode>>, IEnumerable<KeyValuePair<string, JsonNode>>
		{
			private JsonNode _mNode;
			private Enumerator _mEnumerator;
			internal LinqEnumerator(JsonNode aNode)
			{
				_mNode = aNode;
				if (_mNode != null)
					_mEnumerator = _mNode.GetEnumerator();
			}
			public KeyValuePair<string, JsonNode> Current { get { return _mEnumerator.Current; } }
			object IEnumerator.Current { get { return _mEnumerator.Current; } }
			public bool MoveNext() { return _mEnumerator.MoveNext(); }

			public void Dispose()
			{
				_mNode = null;
				_mEnumerator = new Enumerator();
			}

			public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator()
			{
				return new LinqEnumerator(_mNode);
			}

			public void Reset()
			{
				if (_mNode != null)
					_mEnumerator = _mNode.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new LinqEnumerator(_mNode);
			}
		}

		#endregion Enumerators

		#region common interface

		public static bool forceAscii = false; // Use Unicode by default

		public abstract JsonNodeType Tag { get; }

		public virtual JsonNode this[int aIndex] { get { return null; } set { } }

		public virtual JsonNode this[string aKey] { get { return null; } set { } }

		public virtual string Value { get { return ""; } set { } }

		public virtual int Count { get { return 0; } }

		public virtual bool IsNumber { get { return false; } }
		public virtual bool IsString { get { return false; } }
		public virtual bool IsBoolean { get { return false; } }
		public virtual bool IsNull { get { return false; } }
		public virtual bool IsArray { get { return false; } }
		public virtual bool IsObject { get { return false; } }

		public virtual bool Inline { get { return false; } set { } }

		public virtual void Add(string aKey, JsonNode aItem)
		{
		}
		public virtual void Add(JsonNode aItem)
		{
			Add("", aItem);
		}

		public virtual JsonNode Remove(string aKey)
		{
			return null;
		}

		public virtual JsonNode Remove(int aIndex)
		{
			return null;
		}

		public virtual JsonNode Remove(JsonNode aNode)
		{
			return aNode;
		}

		public virtual IEnumerable<JsonNode> Children
		{
			get
			{
				yield break;
			}
		}

		public IEnumerable<JsonNode> DeepChildren
		{
			get
			{
				foreach (var C in Children)
					foreach (var D in C.DeepChildren)
						yield return D;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, 0, JsonTextMode.Compact);
			return sb.ToString();
		}

		public virtual string ToString(int aIndent)
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, aIndent, JsonTextMode.Indent);
			return sb.ToString();
		}
		internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode);

		public abstract Enumerator GetEnumerator();
		public IEnumerable<KeyValuePair<string, JsonNode>> Linq { get { return new LinqEnumerator(this); } }
		public KeyEnumerator Keys { get { return new KeyEnumerator(GetEnumerator()); } }
		public ValueEnumerator Values { get { return new ValueEnumerator(GetEnumerator()); } }

		#endregion common interface

		#region typecasting properties


		public virtual double AsDouble
		{
			get
			{
				double v = 0.0;
				if (double.TryParse(Value, out v))
					return v;
				return 0.0;
			}
			set
			{
				Value = value.ToString();
			}
		}

		public virtual int AsInt
		{
			get { return (int)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual float AsFloat
		{
			get { return (float)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual bool AsBool
		{
			get
			{
				bool v = false;
				if (bool.TryParse(Value, out v))
					return v;
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				Value = (value) ? "true" : "false";
			}
		}

		public virtual JsonArray AsArray
		{
			get
			{
				return this as JsonArray;
			}
		}

		public virtual JsonObject AsObject
		{
			get
			{
				return this as JsonObject;
			}
		}


		#endregion typecasting properties

		#region operators

		public static implicit operator JsonNode(string s)
		{
			return new JsonString(s);
		}
		public static implicit operator string(JsonNode d)
		{
			return (d == null) ? null : d.Value;
		}

		public static implicit operator JsonNode(double n)
		{
			return new JsonNumber(n);
		}
		public static implicit operator double(JsonNode d)
		{
			return (d == null) ? 0 : d.AsDouble;
		}

		public static implicit operator JsonNode(float n)
		{
			return new JsonNumber(n);
		}
		public static implicit operator float(JsonNode d)
		{
			return (d == null) ? 0 : d.AsFloat;
		}

		public static implicit operator JsonNode(int n)
		{
			return new JsonNumber(n);
		}
		public static implicit operator int(JsonNode d)
		{
			return (d == null) ? 0 : d.AsInt;
		}

		public static implicit operator JsonNode(bool b)
		{
			return new JsonBool(b);
		}
		public static implicit operator bool(JsonNode d)
		{
			return (d == null) ? false : d.AsBool;
		}

		public static implicit operator JsonNode(KeyValuePair<string, JsonNode> aKeyValue)
		{
			return aKeyValue.Value;
		}

		public static bool operator ==(JsonNode a, object b)
		{
			if (ReferenceEquals(a, b))
				return true;
			bool aIsNull = a is JsonNull || ReferenceEquals(a, null) || a is JsonLazyCreator;
			bool bIsNull = b is JsonNull || ReferenceEquals(b, null) || b is JsonLazyCreator;
			if (aIsNull && bIsNull)
				return true;
			return !aIsNull && a.Equals(b);
		}

		public static bool operator !=(JsonNode a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion operators

		[ThreadStatic]
		private static StringBuilder mEscapeBuilder;
		internal static StringBuilder EscapeBuilder
		{
			get {
				if (mEscapeBuilder == null)
					mEscapeBuilder = new StringBuilder();
				return mEscapeBuilder;
			}
		}
		internal static string Escape(string aText)
		{
			var sb = EscapeBuilder;
			sb.Length = 0;
			if (sb.Capacity < aText.Length + aText.Length / 10)
				sb.Capacity = aText.Length + aText.Length / 10;
			foreach (char c in aText)
			{
				switch (c)
				{
				case '\\':
					sb.Append("\\\\");
					break;
				case '\"':
					sb.Append("\\\"");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				default:
					if (c < ' ' || (forceAscii && c > 127))
					{
						ushort val = c;
						sb.Append("\\u").Append(val.ToString("X4"));
					}
					else
						sb.Append(c);
					break;
				}
			}
			string result = sb.ToString();
			sb.Length = 0;
			return result;
		}

		static void ParseElement(JsonNode ctx, string token, string tokenName, bool quoted)
		{
			if (quoted)
			{
				ctx.Add(tokenName, token);
				return;
			}
			string tmp = token.ToLower();
			if (tmp == "false" || tmp == "true")
				ctx.Add(tokenName, tmp == "true");
			else if (tmp == "null")
				ctx.Add(tokenName, null);
			else
			{
				double val;
				if (double.TryParse(token, out val))
					ctx.Add(tokenName, val);
				else
					ctx.Add(tokenName, token);
			}
		}

		public static JsonNode Parse(string aJSON)
		{
			Stack<JsonNode> stack = new Stack<JsonNode>();
			JsonNode ctx = null;
			int i = 0;
			StringBuilder Token = new StringBuilder();
			string TokenName = "";
			bool QuoteMode = false;
			bool TokenIsQuoted = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
				case '{':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					stack.Push(new JsonObject());
					if (ctx != null)
					{
						ctx.Add(TokenName, stack.Peek());
					}
					TokenName = "";
					Token.Length = 0;
					ctx = stack.Peek();
					break;

				case '[':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}

					stack.Push(new JsonArray());
					if (ctx != null)
					{
						ctx.Add(TokenName, stack.Peek());
					}
					TokenName = "";
					Token.Length = 0;
					ctx = stack.Peek();
					break;

				case '}':
				case ']':
					if (QuoteMode)
					{

						Token.Append(aJSON[i]);
						break;
					}
					if (stack.Count == 0)
						throw new Exception("JSON Parse: Too many closing brackets");

					stack.Pop();
					if (Token.Length > 0 || TokenIsQuoted)
					{
						ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
						TokenIsQuoted = false;
					}
					TokenName = "";
					Token.Length = 0;
					if (stack.Count > 0)
						ctx = stack.Peek();
					break;

				case ':':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					TokenName = Token.ToString();
					Token.Length = 0;
					TokenIsQuoted = false;
					break;

				case '"':
					QuoteMode ^= true;
					TokenIsQuoted |= QuoteMode;
					break;

				case ',':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					if (Token.Length > 0 || TokenIsQuoted)
					{
						ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
						TokenIsQuoted = false;
					}
					TokenName = "";
					Token.Length = 0;
					TokenIsQuoted = false;
					break;

				case '\r':
				case '\n':
					break;

				case ' ':
				case '\t':
					if (QuoteMode)
						Token.Append(aJSON[i]);
					break;

				case '\\':
					++i;
					if (QuoteMode)
					{
						char C = aJSON[i];
						switch (C)
						{
						case 't':
							Token.Append('\t');
							break;
						case 'r':
							Token.Append('\r');
							break;
						case 'n':
							Token.Append('\n');
							break;
						case 'b':
							Token.Append('\b');
							break;
						case 'f':
							Token.Append('\f');
							break;
						case 'u':
							{
								string s = aJSON.Substring(i + 1, 4);
								Token.Append((char)int.Parse(
									s,
									System.Globalization.NumberStyles.AllowHexSpecifier));
								i += 4;
								break;
							}
						default:
							Token.Append(C);
							break;
						}
					}
					break;

				default:
					Token.Append(aJSON[i]);
					break;
				}
				++i;
			}
			if (QuoteMode)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return ctx;
		}

	}
	// End of JSONNode

	public partial class JsonArray : JsonNode
	{
		private List<JsonNode> _mList = new List<JsonNode>();
		private bool _inline = false;
		public override bool Inline
		{
			get { return _inline; }
			set { _inline = value; }
		}

		public override JsonNodeType Tag { get { return JsonNodeType.Array; } }
		public override bool IsArray { get { return true; } }
		public override Enumerator GetEnumerator() { return new Enumerator(_mList.GetEnumerator()); }

		public override JsonNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= _mList.Count)
					return new JsonLazyCreator(this);
				return _mList[aIndex];
			}
			set
			{
				if (value == null)
					value = JsonNull.CreateOrGet();
				if (aIndex < 0 || aIndex >= _mList.Count)
					_mList.Add(value);
				else
					_mList[aIndex] = value;
			}
		}

		public override JsonNode this[string aKey]
		{
			get { return new JsonLazyCreator(this); }
			set
			{
				if (value == null)
					value = JsonNull.CreateOrGet();
				_mList.Add(value);
			}
		}

		public override int Count
		{
			get { return _mList.Count; }
		}

		public override void Add(string aKey, JsonNode aItem)
		{
			if (aItem == null)
				aItem = JsonNull.CreateOrGet();
			_mList.Add(aItem);
		}

		public override JsonNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= _mList.Count)
				return null;
			JsonNode tmp = _mList[aIndex];
			_mList.RemoveAt(aIndex);
			return tmp;
		}

		public override JsonNode Remove(JsonNode aNode)
		{
			_mList.Remove(aNode);
			return aNode;
		}

		public override IEnumerable<JsonNode> Children
		{
			get
			{
				foreach (JsonNode N in _mList)
					yield return N;
			}
		}


		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append('[');
			int count = _mList.Count;
			if (_inline)
				aMode = JsonTextMode.Compact;
			for (int i = 0; i < count; i++)
			{
				if (i > 0)
					aSB.Append(',');
				if (aMode == JsonTextMode.Indent)
					aSB.AppendLine();

				if (aMode == JsonTextMode.Indent)
					aSB.Append(' ', aIndent + aIndentInc);
				_mList[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JsonTextMode.Indent)
				aSB.AppendLine().Append(' ', aIndent);
			aSB.Append(']');
		}
	}
	// End of JSONArray

	public partial class JsonObject : JsonNode
	{
		private Dictionary<string, JsonNode> _mDict = new Dictionary<string, JsonNode>();

		private bool _inline = false;
		public override bool Inline
		{
			get { return _inline; }
			set { _inline = value; }
		}

		public override JsonNodeType Tag { get { return JsonNodeType.Object; } }
		public override bool IsObject { get { return true; } }

		public override Enumerator GetEnumerator() { return new Enumerator(_mDict.GetEnumerator()); }


		public override JsonNode this[string aKey]
		{
			get
			{
				if (_mDict.ContainsKey(aKey))
					return _mDict[aKey];
				else
					return new JsonLazyCreator(this, aKey);
			}
			set
			{
				if (value == null)
					value = JsonNull.CreateOrGet();
				if (_mDict.ContainsKey(aKey))
					_mDict[aKey] = value;
				else
					_mDict.Add(aKey, value);
			}
		}

		public override JsonNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= _mDict.Count)
					return null;
				return _mDict.ElementAt(aIndex).Value;
			}
			set
			{
				if (value == null)
					value = JsonNull.CreateOrGet();
				if (aIndex < 0 || aIndex >= _mDict.Count)
					return;
				string key = _mDict.ElementAt(aIndex).Key;
				_mDict[key] = value;
			}
		}

		public override int Count
		{
			get { return _mDict.Count; }
		}

		public override void Add(string aKey, JsonNode aItem)
		{
			if (aItem == null)
				aItem = JsonNull.CreateOrGet();

			if (!string.IsNullOrEmpty(aKey))
			{
				if (_mDict.ContainsKey(aKey))
					_mDict[aKey] = aItem;
				else
					_mDict.Add(aKey, aItem);
			}
			else
				_mDict.Add(Guid.NewGuid().ToString(), aItem);
		}

		public override JsonNode Remove(string aKey)
		{
			if (!_mDict.ContainsKey(aKey))
				return null;
			JsonNode tmp = _mDict[aKey];
			_mDict.Remove(aKey);
			return tmp;
		}

		public override JsonNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= _mDict.Count)
				return null;
			var item = _mDict.ElementAt(aIndex);
			_mDict.Remove(item.Key);
			return item.Value;
		}

		public override JsonNode Remove(JsonNode aNode)
		{
			try
			{
				var item = _mDict.Where(k => k.Value == aNode).First();
				_mDict.Remove(item.Key);
				return aNode;
			}
			catch
			{
				return null;
			}
		}

		public override IEnumerable<JsonNode> Children
		{
			get
			{
				foreach (KeyValuePair<string, JsonNode> N in _mDict)
					yield return N.Value;
			}
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append('{');
			bool first = true;
			if (_inline)
				aMode = JsonTextMode.Compact;
			foreach (var k in _mDict)
			{
				if (!first)
					aSB.Append(',');
				first = false;
				if (aMode == JsonTextMode.Indent)
					aSB.AppendLine();
				if (aMode == JsonTextMode.Indent)
					aSB.Append(' ', aIndent + aIndentInc);
				aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
				if (aMode == JsonTextMode.Compact)
					aSB.Append(':');
				else
					aSB.Append(" : ");
				k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JsonTextMode.Indent)
				aSB.AppendLine().Append(' ', aIndent);
			aSB.Append('}');
		}

	}
	// End of JSONObject

	public partial class JsonString : JsonNode
	{
		private string _mData;

		public override JsonNodeType Tag { get { return JsonNodeType.String; } }
		public override bool IsString { get { return true; } }

		public override Enumerator GetEnumerator() { return new Enumerator(); }


		public override string Value
		{
			get { return _mData; }
			set
			{
				_mData = value;
			}
		}

		public JsonString(string aData)
		{
			_mData = aData;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append('\"').Append(Escape(_mData)).Append('\"');
		}
		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;
			string s = obj as string;
			if (s != null)
				return _mData == s;
			JsonString s2 = obj as JsonString;
			if (s2 != null)
				return _mData == s2._mData;
			return false;
		}
		public override int GetHashCode()
		{
			return _mData.GetHashCode();
		}
	}
	// End of JSONString

	public partial class JsonNumber : JsonNode
	{
		private double _mData;

		public override JsonNodeType Tag { get { return JsonNodeType.Number; } }
		public override bool IsNumber { get { return true; } }
		public override Enumerator GetEnumerator() { return new Enumerator(); }

		public override string Value
		{
			get { return _mData.ToString(); }
			set
			{
				double v;
				if (double.TryParse(value, out v))
					_mData = v;
			}
		}

		public override double AsDouble
		{
			get { return _mData; }
			set { _mData = value; }
		}

		public JsonNumber(double aData)
		{
			_mData = aData;
		}

		public JsonNumber(string aData)
		{
			Value = aData;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append(_mData);
		}
		private static bool IsNumeric(object value)
		{
			return value is int || value is uint
				|| value is float || value is double
				|| value is decimal
				|| value is long || value is ulong
				|| value is short || value is ushort
				|| value is sbyte || value is byte;
		}
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (base.Equals(obj))
				return true;
			JsonNumber s2 = obj as JsonNumber;
			if (s2 != null)
				return _mData == s2._mData;
			if (IsNumeric(obj))
				return Convert.ToDouble(obj) == _mData;
			return false;
		}
		public override int GetHashCode()
		{
			return _mData.GetHashCode();
		}
	}
	// End of JSONNumber

	public partial class JsonBool : JsonNode
	{
		private bool _mData;

		public override JsonNodeType Tag { get { return JsonNodeType.Boolean; } }
		public override bool IsBoolean { get { return true; } }
		public override Enumerator GetEnumerator() { return new Enumerator(); }

		public override string Value
		{
			get { return _mData.ToString(); }
			set
			{
				bool v;
				if (bool.TryParse(value, out v))
					_mData = v;
			}
		}
		public override bool AsBool
		{
			get { return _mData; }
			set { _mData = value; }
		}

		public JsonBool(bool aData)
		{
			_mData = aData;
		}

		public JsonBool(string aData)
		{
			Value = aData;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append((_mData) ? "true" : "false");
		}
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is bool)
				return _mData == (bool)obj;
			return false;
		}
		public override int GetHashCode()
		{
			return _mData.GetHashCode();
		}
	}
	// End of JSONBool

	public partial class JsonNull : JsonNode
	{
		static JsonNull mStaticInstance = new JsonNull();
		public static bool reuseSameInstance = true;
		public static JsonNull CreateOrGet()
		{
			if (reuseSameInstance)
				return mStaticInstance;
			return new JsonNull();
		}
		private JsonNull() { }

		public override JsonNodeType Tag { get { return JsonNodeType.NullValue; } }
		public override bool IsNull { get { return true; } }
		public override Enumerator GetEnumerator() { return new Enumerator(); }

		public override string Value
		{
			get { return "null"; }
			set { }
		}
		public override bool AsBool
		{
			get { return false; }
			set { }
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(this, obj))
				return true;
			return (obj is JsonNull);
		}
		public override int GetHashCode()
		{
			return 0;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONNull

	internal partial class JsonLazyCreator : JsonNode
	{
		private JsonNode _mNode = null;
		private string _mKey = null;
		public override JsonNodeType Tag { get { return JsonNodeType.None; } }
		public override Enumerator GetEnumerator() { return new Enumerator(); }

		public JsonLazyCreator(JsonNode aNode)
		{
			_mNode = aNode;
			_mKey = null;
		}

		public JsonLazyCreator(JsonNode aNode, string aKey)
		{
			_mNode = aNode;
			_mKey = aKey;
		}

		private void Set(JsonNode aVal)
		{
			if (_mKey == null)
			{
				_mNode.Add(aVal);
			}
			else
			{
				_mNode.Add(_mKey, aVal);
			}
			_mNode = null; // Be GC friendly.
		}

		public override JsonNode this[int aIndex]
		{
			get
			{
				return new JsonLazyCreator(this);
			}
			set
			{
				var tmp = new JsonArray();
				tmp.Add(value);
				Set(tmp);
			}
		}

		public override JsonNode this[string aKey]
		{
			get
			{
				return new JsonLazyCreator(this, aKey);
			}
			set
			{
				var tmp = new JsonObject();
				tmp.Add(aKey, value);
				Set(tmp);
			}
		}

		public override void Add(JsonNode aItem)
		{
			var tmp = new JsonArray();
			tmp.Add(aItem);
			Set(tmp);
		}

		public override void Add(string aKey, JsonNode aItem)
		{
			var tmp = new JsonObject();
			tmp.Add(aKey, aItem);
			Set(tmp);
		}

		public static bool operator ==(JsonLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return System.Object.ReferenceEquals(a, b);
		}

		public static bool operator !=(JsonLazyCreator a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return true;
			return System.Object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override int AsInt
		{
			get
			{
				JsonNumber tmp = new JsonNumber(0);
				Set(tmp);
				return 0;
			}
			set
			{
				JsonNumber tmp = new JsonNumber(value);
				Set(tmp);
			}
		}

		public override float AsFloat
		{
			get
			{
				JsonNumber tmp = new JsonNumber(0.0f);
				Set(tmp);
				return 0.0f;
			}
			set
			{
				JsonNumber tmp = new JsonNumber(value);
				Set(tmp);
			}
		}

		public override double AsDouble
		{
			get
			{
				JsonNumber tmp = new JsonNumber(0.0);
				Set(tmp);
				return 0.0;
			}
			set
			{
				JsonNumber tmp = new JsonNumber(value);
				Set(tmp);
			}
		}

		public override bool AsBool
		{
			get
			{
				JsonBool tmp = new JsonBool(false);
				Set(tmp);
				return false;
			}
			set
			{
				JsonBool tmp = new JsonBool(value);
				Set(tmp);
			}
		}

		public override JsonArray AsArray
		{
			get
			{
				JsonArray tmp = new JsonArray();
				Set(tmp);
				return tmp;
			}
		}

		public override JsonObject AsObject
		{
			get
			{
				JsonObject tmp = new JsonObject();
				Set(tmp);
				return tmp;
			}
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONLazyCreator

	public static class Json
	{
		public static JsonNode Parse(string aJSON)
		{
			return JsonNode.Parse(aJSON);
		}
	}
}