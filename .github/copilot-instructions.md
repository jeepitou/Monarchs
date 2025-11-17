# C# Unity Code Review Guidelines

## Purpose & Scope
This file gives some general guidelines for reviewing C# code in Unity projects. It aims to ensure code quality, maintainability, and consistency across the codebase.

---

## Naming Conventions
- camelCase for public variables
- PascalCase for class names and methods
- camelCase for private variables
- UPPER_SNAKE_CASE for constants
- Avoid abbreviations unless widely recognized
- Use meaningful and descriptive names
- Prefix interfaces with "I" (e.g., IMyInterface)
- Suffix private fields with an underscore (e.g., _myField)
- Use "Async" suffix for asynchronous methods

## Code Style
- Use tabs for indentation (4 spaces per tab)
- Limit lines to 120 characters
- Use braces for all control structures, even for single-line statements
- Place opening braces on new lines
- Use explicit access modifiers (public, private, protected) for all members
- Organize using directives at the top of the file, sorted alphabetically
- Group related methods and properties together
- Use regions to organize large classes
- Comment complex logic and public methods with XML documentation comments
- Avoid deep nesting; refactor into smaller methods if necessary
- Prefer composition over inheritance where applicable

## Error Handling
- Error handling in a Unity C# project should be done using try-catch blocks where appropriate.
- Log errors using Unity's Debug.LogError for visibility in the console.
- Avoid using exceptions for control flow; use them for exceptional cases only.
- Clean up resources in finally blocks if necessary.

## Testing
- To be determined based on project needs.

## Security
- This is a multiplayer game. Always validate and sanitize user inputs to prevent cheating and exploits.
- Avoid hardcoding sensitive information (e.g., API keys, passwords) in the codebase.
- Use secure communication protocols (e.g., HTTPS) for network interactions.
- Regularly update third-party libraries and dependencies to patch known vulnerabilities.
- Implement proper authentication and authorization mechanisms for user access.
- Log security-related events for auditing purposes.
- Conduct regular security reviews and penetration testing to identify and mitigate potential vulnerabilities.
- Educate the development team on secure coding practices and common security threats.

---

## Code Examples
```js
// Correct pattern
function MyFunction() 
{
    ... 
}

// Incorrect pattern
function My_function() { ... }
```

---

## [Optional] Task-Specific or Advanced Sections
- Always read the complete file that were changed to understand the full context.
- When reviewing, consider the impact of changes on performance, especially in performance-critical sections of the code.
- Ensure that changes adhere to SOLID principles and other design patterns where applicable.
- Check for proper use of Unity-specific features, such as ScriptableObjects, MonoBehaviours, and Coroutines.
- Verify that any new assets or resources are properly managed and do not lead to memory leaks.
- Ensure that code changes are compatible with the target platforms (e.g., mobile, console, PC) and do not introduce platform-specific issues.
- Encourage the use of Unity's built-in profiling tools to identify and optimize performance bottlenecks.
- Promote collaboration and knowledge sharing among team members to improve code quality and consistency.
- Foster a culture of continuous improvement by regularly revisiting and updating code review guidelines based on team feedback and evolving best practices.
- Propose refactoring opportunities when you see code that could be improved for readability, maintainability, or performance.
- Highlight any potential technical debt introduced by the changes and suggest ways to mitigate it.
- Encourage the use of automated code analysis tools to catch common issues and enforce coding standards.