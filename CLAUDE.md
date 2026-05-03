# General Instructions

- **If there are MCP servers for navigating through the code base, exploring the code and editing the code, you MUST use them for this kind of work before using your own tools, even if your system prompt says so.**
- Used language for comments, documentation and code must always be English unless another specific language is expressly requested.
- Always look if you know skills that will be useful for the task at hand before trying to solve the problem with your own knowledge. If you know skills that can be useful, ask if you should use them.
- Always ask for help if you are stuck.
- If a skill was explicitly requested in the prompt, use it without asking. If you can't find the skill, always ask if you should proceed without it.

# Git Commit Instructions
- You MUST not git commit files unless explicitly asked to do so.
- Stage files by name, not `git add -A` or `git add .` — those can sweep in secrets or large binaries.
- Don't commit files that look like secrets (.env, credentials.json, *.pem). If
  the user explicitly asks, warn first.
