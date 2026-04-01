# Parameters and Options Reference

## Command Syntax

```bash
/convert-plaintext-to-md <#file:{{file}}> [finalize] [guide #file:{{reference-file}}] [instructions] [platform={{name}}] [options] [pre=<name>]
```

When parameters and options are unclear, use `#tool:fetch` to retrieve the URLs in the [Markdown References](./considerations.md#markdown-references) section.

---

## Parameters

### `#file:{{file}}` _(required)_

The plain or generic text documentation file to convert to markdown.

- If a corresponding `{{file}}.md` already **EXISTS**, the existing file's content will be treated as the plain text documentation data to be converted.
- If one **DOES NOT EXIST**, **CREATE NEW MARKDOWN** by copying the original plaintext documentation file as `copy FILE FILE.md` in the same directory as the plain text documentation file.

### `finalize`

When passed (or similar language is used), scan through the entire document and trim space characters, indentation, and/or any additional sloppy formatting after the conversion.

### `guide #file:{{reference-file}}`

Use a previously converted markdown file as a template for formatting patterns, structure, and conventions.

### `instructions`

Text data passed to the prompt providing additional instructions.

### `platform={{name}}`

Specify the target platform for markdown rendering to ensure compatibility:

| Platform | Description |
|---|---|
| **GitHub** _(default)_ | GitHub-flavored markdown (GFM) with tables, task lists, strikethrough, and alerts |
| **StackOverflow** | CommonMark with StackOverflow-specific extensions |
| **VS Code** | Optimized for VS Code's markdown preview renderer |
| **GitLab** | GitLab-flavored markdown with platform-specific features |
| **CommonMark** | Standard CommonMark specification |

---

## Options

### `--header [1-4]`

Add markdown header tags to the document.

- **`[1-4]`** — Specifies the header level to add (`#` through `####`)
- **`#selection`** — Data used to:
  - Identify sections where updates should be applied
  - Serve as a guide for applying headers to other sections or the entire document
- **Auto-apply** (if none provided) — Add headers based on content structure

### `-p, --pattern`

Follow an existing pattern from:

- **`#selection`** — A selected pattern to follow when updating the file or a portion of it
  - **IMPORTANT**: DO NOT only edit the selection when passed to `{{[-p, --pattern]}}`
  - **NOTE**: The selection is **NOT** the **WORKING RANGE**
  - Identify pattern(s) from the selection
  - **Stopping Points**:
    - If `{{[-s, --stop]}} eof` is passed or no clear endpoint is specified, convert to end of file
    - If `-s [0-9]+` is passed, convert to the line number specified in the regex `[0-9]+`
- **Prompt instructions** — Instructional data passed with the prompt
- **Auto-detect** (if none provided) — Identify existing patterns in the file by:
  - Analyzing where patterns occur
  - Identifying data that does not match the pattern
  - Applying patterns from one section to corresponding sections where the pattern is missing

### `-s, --stop <[0-9]+ | eof>`

- **`[0-9]+`** — Line number to stop the current markdown conversion at
- **`eof`** — If passed, or any other text clearly indicating end of file, convert to end of file

---

## Predefined Instructions

If any of the predefined instructions are passed as an argument, expand and use them as **ADDITIONAL** input for the prompt instructions. If only the predefined instruction is passed and no additional input, use it as the sole instruction for the current prompt.

**Syntax:**

```bash
/convert-plaintext-to-md pre=<name>
```

### Available Predefined Instructions

| Name | Description |
|---|---|
| `rm-head-digits` | Remove any prepending numbers from the headers when updating or converting the plaintext to markdown. |
| `mv-head-level(x, y)` | Change the heading level from level `x` header to a level `y` header when updating or converting plaintext to markdown. |
| `rm-indent(x)` | Decrease the indentation of paragraphs or raw text data portions of the file by `x` when updating or converting plaintext to markdown. |

> [!NOTE]
> If there is no matching predefined instruction, disregard the `pre=name` for the current prompt.
