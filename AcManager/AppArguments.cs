﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AcTools.Utils.Helpers;

namespace AcManager {
    public static class AppArguments {
        private static Regex _regex;
        private static Dictionary<AppFlag, string> _args;

        public static IReadOnlyList<string> Values { get; private set; }

        public static void Initialize(IEnumerable<string> args) {
            var list = args.ToListIfItsNot();

            _args = list.TakeWhile(x => x != "-")
                .Where(x => x.StartsWith("--"))
                .Select(x => x.Split(new[] { '=' }, 2))
                .Select(x => new {
                    Key = ArgStringToFlag(x[0]),
                    Value = x.Length == 2 ? x[1] : null
                })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key.Value, x => x.Value);

            Values = list.Where(x => !x.StartsWith("-"))
                         .Union(list.SkipWhile(x => x != "-").Skip(1).Where(x => x.StartsWith("-"))).ToList();
        }

        public static void AddFromFile(string filename) {
            if (!File.Exists(filename)) return;

            foreach (var pair in File.ReadAllLines(filename).Where(x => x.StartsWith("--"))
                    .Select(x => x.Split(new[] { '=' }, 2).Select(y => y.Trim()).ToArray())
                    .Select(x => new {
                        Key = ArgStringToFlag(x[0]),
                        Value = x.Length == 2 ? x[1] : null
                    })
                    .Where(x => x.Key != null)) {
                _args[pair.Key.Value] = pair.Value;
            }
        }

        internal static string FlagToArgString(AppFlag flag) {
            return "-" + (_regex ?? (_regex = new Regex(@"[A-Z]", RegexOptions.Compiled))).Replace(flag.ToString(), x => "-" + x.Value.ToLower());
        }

        internal static AppFlag? ArgStringToFlag(string arg) {
            AppFlag result;
            var s = arg.Split('-').Where(x => x.Length > 0).Select(x => (char)(x[0] + 'A' - 'a') + (x.Length > 1 ? x.Substring(1) : "")).JoinToString();
            return Enum.TryParse(s, out result) ? result : (AppFlag?)null;
        }

        public static bool Has(AppFlag flag) {
            return _args != null && _args.ContainsKey(flag);
        }

        public static string Get(AppFlag flag) {
            return _args != null && _args.ContainsKey(flag) ? _args[flag] : null;
        }

        public static bool GetBool(AppFlag flag, bool defaultValue = false) {
            Set(flag, ref defaultValue);
            return defaultValue;
        }

        public static double GetDouble(AppFlag flag, double defaultValue = 0d) {
            Set(flag, ref defaultValue);
            return defaultValue;
        }

        public static void Set(AppFlag flag, ref bool option) {
            var value = Get(flag);
            if (value == null) {
                if (Has(flag)) {
                    option = true;
                }
                return;
            }

            if (value == "1" ||
                    string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "ok", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "y", StringComparison.OrdinalIgnoreCase)) {
                option = true;
            } else if (value == "0" ||
                    string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "no", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "not", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "n", StringComparison.OrdinalIgnoreCase)) {
                option = false;
            }
        }

        public static void Set(AppFlag flag, ref int option) {
            var value = Get(flag);
            if (value == null) return;
            option = FlexibleParser.ParseInt(value, option);
        }

        public static void Set(AppFlag flag, ref double option) {
            var value = Get(flag);
            if (value == null) return;
            option = FlexibleParser.ParseDouble(value, option);
        }

        public static void Set(AppFlag flag, ref string option) {
            var value = Get(flag);
            if (value == null) return;
            option = value;
        }
    }
}
