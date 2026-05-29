## MODIFIED Requirements

### Requirement: Workflow list projections include active version metadata
The system SHALL include active-version metadata in workflow list projections used by Home and Rules page grids, and SHALL also provide latest-version (`LastVersion`) metadata so UI surfaces can distinguish active revision from total retained revision count.

#### Scenario: List response includes active and last version fields
- **WHEN** the client requests workflow list data
- **THEN** each workflow row includes active version and last version metadata that can be rendered independently

#### Scenario: Rules page workflow grid can place Active Version after Guid ID
- **WHEN** the Rules page renders workflow rows from list projection data
- **THEN** the projection contains explicit active-version metadata suitable for rendering immediately after Guid ID without deriving it from last version
