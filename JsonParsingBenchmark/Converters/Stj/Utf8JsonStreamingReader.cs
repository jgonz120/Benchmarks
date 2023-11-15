using BenchmarkDotNet.Loggers;
using JsonParsingBenchmark.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonParsingBenchmark.Converters.Stj
{
    static class Utf8JsonStreamingReader
    {
        private static readonly byte[] utf8TotalHits = Encoding.UTF8.GetBytes("totalHits");
        private static readonly byte[] utf8Data = Encoding.UTF8.GetBytes("data");
        private static readonly byte[] utf8Id = Encoding.UTF8.GetBytes("id");
        private static readonly byte[] utf8Version = Encoding.UTF8.GetBytes("version");
        private static readonly byte[] utf8Description = Encoding.UTF8.GetBytes("description");
        private static readonly byte[] utf8Versions = Encoding.UTF8.GetBytes("versions");
        private static readonly byte[] utf8Authors = Encoding.UTF8.GetBytes("authors");
        private static readonly byte[] utf8IconUrl = Encoding.UTF8.GetBytes("iconUrl");
        private static readonly byte[] utf8LicenseUrl = Encoding.UTF8.GetBytes("licenseUrl");
        private static readonly byte[] utf8Owners = Encoding.UTF8.GetBytes("owners");
        private static readonly byte[] utf8ProjectUrl = Encoding.UTF8.GetBytes("projectUrl");
        private static readonly byte[] utf8Registration = Encoding.UTF8.GetBytes("registration");
        private static readonly byte[] utf8Summary = Encoding.UTF8.GetBytes("summary");
        private static readonly byte[] utf8Tags = Encoding.UTF8.GetBytes("tags");
        private static readonly byte[] utf8Title = Encoding.UTF8.GetBytes("title");
        private static readonly byte[] utf8TotalDownloads = Encoding.UTF8.GetBytes("totalDownloads");
        private static readonly byte[] utf8Verified = Encoding.UTF8.GetBytes("verified");
        private static readonly byte[] utf8AtId = Encoding.UTF8.GetBytes("@id");
        private static readonly byte[] utf8Downloads = Encoding.UTF8.GetBytes("downloads");

        public static SearchResults Read(string fileLocation)
        {
            using (var stream = File.OpenRead(fileLocation))
            {
                var firstByte = (byte)stream.ReadByte();
                var buffer = new byte[1024];

                if (firstByte == 0xEF)
                {
                    //Reading BB
                    stream.ReadByte();
                    //Reading BF
                    stream.ReadByte();
                    stream.Read(buffer, 0, buffer.Length);
                }
                else
                {
                    buffer[0] = firstByte;
                    stream.Read(buffer, 1, buffer.Length - 1);
                }
                var reader = new Utf8JsonReader(buffer, isFinalBlock: false, state: default);
                reader.ReadWithBuffer(ref buffer, stream);
                return ReadSearchResults(ref reader, ref buffer, stream);
            }

        }

        private static SearchResults ReadSearchResults(ref Utf8JsonReader reader, ref byte[] buffer, Stream stream)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject, found " + reader.TokenType);
            }

            var searchResults = new SearchResults();

            var finished = false;
            while (!finished)
            {
                reader.ReadWithBuffer(ref buffer, stream);

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(utf8TotalHits))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResults.TotalHits = reader.GetInt32();
                        }
                        else if (reader.ValueTextEquals(utf8Data))
                        {
                            searchResults.Data = new List<SearchResult>();

                            reader.ReadWithBuffer(ref buffer, stream);
                            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("data should be an array");
                            reader.ReadWithBuffer(ref buffer, stream);
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                var searchResult = ReadSearchResult(ref reader, ref buffer, stream);
                                searchResults.Data.Add(searchResult);
                                reader.ReadWithBuffer(ref buffer, stream);
                            }
                        }
                        else
                        {
                            reader.TrySkipWithBuffer(ref buffer, stream);
                        }
                        break;

                    case JsonTokenType.EndObject:
                        finished = true;
                        break;

                    default:
                        throw new JsonException();
                }
            }

            return searchResults;
        }

        private static SearchResult ReadSearchResult(ref Utf8JsonReader reader, ref byte[] buffer, Stream stream)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObjected, found " + reader.TokenType);
            }

            var searchResult = new SearchResult();

            bool finished = false;
            while (!finished)
            {
                reader.ReadWithBuffer(ref buffer, stream);
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(utf8Id))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Id = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Version))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Version = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Description))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Description = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Versions))
                        {
                            searchResult.Versions = new List<SearchResultVersion>();
                            reader.ReadWithBuffer(ref buffer, stream);
                            if (reader.TokenType != JsonTokenType.StartArray)
                            {
                                throw new JsonException("Expected StartArray, found " + reader.TokenType);
                            }
                            reader.ReadWithBuffer(ref buffer, stream);
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                var version = ReadSearchResultVersion(ref reader, ref buffer, stream);
                                searchResult.Versions.Add(version);
                                reader.ReadWithBuffer(ref buffer, stream);
                            }
                        }
                        else if (reader.ValueTextEquals(utf8Authors))
                        {
                            searchResult.Authors = new List<string>();
                            reader.ReadWithBuffer(ref buffer, stream);
                            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected start array");
                            reader.ReadWithBuffer(ref buffer, stream);
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.String)
                                {
                                    var author = reader.GetString();
                                    searchResult.Authors.Add(author);
                                }
                                else
                                {
                                    throw new JsonException("Expected string");
                                }
                                reader.ReadWithBuffer(ref buffer, stream);
                            }
                        }
                        else if (reader.ValueTextEquals(utf8IconUrl))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.IconUrl = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8LicenseUrl))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.LicenseUrl = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Owners))
                        {
                            searchResult.Owners = new List<string>();
                            reader.ReadWithBuffer(ref buffer, stream);
                            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected start array");
                            reader.ReadWithBuffer(ref buffer, stream);
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.String)
                                {
                                    var owner = reader.GetString();
                                    searchResult.Owners.Add(owner);
                                }
                                else
                                {
                                    throw new JsonException("Expected string");
                                }
                                reader.ReadWithBuffer(ref buffer, stream);
                            }
                        }
                        else if (reader.ValueTextEquals(utf8ProjectUrl))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.ProjectUrl = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Registration))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Registration = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Summary))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Summary = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Tags))
                        {
                            searchResult.Tags = new List<string>();
                            reader.ReadWithBuffer(ref buffer, stream);
                            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected start array");
                            reader.ReadWithBuffer(ref buffer, stream);
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.String)
                                {
                                    var tag = reader.GetString();
                                    searchResult.Tags.Add(tag);
                                }
                                else
                                {
                                    throw new JsonException("Expected string");
                                }
                                reader.ReadWithBuffer(ref buffer, stream);
                            }
                        }
                        else if (reader.ValueTextEquals(utf8Title))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Title = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8TotalDownloads))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.TotalDownloads = unchecked((int)reader.GetInt64());
                        }
                        else if (reader.ValueTextEquals(utf8Verified))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            searchResult.Verified = reader.GetBoolean();
                        }
                        else
                        {
                            reader.TrySkipWithBuffer(ref buffer, stream);
                        }
                        break;

                    case JsonTokenType.EndObject:
                        finished = true;
                        break;

                    default:
                        throw new JsonException("Unexpected type " + reader.TokenType);
                }
            }

            return searchResult;
        }

        private static SearchResultVersion ReadSearchResultVersion(ref Utf8JsonReader reader, ref byte[] buffer, Stream stream)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject, found " + reader.TokenType);
            }

            var version = new SearchResultVersion();

            bool finished = false;
            while (!finished)
            {
                reader.ReadWithBuffer(ref buffer, stream);
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(utf8AtId))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            version.Id = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Version))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            version.Version = reader.GetString();
                        }
                        else if (reader.ValueTextEquals(utf8Downloads))
                        {
                            reader.ReadWithBuffer(ref buffer, stream);
                            version.Downloads = reader.GetInt32();
                        }
                        else
                        {
                            reader.TrySkipWithBuffer(ref buffer, stream);
                        }
                        break;

                    case JsonTokenType.EndObject:
                        finished = true;
                        break;

                    default:
                        throw new JsonException("Unexpected " + reader.TokenType);
                }
            }

            return version;
        }

        public static bool ReadWithBuffer(this ref Utf8JsonReader reader, ref byte[] buffer, Stream stream)
        {
            bool wasRead;

            while(!(wasRead = reader.Read()) && !reader.IsFinalBlock)
            {
                GetMoreBytesFromStream(stream, ref buffer, ref reader);
            }

            return wasRead;
        }

        public static bool TrySkipWithBuffer(this ref Utf8JsonReader reader, ref byte[] buffer, Stream stream)
        {
            var wasSkipped = true;
            while (!(wasSkipped = reader.TrySkip()) && !reader.IsFinalBlock)
            {
                GetMoreBytesFromStream(stream, ref buffer, ref reader);
            }
            return wasSkipped;
        }

        private static void GetMoreBytesFromStream(
            Stream stream, ref byte[] buffer, ref Utf8JsonReader reader)
        {
            int bytesRead;
            if (reader.BytesConsumed < buffer.Length)
            {
                ReadOnlySpan<byte> leftover = buffer.AsSpan((int)reader.BytesConsumed);
                if (leftover.Length == buffer.Length)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                }
                leftover.CopyTo(buffer);

                //bytesRead = stream.Read(buffer.AsSpan(leftover.Length));
                bytesRead = stream.Read(buffer, leftover.Length, buffer.Length - leftover.Length);
            }
            else
            {
                //bytesRead = stream.Read(buffer);
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            reader = new Utf8JsonReader(buffer, isFinalBlock: bytesRead == 0, reader.CurrentState);
        }
    }
}
