﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service.Models
{
    public class Entry : EntityData
    {
        [Index]
        [MaxLength(256)]
        public string Text { get; set; }

        public string TextLanguageCode { get; set; }

        public string Definition { get; set; }

        public string DefinitionLanguageCode { get; set; }

        public TranslationState TranslationState { get; set; }

        [Index]
        [MaxLength(256)]
        public string UserId { get; set; }

        [MaxLength(256)]
        public string UserEmail { get; set; }

        public bool IsLearnt { get; set; }

        public DateTimeOffset ClientCreatedAt { get; set; }
    }
}