Slack integrations for Magic Cloud, allowing you to have your cloudlet interact with Slack somehow. Useful for escalating AI support queries to human customer service agents for instance.

## Slots

* __[magic.slack.post]__ - Posts **[text]** to **[channel]** with **[bot_token]**

Notice, the above __[bot_token]__ must start with _"xoxb-"_. If it doesn't it's _not_ a bot token!

## Events / webhooks

To register your webhook/event, make sure you find the correct URL to the _"slack/webhook"_ HTTP POST endpoint. You can register this as an event callback URL in Slack, at which point the endpoint will be invoked if your `bot_token` has the correct scopes. Remember to configure the scopes correctly, such that it's at least invoked for replies to messages.

Ta add business logic to your callbacks, you can create a dynamic Hyperlambda slot that's named anything starting with _"magic.slack.callbacks."_, for instance **[magic.slack.callbacks.deal_with_message]**. For an example consider the following code.

```plaintext
slots.create:magic.slack.callbacks.deal_with_message
   lambda2hyper:x:../*
   log.info:x:-
```

The slot doesn't need to return anything, but if an exception occurs in it, any remaining callback invocations will be prevented from completing. To match invocations towards external systems, you can use the `external_id` of the _"requests"_ table, which will be populated with the **[external_id]** argument provided to invocations to **[magic.slack.post]**. This allows you to store meta data associated with each query going towards Slack, but the actual persisting of this is up to you. Notice, the `external_id` field is a pure text field, and can store anything - Including in theory Hyperlambda code to execute upon replies.
