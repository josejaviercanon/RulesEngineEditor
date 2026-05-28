## ADDED Requirements

### Requirement: WorkflowDto serializes to RulesEngine canonical JSON format
The system SHALL serialize a `WorkflowDto` to JSON that is structurally compatible with the RulesEngine `workflow-list-schema.json` schema, using the same property names as the `RulesEngine.Models.Workflow` class (`WorkflowName`, `Rules`, `GlobalParams`, etc.).

#### Scenario: Serialized WorkflowDto JSON passes schema validation
- **WHEN** a populated `WorkflowDto` is serialized to JSON by the backend
- **THEN** the resulting JSON validates against `RulesEngine/schema/workflow-schema.json` with no errors

#### Scenario: Serialized JSON can be deserialized back to WorkflowDto without loss
- **WHEN** a `WorkflowDto` is serialized to JSON and then deserialized back to `WorkflowDto`
- **THEN** all fields including nested `Rules`, `GlobalParams`, `LocalParams`, and `Actions` are identical to the original

### Requirement: Backend accepts raw RulesEngine JSON for import
The system SHALL accept a raw JSON string (in RulesEngine workflow format) via a dedicated import endpoint or field and deserialize it into the typed `WorkflowDto` model.

#### Scenario: Valid RulesEngine JSON imports to WorkflowDto
- **WHEN** a valid RulesEngine workflow JSON string is submitted to the import path
- **THEN** the backend deserializes it to a `WorkflowDto` with all fields populated and returns the typed DTO in the response

#### Scenario: Invalid JSON returns structured parse error
- **WHEN** a malformed or schema-incompatible JSON string is submitted to the import path
- **THEN** the backend returns a structured error response with a human-readable parse failure message and does not persist

### Requirement: WorkflowDto serializes using PascalCase property names matching RulesEngine model
The system SHALL configure JSON serialization for `WorkflowDto` and its nested types to use PascalCase property names (e.g., `WorkflowName`, `RuleName`, `GlobalParams`) matching the RulesEngine library convention, not camelCase.

#### Scenario: API response uses PascalCase for workflow model fields
- **WHEN** a workflow is retrieved via GET /api/workflows/{id}
- **THEN** the JSON response body uses `WorkflowName`, `Rules`, `GlobalParams`, `RuleName`, `Expression` (PascalCase) for the embedded workflow model fields
