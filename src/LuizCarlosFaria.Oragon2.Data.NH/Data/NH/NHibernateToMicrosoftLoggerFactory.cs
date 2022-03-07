using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public class NHibernateToMicrosoftLoggerFactory : INHibernateLoggerFactory
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

    public NHibernateToMicrosoftLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public INHibernateLogger LoggerFor(string keyName)
    {
        var msLogger = this._loggerFactory.CreateLogger(keyName);
        return new NHibernateToMicrosoftLogger(msLogger);
    }

    public INHibernateLogger LoggerFor(Type type)
    {
        return this.LoggerFor(TypeNameHelper.GetTypeDisplayName(type));
    }

    public class NHibernateToMicrosoftLogger : INHibernateLogger
    {
        private readonly ILogger _msLogger;

        public NHibernateToMicrosoftLogger(ILogger msLogger)
        {
            this._msLogger = msLogger ?? throw new ArgumentNullException(nameof(msLogger));
        }

        private static readonly Dictionary<NHibernateLogLevel, LogLevel> MapLevels = new()
        {
        { NHibernateLogLevel.Trace, LogLevel.Trace },
        { NHibernateLogLevel.Debug, LogLevel.Debug },
        { NHibernateLogLevel.Info, LogLevel.Information },
        { NHibernateLogLevel.Warn, LogLevel.Warning },
        { NHibernateLogLevel.Error, LogLevel.Error },
        { NHibernateLogLevel.Fatal, LogLevel.Critical },
        { NHibernateLogLevel.None, LogLevel.None },
    };

        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
            this._msLogger.Log(MapLevels[logLevel], 0, new FormattedLogValues(state.Format, state.Args), exception, this.MessageFormatter);
        }

        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            return this._msLogger.IsEnabled(MapLevels[logLevel]);
        }

        private string MessageFormatter(FormattedLogValues state, Exception? error)
        {
            return state.ToString();
        }
    }
}

internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
{
    internal const int MaxCachedFormatters = 1024;
    private static int _count;

    private static readonly ConcurrentDictionary<string, LogValuesFormatter> _formatters = new();

    private readonly object[]? _values;

    private readonly string _originalMessage;

    private readonly LogValuesFormatter? _formatter;

    public KeyValuePair<string, object> this[int index]
    {
        get
        {
            return index < 0 || index >= this.Count
                ? throw new IndexOutOfRangeException("index")
                : index == this.Count - 1
                ? new KeyValuePair<string, object>("{OriginalFormat}", this._originalMessage)
                : this._formatter != null && this._values != null
                ? this._formatter.GetValue(this._values, index)
                : default;
        }
    }

    public int Count
    {
        get
        {
            return this._formatter == null ? 1 : this._formatter.ValueNames.Count + 1;
        }
    }

    public FormattedLogValues(string format, params object[] values)
    {
        if (values != null && values.Length != 0 && format != null)
        {
            if (_count >= 1024)
            {
                if (!_formatters.TryGetValue(format, out this._formatter))
                    this._formatter = new LogValuesFormatter(format);
            }
            else
            {
                this._formatter = _formatters.GetOrAdd(format, (string f) =>
                {
                    Interlocked.Increment(ref _count);
                    return new LogValuesFormatter(f);
                });
            }
        }
        else
        {
            this._formatter = null;
        }

        this._originalMessage = format ?? "[null]";
        this._values = values;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        int i = 0;
        while (i < this.Count)
        {
            yield return this[i];
            int num = i + 1;
            i = num;
        }
    }

    public override string ToString()
    {
        return this._formatter == null
            ? this._originalMessage
            : this._values != null
            ? this._formatter.Format(this._values)
            : string.Empty;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}

internal class LogValuesFormatter
{
    private static readonly char[] FormatDelimiters = new char[2]
    {
        ',',
        ':'
    };

    private readonly string _format;

    public string OriginalFormat
    {
        get;
    }

    public List<string> ValueNames { get; } = new();

    public LogValuesFormatter(string format)
    {
        this.OriginalFormat = format;
        StringBuilder stringBuilder = new();
        int num = 0;
        int length = format.Length;
        while (num < length)
        {
            int num2 = FindBraceIndex(format, '{', num, length);
            int num3 = FindBraceIndex(format, '}', num2, length);
            if (num3 == length)
            {
                stringBuilder.Append(format, num, length - num);
                num = length;
                continue;
            }

            int num4 = FindIndexOfAny(format, FormatDelimiters, num2, num3);
            stringBuilder.Append(format, num, num2 - num + 1);
            stringBuilder.Append(this.ValueNames.Count.ToString(CultureInfo.InvariantCulture));
            this.ValueNames.Add(format.Substring(num2 + 1, num4 - num2 - 1));
            stringBuilder.Append(format, num4, num3 - num4 + 1);
            num = num3 + 1;
        }

        this._format = stringBuilder.ToString();
    }

    private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
    {
        int result = endIndex;
        int i = startIndex;
        int num = 0;
        for (; i < endIndex; i++)
        {
            if (num > 0 && format[i] != brace)
            {
                if (num % 2 != 0)
                    break;

                num = 0;
                result = endIndex;
            }
            else
            {
                if (format[i] != brace)
                    continue;

                if (brace == '}')
                {
                    if (num == 0)
                        result = i;
                }
                else
                {
                    result = i;
                }

                num++;
            }
        }

        return result;
    }

    private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
    {
        int num = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
        return num != -1 ? num : endIndex;
    }

    public string Format(object[] values)
    {
        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = FormatArgument(values[i]);
            }
        }

        return string.Format(CultureInfo.InvariantCulture, this._format, values ?? Array.Empty<object>());
    }

    internal string Format()
    {
        return this._format;
    }

    internal string Format(object arg0)
    {
        return string.Format(CultureInfo.InvariantCulture, this._format, FormatArgument(arg0));
    }

    internal string Format(object arg0, object arg1)
    {
        return string.Format(CultureInfo.InvariantCulture, this._format, FormatArgument(arg0), FormatArgument(arg1));
    }

    internal string Format(object arg0, object arg1, object arg2)
    {
        return string.Format(CultureInfo.InvariantCulture, this._format, FormatArgument(arg0), FormatArgument(arg1), FormatArgument(arg2));
    }

    public KeyValuePair<string, object> GetValue(object[] values, int index)
    {
        return index < 0 || index > this.ValueNames.Count
            ? throw new IndexOutOfRangeException("index")
            : this.ValueNames.Count > index
            ? new KeyValuePair<string, object>(this.ValueNames[index], values[index])
            : new KeyValuePair<string, object>("{OriginalFormat}", this.OriginalFormat);
    }

    public IEnumerable<KeyValuePair<string, object>> GetValues(object[] values)
    {
        KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[values.Length + 1];
        for (int i = 0; i != this.ValueNames.Count; i++)
        {
            array[i] = new KeyValuePair<string, object>(this.ValueNames[i], values[i]);
        }

        array[^1] = new KeyValuePair<string, object>("{OriginalFormat}", this.OriginalFormat);
        return array;
    }

    private static object FormatArgument(object value)
    {
        return value == null
            ? (object)"(null)"
            : value is string
            ? value
            : value is IEnumerable enumerableValue
            ? string.Join(", ", from object o in enumerableValue
                                select o ?? "(null)")
            : value;
    }
}
