## 1. API Contracts and DTO Updates

- [x] 1.1 Add active-version metadata to workflow list/read DTOs used by Home and Rules page grids
- [x] 1.2 Add/extend request models to distinguish update-selected-version from create-new-version for workflow and rule operations
- [x] 1.3 Add/extend validation result DTOs for rule-expression and workflow-level validation responses

## 2. Workflow Versioning and Initialization Logic (RulesEngine.API)

- [x] 2.1 Refactor workflow save path so standard save updates only the selected workflow version
- [x] 2.2 Implement explicit workflow create-new-version path with confirmation-safe API intent
- [x] 2.3 Enforce new workflow version initialization with exactly one `Default Rule` and no other implicit rules
- [x] 2.4 Ensure new workflow creation defaults to `IsEnabled=false` when not explicitly overridden

## 3. Rule Versioning, Deletion, and Validation Logic (RulesEngine.API)

- [x] 3.1 Refactor rule save path so standard save updates only the selected rule version
- [x] 3.2 Implement explicit rule create-new-version path with confirmation-safe API intent
- [x] 3.3 Implement rule delete operation for selected workflow-rule context
- [x] 3.4 Implement/extend rule-expression validation operation that returns structured valid/error results without persistence side effects
- [x] 3.5 Implement/extend workflow-level validation operation for selected workflow rows with structured valid/error results

## 4. Home Page UI Updates (RulesEngine.UI)

- [x] 4.1 Add `Active Version` column after `Guid ID` in Home workflow grid
- [x] 4.2 Update Edit Workflow modal so Save updates only the selected version from the version selector
- [x] 4.3 Add `Create New Version` button to Edit Workflow modal with yes/no confirmation dialog
- [x] 4.4 Set New Workflow modal `Enable` toggle initial state to OFF

## 5. Rules Page UI Updates (RulesEngine.UI)

- [x] 5.1 Update Edit Rule modal so Save updates only the selected rule version
- [x] 5.2 Add `Create New Version` button to Edit Rule modal with yes/no confirmation dialog
- [x] 5.3 Add `Delete` action button in rules details grid after Edit, with yes/no confirmation dialog before delete
- [x] 5.4 Add `Validate` action button in rules details grid after Delete, and display expression validation results
- [x] 5.5 Add `Validate` action button in workflows main grid after New Rule, and display workflow validation results

## 6. Testing and Regression Coverage

- [x] 6.1 Add API tests proving save-in-place does not create new workflow/rule revisions
- [x] 6.2 Add API tests proving explicit create-version operations create exactly one new revision
- [x] 6.3 Add API tests for default workflow disabled state and default-rule-only initialization on new workflow versions
- [x] 6.4 Add API tests for rule delete and both validation endpoints (success and error cases)
- [x] 6.5 Add UI/component tests for new buttons/dialog flows and grid column/action ordering
- [x] 6.6 Run RulesEngine.API and RulesEngine.UI build/test validation and resolve regressions
