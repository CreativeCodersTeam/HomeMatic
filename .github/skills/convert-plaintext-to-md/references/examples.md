# Usage Examples

## Basic Conversion

**User**

```bash
/convert-plaintext-to-md #file
```

**Agent**

```
I see there is no corresponding `file.md`. Let me copy the plaintext document:

    ```bash
    cp file file.md
    ```

Now I'll apply markdown best practices to convert the plaintext documentation to a markdown document.
```

---

## Guide-Based Conversion

Uses a previously converted markdown file as a formatting reference.

**User**

```bash
/convert-plaintext-to-md #file.md --guide #CODE.md
```

**Agent**

```
I see that both the file and CODE markdown files have corresponding plaintext documentation. Let me
check CODE and compare it to CODE.md, and identify patterns to help with conversion.

    CODE
    ```text
    Title

    Text in opening.

    1. Section
      1.1 Section Item

    =============================================

    1. Section
      1.1
      Full details of section item.
    ```

And looking at CODE.md:

    CODE.md
    ```markdown
    # Title

    Text in opening.

    ### [Section](#1-section)

    - [1.1](#11) Section Item but Only Numbers in Sub-heading

    ***

    ## 1. Section

    ### 1.1

    Full details of section item.
    ```

I see file.md exists and matches the text in file.

I'll convert file.md similarly. Let's look at the file text documentation:

    file
    ```text
    Title

    1. Command
      1.10.0 Options

    ---------------------------------------------

    Command
      1.10.0
      Info on the commands' options.
    ```

OK, after analyzing the guide file, I've updated file.md to:

    file.md
    ```markdown
    # Title

    ### [1. Command](#command)
    - [1.10.0](#1100-option-with-text-subheading) Option With Text Subheading

    ***

    ## Command

    ### 1.10.0 Option With Text Subheading
    Info on the commands' options.
    ```
```

---

## Finalize / Polish

Natural language variations like "polish", "clean up", or "finalize" all trigger the `finalize` behavior.

**User**

```bash
/convert-plaintext-to-md #file.md polish the converted markdown file
```

**Agent**

```
I'll finalize file.md. I see the current converted markdown file has:

    ```markdown
    # Title

    Some paragraph with space character at start of line. This <html> should be escaped.

    -  Some list item with extra space character.
    -  Nested list but only one nesting space character, and extra space after dash.
      if (a == 1) {
        return "Indented code block without enough space characters.";
      } else {
        return "Passing `finalize` or similar should resolve this.";
      }

    But when `python` is used, syntax will be like:
    ```

    ```
    import math
    radius = float(input("This is clearly python, but let's do math."))
    area = math.pi * radius**2
    circumference = 2 * math.pi * radius
    print(f"Area: {area:.2f}, Circumference: {circumference:.2f}")
    ```

Let me trim a few things:

    ```markdown
    # Title

    Some paragraph with space character at start of line. This \<html\> should be escaped.

    - Some list item with extra space character.
      - Nested list but only one space character.

        if (a == 1) {
          return "Indented code block without enough space characters.";
        } else {
          return "Passing `finalize` or similar should resolve this.";
        }

    But when `python` is used, syntax will be like:

    ```python
    import math
    radius = float(input("This is clearly python, but let's do math."))
    area = math.pi * radius**2
    circumference = 2 * math.pi * radius
    print(f"Area: {area:.2f}, Circumference: {circumference:.2f}")
    ```
```

---

## Inline Pattern with Shorthand Regex

**User**

```bash
/convert-plaintext-to-md #BUGS --p "regex()=> ^ {1,}([0-9]+\.[0-9]+\.[0-9]+) to ^### $1"
```

**Agent**

```
I see that there is no `BUGS.md` file. Let me copy the current plaintext BUGS documentation:

    ```bash
    copy BUGS BUGS.md
    ```

Now I'll update the copied file BUGS.md, converting it to proper markdown.
Let me read the current contents:

    ```markdown
    1 Section
      1.1 Section Item
      1.2 Section Item
      ...
      1.12 Section Item
    ```

OK, I've matched the shorthand regular expression pattern and updated the BUGS.md to:

    ```markdown
    1 Section
    ### 1.1 Section Item
    ### 1.2 Section Item
    ### ...
    ### 1.12 Section Item
    ```
```
