using System;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server;

namespace Linqua.Service.DataObjects
{
    public class ClientEntry : EntityData
    {
        public string Text { get; set; }

        public string TextLanguageCode { get; set; }

        public string Definition { get; set; }

        public string DefinitionLanguageCode { get; set; }

        public TranslationState TranslationState { get; set; }

        public string UserId { get; set; }

        public bool IsLearnt { get; set; }

        public DateTimeOffset ClientCreatedAt { get; set; }
    }
}