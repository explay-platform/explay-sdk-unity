/**
 * Explay SDK JavaScript Plugin for Unity WebGL
 * This file provides the bridge between Unity C# and JavaScript postMessage API
 */

mergeInto(LibraryManager.library, {
    SendMessageToParent: function(typePtr, requestId, payloadPtr) {
        var type = UTF8ToString(typePtr);
        var payload = UTF8ToString(payloadPtr);

        // Parse payload JSON
        var payloadObj = {};
        try {
            if (payload && payload !== '{}') {
                payloadObj = JSON.parse(payload);
            }
        } catch (e) {
            console.error('[Explay SDK] Failed to parse payload:', e);
        }

        // Send message to parent window
        window.parent.postMessage({
            type: type,
            requestId: requestId,
            payload: payloadObj
        }, '*');
    },

    NotifyReady: function() {
        window.parent.postMessage({
            type: 'GAME_READY'
        }, '*');

        console.log('[Explay SDK] Game Ready notification sent');
    }
});

// Listen for responses from parent window
window.addEventListener('message', function(event) {
    var data = event.data;

    // Only handle RESPONSE messages
    if (data.type === 'RESPONSE') {
        try {
            // Convert response to JSON string
            var responseJson = JSON.stringify({
                type: data.type,
                requestId: data.requestId,
                success: data.success,
                data: data.data ? JSON.stringify(data.data) : null,
                error: data.error || null
            });

            // Send to Unity
            if (typeof unityInstance !== 'undefined') {
                unityInstance.SendMessage('ExplaySDK', 'OnMessageReceived', responseJson);
            } else if (typeof gameInstance !== 'undefined') {
                gameInstance.SendMessage('ExplaySDK', 'OnMessageReceived', responseJson);
            } else {
                console.warn('[Explay SDK] Unity instance not found');
            }
        } catch (e) {
            console.error('[Explay SDK] Error handling message:', e);
        }
    }
});

console.log('[Explay SDK] JavaScript plugin loaded');
