/**
 * Explay SDK JavaScript Plugin for Unity WebGL
 * This file provides the bridge between Unity C# and JavaScript postMessage API
 */

var ExplaySDKPlugin = {
    $ExplaySDKState: {
        initialized: false,
        unityInstanceRef: null,

        getUnityInstance: function() {
            // Return cached instance if available
            if (this.unityInstanceRef) return this.unityInstanceRef;

            // Try different Unity instance patterns
            var candidates = [
                typeof Module !== 'undefined' ? Module : null,
                typeof unityInstance !== 'undefined' ? unityInstance : null,
                typeof gameInstance !== 'undefined' ? gameInstance : null,
                typeof window.unityInstance !== 'undefined' ? window.unityInstance : null
            ];

            for (var i = 0; i < candidates.length; i++) {
                var candidate = candidates[i];
                if (candidate && typeof candidate.SendMessage === 'function') {
                    console.log('[explay SDK] Found Unity instance at index ' + i);
                    this.unityInstanceRef = candidate;
                    return candidate;
                }
            }

            return null;
        },

        init: function() {
            if (this.initialized) return;
            this.initialized = true;

            var self = this;

            // Listen for responses from parent window
            window.addEventListener('message', function(event) {
                var data = event.data;

                // Only handle RESPONSE messages
                if (data && data.type === 'RESPONSE') {
                    try {
                        // Convert response to JSON string
                        var responseJson = JSON.stringify({
                            type: data.type,
                            requestId: data.requestId,
                            success: data.success,
                            data: data.data ? JSON.stringify(data.data) : null,
                            error: data.error || null
                        });

                        // Get Unity instance
                        var instance = self.getUnityInstance();

                        if (instance && instance.SendMessage) {
                            instance.SendMessage('explayGameServices', 'OnMessageReceived', responseJson);
                        } else {
                            console.warn('[explay SDK] Unity instance not found');
                        }
                    } catch (e) {
                        console.error('[explay SDK] Error handling message:', e);
                    }
                }
            });

            console.log('[explay SDK] JavaScript plugin loaded');
        }
    },

    SendMessageToParent: function(typePtr, requestId, payloadPtr) {
        ExplaySDKState.init();

        var type = UTF8ToString(typePtr);
        var payload = UTF8ToString(payloadPtr);

        // Parse payload JSON
        var payloadObj = {};
        try {
            if (payload && payload !== '{}') {
                payloadObj = JSON.parse(payload);
            }
        } catch (e) {
            console.error('[explay SDK] Failed to parse payload:', e);
        }

        // Send message to parent window
        window.parent.postMessage({
            type: type,
            requestId: requestId,
            payload: payloadObj
        }, '*');
    },

    NotifyReady: function() {
        ExplaySDKState.init();

        window.parent.postMessage({
            type: 'GAME_READY'
        }, '*');

        console.log('[explay SDK] Game Ready notification sent');
    }
};

autoAddDeps(ExplaySDKPlugin, '$ExplaySDKState');
mergeInto(LibraryManager.library, ExplaySDKPlugin);
