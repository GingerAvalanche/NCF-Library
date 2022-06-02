﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using Nintendo.Aamp.IO;
using Nintendo.Aamp.Parser;
using Nintendo.Aamp.Shared;
using Newtonsoft.Json;
using Syroot.BinaryData;
using System.Text;

namespace Nintendo.Aamp
{
    public enum OpenMode : uint
    {
        /// <summary>
        /// Open the file and read the text insode it.
        /// </summary>
        File = 0x00,

        /// <summary>
        /// Read the given text.
        /// </summary>
        Text = 0x01,
    }

    public class AampFile
    {
        //
        // Constructors 
        //
        #region Expand

        internal AampFile() { }
        public AampFile(string fileName) => Setter(FromBinary(File.OpenRead(fileName)));
        public AampFile(byte[] bytes) => Setter(FromBinary(new MemoryStream(bytes)));
        public AampFile(Stream stream) => Setter(FromBinary(stream));
        public AampFile(string input, OpenMode openMode)
        {
            if (openMode == OpenMode.Text)
                Setter(FromYml(input));
            if (openMode == OpenMode.Text)
                Setter(FromYml(input));
        }

        #endregion

        //
        // Internal functions
        //
        #region Expand

        private void Setter(AampFile aamp)
        {
            ParameterIOVersion = aamp.ParameterIOVersion;
            ParameterIOType = aamp.ParameterIOType;
            UnknownValue = aamp.UnknownValue;
            Endianness = aamp.Endianness;
            RootNode = aamp.RootNode;
            Version = aamp.Version;
        }

        #endregion

        // 
        // Parameters
        // 
        #region Expand

        public string EffectType => ParamEffect.GetEffectType(ParameterIOType);
        public uint Endianness { get; internal set; } = 0x01000000;
        public string ParameterIOType { get; set; }
        public uint ParameterIOVersion { get; internal set; }
        public ParamList RootNode { get; set; }
        public uint UnknownValue { get; set; } = 0x01000000;
        public uint Version { get; internal set; }

        #endregion

        // 
        // Local functions
        // 
        #region Expand

        public void ToVersion1() => Setter(ConvertToVersion1());
        internal AampFileV1 ConvertToVersion1()
        {
            return new AampFileV1() {
                Endianness = Endianness,
                ParameterIOType = ParameterIOType,
                ParameterIOVersion = 0,
                RootNode = RootNode,
                Version = 1,
                UnknownValue = 0,
                EffectName = Encoding.UTF8.GetBytes(ParameterIOType),
            };
        }

        public void ToVersion2() => Setter(ConvertToVersion2());
        internal AampFileV2 ConvertToVersion2()
        {
            return new AampFileV2() {
                Endianness = Endianness,
                ParameterIOType = ParameterIOType,
                ParameterIOVersion = 410,
                RootNode = RootNode,
                Version = 2,
                UnknownValue = 0,
            };
        }

        public byte[] ToBinary() => ToBinary(this);
        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public string ToYml() => YamlConverter.ToYaml(this);

        public void WriteBinary(string fileName) => File.WriteAllBytes(fileName, ToBinary(this));
        public void WriteJson(string fileName) => File.WriteAllText(fileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        public void WriteYaml(string fileName) => File.WriteAllText(fileName, ToYml());

        #endregion

        // 
        // Static functions
        // 
        #region Expand

        private static uint CheckVersion(Stream stream)
        {
            using FileReader reader = new(stream, true);
            reader.ByteConverter = ByteConverter.Little;
            reader.Position = 4;

            return reader.ReadUInt32();
        }

        public static AampFile FromBinary(string fileName) => FromBinary(File.OpenRead(fileName));
        public static AampFile FromBinary(byte[] bytes) => FromBinary(new MemoryStream(bytes));
        public static AampFile FromBinary(Stream stream)
        {
            uint version = CheckVersion(stream);

            if (version == 2)
                return new AampFileV2(stream);
            else
                return new AampFileV1(stream);
        }

        public static AampFile FromYml(string text) => YamlConverter.FromYaml(text);
        public static byte[] ToBinary(AampFile aampFile)
        {
            if (aampFile.Version == 2)
                return aampFile.ConvertToVersion2().CompileV2();
            else
                return aampFile.ConvertToVersion1().CompileV1();
        }

        #endregion
    }
}