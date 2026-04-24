# GitHub Markdown Alerts (Admonition Syntax)

GitHub supports a markdown extension for creating highlighted alert blocks using blockquote syntax. This feature helps emphasize critical information in documentation with distinctive colors and icons.

## Syntax

The basic syntax uses blockquote markers (`>`) with a bracket notation:

```markdown
> [!TYPE]
> Your alert content here
```

## Supported Alert Types

### NOTE

```markdown
> [!NOTE]
> Highlights information users should consider, even when skimming.
```

### TIP

```markdown
> [!TIP]
> Optional information to help users be more successful.
```

### IMPORTANT

```markdown
> [!IMPORTANT]
> Crucial information necessary for users to succeed.
```

### WARNING

```markdown
> [!WARNING]
> Critical content demanding immediate attention due to potential risks.
```

### CAUTION

```markdown
> [!CAUTION]
> Negative potential consequences of an action.
```

## Key Features

- A line break must follow the alert type marker
- Alerts render as `div` elements with semantic styling rather than blockquote elements
- Works in READMEs, Markdown files, wikis, issues, pull requests, and discussions
- Each alert type uses distinct visual styling (colors and icons) for clarity

## Source

Originally discussed at: https://github.com/orgs/community/discussions/16925
