using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace SolidTradeServer.Serilog
{
    public class SerilogMessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var typeName = logEvent.Properties.GetValueOrDefault("SourceContext")?.ToString();
            var actionName = logEvent.Properties.GetValueOrDefault("ActionName")?.ToString();
            
            if (actionName is not null)
            {
                var name = actionName.AsSpan();
                var p1 = name[..name.LastIndexOf('.')];
                var p2 = p1[(p1.LastIndexOf('.') + 1)..];

                typeName = p2.ToString();
            } else if (typeName is not null)
            {
                var pos = typeName.LastIndexOf('.');
                typeName = typeName.Substring(pos + 1, typeName.Length - pos - 2);
            }

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", typeName));
        }
    }
}