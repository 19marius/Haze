using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Text;
using System.Net;
using System.IO;
using Haze.Keys;

namespace System.Linq.Extensions
{
    /// <summary>
    /// Provides <see langword="static"/> methods useful for manipulation of <see cref="byte"/>[] objects.
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Removes all specified trailing and leading bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] Trim(this byte[] bytes, byte element)
        {
            bytes = bytes.TrimEnd(element);
            bytes = bytes.TrimStart(element);
            return bytes;
        }

        /// <summary>
        /// Removes all specified trailing bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] TrimEnd(this byte[] bytes, byte element)
        {
            //Declarations
            int removals = 0;

            //Check from the end to the start until element doesn't match
            for (int i = bytes.Length - 1; i > 0; i--)
            {
                if (bytes[i] != element) break;
                removals++;
            }

            Array.Resize(ref bytes, bytes.Length - removals);
            return bytes;
        }

        /// <summary>
        /// Removes all specified leading bytes from a <see cref="byte"/>[].
        /// </summary>
        public static byte[] TrimStart(this byte[] bytes, byte element)
        {
            return bytes.SkipWhile(x => x == element).ToArray();
        }

        /// <summary>
        /// Serializes an <see cref="object"/> to a <see cref="byte"/>[], which can be chosen to be encrypted.
        /// </summary>
        public static byte[] Serialize(this object obj, bool encrypt)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);

                //Encrypt adding only 3 control arguments, one more serialization key and two strings
                return !encrypt ? stream.ToArray() : stream.ToArray().Encrypt(new SerializationKey("wSKp69tArKcntPXp", "th6JuCYS9MHlPF7H", "L4uO4JWO0SjiV4gz"));
            }
        }

        /// <summary>
        /// Deserializes a <see cref="byte"/>[] and returns the <see cref="object"/> serialized into it.
        /// </summary>
        public static object Deserialize(this byte[] bytes, bool encrypted)
        {
            using (var stream = new MemoryStream(encrypted ? bytes.Decrypt(new SerializationKey("wSKp69tArKcntPXp", "th6JuCYS9MHlPF7H", "L4uO4JWO0SjiV4gz")) : bytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new BinaryFormatter().Deserialize(stream);
            }
        }

        /// <summary>
        /// Creates a <see cref="byte"/>[] of length <paramref name="length"/> from <paramref name="ptr"/>.
        /// </summary>
        public static byte[] FromIntPtr(this IntPtr ptr, int length)
        {
            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);

            return bytes;
        }

        /// <summary>
        /// Creates an <see cref="IntPtr"/> from <paramref name="bytes"/>.
        /// </summary>
        public static IntPtr ToIntPtr(this byte[] bytes)
        {
            var ptr = new IntPtr();
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            return ptr;
        }

        /// <summary>
        /// Encrypts a <see cref="byte"/>[] using the AES cipher with a specified key and IV.
        /// </summary>
        public static byte[] Encrypt(this byte[] bytes, Key key)
        {
            byte[] skey = key.Serialize(false),

            //Get the last bytes from the key's header
                   ivBytes = skey.Length >= 169 ? skey.Take(105).Skip(89).ToArray() : skey.Take(54).Skip(38).ToArray(),

            //Get 3 groups of bytes from the key's content
                   keyBytes = (skey = skey.Skip(skey.Length >= 169 ? 105 : 54).ToArray()).Take(8).Concat(skey.Skip(24).Take(8)).Concat(skey.Skip(skey.Length - 16)).ToArray();

            //Initialize AES object
            using (Aes aes = Aes.Create())
            {
                //Assign properties
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;

                //Encrypt byte array
                using (var stream = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(bytes, 0, bytes.Length);
                        crypto.FlushFinalBlock();
                    }

                    return stream.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Decrypts a <see cref="byte"/>[] using the AES cipher from a specified key and IV.
        /// </summary>
        public static byte[] Decrypt(this byte[] bytes, Key key)
        {
            byte[] skey = key.Serialize(false),

            //Get the last bytes from the key's header
                   ivBytes = skey.Length >= 169 ? skey.Take(105).Skip(89).ToArray() : skey.Take(54).Skip(38).ToArray(),

            //Get 3 groups of bytes from the key's content
                   keyBytes = (skey = skey.Skip(skey.Length >= 169 ? 105 : 54).ToArray()).Take(8).Concat(skey.Skip(24).Take(8)).Concat(skey.Skip(skey.Length - 16)).ToArray();

            //Initialize AES object
            using (Aes aes = Aes.Create())
            {
                //Assign properties
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;

                //Decrypt byte array
                using (var stream = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(bytes, 0, bytes.Length);
                        crypto.FlushFinalBlock();
                    }

                    return stream.ToArray().TrimEnd(0);
                }
            }
        }

        /// <summary>
        /// Returns a combination of <paramref name="length"/> bytes from permutations of <paramref name="bytes"/>.
        /// </summary>
        public static byte[] Permute(this byte[] bytes, int length)
        {
            return Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(bytes).Permute(length));
        }
    }

    /// <summary>
    /// Provides <see langword="static"/> methods that extend the functionality provided by the <see cref="Enumerable"/> class.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Performs an action for each element of an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>The initial <see cref="IEnumerable{T}"/> to allow other operations on it.</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable) action(item);
            return enumerable;
        }

        /// <summary>
        /// Resizes an <see cref="IEnumerable{T}"/> to the specified size.
        /// <para>
        /// If the <paramref name="size"/> is bigger than the length of <paramref name="enumerable"/>, <paramref name="enumerable"/> is returned.
        /// </para>
        /// </summary>
        public static IEnumerable<T> Shrink<T>(this IEnumerable<T> enumerable, int size)
        {
            return enumerable.Where((x, i) => i < size);
        }
    }

    /// <summary>
    /// Provides static methods which allow better manipulation of the <see cref="string"/> type.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Returns a combination of <paramref name="length"/> characters from permutations of <paramref name="str"/>.
        /// </summary>
        public static string Permute(this string str, int length)
        {
            //Declare the final string
            string result = str;

            //Reset i after surpassing string length
            for (int i = 1; result.Length <= length; i = i + 1 > str.Length ? 1 : i + 1) result += str.Substring(str.Length - i, i) + str.Substring(0, str.Length - i);

            //Ensure the length
            return result.Substring(0, length);
        }

        /// <summary>
        /// Splits <paramref name="str"/> into a sequence of elements based on a <paramref name="separator"/>.
        /// <para>
        /// Any characters that match <paramref name="separator"/> that are found inside a pair from <paramref name="pairingChars"/> are ignored. Any character that is part of <paramref name="pairingChars"/> can be escaped.
        /// </para>
        /// </summary>
        public static string[] SplitSequence(this string str, string separator, Dictionary<char, char> pairingChars)
        {
            var sections = new List<string>();

            //Get the indexes of the separators in the string
            var separations = Regex.Matches(str, separator).Cast<Match>().Select(x => x.Index).ToHashSet();

            //Track state for each pair & detect pair enders
            var pairDict = pairingChars.ToDictionary(x => x, x => 0);
            var invPairs = pairingChars.Invert();

            //Each section will increase this
            int currentIndex = 0;
            for (int i = 0; i < str.Length; i++)
            {
                //Check for pair characters
                if ((pairingChars.ContainsKey(str[i]) || invPairs.ContainsKey(str[i])) && (i < 1 || str[i - 1] != '\\'))
                {
                    var pair = pairingChars.ContainsKey(str[i]) ? new KeyValuePair<char, char>(str[i], pairingChars[str[i]]) : new KeyValuePair<char, char>(invPairs[str[i]], str[i]);
                   
                    //If the character is a pair ender, lower the state for that pair (same character pairs cannot be nested, so their state cannot pass 1)
                    pairDict[pair] = pairingChars.ContainsKey(str[i]) ? (invPairs.ContainsKey(str[i]) && pairDict[pair] >= 1 ? --pairDict[pair] : ++pairDict[pair]) : --pairDict[pair];
                }

                //Check for separator & if the current index is not in a pair
                if (separations.Contains(i) && pairDict.Values.Sum() == 0)
                {
                    //Ensure substring length such that it is always greater than 0 and doesn't include seaparator
                    sections.Add(str.Substring(currentIndex, i - currentIndex));
                    currentIndex = i + separator.Length;
                }
            }

            //Add the last section
            sections.Add(str.Substring(currentIndex, str.Length - currentIndex));
            return sections.ToArray();
        }

        /// <summary>
        /// Uncapitalizes the first character in a <see cref="string"/>.
        /// </summary>
        public static string LowerFirst(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Capitalizes the first character in a <see cref="string"/>.
        /// </summary>
        public static string UpperFirst(this string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }

    /// <summary>
    /// Provides static methods useful for manipulating and obtaining local and public addresses.
    /// </summary>
    public static class IPAddressHelper
    {
        /// <summary>
        /// Gets the machine's local <see cref="IPAddress"/>.
        /// </summary>
        public static IPAddress GetLocalAddress()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).First();
        }

        /// <summary>
        /// Gets the public <see cref="IPAddress"/> assigned to the router.
        /// <para>
        /// This method may block the calling thread.
        /// </para>
        /// </summary>
        public static IPAddress GetPublicAddress()
        {
            //Try getting html from address until successful
            using (var client = new WebClient()) while (true)
                    try
                    {
                        return IPAddress.Parse(Regex.Match(
                                               client.DownloadString(new Uri("http://checkip.dyndns.org/")),
                                                                     @"(?<=Current IP Address: )[\d.]+(?=<)").Value);
                    }
                    catch
                    { }
        }

        /// <summary>
        /// Asynchronously gets the public <see cref="IPAddress"/> assigned to the router.
        /// <para>
        /// Being asynchronous, this method won't block the calling thread.
        /// </para>
        /// </summary>
        public static async Task<IPAddress> GetPublicAddressAsync()
        {
            //Try getting html from address until successful
            using (var client = new WebClient()) while (true)
                    try
                    {
                        return IPAddress.Parse(Regex.Match(
                                               client.DownloadString(new Uri("http://checkip.dyndns.org/")),
                                                                     @"(?<=Current IP Address: )[\d.]+(?=<)").Value);
                    }
                    catch
                    { await Task.Delay(1); }
        }
    }

    /// <summary>
    /// Provides static methods useful for manipulating <see cref="ObservableCollection{T}"/> objects while preserving their events' invocation lists.
    /// </summary>
    public static class ObservableHelper
    {
        /// <summary>
        /// Resizes a <see cref="ObservableCollection{T}"/> to a specified length.
        /// <para>
        /// The invocation list of the <see cref="ObservableCollection{T}.CollectionChanged"/> reamins unchanged.
        /// </para>
        /// </summary>
        public static ObservableCollection<T> Resize<T>(this ObservableCollection<T> collection, int size)
        {
            //Get the invocation list of the collection's changed event
            var invocationList = ((Delegate)typeof(ObservableCollection<T>).GetField("CollectionChanged", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(collection))?.GetInvocationList()?.Select(x => (NotifyCollectionChangedEventHandler)x)?.ToArray();

            //Reassign the collection & add the delegates to the event
            collection = new ObservableCollection<T>(collection.Shrink(size));
            for (int i = 0; i < (invocationList?.Length ?? 0); i++) collection.CollectionChanged += invocationList[i];
            return collection;
        }
    }

    /// <summary>
    /// Provides static methods useful for dealing with the limitations of conditional expressions.
    /// </summary>
    public static class ConditionalHelper
    {
        /// <summary>
        /// Represents a switch statement which can function for non-constants.
        /// </summary>
        public static void Switch<T>(T value, bool breakSwitch, Action defaultAction, Dictionary<T, Action> conditions)
        {
            foreach (var condition in conditions)
            {
                if (value.Equals(condition.Key))
                {
                    condition.Value();
                    if (breakSwitch) return;
                }
            }

            if (defaultAction != null) defaultAction();
        }
    }

    /// <summary>
    /// Provides static methods useful for simplifying asynchronous operations.
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Returns a <see cref="TaskAwaiter"/> for an <see cref="int"/>. It is the equivalent of the <see cref="Task.GetAwaiter()"/> of the <see cref="Task.Delay(int)"/> method.
        /// </summary>
        public static TaskAwaiter GetAwaiter(this int n)
        {
            return Task.Delay(n).GetAwaiter();
        }
    }

    /// <summary>
    /// Provides static methods useful for acquiring time information.
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        public static DateTime GetUtcTime()
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B;

            var addresses = Dns.GetHostEntry("time.windows.com").AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            //NTP uses UDP
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, 40);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //UTC time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime;

            uint SwapEndianness(ulong x)
            {
                return (uint)(((x & 0x000000ff) << 24) +
                               ((x & 0x0000ff00) << 8) +
                               ((x & 0x00ff0000) >> 8) +
                               ((x & 0xff000000) >> 24));
            }
        }
    }

    /// <summary>
    /// Provides static methods which provide additional functionality to the <see cref="Type"/> class.
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Returns the type that called this method, or, if <paramref name="self"/> is <see langword="false"/>, the type that called the method where this method was called.
        /// </summary>
        public static Type GetCallingType(bool self = true)
        {
            //Get the type that called this method
            var type = new StackTrace().GetFrames().Where(x => !(x.GetMethod().Name == "MoveNext" && Attribute.IsDefined(x.GetMethod().DeclaringType, typeof(CompilerGeneratedAttribute))) && !(x.GetMethod().Name == "Start" && Regex.IsMatch(x.GetMethod().DeclaringType.Name, @"Async.+MethodBuilder"))).Skip(self ? 0 : 1).First().GetMethod().DeclaringType;

            //Get the actual type name from compiler generated types if necessary
            return type.Assembly.GetType(Regex.Replace(type.FullName, @"\+.*$", ""));
        }

        /// <summary>
        /// Returns the type at the specified frame, starting from the caller of this method, which is frame 1.
        /// </summary>
        public static Type GetCallingType(uint level)
        {
            //Get the type that called this method
            var type = new StackTrace().GetFrames().Where(x => !(x.GetMethod().Name == "MoveNext" && Attribute.IsDefined(x.GetMethod().DeclaringType, typeof(CompilerGeneratedAttribute))) && !(x.GetMethod().Name == "Start" && Regex.IsMatch(x.GetMethod().DeclaringType.Name, @"Async.+MethodBuilder"))).Skip((int)level).First().GetMethod().DeclaringType;

            //Get the actual type name from compiler generated types if necessary
            return type.Assembly.GetType(Regex.Replace(type.FullName, @"\+.*$", ""));
        }

        /// <summary>
        /// Returns a type's methods which represent overloaded operators.
        /// </summary>
        public static MethodInfo[] GetOperatorMethods(this Type t)
        {
            return t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.Name.StartsWith("op_")).ToArray();
        }

        /// <summary>
        /// Parses a string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public static T Parse<T>(string str)
        {
            //If parsing to string, then just remove the quotation marks
            if (typeof(T) == typeof(string)) return Regex.IsMatch(str, @"^"".+""$") ? (T)(object)str.Substring(1, str.Length - 2) : (T)(object)str;

            //Check for arrays or enums
            if (typeof(T).IsArray || typeof(T).IsEnum)
                return typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), str) : 
                       (T)typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)
                                            .MakeGenericMethod(typeof(T).GetElementType())
                                            .Invoke(null, new object[] { typeof(Enumerable)
                                                                         .GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
                                                                         .MakeGenericMethod(typeof(T).GetElementType())
                                                                         .Invoke(null, new object[] 
                                                                         {
                                                                            str.Substring(1, str.Length - 2)
                                                                               .SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })
                                                                               .Select(x => typeof(TypeHelper)
                                                                               .GetMethod("Parse", BindingFlags.Static | BindingFlags.Public)
                                                                               .MakeGenericMethod(typeof(T).GetElementType())
                                                                               .Invoke(null, new object[] { x }))
                                                                         }
                                            )});

            //Get the parse method
            var parseMethod = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });
            if (parseMethod is null) throw new InvalidDataException($"{typeof(T).Name} cannot be parsed from a string.");

            //Return the methods result
            return (T)parseMethod.Invoke(null, new object[] { str });
        }

        /// <summary>
        /// Parses a string to an object of type <paramref name="type"/>.
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public static object Parse(Type type, string str)
        {
            //If parsing to string, then just remove the quotation marks
            if (type == typeof(string)) return Regex.IsMatch(str, @"^"".+""$") ? str.Substring(1, str.Length - 2) : str;

            //Check for arrays or enums
            if (type.IsArray || type.IsEnum)
                return type.IsEnum ? Enum.Parse(type, str) :
                       typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)
                                            .MakeGenericMethod(type.GetElementType())
                                            .Invoke(null, new object[] { typeof(Enumerable)
                                                                         .GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
                                                                         .MakeGenericMethod(type.GetElementType())
                                                                         .Invoke(null, new object[]
                                                                         {
                                                                            str.Substring(1, str.Length - 2)
                                                                               .SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })
                                                                               .Select(x => typeof(TypeHelper)
                                                                               .GetMethod("Parse", BindingFlags.Static | BindingFlags.Public)
                                                                               .MakeGenericMethod(type.GetElementType())
                                                                               .Invoke(null, new object[] { x }))
                                                                         }
                                            )});

            //Get the parse method
            var parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) });
            if (parseMethod is null) throw new InvalidDataException($"{type.Name} cannot be parsed from a string.");

            //Return the methods result
            return parseMethod.Invoke(null, new object[] { str });
        }

        /// <summary>
        /// Detects what <see cref="Type"/> <paramref name="str"/> resembles.
        /// <para>
        /// Only works for types that have a known constant representation.
        /// </para>
        /// </summary>
        public static Type DetectType(string str)
        {
            if (str.Length == 0) return null;

            //Array
            if (str[0].Equals('{')) return DetectType(str.Substring(1).SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } })[0]).MakeArrayType();

            //String
            if (str[0].Equals('"')) return typeof(string);

            //Char
            if (str[0].Equals('\'')) return typeof(char);

            //Bool
            if (str.Equals("true") || str.Equals("false")) return typeof(bool);

            //Double
            if (Regex.IsMatch(str, @"^-?(\d+\.(\d{9,16})|\d+(\.\d+)?d)", RegexOptions.IgnoreCase)) return typeof(double);

            //Float
            if (Regex.IsMatch(str, @"^-?(\d+\.(\d{1,8})|\d+(\.\d+)?f)", RegexOptions.IgnoreCase)) return typeof(float);

            //Long
            if (Regex.IsMatch(str, @"^-?(\d{11,19}|\d+l)", RegexOptions.IgnoreCase)) return typeof(long);

            //Int
            if (Regex.IsMatch(str, @"^-?\d{1,10}")) return typeof(int);

            //TimeSpan
            if (Regex.IsMatch(str, @"^(\d{1,2}\:){0,2}\d{1,2}(\.\d+)?")) return typeof(TimeSpan);

            //DateTime
            if (Regex.IsMatch(str, @"^(1[012]|[1-9])[./]([12]?[0-9]|3[01])[./][0-9]+")) return typeof(DateTime);

            //Default type is string
            return typeof(string);
        }
    }

    /// <summary>
    /// Provides static methods that simplify the interaction with an object's events.
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// Clears the invocation list of an object's event
        /// </summary>
        public static void ClearInvocationList(object instance, string eventName)
        {
            instance.GetType().GetField(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).SetValue(instance, null);
        }
    }

    /// <summary>
    /// Provides static methods that extend the functionality of a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// Combines two dictionaries together.
        /// </summary>
        public static Dictionary<TKey, TValue> Combine<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> second)
        {
            foreach (var kvp in second) dict.Add(kvp.Key, kvp.Value);
            return dict;
        }
    
        /// <summary>
        /// Inverts a <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(x => x.Value, x => x.Key);
        }
    }

    /// <summary>
    /// Provides static method that perform additional mathematical functions.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Clamps the value of <paramref name="x"/> inbetween <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="min"/> is larger than <paramref name="max"/>.
        /// </exception>
        public static long Clamp(this long x, long min, long max)
        {
            if (min > max) throw new ArgumentException("Minimum value cannot be higher than the maximum value.");
            return x < min ? min : x > max ? max : x;
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> to be smaller or equal to <paramref name="limit"/>, or, if <paramref name="min"/> is <see langword="true"/>, clamps the value to be higher or equal to <paramref name="limit"/>.
        /// </summary>
        public static long Clamp(this long x, long limit, bool min)
        {
            return min ? (x < limit ? limit : x) : (x > limit ? limit : x);
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> inbetween <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="min"/> is larger than <paramref name="max"/>.
        /// </exception>
        public static int Clamp(this int x, int min, int max)
        {
            if (min > max) throw new ArgumentException("Minimum value cannot be higher than the maximum value.");
            return x < min ? min : x > max ? max : x;
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> to be smaller or equal to <paramref name="limit"/>, or, if <paramref name="min"/> is <see langword="true"/>, clamps the value to be higher or equal to <paramref name="limit"/>.
        /// </summary>
        public static int Clamp(this int x, int limit, bool min)
        {
            return min ? (x < limit ? limit : x) : (x > limit ? limit : x);
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> inbetween <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="min"/> is larger than <paramref name="max"/>.
        /// </exception>
        public static double Clamp(this double x, double min, double max)
        {
            if (min > max) throw new ArgumentException("Minimum value cannot be higher than the maximum value.");
            return x < min ? min : x > max ? max : x;
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> to be smaller or equal to <paramref name="limit"/>, or, if <paramref name="min"/> is <see langword="true"/>, clamps the value to be higher or equal to <paramref name="limit"/>.
        /// </summary>
        public static double Clamp(this double x, double limit, bool min)
        {
            return min ? (x < limit ? limit : x) : (x > limit ? limit : x);
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> inbetween <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="min"/> is larger than <paramref name="max"/>.
        /// </exception>
        public static float Clamp(this float x, float min, float max)
        {
            if (min > max) throw new ArgumentException("Minimum value cannot be higher than the maximum value.");
            return x < min ? min : x > max ? max : x;
        }

        /// <summary>
        /// Clamps the value of <paramref name="x"/> to be smaller or equal to <paramref name="limit"/>, or, if <paramref name="min"/> is <see langword="true"/>, clamps the value to be higher or equal to <paramref name="limit"/>.
        /// </summary>
        public static float Clamp(this float x, float limit, bool min)
        {
            return min ? (x < limit ? limit : x) : (x > limit ? limit : x);
        }
    }

    /// <summary>
    /// Provides static methods useful for performing operations on ADO.NET types.
    /// </summary>
    public static class DataHelper
    {
        /// <summary>
        /// Creates a <see cref="DataTable"/> with implicit column types from an <see cref="object"/>[][].
        /// </summary>
        public static DataTable CreateTable(object[][] tableArr)
        {
            var table = new DataTable();

            //Add columns
            for (int i = 0; i < tableArr[0].Length; i++) table.Columns.Add(null, tableArr[0][i].GetType());

            //Add rows
            for (int i = 0; i < tableArr.Length; i++) table.Rows.Add(tableArr[i]);

            return table;
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> with explicit column types from an <see cref="object"/>[][].
        /// </summary>
        public static DataTable CreateTable(object[][] tableArr, params string[] columnNames)
        {
            var table = new DataTable();

            //Add columns
            for (int i = 0; i < columnNames.Length; i++) table.Columns.Add(null, columnNames[i].GetType());

            //Add rows
            for (int i = 0; i < tableArr.Length; i++) table.Rows.Add(tableArr[i]);

            return table;
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> with explicit column types from an <see cref="object"/>[][].
        /// </summary>
        public static DataTable CreateTable(object[][] tableArr, params Type[] columnTypes)
        {
            var table = new DataTable();

            //Add columns
            for (int i = 0; i < columnTypes.Length; i++) table.Columns.Add(null, columnTypes[i].GetType());

            //Add rows
            for (int i = 0; i < tableArr.Length; i++) table.Rows.Add(tableArr[i]);

            return table;
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> with named columns and explicit column types from an <see cref="object"/>[][].
        /// </summary>
        public static DataTable CreateTable(object[][] tableArr, string[] columnNames, params Type[] columnTypes)
        {
            var table = new DataTable();

            //Add columns
            for (int i = 0; i < columnTypes.Length; i++) table.Columns.Add(columnNames[i], columnTypes[i].GetType());

            //Add rows
            for (int i = 0; i < tableArr.Length; i++) table.Rows.Add(tableArr[i]);

            return table;
        }
    }
}