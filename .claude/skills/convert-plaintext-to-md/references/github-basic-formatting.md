# GitHub Basic Writing and Formatting Syntax Reference

## Headings

Create hierarchy using 1-6 hash symbols. GitHub automatically generates a table of contents when you use multiple headings.

```markdown
# First-level heading
## Second-level heading
### Third-level heading
```

## Text Styling

| Style | Syntax | Example |
|-------|--------|---------|
| Bold | `** **` or `__ __` | `**bold text**` |
| Italic | `* *` or `_ _` | `*italicized text*` |
| Strikethrough | `~~ ~~` | `~~mistaken text~~` |
| Bold & italic | `** **` and `_ _` | `**_extremely_ important**` |
| Subscript | `<sub> </sub>` | `<sub>subscript</sub>` |
| Superscript | `<sup> </sup>` | `<sup>superscript</sup>` |
| Underline | `<ins> </ins>` | `<ins>underlined</ins>` |

## Quoting Text

Use `>` to create block quotes:

```markdown
> Text that is a quote
```

## Code Formatting

**Inline code:** Use single backticks for code within sentences:

```markdown
Use `git status` to list changes.
```

**Code blocks:** Use triple backticks for standalone code:

````markdown
```
git status
git add
git commit
```
````

## Color Support

Display colors using supported models within backticks:

- Hex: `` `#0969DA` ``
- RGB: `` `rgb(9, 105, 218)` ``
- HSL: `` `hsl(212, 92%, 45%)` ``

## Links

Wrap link text in brackets and URL in parentheses:

```markdown
[Link text](https://example.com)
```

**Section links:** Link directly to any heading using its anchor.

**Relative links:** Use paths relative to current file:

```markdown
[Contribution guidelines](docs/CONTRIBUTING.md)
```

## Images

Include images using exclamation mark, alt text in brackets, and URL in parentheses:

```markdown
![Alt text](image-url.png)
```

The `<picture>` HTML element is supported for theme-adaptive images.

## Lists

**Unordered lists:** Use `-`, `*`, or `+`

**Ordered lists:** Use numbers followed by periods

**Nested lists:** Indent items under parent items using spaces or tabs

## Task Lists

Create checkboxes with `[ ]` for incomplete and `[x]` for complete tasks:

```markdown
- [x] Completed task
- [ ] Incomplete task
```

## Mentions & References

- **Mention people/teams:** `@username` or `@organization/team-name`
- **Reference issues:** `#` followed by issue number or title

## Emojis

Use `:EMOJICODE:` format: `:+1:`, `:shipit:`

## Paragraphs & Line Breaks

Create paragraphs by leaving blank lines between text. For line breaks:

- Two trailing spaces at line end
- Backslash `\` at line end
- HTML `<br/>`

## Footnotes

```markdown
Here is a footnote[^1].

[^1]: Footnote content here.
```

Footnotes render at document bottom regardless of placement.

## Alerts

Five alert types emphasize critical information:

```markdown
> [!NOTE]
> Essential information users should know.

> [!TIP]
> Helpful advice for better outcomes.

> [!IMPORTANT]
> Key information for success.

> [!WARNING]
> Urgent information requiring immediate attention.

> [!CAUTION]
> Warns about risks and negative outcomes.
```

## Special Features

- **HTML comments:** `<!-- Hidden content -->`
- **Escape formatting:** Backslash before markdown characters: `\*not italicized\*`
