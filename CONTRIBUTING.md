# Contributing

Thank you for wanting to contribute to this project. This document explains how to report issues, propose changes, and submit pull requests.

## Table of contents
- [Code of Conduct](#code-of-conduct)
- [How to get started](#how-to-get-started)
- [Reporting issues](#reporting-issues)
- [Suggesting enhancements](#suggesting-enhancements)
- [Pull request process](#pull-request-process)
- [Code style & formatting](#code-style--formatting)

## Code of Conduct
All contributors must follow the project's `CODE_OF_CONDUCT.md`. Be respectful, constructive, and inclusive.

## How to get started
1. Fork the repository.
2. Create a branch named using the pattern: `fix/<short-description>` or `feat/<short-description>`.
3. Make small, focused commits. Use descriptive commit messages (see *Commit messages* below).
4. Make sure the code builds and works as expected before opening a pull request.

## Reporting issues
When opening an issue:
- Use a clear title.
- Describe expected vs actual behavior.
- Provide steps to reproduce.
- Include environment details.
- Attach logs or minimal reproducer if possible.

Example template:
- **Title**: Short description
- **Steps to reproduce**: ...
- **Expected behavior**: ...
- **Actual behavior**: ...
- **Environment**: Linux

## Suggesting enhancements
- Open an issue to discuss major changes before implementing.
- For proposals, include rationale, alternative approaches considered, and high-level design.

## Pull request process
1. Ensure your branch is up to date with `master`.
2. Open a PR that references the related issue (if any).
3. Include a clear description of changes and rationale.
4. Add tests for bug fixes and new features.
5. Ensure the build passes and all checks succeed.

## Code style & formatting
- This project is written in C\#. Follow standard C\# conventions (naming, file structure).
- Prefer .NET idioms and use `async`/`await` for asynchronous code.
- Keep methods small and focused. Add XML doc comments for public APIs.