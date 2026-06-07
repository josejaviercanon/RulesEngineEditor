# Debug Guide – React Workflow Editor

## Common Issues
1. **Node Rendering Errors**
   - Symptom: Workflow nodes not visible or misaligned.
   - Fix: Inspect ReactFlow component props; check CSS in `WorkflowCanvas.tsx`.

2. **API Call Failures**
   - Symptom: Validation/execute endpoints not reachable.
   - Fix: Verify `vite.config.js` proxy settings; confirm backend port.

3. **Schema Export Problems**
   - Symptom: JSON missing required fields.
   - Fix: Review `SchemaExporter.ts` logic; compare against RulesEngine schema docs.

## Debugging Steps
- Run `npm run dev` and open Chrome DevTools.
- Use React Developer Tools to inspect component state.
- Check console logs for API errors.
- Validate JSON output with backend `/validate` endpoint.

## Human Intervention
- Developers manually adjust node rendering logic.
- Review schema export before committing.
- Approve agent‑generated UI changes via pull requests.
