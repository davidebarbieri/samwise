# Changelog

## [0.0.19b] - 2024-07-1

### Added
- Added Options Alternatives (Check docs for syntax)

### Changed
- Refactored Choices interface. Now, GetOption(i) will return an IOptions, with a slightly different and simplified interface.
An option will be checked for availability and the content is read with the following methods:

bool IsAvailable(IDialogueContext context, out ITextContent textContent);
ITextContent GetTextContent(IDialogueContext context);
bool HasCheck(out bool isPreCheck, out string checkName);
bool HasTime(out double time);

In general, an option can have multiple alternative texts, so both IsAvailable and GetTextContent will return the current available text content,
that must be used to show text to the player.

For better understanding, ICheckable is now used for content that provides Challenge Checks, while IConditional is used for content with conditions.

- In the Vscode Extension, "Assign IDs to all nodes" was changed into "Assign IDs to all content", in order to include options too.

### Fixed
- Fixed some syntax Highlight bugs

## [0.0.17b] - 2024-06-10

### Added
- Added Multi-line Comments
- Added Disable Feature

### Fixed
- Fixed minor bugs

## [0.0.14b] - 2024-05-20

### Added
- Added Cancel Node (<!=) to simplify interruptible flows.
- Updated Documentation

### Fixed
- Fixed minor bugs

## [0.0.13b] - 2024-05-10

### Changed
- Simplified IDialogueContext interface

## [0.0.12b] - 2024-05-10

### Added

- First language specification
- First release of the C# runtime library
- First release of the Unity Samwise plug-in
- First release of the vscode extension

<!--
# Changelog Format

```
## [_._._] - YYYY-MM-DD

### Added

### Fixed

### Changed

### Removed
```
-->