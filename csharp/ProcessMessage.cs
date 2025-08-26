/*
 * Copyright (c) Thomas Hansen, 2021 - 2023 thomas@ainiro.io.
 */

using System;
using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.slack
{
    [Slot(Name = "slack.process_message")]
    public class ProcessMessage : ISlot
    {
        public void Signal(ISignaler signaler, Node input)
        {
            var type = input.Children.FirstOrDefault(x => x.Name == "type").GetEx<string>()
                ?? throw new HyperlambdaException("No [type] supplied to [slack.webhook.process]");

            if (string.Equals(type, "url_verification", StringComparison.Ordinal))
            {
                var challenge = input.Children.FirstOrDefault(x => x.Name == "challenge").GetEx<string>()
                    ?? throw new HyperlambdaException("No [challenge] supplied for url_verification");
                input.Value = challenge;
                return;
            }

            if (string.Equals(type, "event_callback", StringComparison.Ordinal))
            {
                var ev = input.Children.FirstOrDefault(x => x.Name == "event")
                    ?? throw new HyperlambdaException("Missing [event] node for event_callback.");

                var evType = ev.Children.FirstOrDefault(x => x.Name == "type").GetEx<string>();

                if (string.Equals(evType, "message", StringComparison.Ordinal))
                {
                    var subtype = ev.Children.FirstOrDefault(x => x.Name == "subtype").Get<string>();
                    if (!string.IsNullOrEmpty(subtype))
                    {
                        input.Value = "ok";
                        return;
                    }

                    var botId = ev.Children.FirstOrDefault(x => x.Name == "bot_id").Get<string>();
                    if (!string.IsNullOrEmpty(botId))
                    {
                        input.Value = "ok";
                        return;
                    }

                    var channel  = ev.Children.FirstOrDefault(x => x.Name == "channel").Get<string>();
                    var tsMsg    = ev.Children.FirstOrDefault(x => x.Name == "ts").Get<string>();
                    var threadTs = ev.Children.FirstOrDefault(x => x.Name == "thread_ts").Get<string>();
                    var user     = ev.Children.FirstOrDefault(x => x.Name == "user").Get<string>();
                    var text     = ev.Children.FirstOrDefault(x => x.Name == "text").Get<string>() ?? "";
                    
                    // House cleaning.
                    input.Clear();
                    input.Value = null;

                    if (!string.IsNullOrEmpty(channel) &&
                        !string.IsNullOrEmpty(tsMsg) &&
                        !string.IsNullOrEmpty(threadTs) &&
                        !string.Equals(tsMsg, threadTs, StringComparison.Ordinal))
                    {
                        input.Add(new Node("channel", channel));
                        input.Add(new Node("thread_ts", threadTs));
                        input.Add(new Node("ts", tsMsg));
                        input.Add(new Node("user", user));
                        input.Add(new Node("text", text));
                    }
                }
            }
        }
    }
}
