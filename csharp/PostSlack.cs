/*
 * Copyright (c) Thomas Hansen, 2021 - 2023 thomas@ainiro.io.
 */

using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.slack
{
    [Slot(Name = "slack.post")]
    public class PostSlack : ISlotAsync
    {
        private readonly HttpClient _http;

        public PostSlack(HttpClient http)
        {
            _http = http;
        }

        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var botToken = input.Children.FirstOrDefault(x => x.Name == "bot_token").GetEx<string>()
                ?? throw new HyperlambdaException("No [bot_token] supplied to [slack.post]");
            var channel  = input.Children.FirstOrDefault(x => x.Name == "channel").GetEx<string>()
                ?? throw new HyperlambdaException("No [channel] supplied to [slack.post]");
            var text     = input.Children.FirstOrDefault(x => x.Name == "text").GetEx<string>()
                ?? throw new HyperlambdaException("No [text] supplied to [slack.post]");

            using (var req = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage"))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botToken);
                req.Content = new StringContent(JsonSerializer.Serialize(
                    new
                    {
                        channel,
                        text,
                        mrkdwn = true
                    }), Encoding.UTF8, "application/json");

                using (HttpResponseMessage res = await _http.SendAsync(req))
                {
                    if (!res.IsSuccessStatusCode)
                        throw new InvalidOperationException($"Slack HTTP error: {(int)res.StatusCode} {res.ReasonPhrase}");

                    var json = await res.Content.ReadAsStringAsync();

                    var slack = JsonSerializer.Deserialize<SlackPostMessageResponse>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidOperationException("Empty/invalid response from Slack.");

                    if (!slack.ok)
                        throw new HyperlambdaException($"Slack API error: {slack.error ?? "unknown_error"}");

                    input.Value = slack.ts;
                }
            }
        }

        /*
         * Private helper methods.
         */
        private sealed class SlackPostMessageResponse
        {
            public bool ok { get; set; }
            public string? ts { get; set; }
            public string? channel { get; set; }
            public string? error { get; set; }
        }
    }
}
