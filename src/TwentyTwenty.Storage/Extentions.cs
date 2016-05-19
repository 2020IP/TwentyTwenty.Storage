using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyTwenty.Storage
{
    public static class Extentions
    {
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        public static StorageError ToStorageError(this int code)
        {
            return errors
                .Where(x => x.Key == code)
                .Select(x => new StorageError() { Code = x.Key, Message = x.Value })
                .FirstOrDefault();
        }

        public static StorageError ToStorageError(this StorageErrorCode code)
        {
            return errors
                .Where(x => x.Key == (int)code)
                .Select(x => new StorageError() { Code = x.Key, Message = x.Value })
                .FirstOrDefault();
        }

        public static List<T2> SelectToListOrEmpty<T1, T2>(this IEnumerable<T1> e, Func<T1, T2> f)
        {
            if (e == null)
            {
                return new List<T2>();
            }

            return e.Select(f).ToList();
        }

        public static List<T1> WhereToListOrEmpty<T1>(this IEnumerable<T1> e, Func<T1, bool> f)
        {
            if (e == null)
            {
                return new List<T1>();
            }

            return e.Where(f).ToList();
        }

        static Dictionary<int, string> errors = new Dictionary<int, string>
        {
            {
                1000,
                "Invalid security credentials"
            },
            {
                1001,
                "Generic provider exception occurred"
            },
            {
                1002,
                "Invalid access permissions."
            },
            {
                1003,
                "Blob in use."
            },
            {
                1004,
                "Invalid blob and container name."
            },
            {
                1005,
                "Invalid container name."
            },
            {
                1006,
                "Error opening blob."
            }
        };
    }
}