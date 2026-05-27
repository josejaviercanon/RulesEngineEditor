window.workflowCanvasInterop = {
    initializeCanvas: function (elementId, initialData) {
        const state = {
            elementId: elementId,
            graphData: initialData ?? {}
        };

        window.__workflowCanvasState = window.__workflowCanvasState || {};
        window.__workflowCanvasState[elementId] = state;
        return true;
    },

    getGraphData: function (elementId) {
        const state = (window.__workflowCanvasState || {})[elementId];
        return state ? state.graphData : null;
    },

    loadGraphData: function (elementId, graphData) {
        window.__workflowCanvasState = window.__workflowCanvasState || {};
        const state = window.__workflowCanvasState[elementId] || { elementId: elementId, graphData: {} };
        state.graphData = graphData ?? {};
        window.__workflowCanvasState[elementId] = state;
        return true;
    }
};
