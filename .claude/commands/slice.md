---
description: Add a complete vertical slice (command, handler, validator, endpoint, registration)
---

Add a complete vertical slice for: $ARGUMENTS

Follow CLAUDE.md conventions exactly:

1. Application layer: the command record implementing ICommand<TResponse>, its handler
   (internal, thin: load or create via the domain, call domain behavior, save via
   IUnitOfWork), and a validator implementing IValidator<T> for input shape only
   (business rules live in the domain, not the validator).
2. Presentation layer: the minimal-API endpoint, mapping Result failures to
   400 (validation/business) or 404 (not found), successes to 200/201/204 as appropriate.
3. Register the handler with the existing registration extension. Do not create new
   infrastructure.

Constraints:

- Do not modify SharedKernel, Domain, or any test file.
- After implementing, build and run the full test suite and show me the output.
- If any step conflicts with an existing convention, stop and ask instead of improvising.
