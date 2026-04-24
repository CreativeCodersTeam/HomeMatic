# Context Windows

As conversations grow, you'll eventually approach context window limits. This guide explains how context windows work and introduces strategies for managing them effectively.

## Understanding the Context Window

The "context window" refers to all the text a language model can reference when generating a response, including the response itself. This is different from the large corpus of data the language model was trained on, and instead represents a "working memory" for the model. A larger context window allows the model to handle more complex and lengthy prompts, but more context isn't automatically better. As token count grows, accuracy and recall degrade, a phenomenon known as *context rot*. This makes curating what's in context just as important as how much space is available.

> **Tip:** For a deep dive into why long contexts degrade and how to engineer around it, see "Effective context engineering" from Anthropic.

## Key Concepts

- **Progressive token accumulation:** As the conversation advances through turns, each user message and assistant response accumulates within the context window. Previous turns are preserved completely.
- **Linear growth pattern:** The context usage grows linearly with each turn.
- **Context window capacity:** The total available context window (up to 1M tokens) represents the maximum capacity for storing conversation history and generating new output.
- **Input-output flow:** Each turn consists of an input phase (all previous conversation history plus the current user message) and an output phase (generates a response that becomes part of future input).

## The Context Window with Extended Thinking

When using extended thinking, all input and output tokens, including thinking tokens, count toward the context window limit with some nuances:

- Thinking budget tokens are a subset of `max_tokens`, billed as output tokens
- Previous thinking blocks are automatically stripped from the context window calculation by the API
- Effective calculation: `context_window = (input_tokens - previous_thinking_tokens) + current_turn_tokens`

## The Context Window with Tool Use

When combining extended thinking with tool use:

1. **First turn:** Tools configuration + user message as input; extended thinking + text + tool use request as output
2. **Tool result handling:** Extended thinking block **must** be returned with corresponding tool results
3. **Subsequent turns:** Previous thinking blocks are stripped; new thinking generated for each user turn

> **Important:** When posting tool results, the entire unmodified thinking block (including signature portions) must be included. Modified thinking blocks cause API errors.

## Model Context Window Sizes

- Claude Opus 4.6, Sonnet 4.6: 1M-token context window
- Claude Sonnet 4.5, Sonnet 4: 200k-token context window

A single request can include up to 600 images or PDF pages (100 for 200k-token models).

## Context Awareness

Claude Sonnet 4.6, Sonnet 4.5, and Haiku 4.5 feature **context awareness** — they track their remaining token budget throughout a conversation. At conversation start, Claude receives its total budget, and after each tool call, it receives an update on remaining capacity.

Benefits:
- Long-running agent sessions with sustained focus
- Multi-context-window workflows where state transitions matter
- Complex tasks requiring careful token management

## Managing Context with Compaction

For conversations approaching context limits, **server-side compaction** provides automatic summarization that condenses earlier parts of a conversation, enabling long-running conversations beyond context limits.

Additional strategies via **context editing**:
- **Tool result clearing** — Clear old tool results in agentic workflows
- **Thinking block clearing** — Manage thinking blocks with extended thinking

## Newer Model Behavior

Newer Claude models (starting with Claude Sonnet 3.7) return a validation error when prompt and output tokens exceed the context window, rather than silently truncating. Use the token counting API to estimate usage before sending messages.

---

Source: https://platform.claude.com/docs/en/build-with-claude/context-windows
