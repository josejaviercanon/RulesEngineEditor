# UI Architecture – React Workflow Editor

## Purpose
Provide a drag‑and‑drop workflow editor that exports JSON compliant with Microsoft RulesEngine schema.  
Technologies: React, Vite, TypeScript, ReactFlow, Monaco Editor.

---

## Project Structure

UI.React/
├── package.json
├── vite.config.js
├── tsconfig.json
├── public/
└── src/
├── components/
│   ├── WorkflowCanvas.tsx
│   ├── NodeEditor.tsx
│   ├── ParameterPanel.tsx
│   └── SchemaExporter.ts
├── services/
│   └── apiClient.ts
├── hooks/
│   └── useWorkflow.ts
└── App.tsx


---

## Core Components
- **WorkflowCanvas.tsx**  
  Hosts ReactFlow canvas for drag‑drop workflow design.

- **NodeEditor.tsx**  
  Provides node configuration UI (conditions, actions, parameters).

- **ParameterPanel.tsx**  
  Side panel for editing workflow parameters and metadata.

- **SchemaExporter.ts**  
  Converts canvas state into JSON aligned with RulesEngine schema.

---

## Services
- **apiClient.ts**  
  Handles REST calls to backend (`/validate`, `/execute`).  
  Configured with proxy in `vite.config.js` for local dev.

---

## Hooks
- **useWorkflow.ts**  
  Custom React hook for managing workflow state, undo/redo, and schema validation feedback.

---

## Integration Points
- **Backend API**  
  - `/api/workflows/validate` → validate exported JSON.  
  - `/api/workflows/execute` → dry‑run execution.  
  - Results surfaced in UI via toast/notification system.

- **Schema Compliance**  
  - JSON export validated against RulesEngine schema before sending.  
  - Errors highlighted in canvas nodes.

---

## Testing Strategy
- **Playwright E2E**  
  - Simulate drag‑drop workflow creation.  
  - Export JSON and validate via backend.  
  - Assert UI displays backend validation results.

- **Optional Unit Tests**  
  - React Testing Library for isolated component logic.  
  - Keep minimal to avoid duplication with Playwright.

---

## Debugging Guide
- Run `npm run dev` for local development.  
- Use Chrome DevTools + React Developer Tools for component inspection.  
- Check `vite.config.js` proxy settings if API calls fail.  
- Validate JSON output manually with backend `/validate` endpoint.

---

## Human‑in‑the‑Loop
- Developers review schema export logic before merging.  
- Manual debugging of node rendering and API integration.  
- Approve agent‑generated UI changes via pull requests.
