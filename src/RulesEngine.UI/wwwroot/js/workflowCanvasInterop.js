window.workflowCanvasInterop = {
    initializeCanvas: function (elementId, initialData) {
        const stateStore = (window.__workflowCanvasState = window.__workflowCanvasState || {});
        const container = document.getElementById(elementId);

        if (!container || typeof window.LogicFlow === "undefined") {
            return false;
        }

        container.innerHTML = "";

        const lf = new window.LogicFlow({
            container: container,
            grid: true,
            keyboard: true,
            background: true
        });

        const state = {
            elementId: elementId,
            lf: lf,
            selectedNodeId: null
        };

        lf.on("node:click", function (args) {
            state.selectedNodeId = args && args.data ? args.data.id : null;
        });

        lf.on("blank:click", function () {
            state.selectedNodeId = null;
        });

        stateStore[elementId] = state;
        lf.render(initialData && initialData.nodes ? initialData : { nodes: [], edges: [] });
        return true;
    },

    getGraphData: function (elementId) {
        const state = (window.__workflowCanvasState || {})[elementId];
        return state && state.lf ? state.lf.getGraphData() : null;
    },

    loadGraphData: function (elementId, graphData) {
        const state = (window.__workflowCanvasState || {})[elementId];
        if (!state || !state.lf) {
            return false;
        }

        state.selectedNodeId = null;
        state.lf.render(graphData && graphData.nodes ? graphData : { nodes: [], edges: [] });
        return true;
    },

    addRuleNode: function (elementId, node) {
        const state = (window.__workflowCanvasState || {})[elementId];
        if (!state || !state.lf) {
            return null;
        }

        const graphData = state.lf.getGraphData() || { nodes: [] };
        const nextIndex = (graphData.nodes || []).length;
        const created = state.lf.addNode({
            id: node.id,
            type: "rect",
            x: node.x || (160 + nextIndex * 180),
            y: node.y || 160,
            text: node.label || "Rule",
            properties: {
                ruleGuidId: node.ruleGuidId,
                version: node.version,
                executeOrder: node.executeOrder
            }
        });

        state.selectedNodeId = created.id;
        return created.id;
    },

    connectNodes: function (elementId, sourceNodeId, targetNodeId) {
        const state = (window.__workflowCanvasState || {})[elementId];
        if (!state || !state.lf || !sourceNodeId || !targetNodeId || sourceNodeId === targetNodeId) {
            return false;
        }

        state.lf.addEdge({
            type: "polyline",
            sourceNodeId: sourceNodeId,
            targetNodeId: targetNodeId
        });

        return true;
    },

    getSelectedNodeId: function (elementId) {
        const state = (window.__workflowCanvasState || {})[elementId];
        return state ? state.selectedNodeId : null;
    },

    updateRuleNode: function (elementId, nodeId, update) {
        const state = (window.__workflowCanvasState || {})[elementId];
        if (!state || !state.lf || !nodeId) {
            return false;
        }

        const model = state.lf.getNodeModelById(nodeId);
        if (!model) {
            return false;
        }

        if (update && update.label) {
            state.lf.updateText(nodeId, update.label);
        }

        model.setProperties({
            ruleGuidId: update.ruleGuidId,
            version: update.version,
            executeOrder: update.executeOrder
        });

        return true;
    }
};
