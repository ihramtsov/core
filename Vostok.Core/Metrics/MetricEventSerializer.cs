﻿using System;
using System.Collections.Generic;
using System.IO;
using Vostok.Airlock;
using Vostok.Commons.Binary;

namespace Vostok.Metrics
{
    public class MetricEventSerializer :
        IAirlockSerializer<MetricEvent>,
        IAirlockDeserializer<MetricEvent>
    {
        private const byte FormatVersion = 1;

        public void Serialize(MetricEvent item, IAirlockSink sink)
        {
            sink.Writer.Write(FormatVersion);
            sink.Writer.Write(item.Timestamp.Ticks);

            sink.Writer.WriteDictionary(
                item.Tags,
                (w, key) => w.Write(key),
                (w, value) => w.Write(value));

            sink.Writer.WriteDictionary(
                item.Values,
                (w, key) => w.Write(key),
                (w, value) => w.Write(value));
        }

        public MetricEvent Deserialize(IAirlockSource source)
        {
            var version = source.Reader.ReadByte();
            if (version != FormatVersion)
                throw new InvalidDataException("invalid format version: " + version);

            var timestamp = new DateTimeOffset(source.Reader.ReadInt64(), TimeSpan.Zero);
            var tags = source.Reader.ReadDictionary(br => br.ReadString(), br => br.ReadString());
            var values = source.Reader.ReadDictionary(br => br.ReadString(), br => br.ReadDouble());

            return new MetricEvent
            {
                Timestamp = timestamp,
                Tags = tags,
                Values = values
            };
        }
    }
}