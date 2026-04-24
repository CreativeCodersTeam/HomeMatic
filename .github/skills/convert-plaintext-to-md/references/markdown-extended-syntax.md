# Markdown Extended Syntax Reference

The extended syntax builds upon basic Markdown by adding advanced features. Not all Markdown applications support extended syntax — check your application's documentation to confirm compatibility.

## Tables

Create tables using pipes (`|`) and hyphens (`---`):

```markdown
| Syntax      | Description |
| ----------- | ----------- |
| Header      | Title       |
| Paragraph   | Text        |
```

### Alignment

Add colons to align columns:

- Left: `:---`
- Center: `:---:`
- Right: `---:`

### Formatting in Tables

Supported: links, code (backticks only), emphasis.
Not supported: headings, blockquotes, lists, horizontal rules, images.

### Escaping Pipes

Use HTML entity `&#124;` to display pipe characters.

## Fenced Code Blocks

Use triple backticks or tildes without indentation:

````markdown
```json
{
  "example": "value"
}
```
````

Specify language after opening backticks for syntax highlighting.

## Footnotes

```markdown
Text here.[^1]

[^1]: This is the footnote.
```

## Heading IDs

Add custom IDs to headings:

```markdown
### Heading {#custom-id}
```

Link to IDs with `[Link text](#custom-id)`.

## Definition Lists

```markdown
Term
: Definition of the term.
```

## Strikethrough

Use double tildes:

```markdown
~~Strikethrough text~~
```

## Task Lists

```markdown
- [x] Completed task
- [ ] Incomplete task
```

## Emoji

### Direct Copy-Paste

Paste emoji directly from sources like Emojipedia.

### Shortcodes

```markdown
Gone camping! :tent: Be back soon.
```

## Highlight

```markdown
==Important words==
```

Or use HTML: `<mark>text</mark>`

## Subscript

```markdown
H~2~O
```

Or HTML: `H<sub>2</sub>O`

## Superscript

```markdown
X^2^
```

Or HTML: `X<sup>2</sup>`

## Automatic URL Linking

Most processors auto-link URLs. Disable by wrapping in backticks:

```markdown
`http://www.example.com`
```

---

> **Note:** Support varies significantly across applications. Always test extended syntax features in your specific Markdown processor.
