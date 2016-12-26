using System;
using Newtonsoft.Json;

namespace Linqua.Api.Models
{
    public class Entry
    {
        public Guid Id { get; set; }

        public byte[] Version { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }

        public string Text { get; set; }

        public string TextLanguageCode { get; set; }

        public string Definition { get; set; }

        public string DefinitionLanguageCode { get; set; }

        public TranslationState TranslationState { get; set; }

        public Guid? UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public bool IsLearnt { get; set; }

        public DateTimeOffset ClientCreatedAt { get; set; }
    }
}