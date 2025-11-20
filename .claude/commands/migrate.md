I want to follow the migration plan. Please let's go phase by phase.

Prompt the use if he wants to: proceed, cancel, or to skip this phase and go to next one

Example of response:

```
  Goal: Establish a solid baseline to measure against post-migration

  Expected End Result:

  - baseline_tests.log - Complete test results
  - baseline_build.log - Complete build output
  - BASELINE_ISSUES.md - Documentation of all current warnings and issues
  - Clear understanding of which packages need updates and which breaking changes might affect Mockaco
  - List of potential risks identified before making changes

  *Why this is critical:*

  This gives us a "known good state" to compare against. If something breaks during migration, we'll know exactly what changed.

  *Estimated time:*

  10-15 minutes

─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
←  ☐ Phase 1  ✔ Submit  →

Do you want to proceed with Phase 1: Pre-Migration Preparation & Baseline?

❯ 1. Proceed
     Start Phase 1 - create baseline and prepare for migration
  2. Skip to Phase 2
     Skip baseline creation and go directly to updating project files
  3. Cancel
     Stop the migration process
  4. Type something.
```
