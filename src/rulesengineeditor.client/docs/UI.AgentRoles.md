# UI Agent Roles – React Workflow Editor

## Overview
The UI project provides a workflow editor for Microsoft RulesEngine JSON schema.
Technologies: React, Vite, TypeScript, ReactFlow, Monaco Editor.

## Agent Responsibilities
- **Schema Export**: Generate JSON compliant with RulesEngine schema.
- **Canvas Rendering**: Maintain drag‑drop editor nodes and edges.
- **Validation Hooks**: Trigger backend validation calls on save.
- **UI Testing**: Write Vitest/React Testing Library unit tests for components.

## Human Revision Points
- Debug ReactFlow node rendering issues.
- Review schema export before committing.
- Validate API calls in `apiClient.ts`.

## Debug Guide
- Run `npm run dev` for local debugging.
- Use Chrome DevTools for component inspection.
- Check `vite.config.js` proxy settings for API calls.
