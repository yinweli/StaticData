﻿using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StaticData
{
    internal class Program
    {
        private static int Main(string[] args) {
            return Start(args) ? 0 : 1;
        }

        /// <summary>
        /// 開始建立靜態資料
        /// </summary>
        /// <param name="args">參數列表</param>
        /// <returns>true表示成功, false則否</returns>
        private static bool Start(string[] args) {
            using (AutoStopwatch autoStopWatchGlobal = new AutoStopwatch("Static data builder")) {
                File.Delete(Output.errorLog);

                if (args.Length <= 0)
                    return Output.Error("must specify the setting file path");

                Setting setting = ReadSetting(args[0]);

                if (setting == null)
                    return Output.Error("setting read failed");

                if (setting.Check() == false)
                    return Output.Error("setting check failed");

                var result = true;
                List<Collection> collections = new List<Collection>();

                foreach (var itor in setting.elements) {
                    using (AutoStopwatch autoStopWatchLocal = new AutoStopwatch(itor.ToString())) {
                        Import import = new Import();

                        result &= import.Read(setting.global, itor) &&
                            ExportJson.Export(setting.global, itor, import) &&
                            ExportCsStruct.Export(setting.global, itor, import) &&
                            ExportCppStruct.Export(setting.global, itor, import);

                        collections.Add(new Collection() { settingElement = itor, import = import });
                    }//using
                }//for

                return result;
            }//using
        }

        /// <summary>
        /// 讀取設定檔
        /// </summary>
        /// <param name="path">設定檔路徑</param>
        /// <returns>true表示成功, false則否</returns>
        private static Setting ReadSetting(string path) {
            try {
                if (File.Exists(path) == false) {
                    Output.Error("setting file not exist");
                    return null;
                }//if

                var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                var document = File.ReadAllText(path);
                var input = new StringReader(document);

                return deserializer.Deserialize<Setting>(input);
            }
            catch (Exception e) {
                Output.Error(e.InnerException.Message);
                return null;
            }
        }
    }
}