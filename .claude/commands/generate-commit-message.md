---
description: Generate conventional commit message based on changes
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git log:*)
argument-hint: [optional-message]
---

# Git Commit Message Generator

Generate a conventional commit message based on the current changes.

## Context

- Current git status:
  !`git status`

- Staged and unstaged changes:
  !`git diff HEAD`

- Current branch:
  !`git branch --show-current`

- Recent commit messages (for style consistency):
  !`git log --oneline -10`

## Your Task

Based on the above changes and the project's commit history:

1. **Analyze the changes** - Identify what files were modified, added, or deleted
2. **Categorize the change** - Determine if it's a:
   - `feat:` - New feature
   - `fix:` - Bug fix
   - `refactor:` - Code refactoring
   - `chore:` - Maintenance tasks
   - `docs:` - Documentation changes
   - `test:` - Test additions/changes
   - `perf:` - Performance improvements
   - `style:` - Code style changes (formatting, etc.)
   - `ci:` - CI/CD changes

3. **Generate ONLY the commit message** following Conventional Commits specification:

   **Structure:**

   ```text
   <type>(<scope>): <subject>

   <body>

   <footer>
   ```

   **Subject Line:**
   - Max 72 characters
   - Use imperative mood ("add" not "added" or "adds")
   - No period at the end
   - Format: `<type>(<scope>): <description>`

   **Body (optional but recommended for significant changes):**
   - Explain WHAT and WHY (not HOW)
   - Wrap at 72 characters per line
   - Include motivation and context
   - Mention any trade-offs or alternative approaches considered

   **Footer (optional):**
   - Breaking changes: `BREAKING CHANGE: <description>`
   - Closes issues: `Closes #123`, `Fixes #456`
   - References: `Refs #789`

   **Types:**
   - `feat:` - New feature
   - `fix:` - Bug fix
   - `refactor:` - Code refactoring without functional change
   - `chore:` - Maintenance tasks, dependencies, build
   - `docs:` - Documentation only changes
   - `test:` - Test additions or changes
   - `perf:` - Performance improvements
   - `style:` - Code style (formatting, semi-colons, etc.)
   - `ci:` - CI/CD configuration changes
   - `build:` - Build system or dependencies

   **Scopes (examples):**
   - `auth`, `api`, `db`, `persistence`, `domain`, `infrastructure`, etc.

4. **Output ONLY the commit message** - Do not execute any git commands
   - Your entire response should be just the commit message
   - Do NOT include explanations, prefixes like "Commit message:", or any extra text

## Important Notes

- Do NOT execute git commands (git add, git commit, etc.)
- Do NOT commit files containing secrets (.env, credentials.json, etc.)
- If there are no changes to commit, inform the user
- Follow the existing commit message style from the history
- Your OUTPUT should be ONLY the commit message, nothing else

## Example Output

**Simple change:**

```text
feat(auth): implement JWT token generation and validation
```

**Significant change with body and footer:**

```text
feat(persistence): add optimistic concurrency control for entities

Implement RowVersion timestamp property to handle concurrent updates
and prevent lost updates in multi-user scenarios.

- Add RowVersion property to base entities
- Configure concurrency token in DbContext
- Update handlers to catch DbUpdateConcurrencyException

This change ensures data integrity when multiple users attempt to
modify the same entity simultaneously.

Closes #123
```

**Breaking change:**

```text
feat(api): change response structure to use StandardApiResponse

All endpoints now return a standardized response wrapper with success
status, error details, and metadata. This change requires client
updates to parse the new response format.

BREAKING CHANGE: Response structure changed from direct data return
to StandardApiResponse<T> wrapper. Clients must update parsing logic.
```
